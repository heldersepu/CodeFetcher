using ICSharpCode.AvalonEdit;
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

    static class TextEditorExtension
    {
        public static int Find(this TextEditor obj, string QueryText)
        {
            var ct = StringComparison.CurrentCultureIgnoreCase;
            int x = obj.Text.IndexOf(QueryText, obj.SelectionStart + 1, ct);
            if (x <= 0 && obj.SelectionStart > 1)
                x = obj.Text.IndexOf(QueryText, 1, ct);
            return x;
        }
    }

    static class DateTimeExtension
    {
        public static string ToShort(this DateTime obj)
        {
            return obj.ToShortDateString() + " " + obj.ToShortTimeString();
        }
    }

    static class FloatExtension
    {
        public static string ToPercent(this float obj)
        {
            return (obj * 100).ToString("N0");
        }
    }
}
