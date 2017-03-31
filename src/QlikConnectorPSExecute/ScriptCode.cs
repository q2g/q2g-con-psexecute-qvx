namespace QlikConnectorPSExecute
{
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;
    using System.Diagnostics;
    #endregion

    public class ScriptCode
    {
        #region Constructor & Load
        private ScriptCode(string script, bool create)
        {
            Manager = new CryptoManager(PrivateKeyPath);
            OriginalScript = script;
            CreateSign = create;
        }
        #endregion

        #region Static Methods
        public static ScriptCode Parse(string script)
        {
            try
            {
                var resultScript = new ScriptCode(script, false);
                resultScript.Read();
                return resultScript;
            }
            catch (Exception ex)
            {
                throw new Exception("The script is not valid.", ex);
            }
        }

        public static ScriptCode Create(string script)
        {
            try
            {
                var resultScript = new ScriptCode(script, true);
                resultScript.Read();
                return resultScript;
            }
            catch (Exception ex)
            {
                throw new Exception("The script is not valid.", ex);
            }
        }
        #endregion

        #region Methods
        private void Read()
        {
            try
            {
                if (String.IsNullOrEmpty(OriginalScript))
                    throw new ArgumentException("The script is empty.");

                if (!OriginalScript.Trim().StartsWith(ExecuteName))
                    throw new ArgumentException($"The command {ExecuteName} was not found.");

                var text = OriginalScript.Trim().Replace("\r\n", "\n");
                Code = Regex.Replace(text, $"{ExecuteName}[^\n]*\n", "", RegexOptions.Singleline).Trim();
                if (Code.IndexOf(Algorithm) > -1)
                    Code = Code.Substring(0, Code.IndexOf(Algorithm)).Trim();

                var signature = Manager.SignWithPrivateKey(Code, false, true, Algorithm);
                var fullSignature = $"{Algorithm}:\r\n{signature}";

                if (Code.IndexOf(Algorithm) > -1)
                    Code = Code.Substring(0, Code.IndexOf(Algorithm)).Trim();

                var originalWithoutSign = OriginalScript.Replace("\r\n", "\n");
                var sbreak = String.Empty;
                if (originalWithoutSign.IndexOf(Algorithm) > -1)
                {
                    originalWithoutSign = originalWithoutSign.Substring(0, originalWithoutSign.IndexOf(Algorithm));
                    if (originalWithoutSign.EndsWith("\n"))
                        ScriptWithSign = $"{originalWithoutSign.Replace("\n", "\r\n")}{fullSignature}";
                    else
                        ScriptWithSign = $"{originalWithoutSign.Replace("\n", "\r\n")}\r\n{fullSignature}";
                }
                else
                {
                    ScriptWithSign = $"{originalWithoutSign.Replace("\n", "\r\n")}\r\n{fullSignature}";
                }

                var args = Regex.Match(OriginalScript, $"{ExecuteName}\\(({{[^}}]+}})\\)", RegexOptions.Singleline).Groups[1].Value;
                Parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(args);
                if (Parameters == null)
                    Parameters = new Dictionary<string, string>();

                TableName = Regex.Match(text, $"({ExecuteName}[^\\)]*\\))", RegexOptions.Singleline).Groups[1].Value;
                if (String.IsNullOrEmpty(TableName))
                    TableName = ExecuteName;
            }
            catch (Exception ex)
            {
                throw new Exception("The script could not be read.", ex);
            }
        }

        public void CheckSignature()
        {
            var text = OriginalScript.Replace("\r\n", "\n");
            var signature = Regex.Match(text, $"{Algorithm}:\n([^;]*);", RegexOptions.Singleline).Groups[1].Value;
            signature = signature.Replace("\n", "\r\n");

            if (!CryptoManager.IsValidPublicKey(Code, signature, Manager.PublicKey))
                throw new Exception("The signature is not valid.");
        }

        #endregion

        #region Variables & Properties
        public Dictionary<string, string> Parameters { get; private set; }
        public string Code { get; private set; }
        public string ScriptWithSign { get; private set; }
        public string TableName { get; private set; }

        private string ExecuteName { get; set; } = "PSEXECUTE";
        private string Algorithm { get; set; } = "SHA256";
        private string OriginalScript { get; set; }
        private string PrivateKeyPath { get; set; } = @"C:\ProgramData\Qlik\Sense\Repository\Exported Certificates\.Local Certificates\server_key.pem";
        private CryptoManager Manager { get; set; }
        private bool CreateSign { get; set; }
        #endregion
    }

    //source: http://stackoverflow.com/questions/394816/how-to-get-parent-process-in-net-in-managed-way
    public static class ProcessExtensions
    {
        private static string FindIndexedProcessName(int pid)
        {
            var processName = Process.GetProcessById(pid).ProcessName;
            var processesByName = Process.GetProcessesByName(processName);
            string processIndexdName = null;

            for (var index = 0; index < processesByName.Length; index++)
            {
                processIndexdName = index == 0 ? processName : processName + "#" + index;
                var processId = new PerformanceCounter("Process", "ID Process", processIndexdName);
                if ((int)processId.NextValue() == pid)
                {
                    return processIndexdName;
                }
            }

            return processIndexdName;
        }

        private static Process FindPidFromIndexedProcessName(string indexedProcessName)
        {
            var parentId = new PerformanceCounter("Process", "Creating Process ID", indexedProcessName);
            return Process.GetProcessById((int)parentId.NextValue());
        }

        public static Process Parent(this Process process)
        {
            return FindPidFromIndexedProcessName(FindIndexedProcessName(process.Id));
        }
    }
}
