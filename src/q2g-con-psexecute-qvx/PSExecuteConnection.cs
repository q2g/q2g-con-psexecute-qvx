#region License
/*
Copyright (c) 2018 Konrad Mattheis und Martin Berthold
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
#endregion

namespace q2gconpsexecuteqvx
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
    using System.Security.Principal;
    using System.DirectoryServices.AccountManagement;
    using System.DirectoryServices;
    #endregion

    public class PSExecuteConnection : QvxConnection
    {
        #region Logger
        private static PseLogger logger = PseLogger.CreateLogger();
        #endregion

        #region Properties & Variables
        private CryptoManager Manager { get; set; }
        private bool IsQlikDesktopApp
        {
            get
            {
                try
                {
                    return Process.GetCurrentProcess().Parent().MainModule.FileName.ToLowerInvariant().Contains(@"appdata\local\programs\qlik\sense\");
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"The parent process was not found.");
                    if (Process.GetProcessesByName("QlikSenseBrowser").Length > 0)
                        return true;
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
        private bool IsRemoteComputer(string remoteName)
        {
            try
            {
                switch ((remoteName ?? "").ToLowerInvariant())
                {
                    case "":
                    case "localhost":
                    case "127.0.0.1":
                    case ":::1":
                        return false;
                    default:
                        return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool UserSystemCheck(NTAccount account)
        {
            try
            {
                if (account == null)
                    return false;

                var dirEntryLocalMachine = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");
                return dirEntryLocalMachine.Children.Find(account.Value, "user") != null;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"The user {account.Value} not exists.");
                return false;
            }
        }

        private QvxTable GetData(ScriptCode script, string username, string password, string workdir, string remoteName)
        {
            var actualWorkDir = Environment.CurrentDirectory;
            var useRemote = IsRemoteComputer(remoteName);

            if (String.IsNullOrWhiteSpace(workdir))
                workdir = Path.GetDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

            if (Directory.Exists(workdir))
            {
                try
                {
                    Environment.CurrentDirectory = workdir;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"The working dir \"{workdir}\" not set.");
                }
            }

            var resultTable = new QvxTable();
            resultTable.TableName = script.TableName;

            try
            {
                var Errors = new StringBuilder();

                if (String.IsNullOrWhiteSpace(script.Code))
                    return resultTable;

                using (var powerShell = PowerShell.Create())
                {
                    var scriptBlock = ScriptBlock.Create(script.Code);
                    if (useRemote)
                        powerShell.AddCommand("Invoke-Command");
                    else
                        powerShell.AddCommand("Start-Job");

                    NTAccount accountInfo = null;
                    if (username != "" && password != "")
                    {
                        // if username & password are defined -> add as credentials
                        var secPass = new SecureString();
                        Array.ForEach(password.ToArray(), secPass.AppendChar);
                        powerShell.AddParameter("Credential", new PSCredential(username, secPass));
                        accountInfo = new NTAccount(username);

                        if (!useRemote && !UserSystemCheck(accountInfo))
                            accountInfo = null;
                    }
                    else
                    {
                        if (useRemote == true)
                        {
                            logger.Warn("A remote connection without user credentials is not allowed.");
                            return resultTable;
                        }
                            
                        if (!IsQlikDesktopApp)
                        {
                            // check signature
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

                    if (script.Parameters.Count > 0)
                        powerShell.AddParameter("ArgumentList", script.Parameters);

                    if (useRemote)
                        powerShell.AddParameter("ComputerName", remoteName);
                    else
                    {
                        // Wait for the Job to finish
                        powerShell.AddCommand("Wait-Job");
                        // Give all results and not the jobs back
                        powerShell.AddCommand("Receive-Job");
                    }

                    using (var windowsGrandAccess = new WindowsGrandAccess(accountInfo,
                                                        WindowsGrandAccess.WindowStationAllAccess,
                                                        WindowsGrandAccess.DesktopRightsAllAccess))
                    {
                        using (var interactiveUser = new InteractiveUser(accountInfo, IsQlikDesktopApp))
                        {
                            // Run PowerShell
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

                            // fill QvxTable
                            var fields = new List<QvxField>();
                            var rows = new List<QvxDataRow>();
                            foreach (var psObject in results)
                            {
                                var row = new QvxDataRow();
                                foreach (var p in psObject.Properties)
                                {
                                    if (p.Name != "PSComputerName" && p.Name != "RunspaceId" && p.Name != "PSShowComputerName")
                                    {
                                        var field = new QvxField(p.Name, QvxFieldType.QVX_TEXT,
                                                             QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA,
                                                             FieldAttrType.ASCII);

                                        if (fields.SingleOrDefault(s =>
                                            s.FieldName == field.FieldName) == null)
                                        {
                                            fields.Add(field);
                                        }

                                        row[field] = (p.Value ?? "").ToString();
                                    }
                                }

                                rows.Add(row);
                            }

                            resultTable.Fields = fields.ToArray();
                            resultTable.GetRows = () => { return rows; };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "The PowerShell script can not be executed.");
                return resultTable;
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
                var hostName = "";
                this.MParameters.TryGetValue("userid", out username);
                this.MParameters.TryGetValue("password", out password);
                this.MParameters.TryGetValue("workdir", out workdir);
                this.MParameters.TryGetValue("host", out hostName);
                username = (username ?? "").Trim();
                password = (password ?? "").Trim();
                workdir = (workdir ?? "").Trim();
                hostName = (hostName ?? "").Trim();

                var qvxTable = GetData(script, username, password, workdir, hostName);
                var result = new QvxDataTable(qvxTable);
                result.Select(qvxTable.Fields);

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