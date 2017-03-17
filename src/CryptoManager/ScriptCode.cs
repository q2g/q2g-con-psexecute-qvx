namespace QlikConnect.Crypt
{
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;
    using QlikConnect.Crypt;
    #endregion

    public class ScriptCode
    {
        #region Constructor & Load
        public ScriptCode(string script)
        {
            OriginalScript = script;
            Load();
        }
       
        private void Load()
        {
            var text = OriginalScript.Trim().Replace("\r\n", "\n");
            Code = Regex.Replace(text, $"{ExecuteName}[^\n]*\n", "", RegexOptions.Singleline).Trim();
            if (Code.IndexOf(Algorithm) > -1)
                Code = Code.Substring(0, Code.IndexOf(Algorithm)).Trim();

            var qlikPrivateKey = @"C:\ProgramData\Qlik\Sense\Repository\Exported Certificates\.Local Certificates\server_key.pem";
            var manager = new CryptoManager(qlikPrivateKey);
            Signature = manager.SignWithPrivateKey(true, Code, true, Algorithm);
            RawSignature = manager.SignWithPrivateKey(false, Code, false, Algorithm);

            if (Code.IndexOf(Algorithm) > -1)
                Code = Code.Substring(0, Code.IndexOf(Algorithm)).Trim();

            var originalWithoutSign = OriginalScript.Replace("\r\n", "\n");
            var sbreak = String.Empty;
            if (originalWithoutSign.IndexOf(Algorithm) > -1)
            {
                originalWithoutSign = originalWithoutSign.Substring(0, originalWithoutSign.IndexOf(Algorithm));
                if(originalWithoutSign.EndsWith("\n"))
                  ScriptWithSign = $"{originalWithoutSign.Replace("\n", "\r\n")}{Signature}";
                else
                  ScriptWithSign = $"{originalWithoutSign.Replace("\n", "\r\n")}\r\n{Signature}";
            } 
            else
            {
                ScriptWithSign = $"{originalWithoutSign.Replace("\n", "\r\n")}\r\n{Signature}";
            }
                
            var args = Regex.Match(OriginalScript, $"{ExecuteName}\\(({{[^}}]+}})\\)", RegexOptions.Singleline).Groups[1].Value;
            Parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(args);
            if (Parameters == null)
                Parameters = new Dictionary<string, string>();
        }
        #endregion

        #region Variables & Properties
        public string OriginalScript { get; private set; }
        public Dictionary<string, string> Parameters { get; private set; }
        public string Code { get; private set; }
        public string Signature { get; private set; }
        public string ScriptWithSign { get; private set; }
        public string RawSignature { get; private set; }

        public static string ExecuteName { get; private set; } = "PSEXECUTE";
        public static string Algorithm { get; private set; } = "SHA-256";
        #endregion
    }
}
