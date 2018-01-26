#region License
/*
Copyright (c) 2017 Konrad Mattheis und Martin Berthold
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
#endregion

namespace q2gconpsexecuteqvx
{
    #region Usings
    using System;
    using QlikView.Qvx.QvxLibrary;
    using System.Text.RegularExpressions;
    using System.Threading;
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
                    case "getVersion":
                        response = new Info { qMessage = GitVersionInformation.InformationalVersion };
                        break;
                    case "getUsername":
                        response = new Info { qMessage = connection.MParameters["UserId"]};
                        break;
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