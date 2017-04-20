#region License
/*
Copyright (c) 2017 Konrad Mattheis und Martin Berthold
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
#endregion

namespace ConnectorTest
{
    #region Usings
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using QlikConnectorPSExecute;
    using System.IO;
    using ConnectorTest.Properties;
    using QlikView.Qvx.QvxLibrary;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using System.Management.Automation;
    using System.Security;
    using System.Linq;
    using System.Text;
    using System.Net;
    using System.Diagnostics;
    using Newtonsoft.Json.Linq;
    #endregion

    [TestClass]
    public class QlikConnectorPSExecuteTest
    {
        private string SignString { get; } = "LxLLGoR2J6CiMcDbalMKtTgfvVmbkrtJIoLSVFZL1Mj37vUzhYeNBfo0+M1tpGn8hIEKw0wFKnwZ\r\n1h+p+HgLNA34eWw/cYSqqs4zuBh6FEIW738dWlFg4xEG3GeKoCFt5mpaKzZbQoKMYFV0JcTv0rIO\r\nXeeLpZR4UcJ4CAUTLpM6Dw3PmwIHQ05+v3txMZBSojSRpdpfJZ9Thps/5idyCG6VenXiPfU4ktQ7\r\n5HTFK8HBjL+DHKHPgcTsLXMskZVunerDBvE7kQjNCWD6bYgUEyirWOK1sNq+cHJg2j5LWWZ5QgOk\r\n/gF0g1UOl0CVPDM0phRuca4Yp3oT6VArpjcwGw==";
        private string SignStringWrong { get; } = "JEEH+B2Nu6Me0IQlJhPJuTDF93cKRRoen7Zhc7SvD30CGbJcQ6NCcjDjwzLyJ7kHiEhabIdSlLch\r\nFhvIYhirIh7OkNPCqHazVcKK4VtrFvutzQfFlq+bAw063HSdJ4WCjTsl1brUj3dn60EAhh7L4pMQ\r\ndzbUTsJwrAGUO/CjjVuriPTH4d1aFZWcwTDLje5QGcJmFeEWzOtNHfW2toBDpC3aykQ10ZaDCn1t\r\nwSW/0MJcPpd18tBBnsvBnFHO1us7J0fc9BzlrleSprj8baUxEb07gvs9rDDQZTonVT91EyZ3qzm4\r\nbuhXBino1jzb5P9p1Pp8VVG8us8CSZDIvw4nfw==";
        private string SignStringArgs { get; } = "aW6fYJ1n+CBBaXpKVsE66jfqKmfzJ3k9/1CXiTw1CkWhkMKCrED0vU1poJOipam0K8mzg0fUFAp3+Y2ZJQXUmWYQZS/gr0isYOP1Ys0Po7dqfXq+npwC8C7xfhi8Wbil8xFF9pzwq+A17w8OUa2uhjk7ylg+U50Qye22YTO/YI+YpEr6UpoqCwrMvfMg+yCwp8kRVcRhBjuGTqOM2/GUfijUeEsV8zNvhoazIK2zluOmAQoSEhKhZ9YnpDduicsT1vW7HSNjkeez1BQlMzJXs5sys5XBKHrZp1rO0JmBV5NQ+bmQGyHi6rYkqAtDLSzBdiG26w6X7Dg/Wedl4LpU7w==";
        private string SignString2 { get; } = "aW6fYJ1n+CBBaXpKVsE66jfqKmfzJ3k9/1CXiTw1CkWhkMKCrED0vU1poJOipam0K8mzg0fUFAp3+Y2ZJQXUmWYQZS/gr0isYOP1Ys0Po7dqfXq+npwC8C7xfhi8Wbil8xFF9pzwq+A17w8OUa2uhjk7ylg+U50Qye22YTO/YI+YpEr6UpoqCwrMvfMg+yCwp8kRVcRhBjuGTqOM2/GUfijUeEsV8zNvhoazIK2zluOmAQoSEhKhZ9YnpDduicsT1vW7HSNjkeez1BQlMzJXs5sys5XBKHrZp1rO0JmBV5NQ+bmQGyHi6rYkqAtDLSzBdiG26w6X7Dg/Wedl4LpU7w==";

        private string Args { get; } = "[\"explorer\", \"rundll32\"]";

        private PSExecuteConnection TestPSExecute(string script_text)
        {
            var server = new PSExecuteServer();
            var conn = server.CreateConnection() as PSExecuteConnection;
            conn.ExtractQuery(script_text, new List<QvxTable>());
            return conn;
        }

        private string GetCommand(bool with_args)
        {
            if (with_args)
                return "Get-Process args[0], args[1] | Select-Object ProcessName, Handles, ID";

            return "Get-Process | Select-Object ProcessName, Handles, ID";
        }

        private CryptoManager CreateManager()
        {
            var keyFile = @"C:\ProgramData\Qlik\Sense\Repository\Exported Certificates\.Local Certificates\server_key.pem";
            return new CryptoManager(keyFile);
        }

        private ScriptCode CreateScript(string script_text, CryptoManager manager)
        {
            return ScriptCode.Parse(script_text);
        }

        private bool TestScript(string script_text)
        {
            var manager = CreateManager();
            var script = CreateScript(script_text, manager);
            var signature = script.GetSignature();
            return manager.IsValidPublicKey(script.Code, signature);
        }

        private NetworkCredential GetTestUserCredentials()
        {
            //The Standard User Account must be created manually.
            return new NetworkCredential("test1", "test1");
        }

        [TestCategory("ScriptTest"), TestMethod]
        public void ScriptWithArguments()
        {
            var result = TestScript($"PSEXECUTE({Args})\r\n{GetCommand(true)}\r\nSHA256:\r\n{SignStringArgs};");
            Assert.AreEqual(result, true);
        }

        [TestCategory("ScriptTest"), TestMethod]
        public void ScriptWithBreaksAndTabs()
        {
            var result = TestScript($"PSEXECUTE()\r\n\t\t{GetCommand(false)}\r\n\t\tSHA256:\r\n{SignString};");
            Assert.AreEqual(result, true);
        }

        [TestCategory("ScriptTest"), TestMethod]
        public void ScriptWithUnixBreaks()
        {
            var result = TestScript($" PSEXECUTE()\n  {GetCommand(false)}\n SHA256:\n{SignString};");
            Assert.AreEqual(result, true);
        }

        [TestCategory("ScriptTest"), TestMethod]
        public void ScriptWithoutBreak()
        {
            var result = TestScript($" PSEXECUTE() {GetCommand(false)}\n SHA256:\n{SignString};");
            Assert.AreEqual(result, false);
        }

        [TestCategory("ScriptTest"), TestMethod]
        public void ScriptWithNoArguments()
        {
            var result = TestScript($" PSEXECUTE()\r\n  {GetCommand(false)}\r\n\r\n SHA256:\r\n{SignString};");
            Assert.AreEqual(result, true);
        }

        [TestCategory("ScriptTest"), TestMethod]
        public void ScriptWithPreTabs()
        {
            var result = TestScript($"\r\n\t\t PSEXECUTE()\r\n  {GetCommand(false)}\r\n\r\n SHA256:\r\n{SignString};");
            Assert.AreEqual(result, true);
        }

        [TestCategory("ScriptTest"), TestMethod]
        public void ScriptWithMoreBreaksAndArguments()
        {
            var result = TestScript($" \r\n\r\n\r\n\r\n\r\nPSEXECUTE({Args})\r\n{GetCommand(true)}\n\r\n SHA256:\r\n{SignStringArgs};\r\n \r\n \r\n \r\n \r\n ");
            Assert.AreEqual(result, true);
        }

        [TestCategory("ScriptTest"), TestMethod]
        public void ScriptWithWrongSign()
        {
            var result = TestScript($"PSEXECUTE({Args})\r\nGet-Process | Select-Object ProcessName, Handles\r\nSHA256:\r\n{SignString};");
            Assert.AreEqual(result, false);
        }

        [TestCategory("ScriptTest"), TestMethod]
        public void ScriptWithoutSign()
        {
            var result = TestScript($"PSEXECUTE({Args})\r\nGet-Process | Select-Object ProcessName, Handles");
            Assert.AreEqual(result, false);
        }

        [TestCategory("ScriptTest"), TestMethod]
        public void ScriptWrongCommand()
        {
            var manager = CreateManager();
            var script = CreateScript($" PSEXCEUTE()\r\n  Get-Pocss | Select-Object ProcessName, Handles\r\n  \r\n  \r\n SHA256:\r\n{SignString};", manager);
            Assert.AreEqual(script, null);
        }

        [TestCategory("PowerShellTest"), TestMethod]
        public void ScriptWithUnknownArguments()
        {
            var script = $" \r\n\r\n\r\n\r\n\r\nPSEXECUTE\r\n{GetCommand(false)}\n\r\nmehre unbekannte befehle,.-..\r\nSHA256:\r\n{SignStringWrong};\r\n \r\n \r\n \r\n \r\n ";
            var server = new PSExecuteServer();
            var conn = server.CreateConnection();
            conn.MParameters = new Dictionary<string, string>();
            conn.Init();
            conn.ExtractQuery(script, new List<QvxTable>());
        }

        [TestCategory("PowerShellTest"), TestMethod]
        public void CheckTestConnectionWithCredentials()
        {
            //The Standard User Account must be created manually.
            var credentials = GetTestUserCredentials();

            var script = $"PSEXECUTE({Args})\r\n{GetCommand(true)}\r\nSHA256:\r\n{SignString};";
            var server = new PSExecuteServer();
            var conn = server.CreateConnection();
            conn.MParameters = new Dictionary<string, string>();
            conn.MParameters.Add("userid", credentials.UserName);
            conn.MParameters.Add("password", credentials.Password);
            conn.Init();
            conn.ExtractQuery(script, new List<QvxTable>());
            Assert.Inconclusive("The result will only be evaluated in the future.");
        }

        [TestCategory("PowerShellTest"), TestMethod]
        public void CheckTestConnectionWithoutCredentials()
        {
            var script = $"PSEXECUTE({Args})\r\n{GetCommand(true)}\r\nSHA256:\r\n{SignString2};";
            var server = new PSExecuteServer();
            var conn = server.CreateConnection();
            conn.MParameters = new Dictionary<string, string>();
            conn.Init();
            var table = conn.ExtractQuery(script, new List<QvxTable>());
            Assert.Inconclusive("The result will only be evaluated in the future.");
        }

        [TestCategory("PowerShellTest"), TestMethod]
        public void TestPowerShellCredentials()
        {
            //The Standard User Account must be created manually.
            var credentials = GetTestUserCredentials();

            var script = CreateScript($"PSEXECUTE()\r\n$env:username", null);
            using (var powerShell = PowerShell.Create())
            {
                Environment.CurrentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

                powerShell.AddCommand("Start-Job");

                var scriptBlock = ScriptBlock.Create(script.Code);
                powerShell.AddParameter("ScriptBlock", scriptBlock);
                var secPass = new SecureString();
                Array.ForEach(credentials.Password.ToArray(), secPass.AppendChar);
                powerShell.AddParameter("Credential", new PSCredential(credentials.UserName, secPass));
                powerShell.AddParameter("ArgumentList", script.Parameters);
                powerShell.AddCommand("Wait-Job");
                powerShell.AddCommand("Receive-Job");

                var results = powerShell.Invoke();
                var errors = powerShell.Streams.Error.ReadAll();
                Assert.AreEqual(errors.Count, 0);
                Assert.AreEqual(results[0].ToString(), credentials.UserName);
            }
        }

        [TestCategory("LoggerTest"), TestMethod]
        public void CreateLogger()
        {
            var logger = PseLogger.CreateLogger();
            logger.Warn("TestWarning");

            var subex = new Exception("SubExeption");
            var ex = new Exception("MainExeption", subex);
            logger.Error(ex, "TestError");
            logger.Error(ex, "TestError");
        }
    }
}