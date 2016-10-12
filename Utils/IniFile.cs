using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CodeFetcher
{
    /// <summary>
    /// Create a New INI file to store or load data
    /// </summary>
    public class IniFile
    {
        #region Private declarations
        private string path;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        /// <summary>
        /// Write Data to the INI File
        /// </summary>
        /// <param name="Section">Section name</param>
        /// <param name="Key">Key Name</param>
        /// <param name="Value">Value Name</param>
        private void WriteValue(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, path);
        }

        /// <summary>
        /// Read Data Value From the Ini File
        /// </summary>
        /// <param name="Section">Section name</param>
        /// <param name="Key">Key Name</param>
        /// <returns></returns>
        private string ReadValue(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(255);
            GetPrivateProfileString(Section, Key, "", temp, 255, path);
            return temp.ToString();
        }
        #endregion Private declarations

        #region Private constants
        private const string LOCATION = "Location";
        private struct KEYS
        {
            public const string SEARCH_PATTERNS = "Search Patterns";
            public const string SEARCH_DIRECTORY = "Search Directory";
            public const string PATHS_TO_SKIP = "Paths To Skip";
        }

        public string[] Patterns = new string[] { "*.*" };
        public string[] SearchDirs = null;
        public string[] SearchExclude = new string[] {
            "C:\\$RECYCLE.BIN", "\\BIN", "\\OBJ", "\\.SVN", "\\.GIT"
        };
        public string[] ExtensionExclude = new string[] {
            ".PDB", ".DLL", ".EXE", ".GIF", ".JPG", ".PNG",
        };
        public string[] Splitters = new string[] {
            ".", "=", "\"", ":", "<", ">", "(", ")", "[", "]",
            ",", "/", "\\", "{", "}", "-", "+", "*", "%", "#",
        };
        #endregion Private Constants

        /// <summary>
        /// IniFile Constructor.
        /// </summary>
        /// <param name="iniPath"></param>
        public IniFile(string iniPath, string appDir)
        {
            path = iniPath;
            SearchDirs = new string[] { appDir };
            if (File.Exists(iniPath))
            {
                string temp = ReadValue(LOCATION, KEYS.SEARCH_PATTERNS);
                if (!string.IsNullOrEmpty(temp))
                {
                    Patterns = temp.SemiColonSplit();
                }

                temp = ReadValue(LOCATION, KEYS.SEARCH_DIRECTORY);
                if (!string.IsNullOrEmpty(temp))
                {
                    var dirs = new List<string>();
                    foreach (string dir in temp.SemiColonSplit())
                    {
                        dirs.Add(Path.Combine(appDir, dir));
                    }
                    SearchDirs = dirs.ToArray();
                }

                temp = ReadValue(LOCATION, KEYS.PATHS_TO_SKIP);
                if (!string.IsNullOrEmpty(temp))
                {
                    var excludes = new List<string>();
                    foreach (string exclude in temp.SemiColonSplit())
                    {
                        excludes.Add(exclude.ToLower());
                    }
                    SearchExclude = excludes.ToArray();
                }
            }
        }
    }
}