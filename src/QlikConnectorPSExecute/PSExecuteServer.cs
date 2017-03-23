namespace QlikConnectorPSExecute
{
    #region Usings
    using System;
    using QlikView.Qvx.QvxLibrary;
    #endregion

    public class PSExecuteServer : QvxServer
    {
        #region Constructor
        public PSExecuteServer(string script)
        {
            Script = ScriptCode.Parse(script);
        }
        #endregion

        #region Methods
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
            string provider, host, username, password;
            connection.MParameters.TryGetValue("provider", out provider);
            connection.MParameters.TryGetValue("userid", out username);
            connection.MParameters.TryGetValue("password", out password);
            connection.MParameters.TryGetValue("host", out host);

            QvDataContractResponse response;

            switch (method)
            {
                case "getInfo":
                    response = GetInfo();
                    break;
                case "getDatabases":
                    response = GetDatabases(username, password);
                    break;
                case "getTables":
                    response = GetTables(username, password, connection, userParameters[0], userParameters[1]);
                    break;
                case "getFields":
                    response = GetFields(username, password, connection, userParameters[0], userParameters[1], userParameters[2]);
                    break;
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
            return (username == "" && password == "") || (username == "jsonbourne" && password == "everest");
        }

        public QvDataContractResponse GetInfo()
        {
            return new Info
            {
                qMessage = "Example connector for Windows Event Log. Use account sdk-user/sdk-password"
            };
        }

        public QvDataContractResponse GetDatabases(string username, string password)
        {
            if (VerifyCredentials(username, password))
            {
                return new QvDataContractDatabaseListResponse
                {
                    qDatabases = new Database[]
                    {
                        new Database {qName = "PSExecute"}
                    }
                };
            }
            return new Info { qMessage = "Credentials WRONG!" };
        }

        public QvDataContractResponse GetTables(string username, string password, QvxConnection connection, string database, string owner)
        {
            if (VerifyCredentials(username, password))
            {
                return new QvDataContractTableListResponse
                {
                    qTables = connection.MTables
                };
            }
            return new Info { qMessage = "Credentials WRONG!" };
        }

        public QvDataContractResponse GetFields(string username, string password, QvxConnection connection, string database, string owner, string table)
        {
            if (VerifyCredentials(username, password))
            {
                var currentTable = connection.FindTable(table, connection.MTables);
                return new QvDataContractFieldListResponse
                {
                    qFields = (currentTable != null) ? currentTable.Fields : new QvxField[0]
                };
            }
            return new Info { qMessage = "Credentials WRONG!" };
        }

        public QvDataContractResponse TestConnection(string username, string password)
        {
            var message = "Credentials WRONG!";
            if (VerifyCredentials(username, password))
            {
                message = "Credentials OK!";
            }
            return new Info { qMessage = message };
        }
        #endregion

        #region Properties & Variables
        private ScriptCode Script { get; set; }
        #endregion
    }
}
