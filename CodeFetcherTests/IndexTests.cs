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
                index.AddContent(
                    LastWriteTime: DateTime.Now,
                    type: "uTest" + i.ToString(),
                    name: $"Test_{name}_{i}",
                    path: "Logs" + i.ToString(),
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
        public void SearchContentTest()
        {
            SearchTest("(FunctionX)", MAX_FILES);
        }

        [TestMethod()]
        public void SearchNameTest()
        {
            SearchTest("name:Test*", MAX_FILES);
        }

        [TestMethod()]
        public void SearchTypeTest()
        {
            SearchTest("content:void AND type:uTest10", 1);
        }

        [TestMethod()]
        public void SearchPathTest()
        {
            SearchTest("content:void AND path:Logs10", 1);
        }

        private void SearchTest(string query, int expected)
        {
            int findings = 0;
            RunWorkerCompletedEventArgs completed = null;
            var search = index.Search(query, null);
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
                Assert.AreEqual(expected, findings);
        }
    }
}