using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScriptSigner
{
    public class MsgBox
    {
        public static void ShowError(string msg)
        {
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void ShowError(Exception ex)
        {
            ShowError(ex.Message);
        }

        public static void ShowWarning(string msg)
        {
            MessageBox.Show(msg, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void ShowInfo(string msg)
        {
            MessageBox.Show(msg, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
