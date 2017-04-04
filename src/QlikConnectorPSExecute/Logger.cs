using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QlikConnectorPSExecute
{
    public class Logger
    {
        private static string LogPath { get; set; }

        private Logger() { }

        public static Logger CreateLogger()
        {
            LogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Connector.log");
            return new Logger();
        }

        public void Error(Exception ex, string message)
        {
            var sb = new StringBuilder(ex.Message);
            Exception currentEx = ex;
            while (currentEx.InnerException != null)
            {
                currentEx = currentEx.InnerException;
                if (currentEx != null)
                    sb.AppendLine(currentEx.Message);
            }

            File.AppendAllText(LogPath, sb.ToString());
        }

        public void Warn(string message)
        {
            File.AppendAllText(LogPath, message);
        }
    }
}