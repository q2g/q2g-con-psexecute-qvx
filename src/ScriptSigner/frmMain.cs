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

                if (tbxSign.Text.Contains(ScriptCode.ExecuteName) == false)
                {
                    MsgBox.ShowWarning($"Please use a correct code with \"{ScriptCode.ExecuteName}\".");
                    return;
                }

                var script = new ScriptCode(tbxSign.Text);
                Clipboard.SetText(script.ScriptWithSign);
                tbxSign.Text = script.ScriptWithSign;
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(ex);
            }
        }
        #endregion
    }
}
