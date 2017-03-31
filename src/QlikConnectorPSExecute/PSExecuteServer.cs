namespace QlikConnectorPSExecute
{
    #region Usings
    using System;
    using QlikView.Qvx.QvxLibrary;
    using System.Diagnostics;
    using System.IO;
    using SimpleImpersonation;
    using System.Security.Principal;
    #endregion

    //Console.WriteLine("ParentPid: " + Process.GetProcessById(6972).Parent().Id); 
    //https://www.nuget.org/packages/SimpleImpersonation 
    //%appdata%\..\Local\Programs\Common Files\Qlik\Custom Data\

    public class PSExecuteServer : QvxServer
    {
        #region Methods
        public override string CustomCaption
        {
            get
            {
                return "CustomCaption";
            }
        }

        public override QvxConnection CreateConnection()
        {
            return new PSExecuteConnection();
        }

        public override string CreateConnectionString()
        {
            return "";
        }

        public override string HandleJsonRequest(string method, string[] userParameters, QvxConnection connection)
        {
            string provider, host, username, password;
            connection.MParameters.TryGetValue("provider", out provider);
            connection.MParameters.TryGetValue("userid", out username);
            connection.MParameters.TryGetValue("password", out password);
            connection.MParameters.TryGetValue("host", out host);

            QvDataContractResponse response;

            switch (method)
            {
                case "testConnection":
                    response = TestConnection(userParameters[0], userParameters[1]);
                    break;
                default:
                    response = new Info { qMessage = "Unknown command" };
                    break;
            }

            return ToJson(response);
        }

        public bool VerifyCredentials(string username, string password)
        {
            try
            {
                if (username == "" && password == "")
                    return true;

                Impersonation.LogonUser(Environment.MachineName, username, password, LogonType.Interactive);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public QvDataContractResponse TestConnection(string username, string password)
        {
            var message = "Credentials FAIL";
            if (VerifyCredentials(username, password))
            {
                message = "Credentials SUCCESS";
            }
            return new Info { qMessage = message };
        }
        #endregion
    }
}