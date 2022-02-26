using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Runtime.InteropServices;
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
    internal sealed class OpenSolutionFileCommand
    {
        private SolutionFiles m_files = null;
        OpenDialog m_openDialog = null;
        DTE2 m_dt;
        DateTime m_lastSolutionWriteTime;
        string m_lastSolutionFileName;

        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("24d9b5e5-75fc-4173-9201-8681d1738223");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSolutionFileCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private OpenSolutionFileCommand(AsyncPackage package, OleMenuCommandService commandService, DTE2 dt)
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
        public static OpenSolutionFileCommand Instance
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
            // Switch to the main thread - the call to AddCommand in OpenSolutionFileCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new OpenSolutionFileCommand(package, commandService, dt);
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

            if(m_dt == null ||
                m_dt.Solution == null ||
                string.IsNullOrWhiteSpace(m_dt.Solution.FileName))
            {
                return;
            }

            if (null == m_files)
            {
                try
                {
                    System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
                    Log.Info("First time fast open is run, scanning solution for files");
                    m_files = new SolutionFiles(m_dt);
                    m_files.Refresh();
                }
                finally
                {
                    System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                }
            }
            else
            {
                DateTime lastWrite = System.IO.File.GetLastWriteTimeUtc(m_dt.Solution.FileName);
                if (m_lastSolutionWriteTime != lastWrite ||
                    m_lastSolutionFileName != m_dt.Solution.FileName)
                {
                    if (m_files != null)
                    {
                        m_files.Refresh();
                        if (m_openDialog != null)
                        {
                            m_openDialog.RefreshFilter("");
                        }
                    }
                }
            }

            if (m_openDialog != null)
            {
                var source = PresentationSource.FromVisual(m_openDialog.Owner);
                var dpi = source?.CompositionTarget?.TransformFromDevice.M11 ?? 1.0;
                if (dpi != m_openDialog.ClosedDpi)
                {
                    m_openDialog.AllowCloseClose = true;
                    m_openDialog.Close();
                    m_openDialog = null;
                }
            }

            if (m_openDialog == null)
            {
                m_openDialog = new OpenDialog(m_files, true, false, true);
                m_openDialog.Owner = System.Windows.Application.Current.MainWindow;
                m_openDialog.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            }            

            m_openDialog.Init();
            m_openDialog.ShowDialog();

            if (m_openDialog.Result == true)
            {
                foreach (var file in m_openDialog.AllSelected)
                {
                    string name = file.FullPath;
                    if (name.Length > 0)
                        m_dt.ExecuteCommand("File.OpenFile", string.Format("\"{0}\"", name));
                }
            }

            m_lastSolutionWriteTime = System.IO.File.GetLastWriteTimeUtc(m_dt.Solution.FileName);
            m_lastSolutionFileName = m_dt.Solution.FileName;
        }
    }
}
