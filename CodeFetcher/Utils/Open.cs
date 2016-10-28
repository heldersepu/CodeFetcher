using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace CodeFetcher
{
    public static class Open
    {
        public static void File(string[] searchDirs, string path)
        {
            // Loop through each search directory and see if the file exists
            foreach (string searchDir in searchDirs)
            {
                // Remove starting slash in old index files
                if (path.StartsWith(@"\"))
                    path = path.Substring(1);

                string fullPath = Path.Combine(searchDir, path);
                if (System.IO.File.Exists(fullPath))
                {
                    Process.Start(fullPath);
                    return;
                }
            }

            // Didn't find it so return a message
            MessageBox.Show("The file no longer exists, rebuild the index", "Deleted or Moved", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void Directory(string[] searchDirs, string path)
        {
            // Loop through each search directory and see if the file exists
            foreach (string searchDir in searchDirs)
            {
                // Remove starting slash in old index files
                if (path.StartsWith(@"\"))
                    path = path.Substring(1);

                string fullPath = Path.Combine(searchDir, path);
                if (System.IO.Directory.Exists(fullPath))
                {
                    Process.Start(fullPath);
                    return;
                }
            }

            // Didn't find it so return a message
            MessageBox.Show("The directory no longer exists, rebuild the index", "Deleted or Moved", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
