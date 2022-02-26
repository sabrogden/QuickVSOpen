using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Task = System.Threading.Tasks.Task;
using System.Windows;

namespace QuickVSOpen
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class GoToMethodCommand
    {
        List<FileMethods> m_methods = new List<FileMethods>();
        OpenDialog m_openDialog = null;
        DTE2 m_dt;

        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 4129;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("24d9b5e5-75fc-4173-9201-8681d1738223");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="GoToMethodCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private GoToMethodCommand(AsyncPackage package, OleMenuCommandService commandService, DTE2 dt)
        {
            m_dt = dt;

            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static GoToMethodCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package, DTE2 dt)
        {
            // Switch to the main thread - the call to AddCommand in GoToMethodCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new GoToMethodCommand(package, commandService, dt);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (null == m_dt.ActiveDocument)
                return;

            FileMethods found = null;
            try
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
                string fullPath = m_dt.ActiveDocument.FullName;

                found = m_methods.FirstOrDefault(item => item.FileName == fullPath);
                if (null == found)
                {
                    found = new FileMethods(m_dt);
                    found.FileName = fullPath;
                    found.Refresh();

                    //Options options = ((Options)Plugin.Options);
                    //if (options.UseVisualStudioForFileMethods == false)
                    //{
                    //    Log.Info("adding method file to cache, file: " + fullPath);
                    //    m_methods.Insert(0, found);
                    //}

                    m_methods.Insert(0, found);
                }
                else
                {
                    //move the start of list, so it doesn't get removed
                    m_methods.Remove(found);
                    m_methods.Insert(0, found);

                    DateTime lastWrite = System.IO.File.GetLastWriteTimeUtc(fullPath);
                    if (found.LastWrite != lastWrite)
                    {
                        Log.Info("last write time changes for file: " + fullPath + " refreshing methods");
                        found.Refresh();

                        found.LastWrite = lastWrite;
                    }
                }
            }
            finally
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
            }

            if (found != null)
            {
                if(m_openDialog != null)
                {
                    var source = PresentationSource.FromVisual(m_openDialog.Owner);
                    var dpi = source?.CompositionTarget?.TransformFromDevice.M11 ?? 1.0;
                    if(dpi != m_openDialog.ClosedDpi)
                    {
                        m_openDialog.AllowCloseClose = true;
                        m_openDialog.Close();
                        m_openDialog = null;
                    }
                }

                if (m_openDialog == null)
                {
                    m_openDialog = new OpenDialog(found, true, true, true);
                    m_openDialog.Owner = System.Windows.Application.Current.MainWindow;
                }
                else
                {
                    m_openDialog.SetData(found, found.LastSearch);
                }

                m_openDialog.Init();
                m_openDialog.ShowDialog();
                if (m_openDialog.Result == true)
                {
                    var selectedEntry = m_openDialog.Selected;
                    if (selectedEntry != null &&
                        selectedEntry.lineNumber.HasValue)
                    {
                        m_dt.ExecuteCommand("Edit.GoTo", string.Format("{0}", selectedEntry.lineNumber.Value));
                    }
                }

                found.LastSearch = m_openDialog.SelectedSearchString;

                if (m_methods.Count > 30)
                {
                    Log.Info("removing method file to cache, file: " + m_methods[m_methods.Count - 1].FileName);
                    m_methods.RemoveAt(m_methods.Count - 1);
                }
            }
        }
    }
}
