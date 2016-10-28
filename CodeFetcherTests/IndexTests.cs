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
using System.Runtime.InteropServices;

namespace CodeFetcher.Tests
{
    [TestClass()]
    public class IndexTests
    {
        Index index;
        const int MAX_FILES = 10000;
        const string TEST_STRING = "void function test";
        public IndexTests()
        {
            var ini = new IniFile("", ".");
            ini.HitsLimit = MAX_FILES;
            index = new Index(ini);
        }

        [TestMethod()]
        public void SearchTest()
        {
            using (ShimsContext.Create())
            {
                ShimDirectoryInfo.AllInstances.GetFilesString = (dir, pattern) =>
                {
                    var fis = new FileInfo[MAX_FILES];
                    string name = Guid.NewGuid().ToString();
                    for (int i = 0; i < MAX_FILES; i++)
                        fis[i] = new FileInfo($"{name}_{i}.utest");
                    return fis;
                };
                ShimFileInfo.AllInstances.LengthGet = (file) =>
                {
                    return (file.Exists)? GetFileSize(file.FullName): TEST_STRING.Length;
                };
                ShimFile.ReadAllTextString = (path) =>
                {
                    return TEST_STRING;
                };

                var worker = index.Initialize();
                worker.RunWorkerAsync();
                while (worker.IsBusy)
                    Thread.Sleep(100);

                int findings = 0;
                RunWorkerCompletedEventArgs completed = null;
                var search = index.Search("function", null);
                search.ProgressChanged +=
                    delegate (object sender, ProgressChangedEventArgs e)
                    {
                        findings++;
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
                    Assert.Fail(completed.Error.Message);
                else if (findings < 1)
                    Assert.Fail("NOTHING WAS FOUND");
                else
                    Assert.AreEqual(findings, MAX_FILES);
            }
        }

        #region private methods
        private static long GetFileSize(string file)
        {
            uint hosize;
            uint losize = GetCompressedFileSizeW(file, out hosize);
            return (long)hosize << 32 | losize;
        }

        [DllImport("kernel32.dll")]
        static extern uint GetCompressedFileSizeW([In, MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
           [Out, MarshalAs(UnmanagedType.U4)] out uint lpFileSizeHigh);
        #endregion

    }
}