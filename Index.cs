using NLog;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using CodeFetcher.Icons;

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
        int zipMaxSize = 1;
        int indexMaxFileSize = 20;

        Dictionary<string, long> dateStamps;
        Dictionary<string, long> newDateStamps;
        #endregion Private declarations

        public Index(IniFile iniFile)
        {
            this.iniFile = iniFile;
        }

        public void Delete()
        {
            logger.Info("Index Deleted");
            if (System.IO.Directory.Exists(iniFile.IndexPath))
                System.IO.Directory.Delete(iniFile.IndexPath, true);
        }

        public BackgroundWorker Initialize()
        {
            fileCount = 0;
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;

            worker.DoWork += delegate (object sender, DoWorkEventArgs e)
            {
                dateStamps = new Dictionary<string, long>();
                newDateStamps = new Dictionary<string, long>();

                // First load all of the datestamps to check if the file is modified
                if (CheckIndex())
                {
                    var directory = new MMapDirectory(new DirectoryInfo(iniFile.IndexPath));
                    IndexReader indexReader = IndexReader.Open(directory, true);

                    // Check to see if we are in relative or absolute path mode
                    for (int i = 0; i < indexReader.NumDocs(); i++)
                    {
                        if (!indexReader.IsDeleted(i))
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
                    var directory = new MMapDirectory(new DirectoryInfo(iniFile.IndexPath));
                    var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
                    if (CheckIndex())
                    {
                        try
                        {
                            indexWriter = new IndexWriter(directory, analyzer, false, IndexWriter.MaxFieldLength.UNLIMITED);
                            attempts = 5;
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
                        indexWriter = new IndexWriter(directory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
                        attempts = 5;
                    }
                }

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
                    string summary = String.Format("Cancelled. Indexed {0} files. Skipped {1} files.", countTotal, countSkipped);
                    summary += String.Format(" Took {0}", (DateTime.Now - start));
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

                    string summary = String.Format("{0} files. New {1}. Changed {2}, Skipped {3}. Removed {4}. {5}", countTotal, countNew, countChanged, countSkipped, deleted, DateTime.Now - start);
                    worker.ReportProgress(countTotal, summary);
                }

                indexWriter.Optimize();
                indexWriter.Dispose();
            };
            worker.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs e)
            {
                //watcher.EnableRaisingEvents = true;
            };
            return worker;
        }

        public bool CheckIndex()
        {
            try
            {
                if (!System.IO.Directory.Exists(iniFile.IndexPath)) return false;
                var directory = new MMapDirectory(new DirectoryInfo(iniFile.IndexPath));
                searcher = new IndexSearcher(directory, true);
                searcher.Dispose();
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
                    searcher = new IndexSearcher(directory, true);
                }
                catch (IOException ex)
                {
                    logger.Error(ex);
                    throw new Exception("The index doesn't exist or is damaged. Please rebuild the index.", ex);
                }

                Query query;
                try
                {
                    var parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "content", new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30));
                    query = parser.Parse(queryText);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    throw new ArgumentException("Invalid query: " + ex.Message, "Query", ex);
                }

                // Search
                var results = searcher.Search(query, null, iniFile.HitsLimit);
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
                searcher.Dispose();
            };
            return searchWorker;
        }

        /// <summary>
        /// Indexes a folder.
        /// </summary>
        /// <param name="directory"></param>
        private bool addFolder(string searchDir, DirectoryInfo directory)
        {
            // Don't index the indexes.....
            if (directory.FullName.EndsWith(IniFile.SEARCH_INDEX))
                return false;

            // Don't index hidden directories.....
            if ((directory.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                return false;

            // Don't index excluded files
            foreach (string exclude in iniFile.SearchExclude)
            {
                if (directory.FullName.ToUpper().EndsWith(exclude))
                    return false;
            }

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
                    if (iniFile.ExtensionExclude.Contains(extension.ToUpper()))
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
                                worker.ReportProgress(fileCount, Path.GetFileName(fi.FullName));
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
        private void addDocument(string extension, string path, string relPath, bool exists)
        {
            logger.Trace(" File = " + path);
            string filename = Path.GetFileNameWithoutExtension(path);
            FileInfo fi = new FileInfo(path);
            string text = "";
            try
            {
                if (extension.ToLower() == ".zip" && fi.Length < zipMaxSize * 1000000)
                    text = Parser.Parse(path);
                else if (fi.Length < indexMaxFileSize * 1000000)
                    text = Parser.Parse(path);
                if (!string.IsNullOrEmpty(text))
                    foreach (var item in iniFile.Splitters)
                        text = text.Replace(item, " ");
            }
            catch (Exception e)
            {
                // Ignore error, add with no content
                logger.Error(e);
            }
            Document doc = new Document();
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
    }
}
