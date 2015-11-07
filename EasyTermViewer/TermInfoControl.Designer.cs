﻿namespace EasyTermViewer
{
    partial class TermInfoControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.webControl = new System.Windows.Forms.WebBrowser();
            this.txtTermBaseInfo = new System.Windows.Forms.TextBox();
            this.txtID = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // webControl
            // 
            this.webControl.AllowNavigation = false;
            this.webControl.AllowWebBrowserDrop = false;
            this.webControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webControl.IsWebBrowserContextMenuEnabled = false;
            this.webControl.Location = new System.Drawing.Point(0, 31);
            this.webControl.MinimumSize = new System.Drawing.Size(20, 20);
            this.webControl.Name = "webControl";
            this.webControl.ScriptErrorsSuppressed = true;
            this.webControl.Size = new System.Drawing.Size(549, 464);
            this.webControl.TabIndex = 2;
            this.webControl.WebBrowserShortcutsEnabled = false;
            this.webControl.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.webControl_Navigating);
            this.webControl.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.webControl_PreviewKeyDown);
            // 
            // txtTermBaseInfo
            // 
            this.txtTermBaseInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTermBaseInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtTermBaseInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTermBaseInfo.Location = new System.Drawing.Point(0, 7);
            this.txtTermBaseInfo.Name = "txtTermBaseInfo";
            this.txtTermBaseInfo.ReadOnly = true;
            this.txtTermBaseInfo.Size = new System.Drawing.Size(480, 19);
            this.txtTermBaseInfo.TabIndex = 3;
            // 
            // txtID
            // 
            this.txtID.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtID.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.txtID.Location = new System.Drawing.Point(486, 7);
            this.txtID.Name = "txtID";
            this.txtID.Size = new System.Drawing.Size(60, 21);
            this.txtID.TabIndex = 4;
            this.txtID.Text = "<ID>";
            this.txtID.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // TermInfoControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtID);
            this.Controls.Add(this.txtTermBaseInfo);
            this.Controls.Add(this.webControl);
            this.Name = "TermInfoControl";
            this.Size = new System.Drawing.Size(549, 498);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.WebBrowser webControl;
        private System.Windows.Forms.TextBox txtTermBaseInfo;
        private System.Windows.Forms.Label txtID;
    }
}
