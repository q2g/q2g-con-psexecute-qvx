﻿namespace QlikConnectorPSExecute
{
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;
    using QlikView.Qvx.QvxLibrary;
    using System.Management.Automation;
    #endregion

    public class PSExecuteConnection : QvxConnection
    {
        #region Init
        public override void Init() { }
        #endregion

        #region Methods
        public void ScriptInit(string script)
        {
            try
            {
                Script = ScriptCode.Parse(script);
                TableData = GetData();

                var eventLogFields = new List<QvxField>();
                foreach (DataColumn column in TableData.Columns)
                {
                    eventLogFields.Add(new QvxField(column.ColumnName, QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII));
                }

                MTables = new List<QvxTable>()
                {
                    new QvxTable()
                    {
                        TableName = "PSExecute",
                        GetRows = GetPowerShellResult,
                        Fields = eventLogFields.ToArray(),
                    }
                };
            }
            catch (Exception ex)
            {
                throw new Exception("The script could not be initialized.", ex);
            }
        }

        private IEnumerable<QvxDataRow> GetPowerShellResult()
        {
            var table = new QvxTable();
            foreach (DataRow row in TableData.Rows)
            {
                yield return MakeEntry(row, FindTable("PSExecute", MTables));
            }
        }

        private QvxDataRow MakeEntry(DataRow dataRow, QvxTable table)
        {
            var row = new QvxDataRow();
            for (int i = 0; i < dataRow.ItemArray.Length; i++)
            {
                row[table.Fields[i]] = dataRow[i].ToString();
            }

            return row;
        }

        private DataTable GetData()
        {
            try
            {
                if(Script == null)
                    return new DataTable();

                var resultTable = new DataTable();
                using (var powerShell = PowerShell.Create())
                {
                    powerShell.AddScript(Script.Code);
                    Script.Parameters.ToList().ForEach(p => powerShell.AddParameter(p.Key, p.Value));

                    var results = powerShell.Invoke();
                    foreach (var psObject in results)
                    {
                        var row = resultTable.NewRow();
                        foreach (var p in psObject.Properties)
                        {
                            if (!resultTable.Columns.Contains(p.Name))
                            {
                                resultTable.Columns.Add(p.Name);
                            }

                            row[p.Name] = p.Value.ToString();
                        }

                        resultTable.Rows.Add(row);
                    }

                    var errors = powerShell.Streams.Error.ReadAll();
                    foreach (var error in errors)
                    {
                        Errors.Append($"\n{error.Exception.Message}");
                    }
                }

                if (Errors.Length > 0)
                    throw new Exception(Errors.ToString());

                return resultTable;
            }
            catch (Exception ex)
            {
                throw new PowerShellException("The PowerShell script can not be execute.", ex);
            }
        }

        public override QvxDataTable ExtractQuery(string query, List<QvxTable> tables)
        {
            return base.ExtractQuery(query, tables);
        }
        #endregion

        #region Properties & Variables
        private DataTable TableData { get; set; }
        private ScriptCode Script { get; set; }
        private StringBuilder Errors { get; set; } = new StringBuilder();
        public string Command { get; private set; }
        #endregion
    }

    #region Exception Classes
    public class PowerShellException : Exception
    {
        public PowerShellException(string message, Exception ex) : base(message, ex) { }
    }
    #endregion
}