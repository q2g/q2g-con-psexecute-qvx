using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QlikConnectorPSExecute;
using System.IO;
using QlikConnect.Crypt;

namespace ConnectorTest
{
    [TestClass]
    public class QlikConnectorPSExecuteTest
    {
        private QlikConnector GetConnector(string script)
        {
            return new QlikConnector(script);
        }

        private string SignString { get; } = "iojbicpeFmhXVpLo4EqgAMEkpjtIBqjnSnoZehsX4Pf1Rqaguy2dJyj1phyivXLO75y9mNX3nP0spU6Z4Iu8qFpPq185Cv+/i74/H+cMbzkhgpPmzayplLORe6gJKE6eYU59omeSRQ2CWScuhSi5BjVPww8TcT7aYGqPTKucM8LI5bbZtR8aMlRgEFh4R9WotBHHB67VVxWoDLdi6jp9EcMvL0zq1zWJTHSnGBDYw6RUJYQdyAyry54DTyPvB969GqGsvoLzugvLPJ3oeLdA07VFbCL9ddHIKYPCh+VvGH0WjPlzVDn79sc7SRRGXCfwAHArvgqtQxrqgplI4I2xhA==";

        [TestMethod]
        public void Format1()
        {
            var script = $"PSEXECUTE()\r\n\tGet-Process | Select-Object ProcessName, Handles, ID\r\n\tPSSIGNATURE:{SignString}";
            var connector = GetConnector(script);
            var table = connector.GetTable();
        }

        [TestMethod]
        public void Format2()
        {
            var script = $" PSEXECUTE()\r\n  Get-Process | Select-Object ProcessName, Handles, ID\r\n PSSIGNATURE:{SignString}";
            var connector = GetConnector(script);
            var table = connector.GetTable();
        }

        [TestMethod]
        public void Format3()
        {
            var script = $" PSEXECUTE()\r\n  Get-Process | Select-Object ProcessName, Handles, ID\r\n\r\n PSSIGNATURE:{SignString}";
            var connector = GetConnector(script);
            var table = connector.GetTable();
        }

        [TestMethod]
        public void Format4()
        {
            var script = $" \r\n\r\n\r\n\r\n\r\nPSEXECUTE({{arg1:\"Hallo\", arg2:\"test\"}})\r\nGet-Process | Select-Object ProcessName, Handles, ID\\n\r\n PSSIGNATURE:{SignString}\r\n \r\n \r\n \r\n \r\n ";
            var connector = GetConnector(script);
            var table = connector.GetTable();
        }

        [TestMethod]
        public void Format5()
        {
            var script = $" \r\n\r\n\r\n\r\n\r\nPSEXECUTE\r\nGet-Process | Select-Object ProcessName, Handles, ID\n\r\nmehre befehle,.-..\r\nPSSIGNATURE:{SignString}\r\n \r\n \r\n \r\n \r\n ";
            var connector = GetConnector(script);
            var table = connector.GetTable();
        }

        [TestMethod]
        public void Format6()
        {
            var script = $" PSEXCEUTE()\r\n  Get-Process | Select-Object ProcessName, Handles, ID\r\n  \r\n  \r\n PSSIGNATURE:{SignString}";
            var connector = GetConnector(script);
            var table = connector.GetTable();
        }
    }
}
