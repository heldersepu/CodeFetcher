using NLog;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Analysis.Standard;
using CodeFetcher.Icons;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Util;
using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers.ComplexPhrase;

namespace CodeFetcher
{
    public class Index
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public BackgroundWorker worker;
        public IniFile iniFile;

        #region Private declarations
        BackgroundWorker searchWorker;
        IndexWriter indexWriter;
        IndexSearcher searcher = null;

        int fileCount;
        bool portablePaths = true;
        // statistics
        int countTotal = 0;
        int countSkipped = 0;
        int countNew = 0;
        int countChanged = 0;
        int indexMaxFileSize = 20;
        DateTime ProgressReport;

        Dictionary<string, long> dateStamps;
        Dictionary<string, long> newDateStamps;

        private LuceneVersion version
        {
            get
            {
                return LuceneVersion.LUCENE_48;
            }
        }

        private Analyzer analyzer
        {
            get
            {
                return new StandardAnalyzer(version, CharArraySet.EMPTY_SET);
            }
        }

        private ComplexPhraseQueryParser parser
        {
            get
            {
                return new ComplexPhraseQueryParser(version, "content", analyzer);
            }
        }
        #endregion Private declarations

        public Index(IniFile iniFile)
        {
            this.iniFile = iniFile;
        }

        public void Delete()
        {
            logger.Info("Deleting Index");
            Close(); Cancel();
            if (System.IO.Directory.Exists(iniFile.IndexPath))
                System.IO.Directory.Delete(iniFile.IndexPath, true);
            logger.Info("Index Deleted");
        }

        /// <summary>
        /// Try to open the Index for writing
        /// </summary>
        public int TryOpen(int maxAttempts)
        {
            int attempts = 0;
            while (attempts < maxAttempts)
            {
                var directory = new MMapDirectory(new DirectoryInfo(iniFile.IndexPath));
                var config = new IndexWriterConfig(version, analyzer);
                if (CheckIndex())
                {
                    try
                    {
                        indexWriter = new IndexWriter(directory, config);
                        attempts = maxAttempts + 5;
                    }
                    catch (LockObtainFailedException le)
                    {
                        attempts++;
                        logger.Error(le);
                        if (System.IO.Directory.Exists(iniFile.IndexPath))
                            System.IO.Directory.Delete(iniFile.IndexPath, true);
                    }
                }
                else
                {
                    indexWriter = new IndexWriter(directory, config);
                    attempts = maxAttempts + 5;
                }
            }
            return attempts;
        }

        public void Cancel()
        {
            while(worker != null && worker.IsBusy)
            {
                worker.CancelAsync();
                Thread.Sleep(100);
            }
            worker = null;
            while (searchWorker != null && searchWorker.IsBusy)
            {
                searchWorker.CancelAsync();
                Thread.Sleep(100);
            }
            searchWorker = null;
            if (searcher != null)
                searcher.IndexReader.Dispose();
            searcher = null;
        }

        public void Close()
        {
            if (indexWriter != null)
            {
                indexWriter.Dispose();
                IndexWriter.Unlock(indexWriter.Directory);
                indexWriter = null;
            }
        }

