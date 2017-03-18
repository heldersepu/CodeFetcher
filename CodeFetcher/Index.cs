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
using Lucene.Net.Analysis.Util;
using Lucene.Net.Util;
using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers.ComplexPhrase;
using CodeFetcher.Icons;

namespace CodeFetcher
{
    public class Index
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public BgWorker worker = null;
        public IniFile iniFile;

        #region Private declarations
        BgWorker searchWorker;
        IndexWriter indexWriter;
        IndexReader indexReader;
        IndexSearcher searcher = null;

        int fileCount;
        // statistics
        int countTotal = 0;
        int countSkipped = 0;
        int countNew = 0;
        int countChanged = 0;
        int indexMaxFileSize = 20;
        DateTime ProgressReport;

        Dictionary<string, long> dateStamps;
        Dictionary<string, long> newDateStamps;

        private LuceneVersion Version
        {
            get
            {
                return LuceneVersion.LUCENE_48;
            }
        }

        private Analyzer MyAnalyzer
        {
            get
            {
                return new StandardAnalyzer(Version, CharArraySet.EMPTY_SET);
            }
        }

        private ComplexPhraseQueryParser Parser
        {
            get
            {
                return new ComplexPhraseQueryParser(Version, "content", MyAnalyzer);
            }
        }

