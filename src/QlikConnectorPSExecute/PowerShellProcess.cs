using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;

namespace QlikConnectorPSExecute
{
    public class PowerShellProcess : IDisposable
    {
        private StringBuilder OutputResult { get; set; }
        private StringBuilder ErrorResult { get; set; }
        private Process PsProcess { get; set; }
        private int WindowStationMask { get; set; }
        private int DesktopMask { get; set; }

        public PowerShellProcess(string workDir, string script, string[] args)
        {
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
            // If you are running the new process using different credentials, 
            // then the new process won't have permissions to access the window station and desktop.

            var accountInfo = new NTAccount(username);

            if (!String.IsNullOrEmpty(machineName))
                accountInfo = new NTAccount(machineName, username);

            WindowStationMask = WindowsAccess.GrantAccessToWindowStation(accountInfo, WindowsAccess.WindowStationAllAccess);
            DesktopMask = WindowsAccess.GrantAccessToDesktop(accountInfo, WindowsAccess.DesktopRightsAllAccess);

            PsProcess.StartInfo.UserName = username;
            PsProcess.StartInfo.Password = password;
        }

        public string Start()
        {
            try
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
            catch (Win32Exception ex)
            {
                throw new Exception("Win32 Exception", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("The Process has an error.", ex);
            }
            finally
            {
                var accountInfo = new NTAccount(PsProcess.StartInfo.UserName);
                WindowsAccess.GrantAccessToWindowStation(accountInfo, WindowStationMask);
                WindowsAccess.GrantAccessToDesktop(accountInfo, DesktopMask);
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