namespace QlikConnectorPSExecute
{
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;
    using QlikView.Qvx.QvxLibrary;
    using System.Management.Automation;
    using System.IO;
    #endregion

    public class PSExecuteConnection : QvxConnection
    {
        #region Init
        public override void Init() { }
        #endregion

        #region Methods
        private QvxTable.GetRowsHandler GetPowerShellResult(DataTable data)
        {
            return () =>
            {
                var result = new List<QvxDataRow>();
                foreach (DataRow row in data.Rows)
                {
                    result.Add(MakeEntry(row, FindTable("PSExecute", MTables)));
                }

                return result;
            };
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

        private DataTable GetData(ScriptCode script)
        {
            try
            {
                if (script == null)
                    return new DataTable();

                var resultTable = new DataTable();
                using (var powerShell = PowerShell.Create())
                {
                    powerShell.AddScript(script.Code);
                    script.Parameters.ToList().ForEach(p => powerShell.AddParameter(p.Key, p.Value));

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

        //private QvxField[] fields;

        //private IEnumerable<QvxDataRow> GetData2()
        //{            
        //    var row = new QvxDataRow();
        //    row[fields[0]] = "MEIN TEXT";

        //    var dd = row[fields[0]] as QvxDataValue;


        //    return new List<QvxDataRow>() { row };          
        //}



        public override QvxDataTable ExtractQuery(string query, List<QvxTable> tables)
        {
            var script = ScriptCode.Parse(query);
            var data = GetData(script);

            var fields = new List<QvxField>();
            foreach (DataColumn column in data.Columns)
            {
                fields.Add(new QvxField(column.ColumnName, QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII));
            }

            var table = new QvxTable()
            {
                TableName = "PSEXECUTE",
                Fields = fields.ToArray(),
                GetRows = GetPowerShellResult(data),
            };

            var result = new QvxDataTable(table);
            result.Select(fields.ToArray());

            //fields = new QvxField[1];

            //fields[0] = new QvxField("tt", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII);

            //var tb = new QvxTable() {
            //    TableName = query.Substring(0, 5),
            //    Fields = fields,
            //    GetRows = GetData2
            //};

            return result;
        }
        #endregion

        #region Properties & Variables
        //private DataTable TableData { get; set; }
        //private ScriptCode Script { get; set; }
        private StringBuilder Errors { get; set; } = new StringBuilder();
        //public string Command { get; private set; }
        #endregion
    }

    #region Exception Classes
    public class PowerShellException : Exception
    {
        public PowerShellException(string message, Exception ex) : base(message, ex) { }
    }
    #endregion
}