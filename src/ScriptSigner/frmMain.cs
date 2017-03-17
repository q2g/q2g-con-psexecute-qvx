namespace ScriptSigner
{
    #region Usings
    using QlikConnect.Crypt;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    #endregion

    public partial class frmMain : Form
    {
        #region Constructor
        public frmMain()
        {
            InitializeComponent();
        }
        #endregion

        #region Event-Handler
        private void btnClose_Click(object sender, EventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex);
            }
        }

        private void btnSign_Click(object sender, EventArgs e)
        {
            try
            {
                if(String.IsNullOrEmpty(tbxSign.Text))
                {
                    MsgBox.ShowWarning("Please enter or paste a text.");
                    return;
                }

                var code = tbxSign.Text.Trim().Replace("\r\n", "\n");
                if(code.Contains(ExecuteName) == false)
                {
                    MsgBox.ShowWarning($"Please use a correct code with \"{ExecuteName}\".");
                    return;
                }

                if (code.Contains(SignName))
                {
                    MsgBox.ShowWarning($"The source code is already signed.");
                    return;
                }

                var qlikPrivateKey = @"C:\ProgramData\Qlik\Sense\Repository\Exported Certificates\.Local Certificates\server_key.pem";
                var manager = new CryptoManager(qlikPrivateKey);

                code = Regex.Replace(code, $"({ExecuteName}[^\n]+)\n", "", RegexOptions.Singleline).Trim();
                var signature = manager.SignWithPrivateKey(SignName, code);
                code = $"{code}\n{signature}";
                code = code.Replace("\n", "\r\n");

                Clipboard.SetText(code);
                MsgBox.ShowInfo("The created script was copied to the clipboard.");
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex);
            }
        }
        #endregion

        #region Variables & Properties
        private string SignName { get; set; } = "PSSIGNATURE:";
        private string ExecuteName { get; set; } = "PSEXECUTE";
        #endregion
    }
}
