using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QlikConnectorPSExecute
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Length <= 1)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new frmMain());
            }
            else
            {
                //Obejct erstellen
                var connector = new QlikConnector(args[1].ToString());
            }
        }
    }
}
