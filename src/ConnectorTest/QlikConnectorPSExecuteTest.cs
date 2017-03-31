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
    #endregion

    [TestClass]
    public class QlikConnectorPSExecuteTest
    {
        private string Command { get; } = "Get-Process | Select-Object ProcessName, Handles, ID";
        private string SignString { get; } = "LxLLGoR2J6CiMcDbalMKtTgfvVmbkrtJIoLSVFZL1Mj37vUzhYeNBfo0+M1tpGn8hIEKw0wFKnwZ\r\n1h+p+HgLNA34eWw/cYSqqs4zuBh6FEIW738dWlFg4xEG3GeKoCFt5mpaKzZbQoKMYFV0JcTv0rIO\r\nXeeLpZR4UcJ4CAUTLpM6Dw3PmwIHQ05+v3txMZBSojSRpdpfJZ9Thps/5idyCG6VenXiPfU4ktQ7\r\n5HTFK8HBjL+DHKHPgcTsLXMskZVunerDBvE7kQjNCWD6bYgUEyirWOK1sNq+cHJg2j5LWWZ5QgOk\r\n/gF0g1UOl0CVPDM0phRuca4Yp3oT6VArpjcwGw==";

        private PSExecuteConnection TestPSExecute(string script_text)
        {
            var server = new PSExecuteServer();
            var conn = server.CreateConnection() as PSExecuteConnection;
            conn.ExtractQuery(script_text, new List<QvxTable>());
            return conn;
        }

        private ScriptCode TestScript(string script_text)
        {
            return ScriptCode.Parse(script_text);
        }

        [TestCategory("ScriptTest"), TestMethod]
        public void ScriptWithArguments()
        {
            var script = TestScript($"PSEXECUTE({{arg1:\"Hallo\", arg2:\"test\"}})\r\nGet-Process | Select-Object Name, Id\r\nSHA256:\r\n{SignString};");
        }

        [TestCategory("ScriptTest"), TestMethod]
        public void ScriptWithBreaksAndTabs()
        {
            var qconn = TestPSExecute($"PSEXECUTE()\r\n\t\t{Command}\r\n\t\tSHA256:\r\n{SignString};");
        }

        [TestCategory("ScriptTest"), TestMethod]
        public void ScriptWithUnixBreaks()
        {
            var qconn = TestPSExecute($" PSEXECUTE()\n  {Command}\n SHA256:\n{SignString};");
        }

        [TestCategory("ScriptTest"), TestMethod]
        public void ScriptWithNoArguments()
        {
            var qconn = TestPSExecute($" PSEXECUTE()\r\n  {Command}\r\n\r\n SHA256:\r\n{SignString};");
        }

        [TestCategory("ScriptTest"), TestMethod]
        public void ScriptWithPreTabs()
        {
            var qconn = TestPSExecute($"\r\n\t\t PSEXECUTE()\r\n  {Command}\r\n\r\n SHA256:\r\n{SignString};");
        }

        [TestCategory("ScriptTest"), TestMethod]
        public void ScriptWithMoreBreaksAndArguments()
        {
            var qconn = TestPSExecute($" \r\n\r\n\r\n\r\n\r\nPSEXECUTE({{arg1:\"Hallo\", arg2:\"test\"}})\r\n{Command}\n\r\n SHA256:\r\n{SignString};\r\n \r\n \r\n \r\n \r\n ");
        }

        [TestCategory("ScriptTest"), TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ScriptWrongCommand()
        {
            var qconn = TestPSExecute($" PSEXCEUTE()\r\n  {Command}\r\n  \r\n  \r\n SHA256:\r\n{SignString};");
        }

        [TestCategory("PowerShellTest"), TestMethod]
        [ExpectedException(typeof(PowerShellException))]
        public void ScriptWithUnknownArguments()
        {
            var qconn = TestPSExecute($" \r\n\r\n\r\n\r\n\r\nPSEXECUTE\r\n{Command}\n\r\nmehre unbekannte befehle,.-..\r\nSHA256:\r\n{SignString};\r\n \r\n \r\n \r\n \r\n ");
        }

        [TestCategory("PowerShellTest"), TestMethod]
        public void GetDataFromScript()
        {
            var qconn = TestPSExecute($"PSEXECUTE()\r\n{Command}\r\nSHA256:\r\n{SignString};");

            var fields = qconn.MTables[0].Fields.Length;
            Assert.AreEqual(fields, 3);

            int count = 0;
            var res = qconn.MTables[0].GetRows().GetEnumerator();
            while (res.MoveNext()) { count++; }
            Assert.AreEqual(count > 10, true);
        }

        [TestCategory("PowerShellTest"), TestMethod]
        public void CheckTestConnectionWithCredentials()
        {
            var script = $"PSEXECUTE({{arg1:\"Hallo\", arg2:\"test\"}})\r\n{Command}\r\nSHA256:\r\n{SignString};";
            var server = new PSExecuteServer();
            var conn = server.CreateConnection();
            conn.MParameters = new Dictionary<string, string>();
            conn.MParameters.Add("userid", "test");
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

        [TestCategory("TestConnection"), TestMethod]
        public void TestConnectionWithCredentials()
        {
            var server = new PSExecuteServer();
            server.TestConnection("test1", "test1");
        }

        [TestCategory("TestConnection"), TestMethod]
        public void TestConnectionWithoutCredentials()
        {
            var server = new PSExecuteServer();
            server.TestConnection("", "");
        }
    }
}
