using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using CodeFetcher.Icons;

namespace CodeFetcher
{
    public partial class frmMain : Form
    {
        #region Private declarations
        string pathIndex;
        IndexWriter indexWriter;
        string[] patterns;
        SystemImageList imageListDocuments;
        bool portablePaths = true;

        IndexSearcher searcher = null;
        string[] searchDirs;
        string[] searchExclude;
        string appPath;
        string appDir;
        string appName;
        string searchTermsPath;
        Dictionary<string, long> dateStamps;
        Dictionary<string, long> newDateStamps;
        BackgroundWorker indexWorker;
        BackgroundWorker searchWorker;
        AutoCompleteStringCollection searchTerms = new AutoCompleteStringCollection();

        int indexCounter = 0;
        int indexMaxFileSize = 20;
        int zipMaxSize = 5;
        int resultsMax = 200;
        int fileCount;
        string status = "";

        // statistics
        long bytesTotal = 0;
        int countTotal = 0;
        int countSkipped = 0;
        int countNew = 0;
        int countChanged = 0;
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

        private void Form1_Load(object sender, System.EventArgs e)
        {
            appPath = typeof(frmMain).Assembly.Location;
            appDir = Path.GetDirectoryName( appPath );
            appName = Path.GetFileNameWithoutExtension(appPath);

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
                    foreach(string dir in tempSearchDir.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
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

                pathIndex = ini.IniReadValue("Location", "Search Index");
                if (string.IsNullOrEmpty(pathIndex) == false)
                    pathIndex = Path.Combine(appDir, pathIndex);
                string maxSize = ini.IniReadValue("Index", "Max Size");
                if (!string.IsNullOrEmpty(maxSize))
                    indexMaxFileSize = int.Parse(maxSize);
                maxSize = ini.IniReadValue("Index", "Zip Max Size");
                if (!string.IsNullOrEmpty(maxSize))
                    zipMaxSize = int.Parse(maxSize);

                try
                {
                    string max = ini.IniReadValue("Results", "Max Result");
                    if (!string.IsNullOrEmpty(max))
                        resultsMax = int.Parse(max);
                }
                catch
                {
                    MessageBox.Show("The Max Result setting has an invalid value", "Max Result", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

                try
                {
                    string portable = ini.IniReadValue("Options", "Portable Paths");
                    if (!string.IsNullOrEmpty(portable))
                        portablePaths = bool.Parse(portable);
                }
                catch
                {
                    MessageBox.Show("The Portable Paths setting has an invalid value, should be true or false", "Portable Paths", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }

            // Set defaults
            if( patterns == null)
                patterns = new string[] { "*.*" };
            if( searchDirs == null )
                searchDirs =  new string[] { appDir };
            if (searchExclude == null)
                searchExclude = new string[] { "c:\\$Recycle.Bin" };
            if (string.IsNullOrEmpty(pathIndex))
                pathIndex = Path.Combine(appDir, "SearchIndex");
            searchTermsPath = Path.Combine(pathIndex, "searchhistory");

            dateTimePickerFrom.MaxDate = DateTime.Today;
            dateTimePickerFrom.Format = DateTimePickerFormat.Short;
            dateTimePickerFrom.Value = dateTimePickerFrom.MinDate;
            dateTimePickerTo.Format = DateTimePickerFormat.Short;
            dateTimePickerTo.Value = DateTime.Today.AddDays(1);

            Timer t = new Timer();
            t.Interval = 1000;
            t.Tick += delegate(object sender1, EventArgs e1)
            {
                if (indexCounter > 10)
                    indexCounter = 0;

                if (indexWorker != null && indexWorker.IsBusy)
                    labelStatus.Text = status + "".PadRight(indexCounter++, '.');
            };
            t.Start();
            Index();

            searchTerms = LoadSearchTerms();
            textBoxQuery.AutoCompleteCustomSource = searchTerms;
        }

        void Index()
        {
            fileCount = 0;
            indexWorker = new BackgroundWorker();
            indexWorker.WorkerReportsProgress = true;
            indexWorker.WorkerSupportsCancellation = true;

            indexWorker.DoWork += delegate(object sender, DoWorkEventArgs e)
            {
                dateStamps = new Dictionary<string, long>();
                newDateStamps = new Dictionary<string, long>();

                // First load all of the datestamps to check if the file is modified
                if (checkIndex())
                {
                    var directory = new MMapDirectory(new DirectoryInfo(pathIndex));
                    IndexReader indexReader = IndexReader.Open(directory, true);

                    // Check to see if we are in relative or absolute path mode
                    for (int i = 0; i < indexReader.NumDocs(); i++)
                    {
                        if (indexReader.IsDeleted(i) == false)
                        {
                            Document doc = indexReader.Document(i);
                            string path = doc.Get("path");
                            long ticks = long.Parse(doc.Get("ticks"));
                            if (dateStamps.ContainsKey(path))
                            {
                                dateStamps[path] = Math.Max(dateStamps[path], ticks);
                            }
                            else
                                dateStamps.Add(path, ticks);
                        }
                    }
                    indexReader.Dispose();
                }

                // Try to open the Index for writing
                int attempts = 0;
                while (attempts < 5)
                {
                    var directory = new MMapDirectory(new DirectoryInfo(pathIndex));
                    var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
                    if (checkIndex())
                    {

                        try
                        {
                            indexWriter = new IndexWriter(directory, analyzer, false, IndexWriter.MaxFieldLength.UNLIMITED);
                            attempts = 5;
                        }
                        catch (LockObtainFailedException)
                        {
                            attempts++;
                            if (System.IO.Directory.Exists(pathIndex))
                                System.IO.Directory.Delete(pathIndex, true);
                        }
                    }
                    else
                    {
                        indexWriter = new IndexWriter(directory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
                        attempts = 5;
                    }
                }

                // Hide the file
                File.SetAttributes(pathIndex, FileAttributes.Hidden);

                bytesTotal = 0;
                countTotal = 0;
                countSkipped = 0;
                countNew = 0;
                countChanged = 0;
                bool cancel = false;
                DateTime start = DateTime.Now;

                foreach (string searchDir in searchDirs)
                {
                    if (System.IO.Directory.Exists(searchDir))
                    {
                        DirectoryInfo di = new DirectoryInfo(searchDir);
                        cancel = addFolder(searchDir, di);
                        if (cancel)
                            break;
                    }
                }

                if (cancel)
                {
                    string summary = String.Format("Cancelled. Indexed {0} files ({1} bytes). Skipped {2} files.", countTotal, bytesTotal, countSkipped);
                    summary += String.Format(" Took {0}", (DateTime.Now - start));
                    indexWorker.ReportProgress(0, summary);
                    e.Cancel = true;
                }
                else
                {
                    int deleted = 0;

                    // Loop through all the files and delete if it doesn't exist
                    foreach (string file in dateStamps.Keys)
                    {
                        if (newDateStamps.ContainsKey(file) == false)
                        {
                            deleted++;
                            indexWriter.DeleteDocuments(new Term("path", file));
                        }
                    }

                    string summary = String.Format("{0} files ({1} mb). New {2}. Changed {3}, Skipped {4}. Removed {5}. {6}", countTotal, (bytesTotal / 1000000).ToString("N0"), countNew, countChanged, countSkipped, deleted, DateTime.Now - start);
                    indexWorker.ReportProgress(0, summary);
                }

                indexWriter.Optimize();
                indexWriter.Dispose();
            };
            indexWorker.ProgressChanged += delegate(object sender, ProgressChangedEventArgs e)
            {
                if( e.ProgressPercentage == 0 )
                    status = e.UserState.ToString();
                else
                    status = string.Format("Files indexed {0}. {1}", countTotal, e.UserState.ToString());
                this.labelStatus.Text = status;
                indexCounter = 0;
            };
            indexWorker.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs e)
            {
                //watcher.EnableRaisingEvents = true;
            };

            indexWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Indexes a folder.
        /// </summary>
        /// <param name="directory"></param>
        private bool addFolder(string searchDir, DirectoryInfo directory)
        {
            // Don't index the indexes.....
            if (directory.FullName == pathIndex)
                return false;

            // Don't index excluded files
            foreach (string exclude in searchExclude)
            {
                if (directory.FullName.ToLower().Contains(exclude))
                    return false;
            }

            int filesIndexed = 0;

            // find all matching files
            foreach (string pattern in patterns)
            {
                FileInfo[] fis = null;
                try
                {
                    fis = directory.GetFiles(pattern);
                }
                catch (Exception)
                {
                    return false;
                }

                foreach (FileInfo fi in fis)
                {
                    // skip temporary office files
                    if (fi.Name.StartsWith("~") || fi.Name.StartsWith("."))
                        continue;

                    if (indexWorker.CancellationPending)
                        return true;

                    fileCount++;

                    try
                    {
                        string path = fi.FullName;

                        string relPath = path;
                        // Remove the full path
                        if (portablePaths)
                        {
                            relPath = path.Replace(searchDir, "");
                            // Remove the starting slash
                            if (relPath.StartsWith(@"\"))
                                relPath = relPath.Substring(1);
                        }

                        newDateStamps.Add(relPath, fi.LastWriteTime.Ticks);

                        // Check to see of doc has changed
                        if (dateStamps.ContainsKey(relPath) == false)
                        {
                            addDocument(path, relPath, false);
                            filesIndexed++;
                        }
                        else if (dateStamps[relPath] < fi.LastWriteTime.Ticks)
                        {
                            // Delete the existing document
                            addDocument(path, relPath, true);
                            filesIndexed++;
                        }

                        // update statistics
                        this.countTotal++;
                        this.bytesTotal += fi.Length;

                        // show added file
                        indexWorker.ReportProgress(fileCount, Path.GetFileName(fi.FullName));
                    }
                    catch (Exception)
                    {
                        // parsing and indexing wasn't successful, skipping that file
                        this.countSkipped++;
                        indexWorker.ReportProgress(fileCount, "Skipped:" + Path.GetFileName(fi.FullName));
                    }
                }
            }

            // Only commit if things have been indexed
            if(filesIndexed > 0)
                indexWriter.Commit();

            // add subfolders
            foreach (DirectoryInfo di in directory.GetDirectories())
            {
                bool cancel = addFolder(searchDir, di);
                if (cancel)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Parses and indexes an IFilter parseable file.
        /// </summary>
        /// <param name="path"></param>
        private void addDocument(string path, string relPath, bool exists)
        {
            string filename = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path);
            FileInfo fi = new FileInfo(path);

            Document doc = new Document();
            string text = "";

            try
            {
                if (extension.ToLower() == ".zip" && fi.Length < zipMaxSize * 1000000)
                    text = Parser.Parse(path);
                else if (fi.Length < indexMaxFileSize * 1000000)
                    text = Parser.Parse(path);
            }
            catch (Exception)
            {
                // Ignore error, add with not content
            }

            doc.Add(new Field("modified", fi.LastWriteTime.ToString("yyyyMMddHHmmss"), Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("ticks", fi.LastWriteTime.Ticks.ToString(), Field.Store.YES, Field.Index.NO));
            doc.Add(new Field("type", extension.Substring(1), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("name", filename, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("path", relPath, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("content", text, Field.Store.NO, Field.Index.ANALYZED));

            if (exists)
            {
                indexWriter.UpdateDocument(new Term("path", relPath), doc);
                countChanged++;
            }
            else
            {
                indexWriter.AddDocument(doc);
                countNew++;
            }
        }

        private void buttonSearch_Click(object sender, System.EventArgs e)
        {
            Search();
        }

        private bool checkIndex()
        {
            try
            {
                var directory = new MMapDirectory(new DirectoryInfo(pathIndex));
                searcher = new IndexSearcher(directory, true);
                searcher.Dispose();
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        private void Search()
        {
            buttonSearch.Enabled = false;
            DateTime start = DateTime.Now;
            int hitCount = 0;

            searchWorker = new BackgroundWorker();
            searchWorker.WorkerReportsProgress = true;
            searchWorker.WorkerSupportsCancellation = true;

            string queryText = "";
            string queryHistory = "";
            if (tabControl1.SelectedIndex == 0)
            {
                // Parse the query, "content" is the default field to search
                if (this.textBoxQuery.Text.Trim() == String.Empty)
                    return;

                queryText ="(" + textBoxQuery.Text + ")";

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

            queryText = queryText.Trim();

            this.listViewResults.Items.Clear();

            searchWorker.DoWork += delegate(object sender, DoWorkEventArgs e)
            {
                try
                {
                    var directory = new MMapDirectory(new DirectoryInfo(pathIndex));
                    searcher = new IndexSearcher(directory, true);
                }
                catch (IOException ex)
                {
                    throw new Exception("The index doesn't exist or is damaged. Please rebuild the index.", ex);
                }

                Query query;
                try
                {
                    QueryParser parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "content", new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30));
                    query = parser.Parse(queryText);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("Invalid query: " + ex.Message, "Query", ex);
                }

                // Search
                var results = searcher.Search(query, null, 200);
                hitCount = results.ScoreDocs.Length;

                foreach (ScoreDoc scoreDoc in results.ScoreDocs)
                {
                    // get the document from index
                    Document doc = searcher.Doc(scoreDoc.Doc);

                    // create a new row with the result data
                    string filename = doc.Get("name") + "." + doc.Get("type");
                    string path = doc.Get("path");
                    DateTime modified = DateTime.ParseExact(doc.Get("modified"), "yyyyMMddHHmmss", null);
                    string folder = "";
                    try
                    {
                        folder = Path.GetDirectoryName(path);
                    }
                    catch (Exception)
                    {
                        // Couldn't get directory name...
                    }

                    ListViewItem item = new ListViewItem(new string[] { null, filename, (scoreDoc.Score * 100).ToString("N0"), modified.ToShortDateString() + " " + modified.ToShortTimeString(), folder });
                    item.Tag = path;
                    try
                    {
                        item.ImageIndex = imageListDocuments.IconIndex(filename);
                    }
                    catch (Exception)
                    {
                        // Couldn't get icon...
                    }
                    searchWorker.ReportProgress(0, item);
                }
                searcher.Dispose();
            };

            searchWorker.ProgressChanged += delegate(object sender, ProgressChangedEventArgs e)
            {
                this.listViewResults.Items.Add((ListViewItem)e.UserState);
            };
            searchWorker.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs e)
            {
                if (e.Error != null)
                {
                    this.labelSearch.Text = e.Error.Message;
                }
                else
                {
                    if (queryHistory != "" && hitCount > 0 && searchTerms.Contains(queryHistory) == false)
                        searchTerms.Add(queryHistory);
                    this.labelSearch.Text = String.Format("Search took {0}. Found {1} items.", (DateTime.Now - start), hitCount);
                }
                buttonSearch.Enabled = true;
            };

            searchWorker.RunWorkerAsync();
        }

        private void listViewResults_DoubleClick(object sender, System.EventArgs e)
        {
            if (this.listViewResults.SelectedItems.Count != 1)
                return;

            string path = (string) this.listViewResults.SelectedItems[0].Tag;
            Open.File(searchDirs, path);
        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewResults.SelectedItems.Count > 0)
            {
                foreach (ListViewItem item in listViewResults.SelectedItems)
                {
                    string path = (string)item.Tag;
                    Open.File(searchDirs, path);
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
                    Open.Directory(searchDirs, path);
                }
            }
        }

        private void textBoxQuery_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                Search();
        }

        private void buttonClean_Click(object sender, System.EventArgs e)
        {
            System.IO.Directory.Delete(this.pathIndex, true);
            checkIndex();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://lucene.apache.org/java/2_4_0/queryparsersyntax.html");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSearchTerms(searchTerms);

            if (indexWorker != null && indexWorker.IsBusy)
            {
                e.Cancel = true;
                indexWorker.CancelAsync();
                status = "Waiting for index to cancel";
                labelStatus.Text = status;

                Timer t = new Timer();
                t.Interval = 100;
                t.Tick += delegate(object sender1, EventArgs e1)
                {
                    if (!(indexWorker != null && indexWorker.IsBusy))
                    {
                        t.Stop();
                        this.Close();
                    }
                };
                t.Start();
            }
        }

        private void buttonRefreshIndex_Click(object sender, EventArgs e)
        {
            Index();
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
            catch (Exception)
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
    }
}
