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
    #endregion

    [TestClass]
    public class QlikConnectorPSExecuteTest
    {
        private string Command { get; } = "Get-Process | Select-Object ProcessName, Handles, ID";
        private string SignString { get; } = "LxLLGoR2J6CiMcDbalMKtTgfvVmbkrtJIoLSVFZL1Mj37vUzhYeNBfo0+M1tpGn8hIEKw0wFKnwZ\r\n1h+p+HgLNA34eWw/cYSqqs4zuBh6FEIW738dWlFg4xEG3GeKoCFt5mpaKzZbQoKMYFV0JcTv0rIO\r\nXeeLpZR4UcJ4CAUTLpM6Dw3PmwIHQ05+v3txMZBSojSRpdpfJZ9Thps/5idyCG6VenXiPfU4ktQ7\r\n5HTFK8HBjL+DHKHPgcTsLXMskZVunerDBvE7kQjNCWD6bYgUEyirWOK1sNq+cHJg2j5LWWZ5QgOk\r\n/gF0g1UOl0CVPDM0phRuca4Yp3oT6VArpjcwGw==";
        private string SignStringWrong { get; } = "JEEH+B2Nu6Me0IQlJhPJuTDF93cKRRoen7Zhc7SvD30CGbJcQ6NCcjDjwzLyJ7kHiEhabIdSlLch\r\nFhvIYhirIh7OkNPCqHazVcKK4VtrFvutzQfFlq+bAw063HSdJ4WCjTsl1brUj3dn60EAhh7L4pMQ\r\ndzbUTsJwrAGUO/CjjVuriPTH4d1aFZWcwTDLje5QGcJmFeEWzOtNHfW2toBDpC3aykQ10ZaDCn1t\r\nwSW/0MJcPpd18tBBnsvBnFHO1us7J0fc9BzlrleSprj8baUxEb07gvs9rDDQZTonVT91EyZ3qzm4\r\nbuhXBino1jzb5P9p1Pp8VVG8us8CSZDIvw4nfw==";

        private PSExecuteConnection TestPSExecute(string script_text)
        {
            var server = new PSExecuteServer();
            var conn = server.CreateConnection() as PSExecuteConnection;
            conn.ExtractQuery(script_text, new List<QvxTable>());
            return conn;
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

        [TestCategory("ScriptTest"), TestMethod]
        public void ScriptWithArguments()
        {
            var result = TestScript($"PSEXECUTE({{arg1:\"Hallo\", arg2:\"test\"}})\r\n{Command}\r\nSHA256:\r\n{SignString};");
            Assert.AreEqual(result, true);
        }

        [TestCategory("ScriptTest"), TestMethod]
        public void ScriptWithBreaksAndTabs()
        {
            var result = TestScript($"PSEXECUTE()\r\n\t\t{Command}\r\n\t\tSHA256:\r\n{SignString};");
            Assert.AreEqual(result, true);
        }

        [TestCategory("ScriptTest"), TestMethod]
        public void ScriptWithUnixBreaks()
        {
            var result = TestScript($" PSEXECUTE()\n  {Command}\n SHA256:\n{SignString};");
            Assert.AreEqual(result, true);
        }

        [TestCategory("ScriptTest"), TestMethod]
        public void ScriptWithNoArguments()
        {
            var result = TestScript($" PSEXECUTE()\r\n  {Command}\r\n\r\n SHA256:\r\n{SignString};");
            Assert.AreEqual(result, true);
        }

        [TestCategory("ScriptTest"), TestMethod]
        public void ScriptWithPreTabs()
        {
            var result = TestScript($"\r\n\t\t PSEXECUTE()\r\n  {Command}\r\n\r\n SHA256:\r\n{SignString};");
            Assert.AreEqual(result, true);
        }

        [TestCategory("ScriptTest"), TestMethod]
        public void ScriptWithMoreBreaksAndArguments()
        {
            var result = TestScript($" \r\n\r\n\r\n\r\n\r\nPSEXECUTE({{arg1:\"Hallo\", arg2:\"test\"}})\r\n{Command}\n\r\n SHA256:\r\n{SignString};\r\n \r\n \r\n \r\n \r\n ");
            Assert.AreEqual(result, true);
        }

        [TestCategory("ScriptTest"), TestMethod]
        public void ScriptWithWrongSign()
        {
            var result = TestScript($"PSEXECUTE({{arg1:\"Hallo\", arg2:\"test\"}})\r\nGet-Process | Select-Object ProcessName, Handles\r\nSHA256:\r\n{SignString};");
            Assert.AreEqual(result, false);
        }

        [TestCategory("ScriptTest"), TestMethod]
        public void ScriptWithoutSign()
        {
            var result = TestScript($"PSEXECUTE({{arg1:\"Hallo\", arg2:\"test\"}})\r\nGet-Process | Select-Object ProcessName, Handles");
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
            var script = $" \r\n\r\n\r\n\r\n\r\nPSEXECUTE\r\n{Command}\n\r\nmehre unbekannte befehle,.-..\r\nSHA256:\r\n{SignStringWrong};\r\n \r\n \r\n \r\n \r\n ";
            var server = new PSExecuteServer();
            var conn = server.CreateConnection();
            conn.MParameters = new Dictionary<string, string>();
            conn.Init();
            conn.ExtractQuery(script, new List<QvxTable>());
        }

        [TestCategory("PowerShellTest"), TestMethod]
        public void CheckTestConnectionWithCredentials()
        {
            var script = $"PSEXECUTE({{arg1:\"Hallo\", arg2:\"test\"}})\r\n{Command}\r\nSHA256:\r\n{SignString};";
            var server = new PSExecuteServer();
            var conn = server.CreateConnection();
            conn.MParameters = new Dictionary<string, string>();
            conn.MParameters.Add("userid", "test1");
            conn.MParameters.Add("password", "test1");
            conn.Init();
            conn.ExtractQuery(script, new List<QvxTable>());
        }

        [TestCategory("PowerShellTest"), TestMethod]
        public void CheckTestConnectionWithoutCredentials()
        {
            var script = $"PSEXECUTE()\r\n{Command}\r\nSHA256:\r\n{SignString};";
            var server = new PSExecuteServer();
            var conn = server.CreateConnection();
            conn.MParameters = new Dictionary<string, string>();
            conn.Init();
            conn.ExtractQuery(script, new List<QvxTable>());
        }

        [TestCategory("PowerShellTest"), TestMethod]
        public void TestPowerShellCredentials()
        {
            var username = "test1";
            var password = "test1";

            using (var powerShell = PowerShell.Create())
            {
                Environment.CurrentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                var scriptBlock = ScriptBlock.Create(
                    //"New-Object PSObject -Property @{ UserName = $env:username; } | Select-Object UserName"
                    "Get-Process | Select-Object Id"
                    );

                powerShell.AddCommand("Start-Job");
                powerShell.AddParameter("ScriptBlock", scriptBlock);
                var secPass = new SecureString();
                Array.ForEach(password.ToArray(), secPass.AppendChar);
                powerShell.AddParameter("Credential", new PSCredential(username, secPass));
                powerShell.AddCommand("Wait-Job");
                powerShell.AddCommand("Receive-Job");

                var results = powerShell.Invoke();
                var errors = powerShell.Streams.Error.ReadAll();
                Assert.AreEqual(errors.Count, 0);
                Assert.AreEqual(results[0].ToString(), "test1");
            }
        }

    }
}
