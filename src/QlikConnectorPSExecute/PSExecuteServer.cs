namespace QlikConnectorPSExecute
{
    #region Usings
    using System;
    using QlikView.Qvx.QvxLibrary;
    using System.Diagnostics;
    using System.IO;
    using System.Security.Principal;
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
            return "";
        }

        public override string HandleJsonRequest(string method, string[] userParameters, QvxConnection connection)
        {          
            QvDataContractResponse response;

            switch (method)
            {                
                default:
                    response = new Info { qMessage = "Unknown command" };
                    break;
            }

            return ToJson(response);
        }             
        #endregion
    }
}