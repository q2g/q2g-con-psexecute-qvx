namespace QlikConnectorPSExecute
{
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;
    #endregion

    public class ScriptCode
    {
        #region Constructor & Load
        private ScriptCode(string script)
        {
            Manager = new CryptoManager(PrivateKeyPath);
            OriginalScript = script;
        }
        #endregion

        #region Static Methods
        public static ScriptCode Parse(string script)
        {
            try
            {
                var resultScript = new ScriptCode(script);
                resultScript.Vaild();
                return resultScript;
            }
            catch (Exception ex)
            {
                throw new Exception("The script is not valid.", ex);
            }
        }
        #endregion

        #region Methods
        private void Vaild()
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

                var signature = Manager.SignWithPrivateKey(Code, false, false, Algorithm);
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

                if (!CryptoManager.IsValidPublicKey(Code, signature, Manager.PublicKey))
                    throw new Exception("The signature is not valid.");
            }
            catch (Exception ex)
            {
                throw new Exception("The script could not be read.", ex);
            }
        }
        #endregion

        #region Variables & Properties
        public Dictionary<string, string> Parameters { get; private set; }
        public string Code { get; private set; }
        public string ScriptWithSign { get; private set; }

        private string ExecuteName { get; set; } = "PSEXECUTE";
        private string Algorithm { get; set; } = "SHA256";
        private string OriginalScript { get; set; }
        private string PrivateKeyPath { get; set; } = @"C:\ProgramData\Qlik\Sense\Repository\Exported Certificates\.Local Certificates\server_key.pem";
        private CryptoManager Manager { get; set; }
        #endregion
    }
}
