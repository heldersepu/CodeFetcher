using NLog;
using System;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;
using CodeFetcher.Icons;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Folding;

namespace CodeFetcher
{
    public partial class frmMain : Form
    {
        #region Private declarations
        private static Logger logger = LogManager.GetCurrentClassLogger();
        Index index;
        SystemImageList imageListDocuments;
        FoldingManager foldingManager = null;
        BraceFoldingStrategy foldingStrategy = new BraceFoldingStrategy();
        AutoCompleteStringCollection searchTerms = new AutoCompleteStringCollection();
        int timeMouseDown;
        string labelStatus_Text;
        const int MAX_COMBO_ITEMS = 20;
        private string appPath { get { return typeof(frmMain).Assembly.Location; } }
        private string appDir { get { return Path.GetDirectoryName(appPath); } }
        private string appName { get { return Path.GetFileNameWithoutExtension(appPath); } }
        private Column lastColClicked = new Column(0);
        #endregion Private declarations

        private class MessageFilter : IMessageFilter
        {
            public frmMain form { get; set; }
            public bool PreFilterMessage(ref Message msg)
            {
                if (msg.Msg == 0x100) //WM_KEYDOWN
                {
                    if ((Keys)msg.WParam == Keys.F3)
                    {
                        form.findNext();
                        return true;
                    }
                }
                return false;
            }
        }

        public frmMain()
        {
            InitializeComponent();
            imageListDocuments = new SystemImageList(SystemImageListSize.SmallIcons);
            SystemImageListHelper.SetListViewImageList(listViewResults, imageListDocuments, false);
            Application.AddMessageFilter(new MessageFilter { form = this });
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.DoEvents();
            Application.Run(new frmMain());
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeIndex();
            searchTerms = LoadSearchTerms();
            textBoxQuery.AutoCompleteCustomSource = searchTerms;

            dateTimePickerFrom.MaxDate = DateTime.Today;
            dateTimePickerFrom.Format = DateTimePickerFormat.Short;
            dateTimePickerFrom.Value = dateTimePickerFrom.MinDate;
            dateTimePickerTo.Format = DateTimePickerFormat.Short;
            dateTimePickerTo.Value = DateTime.Today.AddDays(1);

            sourceCodeEditor.ShowLineNumbers = true;
            sourceCodeEditor.Options.HighlightCurrentLine = true;
            sourceCodeEditor.FontFamily = new System.Windows.Media.FontFamily("Consolas");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSearchTerms(searchTerms);

            if (index.worker != null && index.worker.IsBusy)
            {
                e.Cancel = true;
                index.worker.CancelAsync();
                labelStatus.Text = "Waiting for index to cancel";

                Timer t = new Timer();
                t.Interval = 100;
                t.Tick += delegate (object sender1, EventArgs e1)
                {
                    if (!(index.worker != null && index.worker.IsBusy))
                    {
                        t.Stop();
                        this.Close();
                    }
                };
                t.Start();
            }
        }

        private void InitializeIndex()
        {
            labelStatus.Text = "Checking files for updates...";
            string iniPath = Path.Combine(appDir, appName + ".ini");
            var ini = new IniFile(iniPath, appDir);

            index = new Index(ini);
            var worker = index.Initialize();
            worker.ProgressChanged += delegate (object s, ProgressChangedEventArgs pe)
            {
                string status = pe.UserState.ToString();
                if (pe.ProgressPercentage != 0)
                    status = string.Format("Files indexed {0}. {1}", pe.ProgressPercentage, status);
                labelStatus.Text = status;
            };
            worker.RunWorkerAsync();
        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewResults.SelectedItems.Count > 0)
            {
                foreach (ListViewItem item in listViewResults.SelectedItems)
                    Open.File(index.iniFile.SearchDirs, (string)item.Tag);
            }
        }

