namespace Filtering_Rainfall_Asc
{
    partial class Provide_Data
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
            this.PolygonFIle = new System.Windows.Forms.TextBox();
            this.BrowsePolygon = new System.Windows.Forms.Button();
            this.BrowseASCFILES = new System.Windows.Forms.Button();
            this.Run = new System.Windows.Forms.Button();
            this.Close = new System.Windows.Forms.Button();
            this.ASCFILES = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.Duration = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // PolygonFIle
            // 
            this.PolygonFIle.Location = new System.Drawing.Point(31, 475);
            this.PolygonFIle.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.PolygonFIle.Name = "PolygonFIle";
            this.PolygonFIle.Size = new System.Drawing.Size(407, 22);
            this.PolygonFIle.TabIndex = 1;
            this.PolygonFIle.TextChanged += new System.EventHandler(this.PolygonFIle_TextChanged);
            // 
            // BrowsePolygon
            // 
            this.BrowsePolygon.Location = new System.Drawing.Point(175, 507);
            this.BrowsePolygon.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.BrowsePolygon.Name = "BrowsePolygon";
            this.BrowsePolygon.Size = new System.Drawing.Size(100, 28);
            this.BrowsePolygon.TabIndex = 2;
            this.BrowsePolygon.Text = "Browse";
            this.BrowsePolygon.UseVisualStyleBackColor = true;
            this.BrowsePolygon.Click += new System.EventHandler(this.BrowsePolygon_Click);
            // 
            // BrowseASCFILES
            // 
            this.BrowseASCFILES.Location = new System.Drawing.Point(175, 347);
            this.BrowseASCFILES.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.BrowseASCFILES.Name = "BrowseASCFILES";
            this.BrowseASCFILES.Size = new System.Drawing.Size(100, 28);
            this.BrowseASCFILES.TabIndex = 3;
            this.BrowseASCFILES.Text = "Browse";
            this.BrowseASCFILES.UseVisualStyleBackColor = true;
            this.BrowseASCFILES.Click += new System.EventHandler(this.BrowseASCFILES_Click);
            // 
            // Run
            // 
            this.Run.Location = new System.Drawing.Point(44, 549);
            this.Run.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Run.Name = "Run";
            this.Run.Size = new System.Drawing.Size(100, 28);
            this.Run.TabIndex = 4;
            this.Run.Text = "Run";
            this.Run.UseVisualStyleBackColor = true;
            this.Run.Click += new System.EventHandler(this.Run_Click);
            // 
            // Close
            // 
            this.Close.Location = new System.Drawing.Point(309, 549);
            this.Close.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Close.Name = "Close";
            this.Close.Size = new System.Drawing.Size(100, 28);
            this.Close.TabIndex = 5;
            this.Close.Text = "Close";
            this.Close.UseVisualStyleBackColor = true;
            this.Close.Click += new System.EventHandler(this.Close_Click);
            // 
            // ASCFILES
            // 
            this.ASCFILES.FormattingEnabled = true;
            this.ASCFILES.ItemHeight = 16;
            this.ASCFILES.Location = new System.Drawing.Point(31, 15);
            this.ASCFILES.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.ASCFILES.Name = "ASCFILES";
            this.ASCFILES.Size = new System.Drawing.Size(423, 324);
            this.ASCFILES.TabIndex = 6;
            this.ASCFILES.SelectedIndexChanged += new System.EventHandler(this.ASCFILES_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.label2.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label2.Location = new System.Drawing.Point(40, 607);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(338, 16);
            this.label2.TabIndex = 31;
            this.label2.Text = "For any issues Contact abhinav.goswami@arcadis.com";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 455);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 16);
            this.label1.TabIndex = 32;
            this.label1.Text = "Polygon File";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(175, 405);
            this.textBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(52, 22);
            this.textBox1.TabIndex = 33;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 405);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(160, 16);
            this.label4.TabIndex = 35;
            this.label4.Text = "Number of Data Needed :";
            // 
            // Duration
            // 
            this.Duration.Location = new System.Drawing.Point(355, 408);
            this.Duration.Margin = new System.Windows.Forms.Padding(4);
            this.Duration.Name = "Duration";
            this.Duration.Size = new System.Drawing.Size(52, 22);
            this.Duration.TabIndex = 36;
            this.Duration.TextChanged += new System.EventHandler(this.Duration_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(284, 411);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 16);
            this.label3.TabIndex = 37;
            this.label3.Text = "Duration :";
            // 
            // Provide_Data
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(492, 634);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Duration);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ASCFILES);
            this.Controls.Add(this.Close);
            this.Controls.Add(this.Run);
            this.Controls.Add(this.BrowseASCFILES);
            this.Controls.Add(this.BrowsePolygon);
            this.Controls.Add(this.PolygonFIle);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "Provide_Data";
            this.Text = "Provide_Data";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox PolygonFIle;
        private System.Windows.Forms.Button BrowsePolygon;
        private System.Windows.Forms.Button BrowseASCFILES;
        private System.Windows.Forms.Button Run;
        private System.Windows.Forms.Button Close;
        private System.Windows.Forms.ListBox ASCFILES;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox Duration;
        private System.Windows.Forms.Label label3;
    }
}