using System;

namespace CodeFetcher
{
    static class StringExtension
    {
        public static string[] SemiColonSplit(this string str)
        {
            return str.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
