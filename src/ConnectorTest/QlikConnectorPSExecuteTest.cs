using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QlikConnectorPSExecute;
using System.IO;
using QlikConnect.Crypt;
using ConnectorTest.Properties;

namespace ConnectorTest
{
    [TestClass]
    public class QlikConnectorPSExecuteTest
    {
        private string Command { get; } = "Get-Process | Select-Object ProcessName, Handles, ID";
        private string SignString { get; } = "A45Wkb0gK+3CWMUyPMfNQpr6aeIAUx3PA8D3NlVd4cibWZi4Ba4SxAwrD4dzArS82tkVidbceRIN+AetQC7Xuo6Kf3a6wEMUrtqrjwe/w8Vqm4u3sPM8iFziEc2yBPA4U3SckHiDL6dv+lILBQXDJFvdF7lVOfGQeWSaDPU5hvV8RFTQtz01Nu937Q5DKRP8txSc1FxMiVXy8uMyPGSTPWohY7EBPiSqHagoBiO2rNv5VqV1hnjUvXKdSfkBLr0s+jXieZcgGE8TFTkH2Ok6tH5BZNjNQd4h6sKnlkdIjyjPr0ERVNbF9kaEKrWs9PE07VSx4qD+m1mO5ECyRro+9w==";

        [TestMethod]
        public void ScriptWithArguments()
        {
            var qconn = new QlikConnector($"PSEXECUTE({{arg1:\"Hallo\", arg2:\"test\"}})\r\nGet-Process | Select-Object Name, Id\r\nSHA256:\r\n{SignString}");
        }

        [TestMethod]
        public void ScriptWithBreaksAndTabs()
        {
            var qconn = new QlikConnector($"PSEXECUTE()\r\n\t{Command}\r\n\tSHA256:\r\n{SignString}");
        }

        [TestMethod]
        public void ScriptWithUnixBreaks()
        {
            var qconn = new QlikConnector($" PSEXECUTE()\n  {Command}\n SHA256:\n{SignString}");
        }

        [TestMethod]
        public void ScriptWithNoArguments()
        {
            var qconn = new QlikConnector($" PSEXECUTE()\r\n  {Command}\r\n\r\n SHA256:\r\n{SignString}");
        }

        [TestMethod]
        public void ScriptWithMoreBreaksAndArguments()
        {
            var qconn = new QlikConnector($" \r\n\r\n\r\n\r\n\r\nPSEXECUTE({{arg1:\"Hallo\", arg2:\"test\"}})\r\n{Command}\n\r\n SHA256:\r\n{SignString}\r\n \r\n \r\n \r\n \r\n ");
        }

        [TestMethod]
        [ExpectedException(typeof(PowerShellException))]
        public void ScriptWithUnknownArguments()
        {
            var qconn = new QlikConnector($" \r\n\r\n\r\n\r\n\r\nPSEXECUTE\r\n{Command}\n\r\nmehre unbekannte befehle,.-..\r\nSHA256:\r\n{SignString}\r\n \r\n \r\n \r\n \r\n ");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ScriptWrongCommand()
        {
            var qconn = new QlikConnector($" PSEXCEUTE()\r\n  {Command}\r\n  \r\n  \r\n SHA256:\r\n{SignString}");
        }
    }
}
