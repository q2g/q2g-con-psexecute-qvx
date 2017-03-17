using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QlikConnect;
using System.IO;

namespace ConnectorTest
{
    [TestClass]
    public class ScriptFormatTest
    {
        private QlikConnector GetConnector(string script)
        {
            return new QlikConnector(script);
        }

        [TestMethod]
        public void Format1()
        {
            var script = "PSEXECUTE()\r\n\tGet-Process | Select-Object ProcessName, Handles, ID\r\n\tPSSIGNATURE:734AF5623CB5234";
            var connector = GetConnector(script);
        }

        [TestMethod]
        public void Format2()
        {
            var script = " PSEXECUTE()\r\n  Get-Process | Select-Object ProcessName, Handles, ID\r\n PSSIGNATURE:734AF5623CB5234";
            var connector = GetConnector(script);
        }

        [TestMethod]
        public void Format3()
        {
            var script = " PSEXECUTE()\r\n  Get-Process | Select-Object ProcessName, Handles, ID\r\n\r\n PSSIGNATURE:734AF5623CB5234";
            var connector = GetConnector(script);
        }

        [TestMethod]
        public void Format4()
        {
            var script = " \r\n\r\n\r\n\r\n\r\nPSEXECUTE({arg1:\"Hallo\", arg2:\"test\"})\r\nGet-Process | Select-Object ProcessName, Handles, ID\\n\r\n PSSIGNATURE:734AF5623CB5234\r\n \r\n \r\n \r\n \r\n ";
            var connector = GetConnector(script);
        }

        [TestMethod]
        public void Format5()
        {
            var script = " \r\n\r\n\r\n\r\n\r\nPSEXECUTE\r\nGet-Process | Select-Object ProcessName, Handles, ID\n\r\nmehre befehle,.-..\r\nPSSIGNATURE:734AF5623CB5234\r\n \r\n \r\n \r\n \r\n ";
            var connector = GetConnector(script);
        }

        [TestMethod]
        public void Format6()
        {
            var script = " PSEXCEUTE()\r\n  Get-Process | Select-Object ProcessName, Handles, ID\r\n  \r\n  \r\n PSSIGNATURE:734AF5623CB5234";
            var connector = GetConnector(script);
        }
    }
}
