namespace QlikConnectorPSExecute
{
    #region Usings
    using System;
    using QlikView.Qvx.QvxLibrary;
    #endregion

    public class PSExecuteServer : QvxServer
    {
        #region Methods
        public override QvxConnection CreateConnection()
        {
            return new PSExecuteConnection();
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
                case "GetInfo":
                    response = GetInfo();
                    break;
                case "GetDatabases":
                    response = GetDatabases(username, password);
                    break;
                case "GetTables":
                    response = GetTables(username, password, connection, userParameters[0], userParameters[1]);
                    break;
                case "GetFields":
                    response = GetFields(username, password, connection, userParameters[0], userParameters[1], userParameters[2]);
                    break;
                case "TestConnection":
                    response = TestConnection(userParameters[0], userParameters[1]);
                    break;
                case "LoadScript":
                    response = LoadScript(userParameters[0], userParameters[1], userParameters[2], connection);
                    break;
                default:
                    response = new Info { qMessage = "Unknown command" };
                    break;
            }

            return ToJson(response);
        }

        public QvDataContractResponse LoadScript(string username, string passwort, string command, QvxConnection connection)
        {
            try
            {
                var res = "FAIL";
                if (username == "json" && passwort == "1q2w3e")
                {
                    var psconn = connection as PSExecuteConnection;
                    psconn.ScriptInit(command);
                    res = $"SUCCESS";
                }

                return new Info { qMessage = res };
            }
            catch (Exception ex)
            {
                return new Info { qMessage = GetMessages(ex) };
            }
        }

        private string GetMessages(Exception e)
        {
            var msgs = String.Empty;
            if (e == null) return String.Empty;
            if (msgs == "") msgs = e.Message;
            if (e.InnerException != null)
                msgs += $"\r\nInnerException: {GetMessages(e.InnerException)}";
            return msgs;
        }

        public bool VerifyCredentials(string username, string password)
        {
            return (username == "" && password == "") || (username == "json" && password == "1q2w3e");
        }

        public QvDataContractResponse GetInfo()
        {
            return new Info
            {
                qMessage = "Connector for Windows PowerShell. Run a PowerShell command."
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
    }
}
