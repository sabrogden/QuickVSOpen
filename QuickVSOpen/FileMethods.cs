using System;
using System.Collections.Generic;
using System.Text;
using EnvDTE;
using EnvDTE80;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using Microsoft.VisualStudio.Shell;

namespace QuickVSOpen
{
    public class FileMethods : ISearchable
    {
        private DTE2 m_dt;
        private List<SearchEntry> m_methods = new List<SearchEntry>();
        private List<SearchEntry> mHits = new List<SearchEntry>();
        string m_fileName;
        DateTime m_lastWrite;
        public System.DateTime LastWrite
        {
            get { return m_lastWrite; }
            set { m_lastWrite = value; }
        }

        public DateTime LastRefresh { get; set; }

        public int LastRefreshDurationMS { get; set; }

        public string FileName
        {
            get
            {
                return m_fileName;
            }
            set
            {
                m_fileName = value;
                LastWrite = System.IO.File.GetLastWriteTimeUtc(m_fileName);
            }
        }

        public int Count
        {
            get { return m_methods.Count; }
        }

        public FileMethods(DTE2 dt)
        {
            m_dt = dt;
        }

        public void RefreshUsingVisualStudio()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Stopwatch w = Stopwatch.StartNew();
            m_methods.Clear();
            mHits.Clear();

            foreach (CodeElement cn in m_dt.ActiveWindow.ProjectItem.FileCodeModel.CodeElements)
            {
                if (cn.Kind == vsCMElement.vsCMElementNamespace)
                {
                    foreach (CodeElement cc in ((CodeNamespace)cn).Members)
                    {
                        if (cc.Kind == vsCMElement.vsCMElementClass)
                        {
                            foreach (CodeElement ce in ((CodeClass)cc).Members)
                            {
                                if (ce.StartPoint != null)
                                {
                                    string methodType = "";

                                    switch (ce.Kind)
                                    {
                                        case vsCMElement.vsCMElementFunction:
                                            methodType = "Function";
                                            break;
                                        case vsCMElement.vsCMElementProperty:
                                            methodType = "Property";
                                            break;
                                        case vsCMElement.vsCMElementVariable:
                                            methodType = "Variable";
                                            break;
                                        case vsCMElement.vsCMElementEnum:
                                            methodType = "Enum";
                                            break;
                                    }

                                    if (methodType != "")
                                    {
                                        SearchEntry entry = new SearchEntry()
                                        {
                                            methodType = methodType,
                                            filename = ce.Name,
                                            lineNumber = ce.StartPoint.Line,
                                            key = ce.Name.ToLower(),
                                            fullPath = ce.Name
                                        };

                                        m_methods.Add(entry);
                                        mHits.Add(entry);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var el = w.ElapsedMilliseconds;
            LastRefresh = DateTime.Now;
            LastRefreshDurationMS = (int)w.ElapsedMilliseconds;
        }


        public void Refresh()
        {
            //Options options = ((Options)mPlugin.Options);
            //if (options.UseVisualStudioForFileMethods)
            //{
            //    RefreshUsingVisualStudio();
            //    return;
            //}

            Stopwatch w = Stopwatch.StartNew();

            m_methods.Clear();
            mHits.Clear();

            System.Diagnostics.Process p = new System.Diagnostics.Process();

            string cTagsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ctags.exe");

            p.StartInfo.FileName = cTagsPath;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.Arguments = " -n -f- \"" + FileName + "\"";
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.CreateNoWindow = true;
            p.Start();

            // Synchronously read the standard output of the spawned process. 
            StreamReader reader = p.StandardOutput;
            string output = reader.ReadToEnd();

            string[] lines = output.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                string[] columns = line.Split('\t');
                if (columns.Count() >= 4)
                {
                    SearchEntry entry = new SearchEntry();
                    entry.filename = columns[0];
                    string lineNumber = columns[2].TrimEnd('\"').TrimEnd(';');
                    int outLineNumber;
                    if (int.TryParse(lineNumber, out outLineNumber))
                    {
                        entry.lineNumber = outLineNumber;
                        entry.methodType = GetMethodType(columns[3]);
                        if (entry.methodType != "")
                        {
                            entry.key = entry.filename.ToLower();
                            entry.fullPath = entry.filename;
                            m_methods.Add(entry);
                            mHits.Add(entry);
                        }
                    }
                }
            }

            p.WaitForExit();
            p.Close();
            var el = w.ElapsedMilliseconds;
            LastRefresh = DateTime.Now;
            LastRefreshDurationMS = (int)w.ElapsedMilliseconds;
        }

        string GetMethodType(string shortName)
        {
            switch (shortName)
            {
                case "c":
                    return "class";
                case "d":
                    return "define";
                case "f":
                    return "variable";
                case "g":
                    return "enumeration";
                case "m":
                    return "method";
                case "p":
                    return "property";
                case "s":
                    return "structure";
                case "v":
                    return "variable";
            }

            return "";
        }

        public int CandidateCount
        {
            get { return mHits.Count; }
        }

        public SearchEntry Candidate(int i)
        {
            return mHits[i];
        }

        public void UpdateSearchQuery(string query, bool incremental)
        {
            if (!incremental)
            {
                mHits = Filter(m_methods, query);
            }
            else
            {
                mHits = Filter(mHits, query);
            }

            // TODO: Sort the files based on relevance.
            mHits.Sort(new SearchEntry.CompareOnRelevance(query));
        }

        private List<SearchEntry> Filter(List<SearchEntry> candidates, string query)
        {
            if (query == "")
            {
                var copy = new List<SearchEntry>();
                copy.AddRange(candidates);
                return copy;
            }

            query = query.ToLower();
            return candidates.FindAll(delegate (SearchEntry e) { return e.key.Contains(query); });
        }
    }
}
