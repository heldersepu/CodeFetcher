using System;
using System.Windows.Forms;

namespace CodeFetcher
{
    static class StringExtension
    {
        public static string[] SemiColonSplit(this string str)
        {
            return str.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }

    static class ListViewExtension
    {
        public static string First(this ListView obj)
        {
            return (string)obj.Items[0].Tag;
        }

        public static string Selected(this ListView obj)
        {
            return (string)obj.SelectedItems[0].Tag;
        }
    }

    static class DateTimePickerExtension
    {
        public static string Selected(this DateTimePicker obj)
        {
            return obj.Value.ToString("yyyyMMdd");
        }
    }
}
