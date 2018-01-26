#region License
/*
Copyright (c) 2017 akquinet
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
#endregion

namespace q2gconpsexecuteqvx
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
            try
            {
                lblStatus.Text = "Status:";
                timer.Stop();
            }
            catch (Exception ex)
            {
                ShowStatus(ex.Message, false);
            }
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
                    ShowStatus($"The certificate file {keyFile} does not exist or has no authorization.");
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