        private void openContainingFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewResults.SelectedItems.Count > 0)
            {
                foreach (ListViewItem item in listViewResults.SelectedItems)
                {
                    string path = (string)item.Tag;
                    path = Path.GetDirectoryName(path);
                    Open.Directory(index.iniFile.SearchDirs, path);
                }
            }
        }

        private void TextBoxAdd(string line)
        {
            if (textBoxQuery.Items.Count > MAX_COMBO_ITEMS)
                textBoxQuery.Items.RemoveAt(0);
            textBoxQuery.Items.Add(line);
        }

        private AutoCompleteStringCollection LoadSearchTerms()
        {
            var result = new AutoCompleteStringCollection();
            if (File.Exists(index.iniFile.SearchTermsPath))
            {
                try
                {
                    using (var fileReader = new StreamReader(index.iniFile.SearchTermsPath))
                    {
                        string line;
                        while ((line = fileReader.ReadLine()) != null)
                        {
                            if (line.Trim() != "" && !result.Contains(line))
                            {
                                result.Add(line);
                                TextBoxAdd(line);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("Unable to load search history", "History", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    logger.Error(e);
                }
            }
            return result;
        }

        private void SaveSearchTerms(AutoCompleteStringCollection items)
        {
            try
            {
                using (var writer = new StreamWriter(index.iniFile.SearchTermsPath))
                {
                    foreach (string item in items)
                    {
                        writer.WriteLine(item);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Unable to save search history", "History", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                logger.Error(e);
            }
        }

        private void buttonToday_Click(object sender, EventArgs e)
        {
            dateTimePickerFrom.Value = DateTime.Today;
        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                tabControl1.Height -= 30;
                splitContainer.Height += 30;
                splitContainer.Location = new System.Drawing.Point(12, 95);
            }
            else
            {
                tabControl1.Height += 30;
                splitContainer.Height -= 30;
                splitContainer.Location = new System.Drawing.Point(12, 125);
            }
        }

        private void splitContainer_DoubleClick(object sender, EventArgs e)
        {
            if (splitContainer.Orientation == Orientation.Horizontal)
            {
                splitContainer.Orientation = Orientation.Vertical;
                splitContainer.SplitterDistance = splitContainer.Width / 2;
            }
            else
            {
                splitContainer.Orientation = Orientation.Horizontal;
                splitContainer.SplitterDistance = splitContainer.Height / 2;
            }
        }

        private void addFileToEditor(string fileName)
        {
            if (!File.Exists(fileName)) return;
            sourceCodeEditor.IsReadOnly = true;
            sourceCodeEditor.Load(fileName);
            sourceCodeEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(Path.GetExtension(fileName));
            if (foldingManager == null)
                foldingManager = FoldingManager.Install(sourceCodeEditor.TextArea);
            foldingStrategy.UpdateFoldings(foldingManager, sourceCodeEditor.Document);
            findNext();
        }

        private void findNext()
        {
            try
            {
                if (sourceCodeEditor.LineCount > 1)
                {
                    int x = sourceCodeEditor.Text.IndexOf(QueryText, sourceCodeEditor.SelectionStart + 1);
                    if (x <= 0 && sourceCodeEditor.SelectionStart > 1)
                        x = sourceCodeEditor.Text.IndexOf(QueryText, 1);
                    if (x > 0)
                    {
                        sourceCodeEditor.SelectionStart = x;
                        sourceCodeEditor.SelectionLength = QueryText.Length;
                        sourceCodeEditor.TextArea.Caret.BringCaretToView();
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }

        #region search Events
        private void buttonSearch_Click(object sender, EventArgs e)
        {
            Search();
        }

        private void textBoxQuery_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                Search();
        }

        private void SearchCompleted(string query, DateTime start, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                logger.Error(e.Error);
                labelStatus.Text = e.Error.Message;
            }
            else
            {
                if (query.Trim() != "" && !searchTerms.Contains(query))
                {
                    searchTerms.Add(query);
                    TextBoxAdd(query);
                }
                if (listViewResults.Items.Count > 0)
                {
                    listViewResults.Select();
                    listViewResults.Items[0].Selected = true;
                    addFileToEditor(listViewResults.Selected());
                }
                else
                {
                    sourceCodeEditor.Clear();
                }

                labelStatus.Text = string.Format("Search took {0}. ", (DateTime.Now - start));
            }
            buttonSearch.Enabled = true;
        }

        private void SearchProgressChanged(ProgressChangedEventArgs e)
        {
            listViewResults.Items.Add((ListViewItem)e.UserState);
            listViewResults.Refresh();
        }

        private string QueryText
        {
            get
            {
                string text = "";
                if (tabControl1.SelectedIndex == 0)
                    text = textBoxQuery.Text;
                else
                    text = textBoxContent.Text;
                return text.Trim();
            }
        }

        private string GetQueryText()
        {
            string queryText = "";
            if (tabControl1.SelectedIndex == 0)
            {
                if (!string.IsNullOrEmpty(textBoxQuery.Text))
                {
                    queryText = "(" + textBoxQuery.Text + ")";

                    // Also search the path if the query isn't qualified
                    if (!queryText.Contains(":"))
                        queryText += " OR name:" + queryText;
                }
            }
            else
            {
                queryText = "";
                if (textBoxContent.Text.Trim() != "")
                    queryText = "content:" + textBoxContent.Text + " AND";

                if (textBoxName.Text.Trim() != "")
                    queryText += " name:" + textBoxName.Text + " AND";

                if (textBoxType.Text != "")
                {
                    string types = "";
                    foreach (string type in textBoxType.Text.Split(','))
                    {
                        types += " type:" + type + " OR";
                    }

                    if (types != "")
                    {
                        // Remove last OR
                        types = types.Substring(0, types.Length - 2);
                        queryText += " (" + types + ") AND";
                    }
                }
                queryText += " modified:[" + dateTimePickerFrom.Selected() + " TO " + dateTimePickerTo.Selected() + "]";
            }
            return queryText;
        }

        private void Search()
        {
            buttonSearch.Enabled = false;
            listViewResults.Items.Clear();
            DateTime start = DateTime.Now;
            string queryText = GetQueryText();
            if (!string.IsNullOrEmpty(queryText))
            {
                string query = (tabControl1.SelectedIndex == 0) ? textBoxQuery.Text : "";
                var search = index.Search(queryText, imageListDocuments);
                search.ProgressChanged += delegate (object sender, ProgressChangedEventArgs e) { SearchProgressChanged(e); };
                search.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs e) { SearchCompleted(query, start, e); };
                search.RunWorkerAsync();
            }
        }

        private void SearchComplete(object sender, ProgressChangedEventArgs e)
        {
            this.listViewResults.Items.Add((ListViewItem)e.UserState);
        }
        #endregion search Events

        #region pictureBox Events
        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                labelStatus_Text = labelStatus.Text;
                RightClickHoldTimer.Start();
                timeMouseDown = 50;
            }
        }

        private void pictureBox2_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                RightClickHoldTimer.Stop();
                labelStatus.Text = labelStatus_Text;
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            if (((MouseEventArgs)e).Button == MouseButtons.Left)
                System.Diagnostics.Process.Start("https://lucene.apache.org/core/2_9_4/queryparsersyntax.html");
        }

        private void RightClickHoldTimer_Tick(object sender, EventArgs e)
        {
            if (timeMouseDown > 0)
            {
                timeMouseDown--;
                labelStatus.Text = "  \t   "  + timeMouseDown.ToString() +
                    "  \t   " + new String('=', timeMouseDown) +
                    ((timeMouseDown % 2 == 0)? "<": ">");
            }
            else
            {
                if (pictureBox2.PointToClient(Cursor.Position).X >= 0)
                {
                    index.iniFile.Save();
                    labelStatus.Text = "IniFile Created...";
                }
                else
                {
                    labelStatus.Text = "Cleaning Index...";
                    index.Delete();
                    InitializeIndex();
                }
                RightClickHoldTimer.Stop();
            }
        }
        #endregion pictureBox Events

        #region listViewResults Events
        private void listViewResults_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            var columnDataType = ListViewItemComparer.ColumnDataType.Generic;
            if (e.Column == colHeaderModified.Index)
            {
                columnDataType = ListViewItemComparer.ColumnDataType.DateTime;
            }
            else if (e.Column == columnHeaderScore.Index)
            {
                columnDataType = ListViewItemComparer.ColumnDataType.Number;
            }
            else
            {
                columnDataType = ListViewItemComparer.ColumnDataType.Generic;
            }
            if (lastColClicked.Id == e.Column)
                lastColClicked.ChangeSort();
            else
                lastColClicked = new Column(e.Column);
            listViewResults.ListViewItemSorter = new ListViewItemComparer(
                listViewResults.Columns.Count, e.Column, lastColClicked.Sort, columnDataType);
        }

        private void listViewResults_DoubleClick(object sender, EventArgs e)
        {
            if (listViewResults.SelectedItems.Count == 1)
                Open.File(index.iniFile.SearchDirs, listViewResults.Selected());
        }

        private void listViewResults_Click(object sender, EventArgs e)
        {
            if (listViewResults.SelectedItems.Count == 1)
                addFileToEditor(listViewResults.Selected());
        }

        private void listViewResults_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            listViewResults_Click(null, null);
        }
        #endregion listViewResults Events

        #region buttonRefreshIndex Events
        private void buttonRefreshIndex_Click(object sender, EventArgs e)
        {
            InitializeIndex();
        }

        private void buttonRefreshIndex_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                labelStatus_Text = labelStatus.Text;
                RightClickHoldTimer.Start();
                timeMouseDown = 50;
            }
        }

        private void buttonRefreshIndex_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                RightClickHoldTimer.Stop();
                labelStatus.Text = labelStatus_Text;
            }
        }
        #endregion buttonRefreshIndex Events
    }
}
