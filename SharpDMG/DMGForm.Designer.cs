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
            this.SuspendLayout();
            // 
            // consoleTextBox
            // 
            this.consoleTextBox.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.consoleTextBox.Location = new System.Drawing.Point(12, 325);
            this.consoleTextBox.Multiline = true;
            this.consoleTextBox.Name = "consoleTextBox";
            this.consoleTextBox.ReadOnly = true;
            this.consoleTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.consoleTextBox.Size = new System.Drawing.Size(281, 161);
            this.consoleTextBox.TabIndex = 0;
            // 
            // stepButton
            // 
            this.stepButton.Location = new System.Drawing.Point(12, 492);
            this.stepButton.Name = "stepButton";
            this.stepButton.Size = new System.Drawing.Size(75, 23);
            this.stepButton.TabIndex = 1;
            this.stepButton.Text = "Step";
            this.stepButton.UseVisualStyleBackColor = true;
            this.stepButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // stepTenButton
            // 
            this.stepTenButton.Location = new System.Drawing.Point(94, 493);
            this.stepTenButton.Name = "stepTenButton";
            this.stepTenButton.Size = new System.Drawing.Size(75, 23);
            this.stepTenButton.TabIndex = 2;
            this.stepTenButton.Text = "Step (10)";
            this.stepTenButton.UseVisualStyleBackColor = true;
            this.stepTenButton.Click += new System.EventHandler(this.stepTenButton_Click);
            // 
            // stepHundredButton
            // 
            this.stepHundredButton.Location = new System.Drawing.Point(176, 493);
            this.stepHundredButton.Name = "stepHundredButton";
            this.stepHundredButton.Size = new System.Drawing.Size(75, 23);
            this.stepHundredButton.TabIndex = 3;
            this.stepHundredButton.Text = "Step (100)";
            this.stepHundredButton.UseVisualStyleBackColor = true;
            this.stepHundredButton.Click += new System.EventHandler(this.stepHundredButton_Click);
            // 
            // DMGForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(329, 527);
            this.Controls.Add(this.stepHundredButton);
            this.Controls.Add(this.stepTenButton);
            this.Controls.Add(this.stepButton);
            this.Controls.Add(this.consoleTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "DMGForm";
            this.Text = "SharpDMG";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox consoleTextBox;
        private System.Windows.Forms.Button stepButton;
        private System.Windows.Forms.Button stepTenButton;
        private System.Windows.Forms.Button stepHundredButton;
    }
}

