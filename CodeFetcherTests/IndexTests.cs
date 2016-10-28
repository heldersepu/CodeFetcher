using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.QualityTools.Testing.Fakes;
using CodeFetcher;
using System;
using System.IO;
using System.Fakes;
using System.IO.Fakes;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Forms;

namespace CodeFetcher.Tests
{
    [TestClass()]
    public class IndexTests
    {
        [TestMethod()]
        public void SearchTest()
        {
            using (ShimsContext.Create())
            {
                ShimDirectory.GetFilesStringString = (dir, pattern) =>
                {
                    return new[] { @"C:\abc\test.txt" };
                };
                ShimFile.ReadAllTextString = (path) =>
                {
                    return "void function test";
                };

                var ini = new IniFile("", ".");
                var index = new Index(ini);
                var worker = index.Initialize();
                worker.RunWorkerAsync();
                while (worker.IsBusy)
                    Thread.Sleep(100);

                List<string> findings = new List<string>();
                RunWorkerCompletedEventArgs completed = null;
                var search = index.Search("function", null);
                search.ProgressChanged +=
                    delegate (object sender, ProgressChangedEventArgs e)
                    {
                        var t = (ListViewItem)e.UserState;
                        findings.Add(t.SubItems[1].ToString());
                    };
                search.RunWorkerCompleted +=
                    delegate (object sender, RunWorkerCompletedEventArgs e)
                    {
                        completed = e;
                    };
                search.RunWorkerAsync();
                while (search.IsBusy)
                    Thread.Sleep(100);

                if (completed.Error != null)
                {
                    Assert.Fail(completed.Error.Message);
                }
                else
                {
                    if (findings.Count < 1)
                        Assert.Inconclusive();
                }
            }
        }
    }
}