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
    using System.ComponentModel;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using System.Security;
    using System.Text;
    using System.Security.Principal;
    #endregion

    public class PowerShellProcess : IDisposable
    {
        #region Varibales && Properties
        private StringBuilder OutputResult { get; set; }
        private StringBuilder ErrorResult { get; set; }
        private Process PsProcess { get; set; }
        private bool UseCredentials { get; set; }
        #endregion

        public PowerShellProcess(string workDir, string script, string[] args)
        {
            UseCredentials = false;
            OutputResult = new StringBuilder();
            ErrorResult = new StringBuilder();

            for (int i = 0; i < args.Length; i++)
            {
                script = script.Replace($"$args[{i}]", args[i]);
            }

            PsProcess = new Process();
            PsProcess.StartInfo.WorkingDirectory = workDir;
            PsProcess.StartInfo.Arguments = script;
            PsProcess.StartInfo.FileName = "powershell.exe";
            PsProcess.StartInfo.UseShellExecute = false;
            PsProcess.StartInfo.CreateNoWindow = true;
            PsProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            PsProcess.StartInfo.RedirectStandardOutput = true;
            PsProcess.StartInfo.RedirectStandardError = true;
            PsProcess.EnableRaisingEvents = true;
            PsProcess.OutputDataReceived += PsProcess_OutputDataReceived;
            PsProcess.ErrorDataReceived += PsProcess_ErrorDataReceived;
        }

        public void SetCredentials(string username, SecureString password, string machineName = null)
        {
            PsProcess.StartInfo.UserName = username;
            PsProcess.StartInfo.Password = password;
            UseCredentials = true;
        }

        public string Start()
        {
            try
            {
                NTAccount accountInfo = null;
                if (UseCredentials)
                {
                    accountInfo = new NTAccount(PsProcess.StartInfo.UserName);
                }

                // If you are running the new process using different credentials,
                // then the new process won't have permissions to access the window station and desktop.
                using (var windowsAccess = new WindowsGrandAccess(accountInfo, WindowsGrandAccess.WindowStationAllAccess,
                                                                  WindowsGrandAccess.DesktopRightsAllAccess))
                {
                    PsProcess.Start();
                    PsProcess.BeginOutputReadLine();
                    PsProcess.BeginErrorReadLine();

                    PsProcess.WaitForExit();

                    var error = ErrorResult.ToString();
                    if (error.Length > 0)
                        throw new Exception(error);

                    return OutputResult.ToString();
                }
            }
            catch (Win32Exception ex)
            {
                throw new Exception("Win32 Exception", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("The Process has an error.", ex);
            }
        }

        public void Dispose()
        {
            try
            {
                PsProcess?.Close();
                PsProcess?.Dispose();
            }
            catch (Exception ex)
            {
                throw new Exception("The Process can´t close.", ex);
            }
        }

        private void PsProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            try
            {
                ErrorResult.Append(e.Data);
            }
            catch (Exception ex)
            {
                ErrorResult.Append(PseLogger.GetFullExceptionString(ex));
            }
        }

        private void PsProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            try
            {
                OutputResult.Append(e.Data);
            }
            catch (Exception ex)
            {
                ErrorResult.Append(PseLogger.GetFullExceptionString(ex));
            }
        }
    }
}