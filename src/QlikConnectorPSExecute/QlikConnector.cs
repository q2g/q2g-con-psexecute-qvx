using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.X509;
using QlikConnect.Crypt;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QlikConnectorPSExecute
{
    public class QlikConnector
    {
        public QlikConnector(string script)
        {
            Init(script);
        }

        private void Init(string script)
        {
            if (String.IsNullOrEmpty(script))
                throw new Exception("The script is empty.");

            if (script.Contains(SignName) == false)
                throw new Exception("The signature was not found.");

            script = script.Replace("\r\n", "\n").Trim();
            var sign = script.Substring(script.IndexOf(SignName) + SignName.Length).Trim();
            var code = Regex.Replace(script, $"{ExecuteName}[^\n]+\n", "", RegexOptions.Singleline).Trim();
            code = code.Substring(0, code.IndexOf(SignName)).Trim();
            var args = Regex.Match(script, $"{ExecuteName}\\(({{[^}}]+}})\\)", RegexOptions.Singleline).Groups[1].Value;
            Parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(args);
            if (Parameters == null)
                Parameters = new Dictionary<string, string>();

            var qlikPublicKey = @"C:\ProgramData\Qlik\Sense\Repository\Exported Certificates\.Local Certificates\server_key.pem";
            var manager = new CryptoManager(qlikPublicKey);
            var src = "Demo 123456789";
            sign = manager.SignWithPrivateKey(src);

            if (CryptoManager.IsValidPublicKey(src, sign, manager.PublicKey) == false)
                throw new Exception("The singnature of the script is invalid.");

            qlikPublicKey = @"C:\ProgramData\Qlik\Sense\Repository\Exported Certificates\.Local Certificates\server.pem";
            var fileStream = File.OpenText(qlikPublicKey);
            var pemReader = new PemReader(fileStream);
            var certificate = (X509Certificate)pemReader.ReadObject();
            var publicKey = certificate.GetPublicKey() as RsaKeyParameters;

            if (CryptoManager.IsValidPublicKey(src, sign, publicKey) == false)
                throw new Exception("The singnature of the script is invalid.");

            Script = code;
        }

        public string GetTable()
        {
            try
            {
                using (var powerShell = PowerShell.Create())
                {
                    powerShell.AddScript(Script);
                    Parameters.ToList().ForEach(p => powerShell.AddParameter(p.Key, p.Value));

                    var results = powerShell.Invoke();
                        foreach (var psObject in results)
                    {
                        Console.WriteLine();
                        foreach (var p in psObject.Properties)
                        {
                            Console.WriteLine($"{p.Name} = {p.Value.ToString()}");
                        }
                    }

                    var errors = powerShell.Streams.Error.ReadAll();
                    foreach (var error in errors)
                    {
                        Errors.Append($"\n{error.Exception.Message}");
                    }
                }

                if (Errors.Length > 0)
                {
                    throw new Exception(Errors.ToString());
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("The PowerShell script can not be execute.", ex);
            }
        }

        private string Script { get; set; }
        private StringBuilder Errors { get; set; } = new StringBuilder();
        private CryptoManager Manager { get; set; }
        private Dictionary<string, string> Parameters { get; set; }

        private string SignName { get; set; } = "PSSIGNATURE:";
        private string ExecuteName { get; set; } = "PSEXECUTE";
    }
}
