using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace QuickVSOpen
{
    public partial class QuickVSOpenDialog : Form
    {
        ISearchable mFiles;
        SearchEntry m_selected;
        List<SearchEntry> m_allSelected = new List<SearchEntry>();
        bool m_showSecondColumn = false;
        bool m_showFileToolTip = false;
        Stopwatch m_stopWatch = Stopwatch.StartNew();

        public SearchEntry SelectedEntry
        {
            get
            {
                return m_selected;
            }
        }

        public string FileToOpen
        {
            get
            {
                if (m_selected != null)
                {
                    return m_selected.fullPath;
                }
                return "";
            }
        }


        public ISearchable Files
        {
            set
            {
                mFiles = value;
                mStatusLabel.Text = string.Format("{0} hits", mFiles.CandidateCount);
                m_refreshStatusLabel.Text = "Last Refresh: " + mFiles.LastRefresh.ToString() + " - " + mFiles.LastRefreshDurationMS + "ms";
                FillListView();
                SelectFirstRow();
            }
        }

        public List<SearchEntry> AllSelected
        {
            get
            {
                return m_allSelected;
            }

            set
            {
                m_allSelected = value;
            }
        }

        public QuickVSOpenDialog(ISearchable files, bool multiSelect, bool showSecondColumn, bool showFileTooltip)
        {
            InitializeComponent();

            m_showSecondColumn = showSecondColumn;
            m_showFileToolTip = showFileTooltip;
            Files = files;

            listView1.Columns.Add("name", 80, HorizontalAlignment.Left);
            if (m_showSecondColumn)
            {
                listView1.Columns.Add("type", 80, HorizontalAlignment.Left);
            }
            listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;

            if (m_showFileToolTip)
            {
                listView1.ShowItemToolTips = true;
            }

            FillListView();
            listView1.MultiSelect = multiSelect;
            SizeColumns();
            SelectFirstRow();
        }

        private void FillListView()
        {
            listView1.VirtualListSize = mFiles.CandidateCount;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            RefreshFilter(mInputText.Text);
        }

        public void RefreshFilter(string filter)
        {
            mFiles.UpdateSearchQuery(filter, false);
            mStatusLabel.Text = string.Format("{0} hits", mFiles.CandidateCount);
            FillListView();
            SelectFirstRow();
        }

        private void SelectFirstRow()
        {
            if (listView1.Items.Count > 0)
            {
                ListView.SelectedIndexCollection col = listView1.SelectedIndices;
                foreach (int item in col)
                {
                    listView1.Items[item].Selected = false;
                }

                if (listView1.Items.Count > 0)
                {
                    listView1.Items[0].Selected = true;
                }
            }
        }

        private void QuickVSOpenDialog_Shown(object sender, EventArgs e)
        {
            mInputText.Focus();
            if (mInputText.Text.Length > 0)
            {
                mInputText.SelectAll();
            }

            SelectFirstRow();
        }

        void SetSelectedItemsAndClose()
        {
            m_selected = null;
            m_allSelected.Clear();

            ListView.SelectedIndexCollection col = listView1.SelectedIndices;
            foreach (int item in col)
            {
                if (m_selected == null)
                {
                    m_selected = (SearchEntry)listView1.Items[item].Tag;
                    m_selected.lastUsed = m_stopWatch.ElapsedMilliseconds;


                    //listView1.Items[item].SubItems[0].BackColor = Color.Red;


                }

                m_allSelected.Add((SearchEntry)listView1.Items[item].Tag);
            }

            RefreshFilter(mInputText.Text);

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void QuickVSOpenDialog_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.KeyCode)
                {
                    case Keys.Escape:
                        e.Handled = true;
                        if (mInputText.Text.Length > 0)
                        {
                            mInputText.Text = "";
                        }
                        else
                        {
                            DialogResult = DialogResult.Cancel;
                            this.Close();
                        }
                        break;

                    case Keys.Enter:
                        SetSelectedItemsAndClose();
                        break;

                    case Keys.F5:

                        try
                        {
                            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
                            mFiles.Refresh();
                            if (mInputText.Text.Length > 0)
                            {
                                mInputText.Text = "";
                            }
                            else
                            {
                                RefreshFilter("");
                            }

                            listView1.Refresh();

                            e.Handled = true;
                            m_refreshStatusLabel.Text = "Last Refresh: " + mFiles.LastRefresh.ToString() + " - " + mFiles.LastRefreshDurationMS + "ms";
                            SelectFirstRow();
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

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            SetSelectedItemsAndClose();
        }

        private void listView1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsLetter(e.KeyChar) ||
                char.IsDigit(e.KeyChar))
            {
                if (mInputText.Focused == false)
                {
                    mInputText.Text += e.KeyChar;
                    mInputText.SelectionStart = mInputText.Text.Length;
                    mInputText.SelectionLength = 0;
                    mInputText.Focus();
                }
            }
        }

        private void mInputText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up ||
                e.KeyCode == Keys.Down ||
                (e.KeyCode == Keys.Home && Control.ModifierKeys.HasFlag(Keys.Control)) ||
                (e.KeyCode == Keys.End && Control.ModifierKeys.HasFlag(Keys.Control)))
            {
                var index = -1;
                if (listView1.SelectedIndices.Count > 0)
                {
                    index = listView1.SelectedIndices[listView1.SelectedIndices.Count - 1];
                }

                if (e.KeyCode == Keys.Home)
                {
                    index = 0;
                }
                else if (e.KeyCode == Keys.End)
                {
                    index = listView1.Items.Count - 1;
                }
                else if (e.KeyCode == Keys.Up)
                {
                    index--;
                }
                else
                {
                    index++;
                }

                if (listView1.Items.Count > index &&
                    index >= 0)
                {
                    ListView.SelectedIndexCollection col = listView1.SelectedIndices;
                    foreach (int item in col)
                    {
                        listView1.Items[item].Selected = false;
                    }

                    listView1.Items[index].Selected = true;
                    listView1.Items[index].Focused = true;
                    listView1.EnsureVisible(index);
                }

                e.Handled = true;
            }
        }


        private void QuickVSOpenDialog_Resize(object sender, EventArgs e)
        {
            SizeColumns();
        }

        private void SizeColumns()
        {
            if (listView1.Columns.Count > 0)
            {
                if (m_showSecondColumn)
                {
                    listView1.Columns[0].Width = listView1.Width - 100;
                    listView1.Columns[1].Width = 78;
                }
                else
                {
                    listView1.Columns[0].Width = listView1.Width - 22;
                }
            }
        }

        private void listView1_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (e.ItemIndex < mFiles.CandidateCount)
            {
                var x = mFiles.Candidate(e.ItemIndex);

                string name = string.Empty;
                if (x.lastUsed.HasValue)
                {
                    name += "* ";
                }
                name += x.filename;

                ListViewItem lvi = new ListViewItem(name);
                lvi.Tag = x;
                if (m_showSecondColumn)
                {
                    lvi.SubItems.Add(x.methodType);
                }

                if (m_showFileToolTip)
                {
                    lvi.ToolTipText = x.fullPath;
                }

                e.Item = lvi;       // assign item to event argument's item-property
            }
        }
    }
}
