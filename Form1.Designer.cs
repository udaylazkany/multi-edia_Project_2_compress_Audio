namespace project_2
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panel1 = new Panel();
            btnPlayPause = new Button();
            fileToolStripMenuItem = new ToolStripMenuItem();
            openToolStripMenuItem = new ToolStripMenuItem();
            compressToToolStripMenuItem = new ToolStripMenuItem();
            aDMSystemToolStripMenuItem = new ToolStripMenuItem();
            dMSystemToolStripMenuItem = new ToolStripMenuItem();
            nQSystemToolStripMenuItem = new ToolStripMenuItem();
            openCompressFileToolStripMenuItem = new ToolStripMenuItem();
            previewToolStripMenuItem = new ToolStripMenuItem();
            aDMPreviewToolStripMenuItem = new ToolStripMenuItem();
            dMPreviewToolStripMenuItem = new ToolStripMenuItem();
            nQPreviewToolStripMenuItem = new ToolStripMenuItem();
            menuStrip1 = new MenuStrip();
            resetToolStripMenuItem = new ToolStripMenuItem();
            saveToolStripMenuItem = new ToolStripMenuItem();
            panel1.SuspendLayout();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(btnPlayPause);
            panel1.Location = new Point(-2, 265);
            panel1.Name = "panel1";
            panel1.Size = new Size(803, 100);
            panel1.TabIndex = 0;
            // 
            // btnPlayPause
            // 
            btnPlayPause.Location = new Point(360, 30);
            btnPlayPause.Name = "btnPlayPause";
            btnPlayPause.Size = new Size(75, 23);
            btnPlayPause.TabIndex = 1;
            btnPlayPause.Text = "||";
            btnPlayPause.UseVisualStyleBackColor = true;
            btnPlayPause.Click += button2_Click;
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openToolStripMenuItem, compressToToolStripMenuItem, openCompressFileToolStripMenuItem, previewToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new Size(180, 22);
            openToolStripMenuItem.Text = "Open";
            openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            // 
            // compressToToolStripMenuItem
            // 
            compressToToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { aDMSystemToolStripMenuItem, dMSystemToolStripMenuItem, nQSystemToolStripMenuItem });
            compressToToolStripMenuItem.Name = "compressToToolStripMenuItem";
            compressToToolStripMenuItem.Size = new Size(180, 22);
            compressToToolStripMenuItem.Text = "Compress to";
            // 
            // aDMSystemToolStripMenuItem
            // 
            aDMSystemToolStripMenuItem.Name = "aDMSystemToolStripMenuItem";
            aDMSystemToolStripMenuItem.Size = new Size(142, 22);
            aDMSystemToolStripMenuItem.Text = "ADM System";
            aDMSystemToolStripMenuItem.Click += aDMSystemToolStripMenuItem_Click;
            // 
            // dMSystemToolStripMenuItem
            // 
            dMSystemToolStripMenuItem.Name = "dMSystemToolStripMenuItem";
            dMSystemToolStripMenuItem.Size = new Size(142, 22);
            dMSystemToolStripMenuItem.Text = "DM System";
            dMSystemToolStripMenuItem.Click += dMSystemToolStripMenuItem_Click;
            // 
            // nQSystemToolStripMenuItem
            // 
            nQSystemToolStripMenuItem.Name = "nQSystemToolStripMenuItem";
            nQSystemToolStripMenuItem.Size = new Size(142, 22);
            nQSystemToolStripMenuItem.Text = "NQ System";
            nQSystemToolStripMenuItem.Click += nQSystemToolStripMenuItem_Click;
            // 
            // openCompressFileToolStripMenuItem
            // 
            openCompressFileToolStripMenuItem.Name = "openCompressFileToolStripMenuItem";
            openCompressFileToolStripMenuItem.Size = new Size(180, 22);
            openCompressFileToolStripMenuItem.Text = "Open Compress File";
            openCompressFileToolStripMenuItem.Click += openCompressFileToolStripMenuItem_Click;
            // 
            // previewToolStripMenuItem
            // 
            previewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { aDMPreviewToolStripMenuItem, dMPreviewToolStripMenuItem, nQPreviewToolStripMenuItem });
            previewToolStripMenuItem.Name = "previewToolStripMenuItem";
            previewToolStripMenuItem.Size = new Size(180, 22);
            previewToolStripMenuItem.Text = "Preview ";
            // 
            // aDMPreviewToolStripMenuItem
            // 
            aDMPreviewToolStripMenuItem.Name = "aDMPreviewToolStripMenuItem";
            aDMPreviewToolStripMenuItem.Size = new Size(148, 22);
            aDMPreviewToolStripMenuItem.Text = "ADM Preview ";
            aDMPreviewToolStripMenuItem.Click += aDMPreviewToolStripMenuItem_Click;
            // 
            // dMPreviewToolStripMenuItem
            // 
            dMPreviewToolStripMenuItem.Name = "dMPreviewToolStripMenuItem";
            dMPreviewToolStripMenuItem.Size = new Size(148, 22);
            dMPreviewToolStripMenuItem.Text = "DM Preview ";
            dMPreviewToolStripMenuItem.Click += dMPreviewToolStripMenuItem_Click;
            // 
            // nQPreviewToolStripMenuItem
            // 
            nQPreviewToolStripMenuItem.Name = "nQPreviewToolStripMenuItem";
            nQPreviewToolStripMenuItem.Size = new Size(148, 22);
            nQPreviewToolStripMenuItem.Text = "NQ Preview ";
            nQPreviewToolStripMenuItem.Click += nQPreviewToolStripMenuItem_Click;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, resetToolStripMenuItem, saveToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(800, 24);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            menuStrip1.ItemClicked += menuStrip1_ItemClicked;
            // 
            // resetToolStripMenuItem
            // 
            resetToolStripMenuItem.Name = "resetToolStripMenuItem";
            resetToolStripMenuItem.Size = new Size(44, 20);
            resetToolStripMenuItem.Text = "reset";
            resetToolStripMenuItem.Click += resetToolStripMenuItem_Click;
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new Size(43, 20);
            saveToolStripMenuItem.Text = "Save";
            saveToolStripMenuItem.Click += saveToolStripMenuItem_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(panel1);
            Controls.Add(menuStrip1);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            panel1.ResumeLayout(false);
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panel1;
        private Button btnPlayPause;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem compressToToolStripMenuItem;
        private ToolStripMenuItem aDMSystemToolStripMenuItem;
        private ToolStripMenuItem openCompressFileToolStripMenuItem;
        private ToolStripMenuItem dMSystemToolStripMenuItem;
        private ToolStripMenuItem nQSystemToolStripMenuItem;
        private ToolStripMenuItem previewToolStripMenuItem;
        private ToolStripMenuItem aDMPreviewToolStripMenuItem;
        private ToolStripMenuItem dMPreviewToolStripMenuItem;
        private ToolStripMenuItem nQPreviewToolStripMenuItem;
        private ToolStripMenuItem resetToolStripMenuItem;
        private ToolStripMenuItem saveToolStripMenuItem;
    }
}
