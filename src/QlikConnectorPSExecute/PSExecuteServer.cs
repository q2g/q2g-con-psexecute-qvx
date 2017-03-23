namespace QlikConnectorPSExecute
{
    #region Usings
    using Newtonsoft.Json;
    using Org.BouncyCastle.Crypto;
    using Org.BouncyCastle.Crypto.Parameters;
    using Org.BouncyCastle.OpenSsl;
    using Org.BouncyCastle.X509;
    using QlikView.Qvx.QvxLibrary;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Text;
    #endregion

    public class PSExecuteServer : QvxServer
    {
        public PSExecuteServer(string script)
        {
            Script = ScriptCode.Parse(script);
        }

        public override QvxConnection CreateConnection()
        {
            return new PSExecuteConnection(Script);
        }

        public override string CreateConnectionString()
        {
            return "Server=localhost";
        }

        public override string HandleJsonRequest(string method, string[] userParameters, QvxConnection connection)
        {
            return base.HandleJsonRequest(method, userParameters, connection);
        }

        private ScriptCode Script { get; set; }
    }
}
