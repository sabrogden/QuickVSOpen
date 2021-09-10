using System;
using System.Collections.Generic;
using System.Text;
using EnvDTE;
using EnvDTE80;
using System.IO;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell;

namespace QuickVSOpen
{
	public class SolutionFiles : ISearchable
	{
		//private Plugin mPlugin;
		private DTE2 m_application = null;
		private List<SearchEntry> m_solutionFiles = new List<SearchEntry>();
		private List<SearchEntry> mHits = new List<SearchEntry>();
		string m_excludeFilePath;
		bool m_equalSearch = false;
		bool m_keyIsFileName = false;

		public DateTime LastRefresh { get; set; }

		public int LastRefreshDurationMS { get; set; }

		public int Count
		{
			get { return m_solutionFiles.Count; }
		}

		public SolutionFiles(DTE2 dt)
		{
			m_application = dt;
		}

		public void Refresh()
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			Stopwatch w = Stopwatch.StartNew();
			m_solutionFiles.Clear();
			mHits.Clear();

			foreach (Project project in m_application.Solution.Projects)
			{
				Log.Info("\tScanning project {0}", project.Name);
				AddProjectItems(project.ProjectItems);
			}
			Log.Info("Scanning done ({0} files in {1} projects)", Count, m_application.Solution.Projects.Count);

			mHits.AddRange(m_solutionFiles);

			LastRefresh = DateTime.Now;
			LastRefreshDurationMS = (int)w.ElapsedMilliseconds;
		}

		private int AddFilesFromDir(string dirname)
		{
			int count = 0;

			foreach (string file in Directory.GetFiles(dirname))
			{
				SearchEntry entry = new SearchEntry();
				entry.fullPath = file;
				entry.filename = Path.GetFileName(file);
				entry.key = entry.filename.ToLower();
				if (m_keyIsFileName)
				{
					entry.key = entry.filename.ToLower();
				}

				if (file != m_excludeFilePath)
				{
					m_solutionFiles.Add(entry);
				}

				count++;
			}

			foreach (string dir in Directory.GetDirectories(dirname))
			{
				count += AddFilesFromDir(dir);
			}

			return count;
		}

		private void AddProjectItems(ProjectItems projectItems)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			// HACK: Horrible! But how will we know what not to include in the list?
			// CPP:   {6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}
			// H:     {6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}
			// Folder:{6BB5F8F0-4483-11D3-8BCF-00C04F8EC28C}
			//
			// "{5F422961-1EE4-47EB-A53B-777B4164F8DF}" <-- it's a folder ?
			if (null == projectItems)
				return;
			foreach (ProjectItem item in projectItems)
			{
				if ("{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}" == item.Kind)
				{
					// Indices starts at 1 ... http://msdn.microsoft.com/en-us/library/envdte.projectitem.filenames.aspx
					for (short i = 0; i < item.FileCount; i++)
					{
						string name = item.get_FileNames((short)i);

						SearchEntry entry = new SearchEntry();
						entry.fullPath = name;
						entry.filename = Path.GetFileName(name);
						entry.key = entry.filename.ToLower();
						if (m_keyIsFileName)
						{
							entry.key = entry.filename.ToLower();
						}
						if (name != m_excludeFilePath)
						{
							m_solutionFiles.Add(entry);
						}
					}
				}

				AddProjectItems(item.ProjectItems);

				if (item.SubProject != null)
				{
					AddProjectItems(item.SubProject.ProjectItems);
				}

			}
		}

		public int CandidateCount
		{
			get { return mHits.Count; }
		}

		public string ExcludeFilePath
		{
			get
			{
				return m_excludeFilePath;
			}

			set
			{
				m_excludeFilePath = value;
			}
		}

		public bool EqualSearch
		{
			get
			{
				return m_equalSearch;
			}

			set
			{
				m_equalSearch = value;
			}
		}

		public bool KeyIsFileName
		{
			get
			{
				return m_keyIsFileName;
			}

			set
			{
				m_keyIsFileName = value;
			}
		}

		public SearchEntry Candidate(int i)
		{
			return mHits[i];
		}

		public void UpdateSearchQuery(string query, bool incremental)
		{
			if (!incremental)
			{
				mHits = Filter(m_solutionFiles, query);
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
			if (m_equalSearch)
			{
				return candidates.FindAll(delegate (SearchEntry e) { return e.key == query; });
			}
			else
			{
				return candidates.FindAll(delegate (SearchEntry e) { return e.key.Contains(query); });
			}
		}
	}
}
