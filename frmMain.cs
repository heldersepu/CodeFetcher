using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using CodeFetcher.Icons;


namespace CodeFetcher
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class frmMain : System.Windows.Forms.Form
	{
		private string pathIndex;
		private IndexWriter indexWriter;
        private string[] patterns;
		private SystemImageList imageListDocuments;
        bool portablePaths = true;

		private IndexSearcher searcher = null;
        private string[] searchDirs;
        private string[] searchExclude;
        private string appPath;
        private string appDir;
        private string appName;
        private string searchTermsPath;
        Dictionary<string, long> dateStamps;
        Dictionary<string, long> newDateStamps;
        BackgroundWorker indexWorker;
        BackgroundWorker searchWorker;
        AutoCompleteStringCollection searchTerms = new AutoCompleteStringCollection();

        int indexCounter = 0;
        int indexMaxFileSize = 20;
        int zipMaxSize = 5;
        int resultsMax = 200;
        int fileCount;
        string status = "";

		// statistics
		private long bytesTotal = 0;
		private int countTotal = 0;
        private int countSkipped = 0;
        int countNew = 0;
        int countChanged = 0;


        private System.Windows.Forms.Label labelStatus;
		private System.Windows.Forms.ListView listViewResults;
		private System.Windows.Forms.ColumnHeader columnHeaderIcon;
		private System.Windows.Forms.ColumnHeader columnHeaderName;
		private System.Windows.Forms.ColumnHeader columnHeaderFolder;
        private System.Windows.Forms.ColumnHeader columnHeaderScore;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private ColumnHeader colHeaderModified;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TextBox textBoxQuery;
        private Button buttonSearch;
        private TabPage tabPage2;
        private Label label4;
        private Button buttonSearch1;
        private Label label3;
        private TextBox textBoxContent;
        private Label label2;
        private TextBox textBoxName;
        private Label label1;
        private Label label5;
        private DateTimePicker dateTimePickerTo;
        private DateTimePicker dateTimePickerFrom;
        private ToolTip toolTip1;
        private PictureBox pictureBox2;
        private Label labelSearch;
        private Button buttonRefreshIndex;
        private TextBox textBoxType;
        private PictureBox pictureBoxCredits;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem openFileToolStripMenuItem;
        private ToolStripMenuItem openContainingFolderToolStripMenuItem;
        private Button buttonToday;
        private System.ComponentModel.IContainer components;

		public frmMain()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			imageListDocuments = new SystemImageList(SystemImageListSize.SmallIcons);
			SystemImageListHelper.SetListViewImageList(listViewResults, imageListDocuments, false);

		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
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
            this.labelSearch = new System.Windows.Forms.Label();
            this.buttonRefreshIndex = new System.Windows.Forms.Button();
            this.pictureBoxCredits = new System.Windows.Forms.PictureBox();
            this.contextMenuStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCredits)).BeginInit();
            this.SuspendLayout();
            //
            // labelStatus
            //
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelStatus.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatus.Location = new System.Drawing.Point(128, 566);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(644, 19);
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
            this.listViewResults.Size = new System.Drawing.Size(758, 316);
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
            this.tabControl1.Size = new System.Drawing.Size(760, 198);
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
            this.tabPage2.Size = new System.Drawing.Size(752, 169);
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
            this.toolTip1.SetToolTip(this.textBoxType, "pdf, doc, docx\r\nxls, xlsx");
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
            this.buttonSearch1.Location = new System.Drawing.Point(566, 44);
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
            this.textBoxContent.Size = new System.Drawing.Size(483, 22);
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
            this.textBoxName.Size = new System.Drawing.Size(483, 22);
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
            // labelSearch
            //
            this.labelSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelSearch.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSearch.Location = new System.Drawing.Point(12, 538);
            this.labelSearch.Name = "labelSearch";
            this.labelSearch.Size = new System.Drawing.Size(752, 20);
            this.labelSearch.TabIndex = 3;
            //
            // buttonRefreshIndex
            //
            this.buttonRefreshIndex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonRefreshIndex.Location = new System.Drawing.Point(12, 561);
            this.buttonRefreshIndex.Name = "buttonRefreshIndex";
            this.buttonRefreshIndex.Size = new System.Drawing.Size(110, 25);
            this.buttonRefreshIndex.TabIndex = 4;
            this.buttonRefreshIndex.Text = "Refresh Index";
            this.buttonRefreshIndex.UseVisualStyleBackColor = true;
            this.buttonRefreshIndex.Click += new System.EventHandler(this.buttonRefreshIndex_Click);
            //
            // pictureBoxCredits
            //
            this.pictureBoxCredits.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxCredits.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBoxCredits.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxCredits.Image")));
            this.pictureBoxCredits.Location = new System.Drawing.Point(745, 5);
            this.pictureBoxCredits.Name = "pictureBoxCredits";
            this.pictureBoxCredits.Size = new System.Drawing.Size(25, 25);
            this.pictureBoxCredits.TabIndex = 5;
            this.pictureBoxCredits.TabStop = false;
            this.pictureBoxCredits.Click += new System.EventHandler(this.pictureBoxCredits_Click);
            //
            // frmMain
            //
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
            this.ClientSize = new System.Drawing.Size(784, 590);
            this.Controls.Add(this.pictureBoxCredits);
            this.Controls.Add(this.buttonRefreshIndex);
            this.Controls.Add(this.labelSearch);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.listViewResults);
            this.Controls.Add(this.labelStatus);
            this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CodeFetcher";
            this.Activated += new System.EventHandler(this.Form1_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCredits)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.DoEvents();

			Application.Run(new frmMain());
		}

		private void Form1_Load(object sender, System.EventArgs e)
		{

            appPath = typeof(frmMain).Assembly.Location;
            appDir = Path.GetDirectoryName( appPath );
            appName = Path.GetFileNameWithoutExtension(appPath);

            string iniPath = Path.Combine(appDir, appName + ".ini");
            if (File.Exists(iniPath))
            {
                IniFile ini = new IniFile(iniPath);

                string tempPatterns = ini.IniReadValue("Location", "Search Patterns");
                if (string.IsNullOrEmpty(tempPatterns) == false)
                {
                    patterns = tempPatterns.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                }

                string tempSearchDir = ini.IniReadValue("Location", "Search Directory");
                if (string.IsNullOrEmpty(tempSearchDir) == false)
                {
                    List<string> dirs = new List<string>();
                    foreach(string dir in tempSearchDir.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        dirs.Add(Path.Combine(appDir, dir));
                    }
                    searchDirs = dirs.ToArray();
                }

                string tempSearchExclude = ini.IniReadValue("Location", "Paths To Skip");
                if (string.IsNullOrEmpty(tempSearchExclude) == false)
                {
                    List<string> excludes = new List<string>();
                    foreach (string exclude in tempSearchExclude.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        excludes.Add(exclude.ToLower());
                    }
                    searchExclude = excludes.ToArray();
                }

                pathIndex = ini.IniReadValue("Location", "Search Index");
                if (string.IsNullOrEmpty(pathIndex) == false)
                    pathIndex = Path.Combine(appDir, pathIndex);
                string maxSize = ini.IniReadValue("Index", "Max Size");
                if (!string.IsNullOrEmpty(maxSize))
                    indexMaxFileSize = int.Parse(maxSize);
                maxSize = ini.IniReadValue("Index", "Zip Max Size");
                if (!string.IsNullOrEmpty(maxSize))
                    zipMaxSize = int.Parse(maxSize);

                try
                {
                    string max = ini.IniReadValue("Results", "Max Result");
                    if (!string.IsNullOrEmpty(max))
                        resultsMax = int.Parse(max);
                }
                catch
                {
                    MessageBox.Show("The Max Result setting has an invalid value", "Max Result", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

                try
                {
                    string portable = ini.IniReadValue("Options", "Portable Paths");
                    if (!string.IsNullOrEmpty(portable))
                        portablePaths = bool.Parse(portable);
                }
                catch
                {
                    MessageBox.Show("The Portable Paths setting has an invalid value, should be true or false", "Portable Paths", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }


            }

            // Set defaults
            if( patterns == null)
                patterns = new string[] { "*.*" };
            if( searchDirs == null )
                searchDirs =  new string[] { appDir };
            if (searchExclude == null)
                searchExclude = new string[] { "c:\\$Recycle.Bin" };
            if (string.IsNullOrEmpty(pathIndex))
                pathIndex = Path.Combine(appDir, "SearchIndex");
            searchTermsPath = Path.Combine(pathIndex, "searchhistory");

            dateTimePickerFrom.MaxDate = DateTime.Today;
            dateTimePickerFrom.Format = DateTimePickerFormat.Short;
            dateTimePickerFrom.Value = dateTimePickerFrom.MinDate;
            dateTimePickerTo.Format = DateTimePickerFormat.Short;
            dateTimePickerTo.Value = DateTime.Today.AddDays(1);


            Timer t = new Timer();
            t.Interval = 1000;
            t.Tick += delegate(object sender1, EventArgs e1)
            {

                if (indexCounter > 10)
                    indexCounter = 0;

                if (indexWorker != null && indexWorker.IsBusy)
                    labelStatus.Text = status + "".PadRight(indexCounter++, '.');
            };
            t.Start();

            Index();

            searchTerms = LoadSearchTerms();
            textBoxQuery.AutoCompleteCustomSource = searchTerms;

		}


        void Index()
        {

            fileCount = 0;

            indexWorker = new BackgroundWorker();
            indexWorker.WorkerReportsProgress = true;
            indexWorker.WorkerSupportsCancellation = true;

            //watcher.EnableRaisingEvents = false;

            indexWorker.DoWork += delegate(object sender, DoWorkEventArgs e)
            {
                dateStamps = new Dictionary<string, long>();
                newDateStamps = new Dictionary<string, long>();


                // First load all of the datestamps to check if the file is modified
                if (checkIndex())
                {
                    var directory = new MMapDirectory(new DirectoryInfo(pathIndex));
                    IndexReader indexReader = IndexReader.Open(directory, true);

                    // Check to see if we are in relative or absolute path mode


                    for (int i = 0; i < indexReader.NumDocs(); i++)
                    {
                        if (indexReader.IsDeleted(i) == false)
                        {
                            Document doc = indexReader.Document(i);
                            string path = doc.Get("path");
                            long ticks = long.Parse(doc.Get("ticks"));
                            if (dateStamps.ContainsKey(path))
                            {
                                dateStamps[path] = Math.Max(dateStamps[path], ticks);
                            }
                            else
                                dateStamps.Add(path, ticks);
                        }
                    }
                    indexReader.Dispose();
                }

                // Try to open the Index for writing
                int attempts = 0;
                while (attempts < 5)
                {
                    var directory = new MMapDirectory(new DirectoryInfo(pathIndex));
                    var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
                    if (checkIndex())
                    {

                        try
                        {
                            indexWriter = new IndexWriter(directory, analyzer, false, IndexWriter.MaxFieldLength.UNLIMITED);
                            attempts = 5;
                        }
                        catch (Lucene.Net.Store.LockObtainFailedException ex)
                        {
                            attempts++;
                            if (System.IO.Directory.Exists(pathIndex))
                                System.IO.Directory.Delete(pathIndex, true);
                        }
                    }
                    else
                    {
                        indexWriter = new IndexWriter(directory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
                        attempts = 5;
                    }
                }

                // Hide the file
                File.SetAttributes(pathIndex, FileAttributes.Hidden);

                bytesTotal = 0;
                countTotal = 0;
                countSkipped = 0;
                countNew = 0;
                countChanged = 0;
                bool cancel = false;
                DateTime start = DateTime.Now;

                foreach (string searchDir in searchDirs)
                {
                    if (System.IO.Directory.Exists(searchDir))
                    {
                        DirectoryInfo di = new DirectoryInfo(searchDir);

                        // Add folder
                        cancel = addFolder(searchDir, di);

                        // Exit if cancel has been pressed
                        if (cancel)
                            break;
                    }
                }


                if (cancel)
                {
                    string summary = String.Format("Cancelled. Indexed {0} files ({1} bytes). Skipped {2} files.", countTotal, bytesTotal, countSkipped);
                    summary += String.Format(" Took {0}", (DateTime.Now - start));
                    indexWorker.ReportProgress(0, summary);
                    e.Cancel = true;
                }
                else
                {
                    int deleted = 0;

                    // Loop through all the files and delete if it doesn't exist
                    foreach (string file in dateStamps.Keys)
                    {
                        if (newDateStamps.ContainsKey(file) == false)
                        {
                            deleted++;
                            indexWriter.DeleteDocuments(new Term("path", file));
                        }
                    }

                    string summary = String.Format("{0} files ({1} mb). New {2}. Changed {3}, Skipped {4}. Removed {5}. {6}", countTotal, (bytesTotal / 1000000).ToString("N0"), countNew, countChanged, countSkipped, deleted, DateTime.Now - start);
                    indexWorker.ReportProgress(0, summary);
                }

                indexWriter.Optimize();
                indexWriter.Dispose();
            };
            indexWorker.ProgressChanged += delegate(object sender, ProgressChangedEventArgs e)
            {
                if( e.ProgressPercentage == 0 )
                    status = e.UserState.ToString();
                else
                    status = string.Format("Files indexed {0}. {1}", countTotal, e.UserState.ToString());
                this.labelStatus.Text = status;
                indexCounter = 0;
            };
            indexWorker.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs e)
            {
                //watcher.EnableRaisingEvents = true;
            };

            indexWorker.RunWorkerAsync();
        }

		/// <summary>
		/// Indexes a folder.
		/// </summary>
		/// <param name="directory"></param>
		private bool addFolder(string searchDir, DirectoryInfo directory)
		{

            // Don't index the indexes.....
            if (directory.FullName == pathIndex)
                return false;

            // Don't index excluded files
            foreach (string exclude in searchExclude)
            {
                if (directory.FullName.ToLower().Contains(exclude))
                    return false;
            }

            int filesIndexed = 0;

			// find all matching files
			foreach (string pattern in patterns)
			{

                FileInfo[] fis = null;
                try
                {
                    fis = directory.GetFiles(pattern);
                }
                catch (Exception)
                {
                    return false;
                }

				foreach (FileInfo fi in fis)
				{
					// skip temporary office files
                    if (fi.Name.StartsWith("~") || fi.Name.StartsWith("."))
						continue;

                    if (indexWorker.CancellationPending)
                        return true;

                    fileCount++;

					try
					{
                        string path = fi.FullName;

                        string relPath = path;
                        // Remove the full path
                        if (portablePaths)
                        {
                            relPath = path.Replace(searchDir, "");
                            // Remove the starting slash
                            if (relPath.StartsWith(@"\"))
                                relPath = relPath.Substring(1);
                        }

                        newDateStamps.Add(relPath, fi.LastWriteTime.Ticks);

                        // Check to see of doc has changed
                        if (dateStamps.ContainsKey(relPath) == false)
                        {
                            addOfficeDocument(path, relPath, false);
                            filesIndexed++;
                        }
                        else if (dateStamps[relPath] < fi.LastWriteTime.Ticks)
                        {
                            // Delete the existing document
                            addOfficeDocument(path, relPath, true);
                            filesIndexed++;
                        }

						// update statistics
						this.countTotal++;
						this.bytesTotal += fi.Length;

						// show added file
                        indexWorker.ReportProgress(fileCount, Path.GetFileName(fi.FullName));
					}
					catch (Exception)
					{
						// parsing and indexing wasn't successful, skipping that file
						this.countSkipped++;
                        indexWorker.ReportProgress(fileCount, "Skipped:" + Path.GetFileName(fi.FullName));
					}
				}
			}

            // Only commit if things have been indexed
            if(filesIndexed > 0)
                indexWriter.Commit();

			// add subfolders
			foreach (DirectoryInfo di in directory.GetDirectories())
			{
                bool cancel = addFolder(searchDir, di);
                if (cancel)
                    return true;
			}

            return false;
		}

		/// <summary>
		/// Parses and indexes an IFilter parseable file.
		/// </summary>
		/// <param name="path"></param>
		private void addOfficeDocument(string path, string relPath, bool exists)
		{

            string filename = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path);
            FileInfo fi = new FileInfo(path);

            Document doc = new Document();
            string text = "";

            try
            {
                if (extension.ToLower() == ".zip" && fi.Length < zipMaxSize * 1000000)
                    text = Parser.Parse(path);
                else if (fi.Length < indexMaxFileSize * 1000000)
                    text = Parser.Parse(path);
            }
            catch (Exception)
            {
                // Ignore error, add with not content
            }

            doc.Add(new Field("modified", fi.LastWriteTime.ToString("yyyyMMddHHmmss"), Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("ticks", fi.LastWriteTime.Ticks.ToString(), Field.Store.YES, Field.Index.NO));
            doc.Add(new Field("type", extension.Substring(1), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("name", filename, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("path", relPath, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("content", text, Field.Store.NO, Field.Index.ANALYZED));

            if (exists)
            {
                indexWriter.UpdateDocument(new Term("path", relPath), doc);
                countChanged++;
            }
            else
            {
                indexWriter.AddDocument(doc);
                countNew++;
            }
		}

		private void buttonSearch_Click(object sender, System.EventArgs e)
		{
			Search();
		}

		private bool checkIndex()
		{
			try
			{
                var directory = new MMapDirectory(new DirectoryInfo(pathIndex));
                searcher = new IndexSearcher(directory, true);
				searcher.Dispose();
                return true;
			}
			catch (IOException)
			{
				return false;
			}
		}

		private void Search()
		{
            buttonSearch.Enabled = false;
            DateTime start = DateTime.Now;
            int hitCount = 0;

            searchWorker = new BackgroundWorker();
            searchWorker.WorkerReportsProgress = true;
            searchWorker.WorkerSupportsCancellation = true;


            string queryText = "";
            string queryHistory = "";
            if (tabControl1.SelectedIndex == 0)
            {
                // Parse the query, "content" is the default field to search
                if (this.textBoxQuery.Text.Trim() == String.Empty)
                    return;

                queryText ="(" + textBoxQuery.Text + ")";

                // Also search the path if the query isn't qualified
                if (queryText.Contains(":") == false)
                    queryText += " OR name:" + queryText;

                queryHistory = textBoxQuery.Text;
            }
            else
            {
                queryText = "";
                if (textBoxContent.Text.Trim() != "")
                    queryText = "content:" + textBoxContent.Text + " AND";

                if (textBoxName.Text.Trim() != "")
                    queryText += " name:" + textBoxName.Text + " AND";

                if (textBoxType.Text != "")
                {
                    string types = "";
                    foreach (string type in textBoxType.Text.Split(','))
                    {
                        types += " type:" + type + " OR";
                    }

                    if (types != "")
                    {
                        // Remove last OR
                        types = types.Substring(0, types.Length - 2);
                        queryText += " (" + types + ") AND";
                    }
                }

                queryText += " modified:[" + dateTimePickerFrom.Value.ToString("yyyyMMdd") + " TO " + dateTimePickerTo.Value.ToString("yyyyMMdd") + "]";

            }

            queryText = queryText.Trim();

            this.listViewResults.Items.Clear();

            searchWorker.DoWork += delegate(object sender, DoWorkEventArgs e)
            {
                try
                {
                    var directory = new MMapDirectory(new DirectoryInfo(pathIndex));
                    searcher = new IndexSearcher(directory, true);
                }
                catch (IOException ex)
                {
                    throw new Exception("The index doesn't exist or is damaged. Please rebuild the index.", ex);
                }

                Query query;
                try
                {
                    QueryParser parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "content", new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30));
                    query = parser.Parse(queryText);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("Invalid query: " + ex.Message, "Query", ex);
                }

                // Search
                var results = searcher.Search(query, null, 200);
                hitCount = results.ScoreDocs.Length;

                //			Optionally limit the result count
                //			int resultsCount = smallerOf(20, hits.Length());

                foreach (ScoreDoc scoreDoc in results.ScoreDocs)
                {
                    // get the document from index
                    Document doc = searcher.Doc(scoreDoc.Doc);

                    // create a new row with the result data
                    string filename = doc.Get("name") + "." + doc.Get("type");
                    string path = doc.Get("path");
                    DateTime modified = DateTime.ParseExact(doc.Get("modified"), "yyyyMMddHHmmss", null);
                    string folder = "";
                    try
                    {
                        folder = Path.GetDirectoryName(path);
                    }
                    catch (Exception)
                    {
                        // Couldn't get directory name...
                    }

                    ListViewItem item = new ListViewItem(new string[] { null, filename, (scoreDoc.Score * 100).ToString("N0"), modified.ToShortDateString() + " " + modified.ToShortTimeString(), folder });
                    item.Tag = path;
                    try
                    {
                        item.ImageIndex = imageListDocuments.IconIndex(filename);
                    }
                    catch (Exception)
                    {
                        // Couldn't get icon...
                    }
                    searchWorker.ReportProgress(0, item);
                }
                searcher.Dispose();
            };

            searchWorker.ProgressChanged += delegate(object sender, ProgressChangedEventArgs e)
            {
                this.listViewResults.Items.Add((ListViewItem)e.UserState);
            };
            searchWorker.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs e)
            {
                if (e.Error != null)
                {
                    this.labelSearch.Text = e.Error.Message;
                }
                else
                {
                    if (queryHistory != "" && hitCount > 0 && searchTerms.Contains(queryHistory) == false)
                        searchTerms.Add(queryHistory);
                    this.labelSearch.Text = String.Format("Search took {0}. Found {1} items.", (DateTime.Now - start), hitCount);
                }
                buttonSearch.Enabled = true;
            };

            searchWorker.RunWorkerAsync();
		}

		private void listViewResults_DoubleClick(object sender, System.EventArgs e)
		{
            if (this.listViewResults.SelectedItems.Count != 1)
				return;

			string path = (string) this.listViewResults.SelectedItems[0].Tag;
            OpenFile(path);
		}

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewResults.SelectedItems.Count > 0)
            {
                foreach (ListViewItem item in listViewResults.SelectedItems)
                {
                    string path = (string)item.Tag;
                    OpenFile(path);
                }
            }
        }

        private void openContainingFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewResults.SelectedItems.Count > 0)
            {
                foreach (ListViewItem item in listViewResults.SelectedItems)
                {
                    string path = (string)item.Tag;
                    path = Path.GetDirectoryName(path);
                    OpenDirectory(path);
                }
            }
        }

        void OpenFile(string path)
        {
            // Loop through each search directory and see if the file exists
            foreach (string searchDir in searchDirs)
            {
                // Remove starting slash in old index files
                if (path.StartsWith(@"\"))
                    path = path.Substring(1);

                string fullPath = Path.Combine(searchDir, path);
                if (File.Exists(fullPath))
                {
                    Process.Start(fullPath);
                    return;
                }
            }

            // Didn't find it so return a message
            MessageBox.Show("The file no longer exists, rebuild the index", "Deleted or Moved", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        void OpenDirectory(string path)
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

		private void textBoxQuery_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				Search();

		}

		private void buttonClean_Click(object sender, System.EventArgs e)
		{
			System.IO.Directory.Delete(this.pathIndex, true);
			checkIndex();
		}

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://lucene.apache.org/java/2_4_0/queryparsersyntax.html");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

            SaveSearchTerms(searchTerms);

            if (indexWorker != null && indexWorker.IsBusy)
            {
                e.Cancel = true;
                indexWorker.CancelAsync();
                status = "Waiting for index to cancel";
                labelStatus.Text = status;

                Timer t = new Timer();
                t.Interval = 100;
                t.Tick += delegate(object sender1, EventArgs e1)
                {
                    if (!(indexWorker != null && indexWorker.IsBusy))
                    {
                        t.Stop();
                        this.Close();
                    }
                };
                t.Start();
            }
        }

        private void buttonRefreshIndex_Click(object sender, EventArgs e)
        {
            Index();
        }


        bool start = true;
        private void Form1_Activated(object sender, EventArgs e)
        {
            if (start)
            {
                textBoxQuery.Focus();
                start = false;
            }
        }

        private void pictureBoxCredits_Click(object sender, EventArgs e)
        {
            string message = "Application based on Dan Letecky's Desktop Search\n";
            message += "  http://www.codeproject.com/KB/office/desktopsearch1.aspx\n";
            message += "\n";
            message += "IFilter implementation from alex_zero\n";
            message += "  http://www.codeproject.com/KB/cs/IFilterReader.aspx";

            MessageBox.Show(message, "Credits", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private AutoCompleteStringCollection LoadSearchTerms()
        {
            AutoCompleteStringCollection result = new AutoCompleteStringCollection();

            if (File.Exists(searchTermsPath))
            {
                try
                {
                    using (var fileReader = new StreamReader(searchTermsPath))
                    {
                        string line;
                        while ((line = fileReader.ReadLine()) != null)
                        {
                            result.Add(line);
                        }
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Unable to load search history", "History", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }

            return result;

        }

        private void SaveSearchTerms(AutoCompleteStringCollection items)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(searchTermsPath))
                {
                    foreach (string item in items)
                    {
                        writer.WriteLine(item);
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to save search history", "History", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

        }

        private void listViewResults_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListViewItemComparer.ColumnDataType columnDataType = ListViewItemComparer.ColumnDataType.Generic;
            if (e.Column == colHeaderModified.Index)
            {
                columnDataType = ListViewItemComparer.ColumnDataType.DateTime;
            }
            else if (e.Column == columnHeaderScore.Index)
            {
                columnDataType = ListViewItemComparer.ColumnDataType.Number;
            }
            else
            {
                columnDataType = ListViewItemComparer.ColumnDataType.Generic;
            }

            if (listViewResults.ListViewItemSorter == null)
            {
                listViewResults.ListViewItemSorter = new ListViewItemComparer(
                    listViewResults.Columns.Count, e.Column, ListSortDirection.Ascending, columnDataType);
                // when you set ListViewItemSorter, sorting happens automatically
            }
            else
            {
                ((ListViewItemComparer)listViewResults.ListViewItemSorter).SetColumnAndType(e.Column, columnDataType);
                listViewResults.Sort(); // must explicitly sort in this case
            }
        }

        private void buttonToday_Click(object sender, EventArgs e)
        {
            dateTimePickerFrom.Value = DateTime.Today;
        }
	}
}
