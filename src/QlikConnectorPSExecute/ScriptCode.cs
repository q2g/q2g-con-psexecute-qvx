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
                if (resultScript.Vaild())
                    return resultScript;
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw new Exception("The script is not valid.", ex);
            }
        }
        #endregion

        #region Methods
        private bool Vaild()
        {
            try
            {
                if (String.IsNullOrEmpty(OriginalScript))
                    throw new ArgumentException("The script is empty.");

                if (!OriginalScript.Contains(ScriptCode.Algorithm))
                    throw new ArgumentException("The signature was not found.");

                //Genauer Prüfen
                if (!OriginalScript.Contains(ScriptCode.ExecuteName))
                    throw new ArgumentException($"The command {ScriptCode.ExecuteName} was not found.");

                var text = OriginalScript.Trim().Replace("\r\n", "\n");
                Code = Regex.Replace(text, $"{ExecuteName}[^\n]*\n", "", RegexOptions.Singleline).Trim();
                if (Code.IndexOf(Algorithm) > -1)
                    Code = Code.Substring(0, Code.IndexOf(Algorithm)).Trim();

                var Signature = Manager.SignWithPrivateKey(Code, false, false, Algorithm);
                var fullSignature = $"{Algorithm}:\n{Signature}";

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

                return CryptoManager.IsValidPublicKey(Code, Signature, Manager.PublicKey);
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Variables & Properties
        public Dictionary<string, string> Parameters { get; private set; }
        public string Code { get; private set; }
        public string ScriptWithSign { get; private set; }
        public string Signature { get; private set; }
        public CryptoManager Manager { get; private set; }

        public static string ExecuteName { get; private set; } = "PSEXECUTE";
        public static string Algorithm { get; private set; } = "SHA256";
        
        private string OriginalScript { get; set; }
        private string PrivateKeyPath { get; set; } = @"C:\ProgramData\Qlik\Sense\Repository\Exported Certificates\.Local Certificates\server_key.pem";
        #endregion
    }
}
