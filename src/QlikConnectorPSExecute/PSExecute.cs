namespace QlikConnectorPSExecute
{
    #region Usings
    using Newtonsoft.Json;
    using Org.BouncyCastle.Crypto;
    using Org.BouncyCastle.Crypto.Parameters;
    using Org.BouncyCastle.OpenSsl;
    using Org.BouncyCastle.X509;
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

    public class PSExecute
    {
        #region Constructor & Init
        public PSExecute(ScriptCode code)
        {
            Script = code;
        }
        #endregion

        #region Methods
        public Dictionary<string, List<string>> GetData()
        {
            try
            {
                var result = new Dictionary<string, List<string>>();
                using (var powerShell = PowerShell.Create())
                {
                    powerShell.AddScript(Script.Code);
                    Script.Parameters.ToList().ForEach(p => powerShell.AddParameter(p.Key, p.Value));

                    var results = powerShell.Invoke();
                    foreach (var psObject in results)
                    {
                        foreach (var p in psObject.Properties)
                        {
                            if (!result.ContainsKey(p.Name))
                            {
                                var values = new List<string>();
                                result.Add(p.Name, values);
                            }
                            else
                            {
                                result[p.Name].Add(p.Value.ToString());
                            }
                        }
                    }

                    var errors = powerShell.Streams.Error.ReadAll();
                    foreach (var error in errors)
                    {
                        Errors.Append($"\n{error.Exception.Message}");
                    }
                }

                if (Errors.Length > 0)
                    throw new Exception(Errors.ToString());

                return result;
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
