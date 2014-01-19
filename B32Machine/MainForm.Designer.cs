namespace B32Machine
{
    partial class MainForm
    {
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
            this.msMainMenu = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timeDelayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.realTime0SecondsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mHz100NanoSecondsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mHz200NanoSecondsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mHz500NanoSecondsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mHz1000NanoSecondsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hz05SecondsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hz1SecondToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hz2SecondsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hz4SecondsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblRegisters = new System.Windows.Forms.Label();
            this.fdOpenB32File = new System.Windows.Forms.OpenFileDialog();
            this.b32Screen1 = new B32Machine.B32Screen();
            this.resumeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pauseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.restartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.msMainMenu.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // msMainMenu
            // 
            this.msMainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.msMainMenu.Location = new System.Drawing.Point(0, 0);
            this.msMainMenu.Name = "msMainMenu";
            this.msMainMenu.Size = new System.Drawing.Size(644, 24);
            this.msMainMenu.TabIndex = 1;
            this.msMainMenu.Text = "Menu";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.timeDelayToolStripMenuItem,
            this.resumeToolStripMenuItem,
            this.pauseToolStripMenuItem,
            this.restartToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.openToolStripMenuItem.Text = "&Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // timeDelayToolStripMenuItem
            // 
            this.timeDelayToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.realTime0SecondsToolStripMenuItem,
            this.mHz100NanoSecondsToolStripMenuItem,
            this.mHz200NanoSecondsToolStripMenuItem,
            this.mHz500NanoSecondsToolStripMenuItem,
            this.mHz1000NanoSecondsToolStripMenuItem,
            this.hz05SecondsToolStripMenuItem,
            this.hz1SecondToolStripMenuItem,
            this.hz2SecondsToolStripMenuItem,
            this.hz4SecondsToolStripMenuItem});
            this.timeDelayToolStripMenuItem.Name = "timeDelayToolStripMenuItem";
            this.timeDelayToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.timeDelayToolStripMenuItem.Text = "Time Delay";
            // 
            // realTime0SecondsToolStripMenuItem
            // 
            this.realTime0SecondsToolStripMenuItem.Name = "realTime0SecondsToolStripMenuItem";
            this.realTime0SecondsToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.realTime0SecondsToolStripMenuItem.Text = "Real Time (0 Seconds)";
            this.realTime0SecondsToolStripMenuItem.Click += new System.EventHandler(this.realTime0SecondsToolStripMenuItem_Click);
            // 
            // mHz100NanoSecondsToolStripMenuItem
            // 
            this.mHz100NanoSecondsToolStripMenuItem.Name = "mHz100NanoSecondsToolStripMenuItem";
            this.mHz100NanoSecondsToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.mHz100NanoSecondsToolStripMenuItem.Text = "10 MHz (100 NanoSeconds)";
            this.mHz100NanoSecondsToolStripMenuItem.Click += new System.EventHandler(this.mHz100NanoSecondsToolStripMenuItem_Click);
            // 
            // mHz200NanoSecondsToolStripMenuItem
            // 
            this.mHz200NanoSecondsToolStripMenuItem.Name = "mHz200NanoSecondsToolStripMenuItem";
            this.mHz200NanoSecondsToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.mHz200NanoSecondsToolStripMenuItem.Text = "5 MHz (200 NanoSeconds)";
            this.mHz200NanoSecondsToolStripMenuItem.Click += new System.EventHandler(this.mHz200NanoSecondsToolStripMenuItem_Click);
            // 
            // mHz500NanoSecondsToolStripMenuItem
            // 
            this.mHz500NanoSecondsToolStripMenuItem.Name = "mHz500NanoSecondsToolStripMenuItem";
            this.mHz500NanoSecondsToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.mHz500NanoSecondsToolStripMenuItem.Text = "2 MHz (500 NanoSeconds)";
            this.mHz500NanoSecondsToolStripMenuItem.Click += new System.EventHandler(this.mHz500NanoSecondsToolStripMenuItem_Click);
            // 
            // mHz1000NanoSecondsToolStripMenuItem
            // 
            this.mHz1000NanoSecondsToolStripMenuItem.Name = "mHz1000NanoSecondsToolStripMenuItem";
            this.mHz1000NanoSecondsToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.mHz1000NanoSecondsToolStripMenuItem.Text = "1 MHz (1000 NanoSeconds)";
            this.mHz1000NanoSecondsToolStripMenuItem.Click += new System.EventHandler(this.mHz1000NanoSecondsToolStripMenuItem_Click);
            // 
            // hz05SecondsToolStripMenuItem
            // 
            this.hz05SecondsToolStripMenuItem.Name = "hz05SecondsToolStripMenuItem";
            this.hz05SecondsToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.hz05SecondsToolStripMenuItem.Text = "2 Hz (0.5 Seconds)";
            this.hz05SecondsToolStripMenuItem.Click += new System.EventHandler(this.hz05SecondsToolStripMenuItem_Click);
            // 
            // hz1SecondToolStripMenuItem
            // 
            this.hz1SecondToolStripMenuItem.Name = "hz1SecondToolStripMenuItem";
            this.hz1SecondToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.hz1SecondToolStripMenuItem.Text = "1 Hz (1 Second)";
            this.hz1SecondToolStripMenuItem.Click += new System.EventHandler(this.hz1SecondToolStripMenuItem_Click);
            // 
            // hz2SecondsToolStripMenuItem
            // 
            this.hz2SecondsToolStripMenuItem.Name = "hz2SecondsToolStripMenuItem";
            this.hz2SecondsToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.hz2SecondsToolStripMenuItem.Text = "0.5 Hz (2 Seconds)";
            this.hz2SecondsToolStripMenuItem.Click += new System.EventHandler(this.hz2SecondsToolStripMenuItem_Click);
            // 
            // hz4SecondsToolStripMenuItem
            // 
            this.hz4SecondsToolStripMenuItem.Name = "hz4SecondsToolStripMenuItem";
            this.hz4SecondsToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.hz4SecondsToolStripMenuItem.Text = "0.25 Hz (4 Seconds)";
            this.hz4SecondsToolStripMenuItem.Click += new System.EventHandler(this.hz4SecondsToolStripMenuItem_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblRegisters);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 302);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(644, 54);
            this.panel1.TabIndex = 2;
            // 
            // lblRegisters
            // 
            this.lblRegisters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRegisters.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRegisters.Location = new System.Drawing.Point(0, 0);
            this.lblRegisters.Name = "lblRegisters";
            this.lblRegisters.Size = new System.Drawing.Size(644, 54);
            this.lblRegisters.TabIndex = 3;
            this.lblRegisters.Text = "Registers";
            this.lblRegisters.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // fdOpenB32File
            // 
            this.fdOpenB32File.DefaultExt = "B32";
            this.fdOpenB32File.Filter = "B32 Files|*.B32";
            // 
            // b32Screen1
            // 
            this.b32Screen1.BackColor = System.Drawing.Color.Black;
            this.b32Screen1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.b32Screen1.Location = new System.Drawing.Point(0, 24);
            this.b32Screen1.Name = "b32Screen1";
            this.b32Screen1.ScreenMemoryLocation = ((ushort)(40960));
            this.b32Screen1.Size = new System.Drawing.Size(644, 332);
            this.b32Screen1.TabIndex = 0;
            // 
            // resumeToolStripMenuItem
            // 
            this.resumeToolStripMenuItem.Name = "resumeToolStripMenuItem";
            this.resumeToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.resumeToolStripMenuItem.Text = "Resume";
            this.resumeToolStripMenuItem.Click += new System.EventHandler(this.resumeToolStripMenuItem_Click);
            // 
            // pauseToolStripMenuItem
            // 
            this.pauseToolStripMenuItem.Name = "pauseToolStripMenuItem";
            this.pauseToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.pauseToolStripMenuItem.Text = "Pause";
            this.pauseToolStripMenuItem.Click += new System.EventHandler(this.pauseToolStripMenuItem_Click);
            // 
            // restartToolStripMenuItem
            // 
            this.restartToolStripMenuItem.Name = "restartToolStripMenuItem";
            this.restartToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.restartToolStripMenuItem.Text = "Restart";
            this.restartToolStripMenuItem.Click += new System.EventHandler(this.restartToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(644, 356);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.b32Screen1);
            this.Controls.Add(this.msMainMenu);
            this.MainMenuStrip = this.msMainMenu;
            this.Name = "MainForm";
            this.Text = "B32Machine";
            this.msMainMenu.ResumeLayout(false);
            this.msMainMenu.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private B32Screen b32Screen1;
        private System.Windows.Forms.MenuStrip msMainMenu;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.OpenFileDialog fdOpenB32File;
        private System.Windows.Forms.Label lblRegisters;
        private System.Windows.Forms.ToolStripMenuItem timeDelayToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem realTime0SecondsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mHz100NanoSecondsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mHz200NanoSecondsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mHz500NanoSecondsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mHz1000NanoSecondsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hz05SecondsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hz1SecondToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hz2SecondsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hz4SecondsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resumeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pauseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem restartToolStripMenuItem;
    }
}

