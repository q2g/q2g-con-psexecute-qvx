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
        private IEnumerable<QvxDataRow> GetPowerShellResult()
        {
            var result = new List<QvxDataRow>();
            foreach (DataRow dataRow in internalData.Rows)
            {
                var row = new QvxDataRow();
                for (int i = 0; i < dataRow.ItemArray.Length; i++)
                {
                    row[fields[i]] = dataRow[i].ToString();
                }
                
                result.Add(row);
            }

            return result;
        }       

        //private void LogTest(string content)
        //{
        //    File.WriteAllText(@"C:\Users\MBerthold\Downloads\Log.txt", content);
        //}

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
                throw new PowerShellException("The PowerShell script can not be executed.", ex);
            }
        }

        private DataTable internalData;
        private QvxField[] fields;

        public override QvxDataTable ExtractQuery(string query, List<QvxTable> tables)
        {
            var script = ScriptCode.Parse(query);
            internalData = GetData(script);

            var fieldsl = new List<QvxField>();
            foreach (DataColumn column in internalData.Columns)
            {
                fieldsl.Add(new QvxField(column.ColumnName, QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII));
            }
            fields = fieldsl.ToArray();

            var table = new QvxTable()
            {
                TableName = script.TableName, // TODO als Name das PSEXCUTE mit Argumenten
                Fields = fields,
                GetRows = GetPowerShellResult
            };

            var result = new QvxDataTable(table);
            result.Select(fields);

            return result;
        }
        #endregion

        #region Properties & Variables
        private StringBuilder Errors { get; set; } = new StringBuilder();
        #endregion
    }

    #region Exception Classes
    public class PowerShellException : Exception
    {
        public PowerShellException(string message, Exception ex) : base(message, ex) { }
    }
    #endregion
}