
namespace Amazon_Stock_Tracker
{
    partial class FrmAbout
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmAbout));
            this.lblTitleVer = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lnkGitHub = new System.Windows.Forms.LinkLabel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rtxtLicense = new System.Windows.Forms.RichTextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.pnlButtonImage = new System.Windows.Forms.Panel();
            this.lblButton = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.pnlButtonImage.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitleVer
            // 
            this.lblTitleVer.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitleVer.Location = new System.Drawing.Point(8, 8);
            this.lblTitleVer.Name = "lblTitleVer";
            this.lblTitleVer.Size = new System.Drawing.Size(592, 16);
            this.lblTitleVer.TabIndex = 0;
            this.lblTitleVer.Text = "Amazon Stock Tracker v0.0.0.0";
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(8, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(592, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "Copyright (C) 2021 Steven Jenkins De Haro";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label2.Location = new System.Drawing.Point(8, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(592, 40);
            this.label2.TabIndex = 2;
            this.label2.Text = resources.GetString("label2.Text");
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label3.Location = new System.Drawing.Point(8, 93);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(200, 15);
            this.label3.TabIndex = 3;
            this.label3.Text = "C# source code is available on GitHub: ";
            // 
            // lnkGitHub
            // 
            this.lnkGitHub.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lnkGitHub.Location = new System.Drawing.Point(200, 93);
            this.lnkGitHub.Name = "lnkGitHub";
            this.lnkGitHub.Size = new System.Drawing.Size(400, 16);
            this.lnkGitHub.TabIndex = 4;
            this.lnkGitHub.TabStop = true;
            this.lnkGitHub.Text = "https://github.com/StevenJDH/Amazon-Stock-Tracker";
            this.lnkGitHub.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkGitHub_LinkClicked);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rtxtLicense);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.groupBox1.Location = new System.Drawing.Point(8, 120);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(592, 168);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "GNU General Public License";
            // 
            // rtxtLicense
            // 
            this.rtxtLicense.BackColor = System.Drawing.Color.White;
            this.rtxtLicense.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rtxtLicense.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.rtxtLicense.Location = new System.Drawing.Point(8, 16);
            this.rtxtLicense.Name = "rtxtLicense";
            this.rtxtLicense.ReadOnly = true;
            this.rtxtLicense.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rtxtLicense.Size = new System.Drawing.Size(576, 144);
            this.rtxtLicense.TabIndex = 0;
            this.rtxtLicense.Text = resources.GetString("rtxtLicense.Text");
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(224, 296);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(160, 32);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // pnlButtonImage
            // 
            this.pnlButtonImage.BackgroundImage = global::Amazon_Stock_Tracker.Properties.Resources.donation_button;
            this.pnlButtonImage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pnlButtonImage.Controls.Add(this.lblButton);
            this.pnlButtonImage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pnlButtonImage.Location = new System.Drawing.Point(8, 296);
            this.pnlButtonImage.Name = "pnlButtonImage";
            this.pnlButtonImage.Size = new System.Drawing.Size(112, 32);
            this.pnlButtonImage.TabIndex = 7;
            this.pnlButtonImage.Click += new System.EventHandler(this.pnlButtonImage_Click);
            // 
            // lblButton
            // 
            this.lblButton.BackColor = System.Drawing.Color.Orange;
            this.lblButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblButton.Location = new System.Drawing.Point(8, 8);
            this.lblButton.Name = "lblButton";
            this.lblButton.Size = new System.Drawing.Size(96, 16);
            this.lblButton.TabIndex = 8;
            this.lblButton.Text = "Donate...";
            this.lblButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblButton.Click += new System.EventHandler(this.lblButton_Click);
            // 
            // FrmAbout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(609, 337);
            this.Controls.Add(this.pnlButtonImage);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lnkGitHub);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblTitleVer);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmAbout";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About Amazon Stock Tracker";
            this.Load += new System.EventHandler(this.FrmAbout_Load);
            this.groupBox1.ResumeLayout(false);
            this.pnlButtonImage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblTitleVer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.LinkLabel lnkGitHub;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RichTextBox rtxtLicense;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Panel pnlButtonImage;
        private System.Windows.Forms.Label lblButton;
    }
}