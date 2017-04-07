namespace QlikConnectorPSExecute
{
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    #endregion

    public class PseLogger
    {
        #region Properties & Variables
        private static string LogPath { get; set; }
        #endregion

        #region Constructor
        private PseLogger() { }
        #endregion

        #region Static Methods
        public static PseLogger CreateLogger()
        {
            LogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Connector.log");
            return new PseLogger();
        }
        #endregion

        #region Methods
        private string GetStamp()
        {
            var stemp = String.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now);
            return stemp;
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

            File.AppendAllText(LogPath, $"[{GetStamp()}] {sb.ToString()}");
        }

        public void Warn(string message)
        {
            File.AppendAllText(LogPath, $"[{GetStamp()}] {message}");
        }
        #endregion
    }
}