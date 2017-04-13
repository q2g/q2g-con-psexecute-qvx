#region License
/*
Copyright (c) 2017 Konrad Mattheis und Martin Berthold
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
#endregion

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
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading;
    #endregion

    public class PSExecuteConnection : QvxConnection
    {
        #region Logger
        private static PseLogger logger = PseLogger.CreateLogger();
        #endregion

        #region Properties & Variables
        private CryptoManager Manager { get; set; }
        private DataTable internalData;
        private QvxField[] fields;

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
            Thread.Sleep(10000);
            try
            {
                var Errors = new StringBuilder();

                if (String.IsNullOrWhiteSpace(script.Code))
                    return resultTable;

                using (var powerShell = PowerShell.Create())
                {
                    Environment.CurrentDirectory = @"C:\Temp\";
                    logger.Warn("************"+ workdir);
                    var scriptBlock = ScriptBlock.Create(script.Code);
                    powerShell.AddCommand("Start-Job");

                    if (username != "" && password != "")
                    {
                        logger.Warn($"TEST: -User:{username} -Pass:{password.Length}");
                        
                        // if username & password are defined -> add as credentials
                        var secPass = new SecureString();
                        Array.ForEach(password.ToArray(), secPass.AppendChar);
                        powerShell.AddParameter("Credential", new PSCredential(username, secPass));
                    }
                    else
                    {
                        if (!IsQlikDesktopApp)
                        {
                            // without check signature
                            var signature = script.GetSignature();
                            if (Manager == null)
                            {
                                logger.Warn("No Certificate file found or no access.");
                                return resultTable;
                            }
                                
                            if (!Manager.IsValidPublicKey(script.Code, signature))
                            {
                                logger.Warn("The signature could not be valid.");
                                return resultTable;
                            }
                        }
                    }

                    powerShell.AddParameter("ScriptBlock", scriptBlock);

                    if(script.Parameters.Count > 0)
                      powerShell.AddParameter("ArgumentList", script.Parameters);

                    // Wait for the Job to finish
                    powerShell.AddCommand("Wait-Job");
                    // Give all results and not the jobs back
                    powerShell.AddCommand("Receive-Job");
                    
                    var results = powerShell.Invoke();

                    foreach (var error in powerShell.Streams.Error.ReadAll())
                    {
                        var exString = PseLogger.GetFullExceptionString(error.Exception);
                        Errors.Append($"\n{exString}");
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