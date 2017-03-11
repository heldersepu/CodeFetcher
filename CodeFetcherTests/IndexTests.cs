using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel;
using System.Threading;

namespace CodeFetcher.Tests
{
    [TestClass()]
    public class IndexTests
    {
        Index index;
        const int MAX_FILES = 200;
        const string TEST_STRING = "void y.FunctionX(){} test";
        string IndexStatus = string.Empty;

        public IndexTests()
        {
            var ini = new IniFile("", ".");
            ini.HitsLimit = MAX_FILES + 1;
            index = new Index(ini);
            index.Delete();

            var worker = index.Initialize();
            worker.ProgressChanged += delegate (object s, ProgressChangedEventArgs pe) { IndexStatus = pe.UserState.ToString(); };
            worker.RunWorkerAsync();
            while (worker.IsBusy)
                Thread.Sleep(100);

            index.TryOpen(5);
            string name = Guid.NewGuid().ToString();
            for (int i = 0; i < MAX_FILES; i++)
                index.addContent(
                    LastWriteTime: DateTime.Now,
                    type: "utest",
                    name: $"{name}_{i}",
                    path: i.ToString(),
                    content: TEST_STRING,
                    exists: false);
            index.Close();
            index.Cancel();
        }

        [TestMethod()]
        public void IndexStatusTest()
        {
            Assert.AreNotEqual(string.Empty, IndexStatus);
        }

        [TestMethod()]
        public void SearchTest()
        {
            int findings = 0;
            RunWorkerCompletedEventArgs completed = null;
            var search = index.Search("(FunctionX)", null);
            search.ProgressChanged += delegate (object sender, ProgressChangedEventArgs e) { findings++; };
            search.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs e) { completed = e; };
            search.RunWorkerAsync();
            while (search.IsBusy)
                Thread.Sleep(100);
            index.Cancel();

            if (completed.Error != null)
                Assert.Fail(completed.Error.Message);
            else if (findings < 1)
                Assert.Fail("NOTHING WAS FOUND");
            else
                Assert.AreEqual(MAX_FILES, findings);
        }

        [TestMethod()]
        public void SearchPathTest()
        {
            int findings = 0;
            RunWorkerCompletedEventArgs completed = null;
            var search = index.Search("content:void AND path:10", null);
            search.ProgressChanged += delegate (object sender, ProgressChangedEventArgs e) { findings++; };
            search.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs e) { completed = e; };
            search.RunWorkerAsync();
            while (search.IsBusy)
                Thread.Sleep(100);
            index.Cancel();

            if (completed.Error != null)
                Assert.Fail(completed.Error.Message);
            else if (findings < 1)
                Assert.Fail("NOTHING WAS FOUND");
            else
                Assert.AreEqual(1, findings);
        }
    }
}