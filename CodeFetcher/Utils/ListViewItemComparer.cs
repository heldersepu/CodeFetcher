using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace CodeFetcher
{
    internal class ListViewItemComparer : IComparer
    {
        public enum ColumnDataType
        {
            Generic,
            DateTime,
            Number
        }

        private int column = 0;
        private readonly ListSortDirection?[] sortDirections = null;
        private readonly ColumnDataType[] columnDataTypes = null;
        public ListViewItemComparer(
            int totalColumns,
            int initialColumn,
            ListSortDirection initialSortDirection,
            ColumnDataType initialDataType)
        {
            this.sortDirections = new ListSortDirection?[totalColumns];
            for (int i = 0; i < this.sortDirections.Length; i++)
            {
                this.sortDirections[i] = null;
            }
            this.columnDataTypes = new ColumnDataType[totalColumns];
            for (int i = 0; i < this.columnDataTypes.Length; i++)
            {
                this.columnDataTypes[i] = ColumnDataType.Generic;
            }
            this.column = initialColumn;
            this.sortDirections[this.column] = initialSortDirection;
            this.columnDataTypes[this.column] = initialDataType;
        }

        public int Column
        {
            get { return this.column; }
            set
            {
                this.column = value;
                if (!this.sortDirections[value].HasValue ||
                    this.sortDirections[value].Value == ListSortDirection.Descending)
                {
                    this.sortDirections[value] = ListSortDirection.Ascending;
                }
                else
                {
                    this.sortDirections[value] = ListSortDirection.Descending;
                }
            }
        }

        public void SetColumnAndType(int column, ColumnDataType type)
        {
            this.column = column;
            this.columnDataTypes[column] = type;
            if (!this.sortDirections[column].HasValue ||
                this.sortDirections[column].Value == ListSortDirection.Descending)
            {
                this.sortDirections[column] = ListSortDirection.Ascending;
            }
            else
            {
                this.sortDirections[column] = ListSortDirection.Descending;
            }
        }

        public int Compare(object x, object y)
        {
            int comparison = 0;
            switch (this.columnDataTypes[this.column])
            {
                case ColumnDataType.Generic:
                    string text1 = ((ListViewItem)x).SubItems[this.column].Text;
                    string text2 = ((ListViewItem)y).SubItems[this.column].Text;
                    comparison = string.Compare(text1, text2, true /* ignoreCase */);
                    break;
                case ColumnDataType.Number:
                    int int1 = int.Parse(((ListViewItem)x).SubItems[this.column].Text);
                    int int2 = int.Parse(((ListViewItem)y).SubItems[this.column].Text);
                    comparison = int1 - int2;
                    break;
                case ColumnDataType.DateTime:
                    DateTime dt1 = DateTime.Parse(((ListViewItem)x).SubItems[this.column].Text);
                    DateTime dt2 = DateTime.Parse(((ListViewItem)y).SubItems[this.column].Text);
                    comparison = DateTime.Compare(dt1, dt2);
                    break;
            }

            if (this.sortDirections[this.column].HasValue &&
                this.sortDirections[this.column] == ListSortDirection.Descending)
            {
                comparison = -comparison;
            }
            return comparison;
        }
    }
}
