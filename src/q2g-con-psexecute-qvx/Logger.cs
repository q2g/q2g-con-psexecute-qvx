#region License
/*
Copyright (c) 2017 Konrad Mattheis und Martin Berthold
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
            var connectorPath = Assembly.GetExecutingAssembly().Location.ToLowerInvariant();
            if(connectorPath.EndsWith("\\q2gconpsexecuteqvx\\q2gconpsexecuteqvx.exe"))
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