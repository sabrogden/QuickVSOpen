using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace QuickVSOpen
{
    /// <summary>
    /// Interaction logic for Open.xaml
    /// </summary>
    public partial class OpenDialog : Window
    {
        ISearchable m_data;
        public SearchEntry Selected { get; set; }
        public List<SearchEntry> AllSelected { get; } = new List<SearchEntry>();
        Stopwatch m_stopWatch = Stopwatch.StartNew();
        public bool Result { get; set; } = false;
        public string SelectedSearchString { get; set; }
        bool m_handleTextChanged = true;

        public double ClosedDpi { get; set; }
        public bool AllowCloseClose { get; set; } = false;

        public OpenDialog(ISearchable files, bool multiSelect, bool showSecondColumn, bool showFileTooltip)
        {
            InitializeComponent();
            m_data = files;
            RefreshFilter("");

            if (multiSelect)
            {
                m_resultsListView.SelectionMode = SelectionMode.Extended;
            }
            else
            {
                m_resultsListView.SelectionMode = SelectionMode.Single;
            }

            GridView gv = (m_resultsListView.View as GridView);
            if(showSecondColumn)
            {
                gv.Columns[0].Width = this.Width-160;
                gv.Columns[1].Width = 100;
            }
            else
            {
                gv.Columns[0].Width = this.Width - 60;
                gv.Columns[1].Width = 0;
            }

            m_searchTextBox.Focus();
        }

        private void m_searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_handleTextChanged)
            {
                RefreshFilter(m_searchTextBox.Text);
            }
        }

        public void SetData(ISearchable value, string searchString)
        {
            m_handleTextChanged = false;

            m_data = value;
            m_searchTextBox.Text = searchString;
            RefreshFilter(searchString);

            m_handleTextChanged = true;
        }

        public void RefreshFilter(string filter)
        {
            m_data.UpdateSearchQuery(filter, false);
            m_resultsListView.ItemsSource = m_data.Hits;
            m_resultsListView.SelectedIndex = 0;
            m_statusTextBlock.Text = $"Hits: {m_data.CandidateCount} | Last Refresh: {m_data.LastRefresh.ToString()} | {m_data.LastRefreshDurationMS}ms";
        }

        void SetSelectedItemsAndClose()
        {
            Selected = null;
            AllSelected.Clear();

            foreach(SearchEntry sel in m_resultsListView.SelectedItems)
            { 
                if (Selected == null)
                {
                    Selected = (SearchEntry)sel;
                    Selected.lastUsed = m_stopWatch.ElapsedMilliseconds;
                }

                AllSelected.Add(sel);
            }

            //RefreshFilter(m_searchTextBox.Text);

            Result = true;
            this.Close();
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            public static implicit operator System.Drawing.Point(POINT p)
            {
                return new System.Drawing.Point(p.X, p.Y);
            }

            public static implicit operator POINT(System.Drawing.Point p)
            {
                return new POINT(p.X, p.Y);
            }
        }

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        static System.Windows.Forms.Screen ScreenFromPoint1(POINT p)
        {
            System.Drawing.Point pt = new System.Drawing.Point((int)p.X, (int)p.Y);
            return System.Windows.Forms.Screen.AllScreens
                            .Where(scr => scr.Bounds.Contains(pt))
                            .FirstOrDefault();
        }

        static System.Windows.Forms.Screen ScreenFromPoint2(POINT p)
        {
            System.Drawing.Point pt = new System.Drawing.Point((int)p.X, (int)p.Y);
            var scr = System.Windows.Forms.Screen.FromPoint(pt);
            return scr.Bounds.Contains(pt) ? scr : null;
        }

        public void Init()
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                m_searchTextBox.Focus();
                if (m_searchTextBox.Text.Length > 0)
                {
                    m_searchTextBox.SelectAll();
                }

                m_resultsListView.SelectedIndex = 0;
             }));

            Result = false;

            var source = PresentationSource.FromVisual(this.Owner);
            var dpi = source?.CompositionTarget?.TransformFromDevice.M11 ?? 1.0;

            var p = this.Owner.PointToScreen(new Point(this.Owner.Width / 2, this.Owner.Height / 2));

            POINT lpPoint;
            lpPoint.X = (int)p.X;
            lpPoint.Y = (int)p.Y;

            var screen = ScreenFromPoint1(lpPoint);

            this.Left = screen.Bounds.Left + ((screen.Bounds.Width * dpi) / 2) - (this.Width / 2);
            this.Top = screen.Bounds.Top + ((screen.Bounds.Height * dpi) / 2) - (this.Height / 2);

            this.ShowInTaskbar = false;
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                switch (e.Key)
                {
                    case Key.Escape:
                        e.Handled = true;
                        if (m_searchTextBox.Text.Length > 0)
                        {
                            m_searchTextBox.Text = "";
                        }
                        else
                        {
                            this.Close();
                        }
                        break;

                    case Key.Enter:
                        SetSelectedItemsAndClose();
                        break;

                    case Key.F5:

                        try
                        {
                            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
                            m_data.Refresh();
                            if (m_searchTextBox.Text.Length > 0)
                            {
                                m_searchTextBox.Text = "";
                            }
                            else
                            {
                                RefreshFilter("");
                            }

                            e.Handled = true;
                        }
                        finally
                        {
                            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                        }
                        break;

                    default:

                        break;
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void m_resultsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SetSelectedItemsAndClose();
        }

        private void m_resultsListView_KeyDown(object sender, KeyEventArgs e)
        {
            var kc = new System.Windows.Forms.KeysConverter();

            string c = kc.ConvertToString(e.Key);

            if (char.IsLetter(c[0]) ||
                char.IsDigit(c[0]))
            {
                if (m_searchTextBox.IsFocused == false)
                {
                    m_searchTextBox.Text += c.ToLower();
                    m_searchTextBox.SelectionStart = m_searchTextBox.Text.Length;
                    m_searchTextBox.SelectionLength = 0;
                    m_searchTextBox.Focus();
                }
            }
        }

        private void m_searchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up ||
                e.Key == Key.Down ||
                (e.Key == Key.Home && Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) ||
                (e.Key == Key.End && Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                var index = -1;
                if (m_resultsListView.SelectedIndex >= 0)
                {
                    index = m_resultsListView.SelectedIndex;
                }

                if (e.Key == Key.Home)
                {
                    index = 0;
                }
                else if (e.Key == Key.End)
                {
                    index = m_data.CandidateCount - 1;
                }
                else if (e.Key == Key.Up)
                {
                    index--;
                }
                else
                {
                    index++;
                }

                if (m_data.CandidateCount > index &&
                    index >= 0)
                {
                    m_resultsListView.SelectedIndex = index;
                    m_resultsListView.ScrollIntoView(m_resultsListView.SelectedItem);

                }

                e.Handled = true;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SelectedSearchString = m_searchTextBox.Text;

            if (this.AllowCloseClose == false)
            {
                e.Cancel = true;

                //keep track of this, not sure why but i was having problems (window would disapear) with closing the window and opening on a different dpi screen
                //this lets us close the window on open if they are on different dpi and that works then
                var s = PresentationSource.FromVisual(this);
                this.ClosedDpi = s?.CompositionTarget?.TransformFromDevice.M11 ?? 1.0;

                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (DispatcherOperationCallback)(arg =>
                {
                    this.Hide();
                    return null;
                }), null);
            }
        }
    }
}
