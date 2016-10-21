using NLog;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CodeFetcher
{
    /// <summary>
    /// Create a New INI file to store or load data
    /// </summary>
    public class IniFile
    {
        #region Private declarations
        private static Logger logger = LogManager.GetCurrentClassLogger();
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
        private void WriteValue(string Section, string Key, string[] Values)
        {
            if (Values.Length > 0)
            {
                string Value = "";
                foreach (var val in Values)
                    Value += val + ";";
                WritePrivateProfileString(Section, Key, Value, path);
            }
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
        public const string SEARCH_INDEX = ".SearchIndex";
        private struct KEYS
        {
            public const string SEARCH_PATTERNS = "Search Patterns";
            public const string SEARCH_DIRECTORIES = "Search Directories";
            public const string PATHS_TO_SKIP = "Paths To Skip";
            public const string EXTENSIONS_EXCLUDE = "Extensions to Exclude";
            public const string SPLITTERS = "Word Splitters";
            public const string INDEX_PATH = "Index Path";
            public const string HITS_LIMIT = "Limit on search hits";
        }
        #endregion Private Constants

        public int HitsLimit = 200;
        public string IndexPath = "";
        public string[] Patterns = new string[] { "*.*" };
        public string[] SearchDirs = null;
        public string[] SearchExclude = new string[] {
            "C:\\$RECYCLE.BIN", "\\BIN", "\\OBJ", "\\.SVN", "\\.GIT", "\\$TF"
        };
        public string[] ExtensionExclude = new string[] {
            ".PDB", ".DLL", ".EXE", ".GIF", ".JPG", ".PNG", ".GZ", ".ZIP",
            ".NUPKG", ".DBSCHEMA", ".DBML", ".CAB",
        };
        public string[] Splitters = new string[] {
            ".", "=", "\"", ":", "<", ">", "(", ")", "[", "]",
            ",", "/", "\\", "{", "}", "-", "+", "*", "%", "#",
        };
        public string SearchTermsPath { get { return Path.Combine(IndexPath, "searchHistory.log"); } }



        /// <summary>
        /// IniFile Constructor.
        /// </summary>
        /// <param name="iniPath"></param>
        public IniFile(string iniPath, string appDir)
        {
            path = iniPath;
            SearchDirs = new string[] { appDir };
            IndexPath = Path.Combine(appDir, SEARCH_INDEX);
            if (File.Exists(iniPath))
            {
                logger.Trace("Start reading ini file...");
                string temp = ReadValue(LOCATION, KEYS.SEARCH_PATTERNS);
                if (!string.IsNullOrEmpty(temp))
                {
                    Patterns = temp.SemiColonSplit();
                }

                temp = ReadValue(LOCATION, KEYS.SEARCH_DIRECTORIES);
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

                temp = ReadValue(LOCATION, KEYS.EXTENSIONS_EXCLUDE);
                if (!string.IsNullOrEmpty(temp))
                {
                    var excludes = new List<string>();
                    foreach (string ex in temp.SemiColonSplit())
                    {
                        excludes.Add(ex.ToUpper());
                    }
                    ExtensionExclude = excludes.ToArray();
                }

                temp = ReadValue(LOCATION, KEYS.SPLITTERS);
                if (!string.IsNullOrEmpty(temp))
                {
                    var splitters = new List<string>();
                    foreach (string sp in temp.SemiColonSplit())
                    {
                        splitters.Add(sp.ToLower());
                    }
                    Splitters = splitters.ToArray();
                }

                temp = ReadValue(LOCATION, KEYS.INDEX_PATH);
                if (!string.IsNullOrEmpty(temp))
                {
                    IndexPath = temp;
                }

                temp = ReadValue(LOCATION, KEYS.HITS_LIMIT);
                if (!string.IsNullOrEmpty(temp))
                {
                    int x;
                    if (int.TryParse(temp, out x))
                        HitsLimit = x;
                }
                logger.Trace("Done reading ini file...");
            }
        }

        public void Save()
        {
            WriteValue(LOCATION, KEYS.SEARCH_PATTERNS,      Patterns);
            WriteValue(LOCATION, KEYS.SEARCH_DIRECTORIES,   SearchDirs);
            WriteValue(LOCATION, KEYS.PATHS_TO_SKIP,        SearchExclude);
            WriteValue(LOCATION, KEYS.EXTENSIONS_EXCLUDE,   ExtensionExclude);
            WriteValue(LOCATION, KEYS.SPLITTERS,            Splitters);
            WriteValue(LOCATION, KEYS.INDEX_PATH,           IndexPath);
            WriteValue(LOCATION, KEYS.HITS_LIMIT,           HitsLimit.ToString());
            logger.Trace("Values written to ini file...");
        }
    }
}
