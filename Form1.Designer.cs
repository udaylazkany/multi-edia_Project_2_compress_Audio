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
            button3 = new Button();
            btnPlayPause = new Button();
            button1 = new Button();
            fileToolStripMenuItem = new ToolStripMenuItem();
            openToolStripMenuItem = new ToolStripMenuItem();
            compressToToolStripMenuItem = new ToolStripMenuItem();
            aDMSystemToolStripMenuItem = new ToolStripMenuItem();
            dMSystemToolStripMenuItem = new ToolStripMenuItem();
            nQSystemToolStripMenuItem = new ToolStripMenuItem();
            openCompressFileToolStripMenuItem = new ToolStripMenuItem();
            menuStrip1 = new MenuStrip();
            progressBar1 = new ProgressBar();
            panel1.SuspendLayout();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(button3);
            panel1.Controls.Add(btnPlayPause);
            panel1.Controls.Add(button1);
            panel1.Location = new Point(-2, 265);
            panel1.Name = "panel1";
            panel1.Size = new Size(803, 100);
            panel1.TabIndex = 0;
            // 
            // button3
            // 
            button3.Location = new Point(582, 30);
            button3.Name = "button3";
            button3.Size = new Size(75, 23);
            button3.TabIndex = 2;
            button3.Text = "button3";
            button3.UseVisualStyleBackColor = true;
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
            // button1
            // 
            button1.Location = new Point(140, 30);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 0;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openToolStripMenuItem, compressToToolStripMenuItem, openCompressFileToolStripMenuItem });
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
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(800, 24);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            menuStrip1.ItemClicked += menuStrip1_ItemClicked;
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(23, 239);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(738, 20);
            progressBar1.TabIndex = 2;
            progressBar1.Visible = false;
            progressBar1.Click += progressBar1_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(progressBar1);
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
        private Button button3;
        private Button btnPlayPause;
        private Button button1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem compressToToolStripMenuItem;
        private ToolStripMenuItem aDMSystemToolStripMenuItem;
        private ToolStripMenuItem openCompressFileToolStripMenuItem;
        private ToolStripMenuItem dMSystemToolStripMenuItem;
        private ToolStripMenuItem nQSystemToolStripMenuItem;
        private ProgressBar progressBar1;
    }
}
