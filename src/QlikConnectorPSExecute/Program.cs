namespace QlikConnectorPSExecute
{
    using Fclp;
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    #endregion

    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var args = Environment.GetCommandLineArgs();
            var fargs = new FluentCommandLineParser<AppArguments>();
            var result = fargs.Parse(args);
            if (result.HasErrors)
            {
                return;
            }

            if (result.EmptyArgs)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new frmMain());
            }
            else
            {
                var script = ScriptCode.Parse(fargs.Object.Script);
                var connector = new PSExecute(script);
            }
        }
    }

    public class AppArguments
    {
        public string Script { get; private set; }
    }
}
