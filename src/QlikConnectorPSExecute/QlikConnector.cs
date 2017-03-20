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

        private void Init(string scriptText)
        {
            if (String.IsNullOrEmpty(scriptText))
                throw new Exception("The script is empty.");

            if (scriptText.Contains(ScriptCode.Algorithm) == false)
                throw new Exception("The signature was not found.");

            var script = new ScriptCode(scriptText);

            var qlikPrivateKey = @"C:\ProgramData\Qlik\Sense\Repository\Exported Certificates\.Local Certificates\server_key.pem";
            var manager = new CryptoManager(qlikPrivateKey);

            if (CryptoManager.IsValidPublicKey(script.Code, script.RawSignature, manager.PublicKey) == false)
                throw new Exception("The singnature of the script is invalid.");

            Script = script;
        }

        public string GetTable()
        {
            try
            {
                using (var powerShell = PowerShell.Create())
                {
                    powerShell.AddScript(Script.Code);
                    Script.Parameters.ToList().ForEach(p => powerShell.AddParameter(p.Key, p.Value));

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

        private ScriptCode Script { get; set; }

        private StringBuilder Errors { get; set; } = new StringBuilder();

        private CryptoManager Manager { get; set; }


        private string SignName { get; set; } = "PSSIGNATURE:";
        private string ExecuteName { get; set; } = "PSEXECUTE";
    }
}