        private bool IndexExists
        {
            get
            {
                bool retVal = false;
                try
                {
                    if (System.IO.Directory.Exists(iniFile.IndexPath)) {
                        var directory = new MMapDirectory(new DirectoryInfo(iniFile.IndexPath));
                        searcher = new IndexSearcher(DirectoryReader.Open(directory));
                        retVal = true;
                    }
                }
                catch (IOException e)
                {
                    logger.Error(e);
                }
                return retVal;
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
                var config = new IndexWriterConfig(Version, MyAnalyzer);
                if (IndexExists)
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

        private ISet<string> fieldsToLoad = new HashSet<string> { "path", "ticks" };
        private void LoadDateStamps(int docID)
        {
            Document doc = indexReader.Document(docID, fieldsToLoad);
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

        private void StartIndexCheck(DateTime start, DoWorkEventArgs e)
        {
            logger.Info("Initialize:TryOpen");
            if (TryOpen(5) == 5)
                logger.Error("Unable to open the Index for writing.");

            // Hide the file
            File.SetAttributes(iniFile.IndexPath, FileAttributes.Hidden);

            countTotal = 0;
            countSkipped = 0;
            countNew = 0;
            countChanged = 0;
            bool cancel = false;

            logger.Info("Initialize:SearchDirs");
            foreach (string searchDir in iniFile.SearchDirs)
            {
                if (System.IO.Directory.Exists(searchDir))
                {
                    DirectoryInfo di = new DirectoryInfo(searchDir);
                    cancel = AddFolder(searchDir, di);
                    if (cancel) break;
                }
            }

            if (cancel)
            {
                string summary = $"Cancelled. \nIndexed {countTotal} files. Skipped {countSkipped} files. Took {DateTime.Now - start}";
                worker?.ReportProgress(countTotal, summary);
                if (e != null) e.Cancel = true;
            }
            else
            {
                logger.Info("Initialize:DateStamps");
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

                string summary = $" {DateTime.Now - start} \nNew {countNew}. Changed {countChanged}, Skipped {countSkipped}. Removed {deleted}.";
                worker?.ReportProgress(countTotal, summary);
            }
        }

        public void IndexRefresh(bool forceCheckIndex, DoWorkEventArgs e = null)
        {
            fileCount = 0;
            var start = DateTime.Now;
            ProgressReport = DateTime.Now;
            dateStamps = new Dictionary<string, long>();
            newDateStamps = new Dictionary<string, long>();

            // First load all of the datestamps to check if the file is modified
            if (IndexExists)
            {
                if (forceCheckIndex)
                {
                    logger.Info("Initialize:CheckIndex");
                    var directory = new MMapDirectory(new DirectoryInfo(iniFile.IndexPath));
                    indexReader = DirectoryReader.Open(directory);
                    for (int i = 0; i < indexReader.NumDocs; i++) LoadDateStamps(i);
                    indexReader.Dispose();
                }
            }
            else
            {
                forceCheckIndex = true;
            }

            if (forceCheckIndex)
            {
                StartIndexCheck(start, e);
            }
            else
            {
                worker?.ReportProgress(0, "");
            }
            logger.Info("Initialize:Close");
            Close();
        }

        public BgWorker Initialize(bool forceCheckIndex = true)
        {
            logger.Info("Initialize");
            worker = new BgWorker();
            worker.DoWork += delegate (object sender, DoWorkEventArgs e)
            {
                IndexRefresh(forceCheckIndex, e);
            };
            return worker;
        }

        public BgWorker Search(string queryText, SystemImageList imageList)
        {
            searchWorker = new BgWorker();
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
                    query = Parser.Parse(queryText);
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
                    var item = new ListViewItem(new string[] {
                        null, filename, scoreDoc.Score.ToPercent(), modified.ToShort(), folder
                    }){ Tag = path };
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
        /// <param name="searchDir"></param>
        /// <param name="directory"></param>
        /// <returns>True if there was a cancelation</returns>
        public bool AddFolder(string searchDir, DirectoryInfo directory)
        {

            if ((directory.FullName.EndsWith(IniFile.SEARCH_INDEX)) || // Don't index the indexes.....
               ((directory.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden) || // Don't index hidden directories.....
               (iniFile.SearchExclude.Any(x => directory.FullName.ToUpper().EndsWith(x)))) // Don't index excluded files
                return false;

            int filesIndexed = 0;
            logger.Trace("   Dir = " + directory.FullName);
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

                    if (worker!=null && worker.CancellationPending)
                        return true;

                    string fullPath = fi.FullName;
                    string extension = Path.GetExtension(fullPath);
                    if (string.IsNullOrEmpty(extension) || iniFile.ExtensionExclude.Contains(extension.ToUpper()))
                    {
                        countSkipped++;
                    }
                    else
                    {
                        fileCount++;
                        try
                        {
                            string relPath = fullPath.Replace(searchDir, "").Trim().Trim('\\');
                            newDateStamps.Add(relPath, fi.LastWriteTime.Ticks);

                            // Check to see of doc has changed
                            if (!dateStamps.ContainsKey(relPath))
                            {
                                AddDocument(extension, fullPath, relPath, false);
                                if ((DateTime.Now - ProgressReport).TotalMilliseconds > 400)
                                {
                                    ProgressReport = DateTime.Now;
                                    worker?.ReportProgress(fileCount, Path.GetFileName(fi.FullName));
                                }
                            }
                            else if (dateStamps[relPath] < fi.LastWriteTime.Ticks)
                            {
                                // Delete the existing document
                                AddDocument(extension, fullPath, relPath, true);
                            }
                            countTotal++;
                        }
                        catch (Exception e)
                        {
                            // parsing and indexing wasn't successful, skipping that file
                            logger.Error(e);
                            countSkipped++;
                            worker?.ReportProgress(fileCount, "Skipped:" + Path.GetFileName(fi.FullName));
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
                bool cancel = AddFolder(searchDir, di);
                if (cancel)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Parses and indexes a file.
        /// </summary>
        /// <param name="extension"></param>
        /// <param name="fullPath"></param>
        /// <param name="relPath"></param>
        /// <param name="exists"></param>
        public void AddDocument(string extension, string fullPath, string relPath, bool exists)
        {
            logger.Trace(" File = " + fullPath);
            string filename = Path.GetFileNameWithoutExtension(fullPath);
            FileInfo fi = new FileInfo(fullPath);
            string text = "";
            try
            {
                if (fi.Length < indexMaxFileSize * 1000000)
                    text = File.ReadAllText(fullPath);
            }
            catch (Exception e)
            {
                // Ignore error, add with no content
                logger.Error(e);
            }
            AddContent(
                LastWriteTime: fi.LastWriteTime,
                type: extension.Substring(1),
                name: filename,
                path: relPath,
                content: text,
                exists: exists);
        }

        /// <summary>
        /// Adds content to the indexes.
        /// </summary>
        public void AddContent(DateTime LastWriteTime, string type, string name, string path, string content, bool exists)
        {
            if (!string.IsNullOrEmpty(content))
                foreach (var item in iniFile.Splitters)
                    content = content.Replace(item, " ");
            string date = LastWriteTime.ToString("yyyyMMddHHmmss");
            string ticks = LastWriteTime.Ticks.ToString();
            Document doc = new Document {
                new StringField("modified", date, Field.Store.YES),
                new StringField("ticks", ticks, Field.Store.YES),
                new TextField("type", type, Field.Store.YES),
                new TextField("name", name, Field.Store.YES),
                new TextField("path", path, Field.Store.YES),
                new TextField("content", content, Field.Store.NO)
            };

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
