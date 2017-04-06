namespace QlikConnectorPSExecute
{
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    #endregion

    public partial class frmMain : Form
    {
        #region Constructor
        public frmMain()
        {
            InitializeComponent();
            timer.Tick += Timer_Tick;
            timer.Interval = 2500;
        }
        #endregion

        #region Methods
        private void ShowStatus(string message, bool use_delay = true)
        {
            lblStatus.Text = $"Status: {message}";
            if (use_delay)
                timer.Start();
        }
        #endregion

        #region Event-Handler
        private void Timer_Tick(object sender, EventArgs e)
        {
            lblStatus.Text = "Status:";
            timer.Stop();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                ShowStatus(ex.Message, false);
            }
        }

        private void btnSign_Click(object sender, EventArgs e)
        {
            try
            {
                var keyFile = @"C:\ProgramData\Qlik\Sense\Repository\Exported Certificates\.Local Certificates\server_key.pem";
                if(!File.Exists(keyFile))
                {
                    ShowStatus("The certificate file does not exist.");
                    return;
                }

                var manager = new CryptoManager(keyFile);
                var script = ScriptCode.Create(tbxSign.Text, manager);
                if(script != null)
                {
                    Clipboard.SetText(script.ScriptWithSign);
                    tbxSign.Text = script.ScriptWithSign;
                    ShowStatus("The code has been copied to the clipboard.");
                }
                else
                {
                    ShowStatus("The script is incorrect and could not be signed.");
                }
            }
            catch (Exception ex)
            {
                ShowStatus(ex.Message, false);
            }
        }
        #endregion
    }
}
