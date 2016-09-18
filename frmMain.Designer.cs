using System.Windows.Forms;

namespace CodeFetcher
{
    partial class frmMain : Form
    {
        Label label1;
        Label label2;
        Label label3;
        Label label4;
        Label label5;
        Label labelStatus;
        ListView listViewResults;
        ColumnHeader columnHeaderIcon;
        ColumnHeader columnHeaderName;
        ColumnHeader columnHeaderFolder;
        ColumnHeader columnHeaderScore;
        ColumnHeader colHeaderModified;
        FolderBrowserDialog folderBrowserDialog1;
        TabControl tabControl1;
        TabPage tabPage1;
        TabPage tabPage2;
        TextBox textBoxQuery;
        TextBox textBoxContent;
        TextBox textBoxName;
        TextBox textBoxType;
        DateTimePicker dateTimePickerTo;
        DateTimePicker dateTimePickerFrom;
        ToolTip toolTip1;
        PictureBox pictureBox2;
        ContextMenuStrip contextMenuStrip1;
        ToolStripMenuItem openFileToolStripMenuItem;
        ToolStripMenuItem openContainingFolderToolStripMenuItem;
        Button buttonToday;
        Button buttonSearch;
        Button buttonSearch1;
        Button buttonRefreshIndex;


        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.labelStatus = new System.Windows.Forms.Label();
            this.listViewResults = new System.Windows.Forms.ListView();
            this.columnHeaderIcon = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderScore = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colHeaderModified = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFolder = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openContainingFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.textBoxQuery = new System.Windows.Forms.TextBox();
            this.buttonSearch = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.buttonToday = new System.Windows.Forms.Button();
            this.textBoxType = new System.Windows.Forms.TextBox();
            this.dateTimePickerTo = new System.Windows.Forms.DateTimePicker();
            this.dateTimePickerFrom = new System.Windows.Forms.DateTimePicker();
            this.label4 = new System.Windows.Forms.Label();
            this.buttonSearch1 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxContent = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.buttonRefreshIndex = new System.Windows.Forms.Button();
            this.contextMenuStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelStatus
            // 
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelStatus.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatus.Location = new System.Drawing.Point(128, 537);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(868, 19);
            this.labelStatus.TabIndex = 0;
            // 
            // listViewResults
            // 
            this.listViewResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderIcon,
            this.columnHeaderName,
            this.columnHeaderScore,
            this.colHeaderModified,
            this.columnHeaderFolder});
            this.listViewResults.ContextMenuStrip = this.contextMenuStrip1;
            this.listViewResults.FullRowSelect = true;
            this.listViewResults.Location = new System.Drawing.Point(12, 212);
            this.listViewResults.Name = "listViewResults";
            this.listViewResults.Size = new System.Drawing.Size(982, 314);
            this.listViewResults.TabIndex = 2;
            this.listViewResults.UseCompatibleStateImageBehavior = false;
            this.listViewResults.View = System.Windows.Forms.View.Details;
            this.listViewResults.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewResults_ColumnClick);
            this.listViewResults.DoubleClick += new System.EventHandler(this.listViewResults_DoubleClick);
            // 
            // columnHeaderIcon
            // 
            this.columnHeaderIcon.Text = "";
            this.columnHeaderIcon.Width = 22;
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Name";
            this.columnHeaderName.Width = 243;
            // 
            // columnHeaderScore
            // 
            this.columnHeaderScore.Text = "Score";
            // 
            // colHeaderModified
            // 
            this.colHeaderModified.Text = "Modified";
            this.colHeaderModified.Width = 150;
            // 
            // columnHeaderFolder
            // 
            this.columnHeaderFolder.Text = "Folder";
            this.columnHeaderFolder.Width = 400;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openFileToolStripMenuItem,
            this.openContainingFolderToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(202, 48);
            // 
            // openFileToolStripMenuItem
            // 
            this.openFileToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.openFileToolStripMenuItem.Name = "openFileToolStripMenuItem";
            this.openFileToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.openFileToolStripMenuItem.Text = "Open File";
            this.openFileToolStripMenuItem.Click += new System.EventHandler(this.openFileToolStripMenuItem_Click);
            // 
            // openContainingFolderToolStripMenuItem
            // 
            this.openContainingFolderToolStripMenuItem.Name = "openContainingFolderToolStripMenuItem";
            this.openContainingFolderToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.openContainingFolderToolStripMenuItem.Text = "Open Containing Folder";
            this.openContainingFolderToolStripMenuItem.Click += new System.EventHandler(this.openContainingFolderToolStripMenuItem_Click);
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.ShowNewFolderButton = false;
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(984, 198);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.pictureBox2);
            this.tabPage1.Controls.Add(this.textBoxQuery);
            this.tabPage1.Controls.Add(this.buttonSearch);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(752, 169);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Search";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(586, 80);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(27, 28);
            this.pictureBox2.TabIndex = 14;
            this.pictureBox2.TabStop = false;
            this.toolTip1.SetToolTip(this.pictureBox2, resources.GetString("pictureBox2.ToolTip"));
            this.pictureBox2.Click += new System.EventHandler(this.pictureBox2_Click);
            // 
            // textBoxQuery
            // 
            this.textBoxQuery.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxQuery.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.textBoxQuery.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.textBoxQuery.Location = new System.Drawing.Point(161, 81);
            this.textBoxQuery.Name = "textBoxQuery";
            this.textBoxQuery.Size = new System.Drawing.Size(419, 22);
            this.textBoxQuery.TabIndex = 0;
            this.textBoxQuery.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxQuery_KeyDown);
            // 
            // buttonSearch
            // 
            this.buttonSearch.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.buttonSearch.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonSearch.Location = new System.Drawing.Point(298, 116);
            this.buttonSearch.Name = "buttonSearch";
            this.buttonSearch.Size = new System.Drawing.Size(160, 24);
            this.buttonSearch.TabIndex = 1;
            this.buttonSearch.Text = "Search";
            this.buttonSearch.Click += new System.EventHandler(this.buttonSearch_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.buttonToday);
            this.tabPage2.Controls.Add(this.textBoxType);
            this.tabPage2.Controls.Add(this.dateTimePickerTo);
            this.tabPage2.Controls.Add(this.dateTimePickerFrom);
            this.tabPage2.Controls.Add(this.label4);
            this.tabPage2.Controls.Add(this.buttonSearch1);
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Controls.Add(this.textBoxContent);
            this.tabPage2.Controls.Add(this.label2);
            this.tabPage2.Controls.Add(this.textBoxName);
            this.tabPage2.Controls.Add(this.label1);
            this.tabPage2.Controls.Add(this.label5);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(976, 169);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Advanced";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // buttonToday
            // 
            this.buttonToday.Location = new System.Drawing.Point(446, 105);
            this.buttonToday.Name = "buttonToday";
            this.buttonToday.Size = new System.Drawing.Size(57, 23);
            this.buttonToday.TabIndex = 24;
            this.buttonToday.Text = "Today";
            this.buttonToday.UseVisualStyleBackColor = true;
            this.buttonToday.Click += new System.EventHandler(this.buttonToday_Click);
            // 
            // textBoxType
            // 
            this.textBoxType.Location = new System.Drawing.Point(77, 105);
            this.textBoxType.Name = "textBoxType";
            this.textBoxType.Size = new System.Drawing.Size(194, 22);
            this.textBoxType.TabIndex = 5;
            this.toolTip1.SetToolTip(this.textBoxType, "html, cs, vb, js, css");
            this.textBoxType.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxQuery_KeyDown);
            // 
            // dateTimePickerTo
            // 
            this.dateTimePickerTo.CustomFormat = "";
            this.dateTimePickerTo.Location = new System.Drawing.Point(528, 105);
            this.dateTimePickerTo.MaxDate = new System.DateTime(2100, 12, 31, 0, 0, 0, 0);
            this.dateTimePickerTo.MinDate = new System.DateTime(1990, 1, 1, 0, 0, 0, 0);
            this.dateTimePickerTo.Name = "dateTimePickerTo";
            this.dateTimePickerTo.Size = new System.Drawing.Size(100, 22);
            this.dateTimePickerTo.TabIndex = 9;
            // 
            // dateTimePickerFrom
            // 
            this.dateTimePickerFrom.CustomFormat = "";
            this.dateTimePickerFrom.Location = new System.Drawing.Point(340, 105);
            this.dateTimePickerFrom.MaxDate = new System.DateTime(2100, 12, 31, 0, 0, 0, 0);
            this.dateTimePickerFrom.MinDate = new System.DateTime(1900, 1, 1, 0, 0, 0, 0);
            this.dateTimePickerFrom.Name = "dateTimePickerFrom";
            this.dateTimePickerFrom.Size = new System.Drawing.Size(100, 22);
            this.dateTimePickerFrom.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(277, 109);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(60, 16);
            this.label4.TabIndex = 6;
            this.label4.Text = "Modified:";
            // 
            // buttonSearch1
            // 
            this.buttonSearch1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSearch1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonSearch1.Location = new System.Drawing.Point(790, 44);
            this.buttonSearch1.Name = "buttonSearch1";
            this.buttonSearch1.Size = new System.Drawing.Size(160, 54);
            this.buttonSearch1.TabIndex = 10;
            this.buttonSearch1.Text = "Search";
            this.buttonSearch1.Click += new System.EventHandler(this.buttonSearch_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 46);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 16);
            this.label3.TabIndex = 0;
            this.label3.Text = "Content:";
            // 
            // textBoxContent
            // 
            this.textBoxContent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxContent.Location = new System.Drawing.Point(77, 46);
            this.textBoxContent.Name = "textBoxContent";
            this.textBoxContent.Size = new System.Drawing.Size(707, 22);
            this.textBoxContent.TabIndex = 1;
            this.textBoxContent.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxQuery_KeyDown);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 74);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "Name:";
            // 
            // textBoxName
            // 
            this.textBoxName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxName.Location = new System.Drawing.Point(77, 74);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(707, 22);
            this.textBoxName.TabIndex = 3;
            this.textBoxName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxQuery_KeyDown);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 102);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 16);
            this.label1.TabIndex = 4;
            this.label1.Text = "Type:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(505, 108);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(21, 16);
            this.label5.TabIndex = 8;
            this.label5.Text = "To";
            // 
            // toolTip1
            // 
            this.toolTip1.AutomaticDelay = 100;
            this.toolTip1.AutoPopDelay = 10000;
            this.toolTip1.InitialDelay = 100;
            this.toolTip1.IsBalloon = true;
            this.toolTip1.ReshowDelay = 20;
            this.toolTip1.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.toolTip1.ToolTipTitle = "Examples";
            // 
            // buttonRefreshIndex
            // 
            this.buttonRefreshIndex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonRefreshIndex.Location = new System.Drawing.Point(12, 532);
            this.buttonRefreshIndex.Name = "buttonRefreshIndex";
            this.buttonRefreshIndex.Size = new System.Drawing.Size(110, 25);
            this.buttonRefreshIndex.TabIndex = 4;
            this.buttonRefreshIndex.Text = "Refresh Index";
            this.buttonRefreshIndex.UseVisualStyleBackColor = true;
            this.buttonRefreshIndex.Click += new System.EventHandler(this.buttonRefreshIndex_Click);
            // 
            // frmMain
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
            this.ClientSize = new System.Drawing.Size(1008, 561);
            this.Controls.Add(this.buttonRefreshIndex);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.listViewResults);
            this.Controls.Add(this.labelStatus);
            this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CodeFetcher";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion
    }
}
