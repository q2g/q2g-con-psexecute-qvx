namespace QlikConnectorPSExecute
{
    #region Usings
    using System;
    using System.Windows.Forms;
    #endregion

    static class Program
    {
        //Doku weiterführen
        //Git-Komponente durschlesen
        //Connector im Server testen (Signierung)
        //Xcopy umbauen in Project file

        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args != null && args.Length >= 2)
            {
                new PSExecuteServer().Run(args[0], args[1]);
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
