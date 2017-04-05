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
    using System.Diagnostics;
    using System.Security;
    #endregion

    public class PSExecuteConnection : QvxConnection
    {
        #region Logger
        private static Logger logger = Logger.CreateLogger();
        #endregion

        #region Properties & Variables
        private CryptoManager Manager { get; set; }
        #endregion

        #region Init
        public override void Init()
        {
            try
            {
                var keyFile = @"C:\ProgramData\Qlik\Sense\Repository\Exported Certificates\.Local Certificates\server_key.pem";
                if (File.Exists(keyFile))
                    Manager = new CryptoManager(keyFile);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "The connection could not be initialized.");
            }
        }
        #endregion

        #region Methods
        private IEnumerable<QvxDataRow> GetPowerShellResult()
        {
            var result = new List<QvxDataRow>();
            try
            {
                foreach (DataRow dataRow in internalData.Rows)
                {
                    var row = new QvxDataRow();
                    for (int i = 0; i < dataRow.ItemArray.Length; i++)
                    {
                        row[fields[i]] = dataRow[i].ToString();
                    }

                    result.Add(row);
                }
            }
            catch { }

            return result;
        }

        private DataTable GetData(ScriptCode script, string username, string password, string workdir)
        {
            var actualWorkDir = Environment.CurrentDirectory;
            var resultTable = new DataTable();

            try
            {
                var Errors = new StringBuilder();

                if (String.IsNullOrWhiteSpace(script.Code))
                    return resultTable;

                using (var powerShell = PowerShell.Create())
                {
                    Environment.CurrentDirectory = workdir;
                    var scriptBlock = ScriptBlock.Create(script.Code);
                    powerShell.AddCommand("Start-Job");

                    if (username != "" && password != "")
                    {
                        // if username & password are defined -> add as credentials
                        var secPass = new SecureString();
                        Array.ForEach(password.ToArray(), secPass.AppendChar);
                        powerShell.AddParameter("Credential", new PSCredential(username, secPass));
                    }
                    else
                    {
                        // without check signature
                        var signature = script.GetSignature();
                        if (Manager == null)
                            return resultTable;

                        if(!Manager.IsValidPublicKey(script.Code, signature))
                        {
                            logger.Warn("The signature could not be valid.");
                            return resultTable;
                        }
                    }

                    var values = new List<string>();
                    foreach (var psPara in script.Parameters)
                        values.Add(psPara.Value);

                    powerShell.AddParameter("ScriptBlock", scriptBlock);

                    if(values.Count > 0)
                      powerShell.AddParameter("ArgumentList", values);

                    // Wait for the Job to finish
                    powerShell.AddCommand("Wait-Job");
                    // Give all results and not the jobs back
                    powerShell.AddCommand("Receive-Job");

                    var results = powerShell.Invoke();
                    foreach (var error in powerShell.Streams.Error.ReadAll())
                    {
                        Errors.Append($"\n{error.Exception.Message}");
                    }
                    if (Errors.Length > 0)
                    {
                        throw new Exception($"Powershell-Error: {Errors.ToString()}");
                    }

                    foreach (var psObject in results)
                    {
                        var row = resultTable.NewRow();
                        foreach (var p in psObject.Properties)
                        {
                            if (p.Name != "PSComputerName" && p.Name != "RunspaceId" && p.Name != "PSShowComputerName")
                            {
                                if (!resultTable.Columns.Contains(p.Name))
                                {
                                    resultTable.Columns.Add(p.Name);
                                }
                                row[p.Name] = (p.Value ?? "").ToString();
                            }
                        }

                        resultTable.Rows.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "The PowerShell script can not be executed.");
                resultTable = new DataTable();
            }
            finally
            {
                Environment.CurrentDirectory = actualWorkDir;
            }

            return resultTable;
        }

        private bool IsQlikDesktopApp
        {
            get
            {
                try
                {
                    return Process.GetCurrentProcess().Parent().MainModule.FileName.Contains(@"AppData\Local\Programs\Qlik\Sense\");
                }
                catch
                {
                    return false;
                }
            }
        }

        private DataTable internalData;
        private QvxField[] fields;

        public override QvxDataTable ExtractQuery(string query, List<QvxTable> tables)
        {
            try
            {
                var script = ScriptCode.Parse(query);

                var username = "";
                var password = "";
                var workdir = "";
                this.MParameters.TryGetValue("userid", out username);
                this.MParameters.TryGetValue("password", out password);
                this.MParameters.TryGetValue("workdir", out workdir);
                username = (username ?? "").Trim();
                password = (password ?? "").Trim();
                workdir = workdir ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                internalData = GetData(script, username, password, workdir);

                var fieldsl = new List<QvxField>();
                foreach (DataColumn column in internalData.Columns)
                {
                    fieldsl.Add(new QvxField(column.ColumnName, QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII));
                }
                fields = fieldsl.ToArray();

                var table = new QvxTable()
                {
                    TableName = script.TableName,
                    Fields = fields,
                    GetRows = GetPowerShellResult
                };

                var result = new QvxDataTable(table);
                result.Select(fields);

                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "The query could not be executed.");
                return new QvxDataTable(new QvxTable() { TableName = "Error" });
            }
        }
        #endregion
    }
}