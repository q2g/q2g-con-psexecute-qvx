namespace QlikConnectorPSExecute
{
    partial class frmMain
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btnSign = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblSign = new System.Windows.Forms.Label();
            this.tbxSign = new System.Windows.Forms.TextBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // btnSign
            // 
            this.btnSign.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSign.Location = new System.Drawing.Point(12, 320);
            this.btnSign.Name = "btnSign";
            this.btnSign.Size = new System.Drawing.Size(120, 34);
            this.btnSign.TabIndex = 0;
            this.btnSign.Text = "Create &Sign";
            this.btnSign.UseVisualStyleBackColor = true;
            this.btnSign.Click += new System.EventHandler(this.btnSign_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(646, 320);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(120, 34);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "&Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lblSign
            // 
            this.lblSign.AutoSize = true;
            this.lblSign.Location = new System.Drawing.Point(8, 16);
            this.lblSign.Name = "lblSign";
            this.lblSign.Size = new System.Drawing.Size(103, 19);
            this.lblSign.TabIndex = 2;
            this.lblSign.Text = "Code Snippet";
            // 
            // tbxSign
            // 
            this.tbxSign.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxSign.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbxSign.Location = new System.Drawing.Point(12, 40);
            this.tbxSign.Multiline = true;
            this.tbxSign.Name = "tbxSign";
            this.tbxSign.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbxSign.Size = new System.Drawing.Size(754, 258);
            this.tbxSign.TabIndex = 3;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(148, 330);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(58, 19);
            this.lblStatus.TabIndex = 4;
            this.lblStatus.Text = "Status:";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(778, 364);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.tbxSign);
            this.Controls.Add(this.lblSign);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnSign);
            this.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ScriptSigner";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSign;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblSign;
        private System.Windows.Forms.TextBox tbxSign;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Timer timer;
    }
}

