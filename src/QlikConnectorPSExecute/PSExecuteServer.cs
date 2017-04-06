namespace QlikConnectorPSExecute
{
    #region Usings
    using System;
    using QlikView.Qvx.QvxLibrary;
    #endregion

    public class PSExecuteServer : QvxServer
    {
        #region Logger
        private static PseLogger logger = PseLogger.CreateLogger();
        #endregion

        #region Methods      
        public override QvxConnection CreateConnection()
        {
            try
            {
                return new PSExecuteConnection();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "The connection could not be created.");
                return null;
            }
        }

        public override string CreateConnectionString()
        {
            try
            {
                return "";
            }
            catch (Exception ex)
            {
                logger.Error(ex, "The connection string could not be created.");
                return null;
            }
        }

        public override string HandleJsonRequest(string method, string[] userParameters, QvxConnection connection)
        {
            try
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
            catch (Exception ex)
            {
                logger.Error(ex, "The json could not be read.");
                return ToJson(new Info { qMessage = "Error" });
            }
        }
        #endregion
    }
}