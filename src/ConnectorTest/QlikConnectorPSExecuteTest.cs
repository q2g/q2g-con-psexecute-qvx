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
        private string SignString { get; } = "A45Wkb0gK+3CWMUyPMfNQpr6aeIAUx3PA8D3NlVd4cibWZi4Ba4SxAwrD4dzArS82tkVidbceRIN\r\n+AetQC7Xuo6Kf3a6wEMUrtqrjwe/w8Vqm4u3sPM8iFziEc2yBPA4U3SckHiDL6dv+lILBQXDJFvd\r\nF7lVOfGQeWSaDPU5hvV8RFTQtz01Nu937Q5DKRP8txSc1FxMiVXy8uMyPGSTPWohY7EBPiSqHago\r\nBiO2rNv5VqV1hnjUvXKdSfkBLr0s+jXieZcgGE8TFTkH2Ok6tH5BZNjNQd4h6sKnlkdIjyjPr0ER\r\nVNbF9kaEKrWs9PE07VSx4qD+m1mO5ECyRro+9w==";

        private PSExecuteConnection TestPSExecute(string script_text)
        {
            var server = new PSExecuteServer();
            var conn = server.CreateConnection() as PSExecuteConnection;
            conn.ExtractQuery(script_text, new List<QvxTable>());
            return conn;
        }

        [TestCategory("ScriptTest"), TestMethod]
        public void ScriptWithArguments()
        {
            var qconn = TestPSExecute($"PSEXECUTE({{arg1:\"Hallo\", arg2:\"test\"}})\r\nGet-Process | Select-Object Name, Id\r\nSHA256:\r\n{SignString};");
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
        public void CheckTestConnection()
        {
            var script = $"PSEXECUTE()\r\n{Command}\r\nSHA256:\r\n{SignString}";
            var server = new PSExecuteServer();
            var conn = server.CreateConnection();
            conn.MParameters = new Dictionary<string, string>();
            conn.MParameters.Add("userid", "json");
            conn.MParameters.Add("password", "1q2w3e");
            conn.MParameters.Add("command", script);
            conn.Init();

            var result = server.HandleJsonRequest("LoadScript", new string[] { "json", "1q2w3e", script }, conn);
            var json = JsonConvert.DeserializeObject<Info>(result);
            Assert.AreEqual(json.qMessage, "SUCCESS");
        }
    }
}
