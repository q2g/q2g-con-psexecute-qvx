namespace QlikConnectorPSExecute
{
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    #endregion

    public class PseLogger
    {
        #region Properties & Variables
        public static string LogPath { get; set; }
        #endregion

        #region Constructor
        private PseLogger() { }
        #endregion

        #region Static Methods
        public static PseLogger CreateLogger()
        {
            var connectorPath = Assembly.GetExecutingAssembly().Location;
            if(connectorPath.EndsWith("\\QlikConnectorPSExecute\\QlikConnectorPSExecute.exe"))
            {
                connectorPath = Path.Combine(Path.GetDirectoryName(connectorPath), "Log");
                Directory.CreateDirectory(connectorPath);
                connectorPath = Path.Combine(connectorPath, "Connector.log");
            }
            else
            {
                connectorPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Connector.log");
            }

            LogPath = connectorPath;
            return new PseLogger();
        }

        public static string GetFullExceptionString(Exception ex)
        {
            var sb = new StringBuilder(ex.Message);
            Exception currentEx = ex;
            while (currentEx.InnerException != null)
            {
                currentEx = currentEx.InnerException;
                if (currentEx != null)
                    sb.Append("\r\n" + currentEx.Message);
            }

            return sb.ToString();
        }
        #endregion

        #region Methods
        private string GetStamp()
        {
            var stemp = String.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now);
            return stemp;
        }

        private void Write(string message)
        {
            File.AppendAllText(LogPath, $"\r\n[{GetStamp()}] {message.Trim()}");
        }

        public void Error(Exception ex, string message)
        {
            var exMessage = GetFullExceptionString(ex);
            Write($"{message} - {exMessage}");
        }

        public void Warn(string message)
        {
            Write(message);
        }
        #endregion
    }
}