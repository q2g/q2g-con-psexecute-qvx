namespace QlikConnectorPSExecute
{
    #region Usings
    using System;
    using System.Windows.Forms;
    #endregion

    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args != null && args.Length >= 2)
            {
                new PSExecuteServer(args[2]).Run(args[0], args[1]);
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new frmMain());
            }
        }
    }
}
