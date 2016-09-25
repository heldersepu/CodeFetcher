using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using CodeFetcher.Icons;

namespace CodeFetcher
{
    public partial class frmMain : Form
    {
        #region Private declarations
        Index index;
        SystemImageList imageListDocuments;
        AutoCompleteStringCollection searchTerms = new AutoCompleteStringCollection();
        private string appPath { get { return typeof(frmMain).Assembly.Location; } }
        private string appDir { get { return Path.GetDirectoryName(appPath); } }
        private string appName { get { return Path.GetFileNameWithoutExtension(appPath); } }
        private string pathIndex { get { return Path.Combine(appDir, ".SearchIndex"); } }
        private string searchTermsPath { get { return Path.Combine(pathIndex, "searchHistory.log"); } }
        #endregion Private declarations

        public frmMain()
        {
            InitializeComponent();

            imageListDocuments = new SystemImageList(SystemImageListSize.SmallIcons);
            SystemImageListHelper.SetListViewImageList(listViewResults, imageListDocuments, false);
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
        }

        private void buttonRefreshIndex_Click(object sender, EventArgs e)
        {
            InitializeIndex();
        }

        private void InitializeIndex()
        {
            string[] patterns = new string[] { "*.*" };
            string[] searchDirs = new string[] { appDir }; ;
            string[] searchExclude = new string[] { "C:\\$RECYCLE.BIN", "\\BIN", "\\OBJ", "\\.SVN", "\\.GIT" };

            string iniPath = Path.Combine(appDir, appName + ".ini");
            if (File.Exists(iniPath))
            {
                IniFile ini = new IniFile(iniPath);

                string tempPatterns = ini.IniReadValue("Location", "Search Patterns");
                if (string.IsNullOrEmpty(tempPatterns) == false)
                {
                    patterns = tempPatterns.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                }

                string tempSearchDir = ini.IniReadValue("Location", "Search Directory");
                if (string.IsNullOrEmpty(tempSearchDir) == false)
                {
                    List<string> dirs = new List<string>();
                    foreach (string dir in tempSearchDir.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        dirs.Add(Path.Combine(appDir, dir));
                    }
                    searchDirs = dirs.ToArray();
                }

                string tempSearchExclude = ini.IniReadValue("Location", "Paths To Skip");
                if (string.IsNullOrEmpty(tempSearchExclude) == false)
                {
                    List<string> excludes = new List<string>();
                    foreach (string exclude in tempSearchExclude.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        excludes.Add(exclude.ToLower());
                    }
                    searchExclude = excludes.ToArray();
                }
            }

            index = new Index(searchExclude, patterns, searchDirs, pathIndex);
            var worker = index.Initialize();
            worker.ProgressChanged += delegate (object s, ProgressChangedEventArgs pe)
            {
                string status;
                if (pe.ProgressPercentage == 0)
                    status = pe.UserState.ToString();
                else
                    status = string.Format("Files indexed {0}. {1}", pe.ProgressPercentage, pe.UserState.ToString());
                labelStatus.Text = status;
            };
            worker.RunWorkerAsync();
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            Search();
        }

        private void textBoxQuery_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                Search();
        }

        private void Search()
        {
            buttonSearch.Enabled = false;
            listViewResults.Items.Clear();
            DateTime start = DateTime.Now;
            string queryText = "";
            string queryHistory = "";
            if (tabControl1.SelectedIndex == 0)
            {
                // Parse the query, "content" is the default field to search
                if (this.textBoxQuery.Text.Trim() == String.Empty)
                    return;

                queryText = "(" + textBoxQuery.Text + ")";

                // Also search the path if the query isn't qualified
                if (queryText.Contains(":") == false)
                    queryText += " OR name:" + queryText;

                queryHistory = textBoxQuery.Text;
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

                queryText += " modified:[" + dateTimePickerFrom.Value.ToString("yyyyMMdd") + " TO " + dateTimePickerTo.Value.ToString("yyyyMMdd") + "]";
            }
            var search = index.Search(queryText, imageListDocuments);
            search.ProgressChanged += delegate (object sender, ProgressChangedEventArgs e)
            {
                listViewResults.Items.Add((ListViewItem)e.UserState);
            };
            search.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs e)
            {
                if (e.Error != null)
                {
                    labelStatus.Text = e.Error.Message;
                }
                else
                {
                    if (queryHistory != "" && searchTerms.Contains(queryHistory) == false)
                        searchTerms.Add(queryHistory);
                    labelStatus.Text = string.Format("Search took {0}. ", (DateTime.Now - start));
                }
                buttonSearch.Enabled = true;
            };
            search.RunWorkerAsync();

        }

        private void SearchComplete(object sender, ProgressChangedEventArgs e)
        {
            this.listViewResults.Items.Add((ListViewItem)e.UserState);
        }

        private void listViewResults_DoubleClick(object sender, EventArgs e)
        {
            if (this.listViewResults.SelectedItems.Count != 1)
                return;

            string path = (string) this.listViewResults.SelectedItems[0].Tag;
            Open.File(index.searchDirs, path);
        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewResults.SelectedItems.Count > 0)
            {
                foreach (ListViewItem item in listViewResults.SelectedItems)
                {
                    string path = (string)item.Tag;
                    Open.File(index.searchDirs, path);
                }
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
                    Open.Directory(index.searchDirs, path);
                }
            }
        }

        private void buttonClean_Click(object sender, EventArgs e)
        {
            index.Clean();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://lucene.apache.org/core/2_9_4/queryparsersyntax.html");
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
                t.Tick += delegate(object sender1, EventArgs e1)
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

        private AutoCompleteStringCollection LoadSearchTerms()
        {
            var result = new AutoCompleteStringCollection();
            if (File.Exists(searchTermsPath))
            {
                try
                {
                    using (var fileReader = new StreamReader(searchTermsPath))
                    {
                        string line;
                        while ((line = fileReader.ReadLine()) != null)
                        {
                            result.Add(line);
                        }
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Unable to load search history", "History", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            return result;
        }

        private void SaveSearchTerms(AutoCompleteStringCollection items)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(searchTermsPath))
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
            }
        }

        private void listViewResults_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListViewItemComparer.ColumnDataType columnDataType = ListViewItemComparer.ColumnDataType.Generic;
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

            if (listViewResults.ListViewItemSorter == null)
            {
                listViewResults.ListViewItemSorter = new ListViewItemComparer(
                    listViewResults.Columns.Count, e.Column, ListSortDirection.Ascending, columnDataType);
                // when you set ListViewItemSorter, sorting happens automatically
            }
            else
            {
                ((ListViewItemComparer)listViewResults.ListViewItemSorter).SetColumnAndType(e.Column, columnDataType);
                listViewResults.Sort(); // must explicitly sort in this case
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
    }
}