        public BackgroundWorker Initialize()
        {
            fileCount = 0;
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;

            worker.DoWork += delegate (object sender, DoWorkEventArgs e)
            {
                ProgressReport = DateTime.Now;
                dateStamps = new Dictionary<string, long>();
                newDateStamps = new Dictionary<string, long>();

                // First load all of the datestamps to check if the file is modified
                if (CheckIndex())
                {
                    var directory = new MMapDirectory(new DirectoryInfo(iniFile.IndexPath));
                    IndexReader indexReader = DirectoryReader.Open(directory);

                    // Check to see if we are in relative or absolute path mode
                    for (int i = 0; i < indexReader.NumDocs; i++)
                    {
                        Document doc = indexReader.Document(i);
                        if (doc.Fields.Count > 0)
                        {
                            string path = doc.Get("path");
                            long ticks = long.Parse(doc.Get("ticks"));
                            if (dateStamps.ContainsKey(path))
                                dateStamps[path] = Math.Max(dateStamps[path], ticks);
                            else
                                dateStamps.Add(path, ticks);
                        }
                    }
                    indexReader.Dispose();
                }

                if (TryOpen(5) == 5)
                    logger.Error("Unable to open the Index for writing.");

                // Hide the file
                File.SetAttributes(iniFile.IndexPath, FileAttributes.Hidden);

                countTotal = 0;
                countSkipped = 0;
                countNew = 0;
                countChanged = 0;
                bool cancel = false;
                DateTime start = DateTime.Now;

                foreach (string searchDir in iniFile.SearchDirs)
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
                    string summary = $"Cancelled. Indexed {countTotal} files. Skipped {countSkipped} files. Took {DateTime.Now - start}";
                    worker.ReportProgress(countTotal, summary);
                    e.Cancel = true;
                }
                else
                {
                    int deleted = 0;

                    // Loop through all the files and delete if it doesn't exist
                    foreach (string file in dateStamps.Keys)
                    {
                        if (!newDateStamps.ContainsKey(file))
                        {
                            deleted++;
                            indexWriter.DeleteDocuments(new Term("path", file));
                        }
                    }

                    string summary = $"{countTotal} files. New {countNew}. Changed {countChanged}, Skipped {countSkipped}. Removed {deleted}. {DateTime.Now - start}";
                    worker.ReportProgress(countTotal, summary);
                }

                Close();
            };
            return worker;
        }

        public bool CheckIndex()
        {
            try
            {
                if (!System.IO.Directory.Exists(iniFile.IndexPath)) return false;
                var directory = new MMapDirectory(new DirectoryInfo(iniFile.IndexPath));
                searcher = new IndexSearcher(DirectoryReader.Open(directory));
                return true;
            }
            catch (IOException e)
            {
                logger.Error(e);
                return false;
            }
        }

        public BackgroundWorker Search(string queryText, SystemImageList imageList)
        {
            searchWorker = new BackgroundWorker();
            searchWorker.WorkerReportsProgress = true;
            searchWorker.WorkerSupportsCancellation = true;
            queryText = queryText.Trim();
            searchWorker.DoWork += delegate (object sender, DoWorkEventArgs e)
            {
                try
                {
                    var directory = new MMapDirectory(new DirectoryInfo(iniFile.IndexPath));
                    searcher = new IndexSearcher(DirectoryReader.Open(directory));
                }
                catch (IOException ex)
                {
                    logger.Error(ex);
                    throw new Exception("The index doesn't exist or is damaged. Please rebuild the index.", ex);
                }

                Query query;
                try
                {
                    query = parser.Parse(queryText);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    throw new ArgumentException("Invalid query: " + ex.Message, "Query", ex);
                }

                // Search
                var results = searcher.Search(query, iniFile.HitsLimit);
                foreach (var scoreDoc in results.ScoreDocs)
                {
                    // get the document from index
                    var doc = searcher.Doc(scoreDoc.Doc);

                    // create a new row with the result data
                    string filename = doc.Get("name") + "." + doc.Get("type");
                    string path = doc.Get("path");
                    string folder = "";
                    try
                    {
                        folder = Path.GetDirectoryName(path);
                    }
                    catch (Exception ex)
                    {
                        // Couldn't get directory name...
                        logger.Error(ex);
                    }

                    var modified = DateTime.ParseExact(doc.Get("modified"), "yyyyMMddHHmmss", null);
                    var item = new ListViewItem( new string[] {
                        null,
                        filename,
                        (scoreDoc.Score * 100).ToString("N0"),
                        modified.ToShortDateString() + " " + modified.ToShortTimeString(),
                        folder
                    });
                    item.Tag = path;
                    try
                    {
                        item.ImageIndex = imageList.IconIndex(filename);
                    }
                    catch (Exception ex)
                    {
                        // Couldn't get icon...
                        logger.Error(ex);
                    }
                    searchWorker.ReportProgress(0, item);
                }
            };
            return searchWorker;
        }

        /// <summary>
        /// Indexes a folder.
        /// </summary>
        /// <param name="directory"></param>
        public bool addFolder(string searchDir, DirectoryInfo directory)
        {
            // Don't index the indexes.....
            if (directory.FullName.EndsWith(IniFile.SEARCH_INDEX))
                return false;

            // Don't index hidden directories.....
            if ((directory.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                return false;

            // Don't index excluded files
            string udir = directory.FullName.ToUpper();
            if (iniFile.SearchExclude.Any(x => udir.EndsWith(x)))
                return false;

            int filesIndexed = 0;
            logger.Info("   Dir = " + directory.FullName);
            // find all matching files
            foreach (string pattern in iniFile.Patterns)
            {
                FileInfo[] fis = null;
                try
                {
                    fis = directory.GetFiles(pattern);
                }
                catch (Exception e)
                {
                    logger.Error(e);
                    return false;
                }

                foreach (FileInfo fi in fis)
                {
                    // skip temporary office files
                    if (fi.Name.StartsWith("~") || fi.Name.StartsWith("."))
                        continue;

                    if (worker.CancellationPending)
                        return true;

                    string path = fi.FullName;
                    string extension = Path.GetExtension(path);
                    if (string.IsNullOrEmpty(extension) || iniFile.ExtensionExclude.Contains(extension.ToUpper()))
                    {
                        this.countSkipped++;
                    }
                    else
                    {
                        fileCount++;
                        try
                        {
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
                            if (!dateStamps.ContainsKey(relPath))
                            {
                                addDocument(extension, path, relPath, false);
                                if ((DateTime.Now - ProgressReport).TotalMilliseconds > 500)
                                {
                                    ProgressReport = DateTime.Now;
                                    worker.ReportProgress(fileCount, Path.GetFileName(fi.FullName));
                                }
                            }
                            else if (dateStamps[relPath] < fi.LastWriteTime.Ticks)
                            {
                                // Delete the existing document
                                addDocument(extension, path, relPath, true);
                            }

                            this.countTotal++;
                        }
                        catch (Exception e)
                        {
                            // parsing and indexing wasn't successful, skipping that file
                            logger.Error(e);
                            this.countSkipped++;
                            worker.ReportProgress(fileCount, "Skipped:" + Path.GetFileName(fi.FullName));
                        }
                    }
                }
            }

            // Only commit if things have been indexed
            if (filesIndexed > 0)
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
        public void addDocument(string extension, string path, string relPath, bool exists)
        {
            logger.Trace(" File = " + path);
            string filename = Path.GetFileNameWithoutExtension(path);
            FileInfo fi = new FileInfo(path);
            string text = "";
            try
            {
                if (fi.Length < indexMaxFileSize * 1000000)
                    text = File.ReadAllText(path);
            }
            catch (Exception e)
            {
                // Ignore error, add with no content
                logger.Error(e);
            }
            addContent(fi.LastWriteTime, extension.Substring(1), filename, relPath, text, exists);
        }

        /// <summary>
        /// Adds content to the indexes.
        /// </summary>
        public void addContent(DateTime LastWriteTime, string type, string name, string path, string content, bool exists)
        {
            if (!string.IsNullOrEmpty(content))
                foreach (var item in iniFile.Splitters)
                    content = content.Replace(item, " ");
            string date = LastWriteTime.ToString("yyyyMMddHHmmss");
            string ticks = LastWriteTime.Ticks.ToString();
            Document doc = new Document();
            doc.Add(new StringField("modified", date, Field.Store.YES));
            doc.Add(new StringField("ticks", ticks, Field.Store.YES));
            doc.Add(new StringField("type", type, Field.Store.YES));
            doc.Add(new StringField("name", name, Field.Store.YES));
            doc.Add(new StringField("path", path, Field.Store.YES));
            doc.Add(new TextField("content", content, Field.Store.NO));

            if (exists)
            {
                indexWriter.UpdateDocument(new Term("path", path), doc);
                countChanged++;
            }
            else
            {
                indexWriter.AddDocument(doc);
                countNew++;
            }
        }
    }
}
