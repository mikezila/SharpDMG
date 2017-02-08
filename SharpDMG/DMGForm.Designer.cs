namespace SharpDMG
{
    partial class DMGForm
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
            this.consoleTextBox = new System.Windows.Forms.TextBox();
            this.stepButton = new System.Windows.Forms.Button();
            this.stepTenButton = new System.Windows.Forms.Button();
            this.stepHundredButton = new System.Windows.Forms.Button();
            this.stepThousandButton = new System.Windows.Forms.Button();
            this.memoryDumpTabs = new System.Windows.Forms.TabControl();
            this.ramView = new System.Windows.Forms.TabPage();
            this.ramDumpTextBox = new System.Windows.Forms.TextBox();
            this.vramDumpTab = new System.Windows.Forms.TabPage();
            this.vramDumpTextBox = new System.Windows.Forms.TextBox();
            this.dumpButton = new System.Windows.Forms.Button();
            this.breakButton = new System.Windows.Forms.Button();
            this.breakpointTextBox = new System.Windows.Forms.TextBox();
            this.memoryDumpTabs.SuspendLayout();
            this.ramView.SuspendLayout();
            this.vramDumpTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // consoleTextBox
            // 
            this.consoleTextBox.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.consoleTextBox.Location = new System.Drawing.Point(12, 320);
            this.consoleTextBox.Multiline = true;
            this.consoleTextBox.Name = "consoleTextBox";
            this.consoleTextBox.ReadOnly = true;
            this.consoleTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.consoleTextBox.Size = new System.Drawing.Size(251, 234);
            this.consoleTextBox.TabIndex = 0;
            // 
            // stepButton
            // 
            this.stepButton.Location = new System.Drawing.Point(12, 560);
            this.stepButton.Name = "stepButton";
            this.stepButton.Size = new System.Drawing.Size(75, 23);
            this.stepButton.TabIndex = 1;
            this.stepButton.Text = "Step";
            this.stepButton.UseVisualStyleBackColor = true;
            this.stepButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // stepTenButton
            // 
            this.stepTenButton.Location = new System.Drawing.Point(94, 561);
            this.stepTenButton.Name = "stepTenButton";
            this.stepTenButton.Size = new System.Drawing.Size(75, 23);
            this.stepTenButton.TabIndex = 2;
            this.stepTenButton.Text = "Step (10)";
            this.stepTenButton.UseVisualStyleBackColor = true;
            this.stepTenButton.Click += new System.EventHandler(this.stepTenButton_Click);
            // 
            // stepHundredButton
            // 
            this.stepHundredButton.Location = new System.Drawing.Point(176, 561);
            this.stepHundredButton.Name = "stepHundredButton";
            this.stepHundredButton.Size = new System.Drawing.Size(75, 23);
            this.stepHundredButton.TabIndex = 3;
            this.stepHundredButton.Text = "Step (100)";
            this.stepHundredButton.UseVisualStyleBackColor = true;
            this.stepHundredButton.Click += new System.EventHandler(this.stepHundredButton_Click);
            // 
            // stepThousandButton
            // 
            this.stepThousandButton.Location = new System.Drawing.Point(258, 561);
            this.stepThousandButton.Name = "stepThousandButton";
            this.stepThousandButton.Size = new System.Drawing.Size(71, 23);
            this.stepThousandButton.TabIndex = 4;
            this.stepThousandButton.Text = "Step (10k)";
            this.stepThousandButton.UseVisualStyleBackColor = true;
            this.stepThousandButton.Click += new System.EventHandler(this.stepThousandButton_Click);
            // 
            // memoryDumpTabs
            // 
            this.memoryDumpTabs.Controls.Add(this.ramView);
            this.memoryDumpTabs.Controls.Add(this.vramDumpTab);
            this.memoryDumpTabs.Location = new System.Drawing.Point(351, 12);
            this.memoryDumpTabs.Name = "memoryDumpTabs";
            this.memoryDumpTabs.SelectedIndex = 0;
            this.memoryDumpTabs.Size = new System.Drawing.Size(535, 571);
            this.memoryDumpTabs.TabIndex = 5;
            // 
            // ramView
            // 
            this.ramView.Controls.Add(this.ramDumpTextBox);
            this.ramView.Location = new System.Drawing.Point(4, 22);
            this.ramView.Name = "ramView";
            this.ramView.Padding = new System.Windows.Forms.Padding(3);
            this.ramView.Size = new System.Drawing.Size(527, 545);
            this.ramView.TabIndex = 0;
            this.ramView.Text = "RAM";
            this.ramView.UseVisualStyleBackColor = true;
            // 
            // ramDumpTextBox
            // 
            this.ramDumpTextBox.Location = new System.Drawing.Point(6, 6);
            this.ramDumpTextBox.Multiline = true;
            this.ramDumpTextBox.Name = "ramDumpTextBox";
            this.ramDumpTextBox.ReadOnly = true;
            this.ramDumpTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ramDumpTextBox.Size = new System.Drawing.Size(515, 516);
            this.ramDumpTextBox.TabIndex = 0;
            // 
            // vramDumpTab
            // 
            this.vramDumpTab.Controls.Add(this.vramDumpTextBox);
            this.vramDumpTab.Controls.Add(this.dumpButton);
            this.vramDumpTab.Location = new System.Drawing.Point(4, 22);
            this.vramDumpTab.Name = "vramDumpTab";
            this.vramDumpTab.Padding = new System.Windows.Forms.Padding(3);
            this.vramDumpTab.Size = new System.Drawing.Size(527, 545);
            this.vramDumpTab.TabIndex = 1;
            this.vramDumpTab.Text = "VRAM";
            this.vramDumpTab.UseVisualStyleBackColor = true;
            // 
            // vramDumpTextBox
            // 
            this.vramDumpTextBox.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.vramDumpTextBox.Location = new System.Drawing.Point(6, 36);
            this.vramDumpTextBox.Multiline = true;
            this.vramDumpTextBox.Name = "vramDumpTextBox";
            this.vramDumpTextBox.ReadOnly = true;
            this.vramDumpTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.vramDumpTextBox.Size = new System.Drawing.Size(515, 486);
            this.vramDumpTextBox.TabIndex = 0;
            // 
            // dumpButton
            // 
            this.dumpButton.Location = new System.Drawing.Point(6, 7);
            this.dumpButton.Name = "dumpButton";
            this.dumpButton.Size = new System.Drawing.Size(108, 23);
            this.dumpButton.TabIndex = 6;
            this.dumpButton.Text = "Dump Memory";
            this.dumpButton.UseVisualStyleBackColor = true;
            this.dumpButton.Click += new System.EventHandler(this.dumpButton_Click);
            // 
            // breakButton
            // 
            this.breakButton.Location = new System.Drawing.Point(270, 320);
            this.breakButton.Name = "breakButton";
            this.breakButton.Size = new System.Drawing.Size(75, 23);
            this.breakButton.TabIndex = 7;
            this.breakButton.Text = "Break At:";
            this.breakButton.UseVisualStyleBackColor = true;
            this.breakButton.Click += new System.EventHandler(this.breakButton_Click);
            // 
            // breakpointTextBox
            // 
            this.breakpointTextBox.Location = new System.Drawing.Point(270, 350);
            this.breakpointTextBox.Name = "breakpointTextBox";
            this.breakpointTextBox.Size = new System.Drawing.Size(75, 20);
            this.breakpointTextBox.TabIndex = 8;
            // 
            // DMGForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(898, 595);
            this.Controls.Add(this.breakpointTextBox);
            this.Controls.Add(this.breakButton);
            this.Controls.Add(this.memoryDumpTabs);
            this.Controls.Add(this.stepThousandButton);
            this.Controls.Add(this.stepHundredButton);
            this.Controls.Add(this.stepTenButton);
            this.Controls.Add(this.stepButton);
            this.Controls.Add(this.consoleTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "DMGForm";
            this.Text = "SharpDMG";
            this.memoryDumpTabs.ResumeLayout(false);
            this.ramView.ResumeLayout(false);
            this.ramView.PerformLayout();
            this.vramDumpTab.ResumeLayout(false);
            this.vramDumpTab.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox consoleTextBox;
        private System.Windows.Forms.Button stepButton;
        private System.Windows.Forms.Button stepTenButton;
        private System.Windows.Forms.Button stepHundredButton;
        private System.Windows.Forms.Button stepThousandButton;
        private System.Windows.Forms.TabControl memoryDumpTabs;
        private System.Windows.Forms.TabPage ramView;
        private System.Windows.Forms.TextBox ramDumpTextBox;
        private System.Windows.Forms.TabPage vramDumpTab;
        private System.Windows.Forms.TextBox vramDumpTextBox;
        private System.Windows.Forms.Button dumpButton;
        private System.Windows.Forms.Button breakButton;
        private System.Windows.Forms.TextBox breakpointTextBox;
    }
}

