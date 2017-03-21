namespace QlikConnectorPSExecute
{
    #region Usings
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
    #endregion

    public interface IView
    {

    }

    public class QlikConnector
    {
        #region Constructor & Init
        public QlikConnector(string script)
        {
            Init(script);
        }

        private void Init(string scriptText)
        {
            if (String.IsNullOrEmpty(scriptText))
                throw new ArgumentException("The script is empty.");

            if (scriptText.Contains(ScriptCode.Algorithm) == false)
                throw new ArgumentException("The signature was not found.");

            if (scriptText.Contains(ScriptCode.ExecuteName) == false)
                throw new ArgumentException($"The command {ScriptCode.ExecuteName} was not found.");

            var script = new ScriptCode(scriptText);

            var qlikPrivateKey = @"C:\ProgramData\Qlik\Sense\Repository\Exported Certificates\.Local Certificates\server_key.pem";
            var manager = new CryptoManager(qlikPrivateKey);
            
            
            if (CryptoManager.IsValidPublicKey(script.Code, script.RawSignature, manager.PublicKey) == false)
                throw new Exception("The singnature of the script is invalid.");

            Script = script;
        }
        #endregion

        #region Methods
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
                        foreach (var p in psObject.Properties)
                        {
                            //Füllen
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
                throw new PowerShellException("The PowerShell script can not be execute.", ex);
            }
        }
        #endregion

        #region Properties & Variables
        private ScriptCode Script { get; set; }
        private StringBuilder Errors { get; set; } = new StringBuilder();
        private CryptoManager Manager { get; set; }

        private string SignName { get; set; } = "PSSIGNATURE:";
        private string ExecuteName { get; set; } = "PSEXECUTE";
        #endregion
    }

    #region Exception Classes
    public class PowerShellException : Exception
    {
        public PowerShellException(string message, Exception ex) : base(message, ex) { }
    }
    #endregion
}
