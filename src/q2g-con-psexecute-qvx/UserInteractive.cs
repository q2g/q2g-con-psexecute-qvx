#region License
/*
Copyright (c) 2017 Konrad Mattheis und Martin Berthold
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
#endregion

//
// This code has been adapted from http://www.codeproject.com/KB/cs/lsadotnet.aspx
// The rights enumeration code came from http://www.tech-archive.net/Archive/DotNet/microsoft.public.dotnet.framework.interop/2004-11/0394.html
//
// Windows Security via .NET is covered on by Pluralsight:http://alt.pluralsight.com/wiki/default.aspx/Keith.GuideBook/HomePage.html
//

namespace q2gconpsexecuteqvx
{
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.Text;
    #endregion

    public class LocalSecurityAuthorityController
    {
        #region API Call
        private const int Access = (int)(LSA_AccessPolicy.POLICY_AUDIT_LOG_ADMIN |
                                         LSA_AccessPolicy.POLICY_CREATE_ACCOUNT |
                                         LSA_AccessPolicy.POLICY_CREATE_PRIVILEGE |
                                         LSA_AccessPolicy.POLICY_CREATE_SECRET |
                                         LSA_AccessPolicy.POLICY_GET_PRIVATE_INFORMATION |
                                         LSA_AccessPolicy.POLICY_LOOKUP_NAMES |
                                         LSA_AccessPolicy.POLICY_NOTIFICATION |
                                         LSA_AccessPolicy.POLICY_SERVER_ADMIN |
                                         LSA_AccessPolicy.POLICY_SET_AUDIT_REQUIREMENTS |
                                         LSA_AccessPolicy.POLICY_SET_DEFAULT_QUOTA_LIMITS |
                                         LSA_AccessPolicy.POLICY_TRUST_ADMIN |
                                         LSA_AccessPolicy.POLICY_VIEW_AUDIT_INFORMATION |
                                         LSA_AccessPolicy.POLICY_VIEW_LOCAL_INFORMATION
                                         );

        [DllImport("advapi32.dll", PreserveSig = true)]
        private static extern UInt32 LsaOpenPolicy(ref LSA_UNICODE_STRING SystemName, ref LSA_OBJECT_ATTRIBUTES ObjectAttributes, Int32 DesiredAccess, out IntPtr PolicyHandle);

        [DllImport("advapi32.dll", SetLastError = true, PreserveSig = true)]
        private static extern uint LsaAddAccountRights(IntPtr PolicyHandle, IntPtr AccountSid, LSA_UNICODE_STRING[] UserRights, int CountOfRights);

        [DllImport("advapi32.dll")]
        public static extern void FreeSid(IntPtr pSid);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true, PreserveSig = true)]
        private static extern bool LookupAccountName(string lpSystemName, string lpAccountName, IntPtr psid, ref int cbsid, StringBuilder domainName, ref int cbdomainLength, ref int use);

        [DllImport("advapi32.dll")]
        private static extern bool IsValidSid(IntPtr pSid);

        [DllImport("advapi32.dll")]
        private static extern int LsaClose(IntPtr ObjectHandle);

        [DllImport("kernel32.dll")]
        private static extern int GetLastError();

        [DllImport("advapi32.dll")]
        private static extern int LsaNtStatusToWinError(uint status);

        [DllImport("advapi32.dll", SetLastError = true, PreserveSig = true)]
        private static extern uint LsaEnumerateAccountRights(IntPtr PolicyHandle, IntPtr AccountSid, out IntPtr UserRightsPtr, out int CountOfRights);

        [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern uint LsaRemoveAccountRights(IntPtr PolicyHandle, IntPtr pSID, bool AllRights, LSA_UNICODE_STRING[] UserRights, int CountOfRights);

        [StructLayout(LayoutKind.Sequential)]
        private struct LSA_UNICODE_STRING
        {
            public UInt16 Length;
            public UInt16 MaximumLength;
            public IntPtr Buffer;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LSA_OBJECT_ATTRIBUTES
        {
            public int Length;
            public IntPtr RootDirectory;
            public LSA_UNICODE_STRING ObjectName;
            public UInt32 Attributes;
            public IntPtr SecurityDescriptor;
            public IntPtr SecurityQualityOfService;
        }

        [Flags]
        private enum LSA_AccessPolicy : long
        {
            POLICY_VIEW_LOCAL_INFORMATION = 0x00000001L,
            POLICY_VIEW_AUDIT_INFORMATION = 0x00000002L,
            POLICY_GET_PRIVATE_INFORMATION = 0x00000004L,
            POLICY_TRUST_ADMIN = 0x00000008L,
            POLICY_CREATE_ACCOUNT = 0x00000010L,
            POLICY_CREATE_SECRET = 0x00000020L,
            POLICY_CREATE_PRIVILEGE = 0x00000040L,
            POLICY_SET_DEFAULT_QUOTA_LIMITS = 0x00000080L,
            POLICY_SET_AUDIT_REQUIREMENTS = 0x00000100L,
            POLICY_AUDIT_LOG_ADMIN = 0x00000200L,
            POLICY_SERVER_ADMIN = 0x00000400L,
            POLICY_LOOKUP_NAMES = 0x00000800L,
            POLICY_NOTIFICATION = 0x00001000L
        }
        #endregion

        #region Static Methods
        private static LSA_OBJECT_ATTRIBUTES CreateLSAObject()
        {
            var newInstance = new LSA_OBJECT_ATTRIBUTES();
            newInstance.Length = 0;
            newInstance.RootDirectory = IntPtr.Zero;
            newInstance.Attributes = 0;
            newInstance.SecurityDescriptor = IntPtr.Zero;
            newInstance.SecurityQualityOfService = IntPtr.Zero;
            return newInstance;
        }
        #endregion

        #region Private Methods
        private void LookupAccount(string accountName, ref IntPtr sid, ref IntPtr policyHandle)
        {
            int sidSize = 0;
            var domainName = new StringBuilder();
            int nameSize = 0;
            int accountType = 0;

            LookupAccountName(String.Empty, accountName, sid, ref sidSize, domainName, ref nameSize, ref accountType);

            domainName = new StringBuilder(nameSize);
            sid = Marshal.AllocHGlobal(sidSize);

            if (!LookupAccountName(String.Empty, accountName, sid, ref sidSize, domainName, ref nameSize, ref accountType))
            {
                var winErrorCode = (int)GetLastError();
                throw new Win32Exception(winErrorCode, $"LookupAccountName failed: {winErrorCode}");
            }
            else
            {
                var systemName = new LSA_UNICODE_STRING();
                var objectAttributes = CreateLSAObject();

                uint resultPolicy = LsaOpenPolicy(ref systemName, ref objectAttributes, Access, out policyHandle);
                var winErrorCode = LsaNtStatusToWinError(resultPolicy);
                if (winErrorCode != 0)
                    throw new Win32Exception((int)winErrorCode, $"OpenPolicy failed: {winErrorCode}");
            }
        }

        public IList<string> GetRights(string accountName)
        {
            var sid = IntPtr.Zero;
            var policyHandle = IntPtr.Zero;
            var rights = new List<string>();
            var userRightsPtr = IntPtr.Zero;
            int countOfRights = 0;

            LookupAccount(accountName, ref sid, ref policyHandle);

            try
            {
                uint result = LsaEnumerateAccountRights(policyHandle, sid, out userRightsPtr, out countOfRights);
                var winErrorCode = LsaNtStatusToWinError(result);
                if (winErrorCode != 0)
                {
                    if (winErrorCode == 2)
                        return new List<string>();

                    throw new Win32Exception((int)winErrorCode, $"LsaEnumerateAccountRights failed: {winErrorCode}");
                }

                var newPtr = IntPtr.Zero;
                try
                {
                    newPtr = new IntPtr(userRightsPtr.ToInt32());
                    if (IntPtr.Size == 8)
                        newPtr = new IntPtr(userRightsPtr.ToInt64());
                }
                catch
                {
                    newPtr = new IntPtr(userRightsPtr.ToInt64());
                }
                
                LSA_UNICODE_STRING userRight;

                int ptr = 0;
                for (int i = 0; i < countOfRights; i++)
                {
                    userRight = Marshal.PtrToStructure<LSA_UNICODE_STRING>(newPtr);
                    var userRightStr = Marshal.PtrToStringAuto(userRight.Buffer);
                    rights.Add(userRightStr);
                    ptr += Marshal.SizeOf(userRight);
                }
            }
            finally
            {
                LsaClose(policyHandle);
                FreeSid(sid);
            }

            return rights;
        }

        public void SetRight(string accountName, string privilegeName, bool remove)
        {
            var sid = IntPtr.Zero;
            var policyHandle = IntPtr.Zero;

            LookupAccount(accountName, ref sid, ref policyHandle);

            try
            {
                var userRights = new LSA_UNICODE_STRING[1];
                userRights[0] = new LSA_UNICODE_STRING();
                userRights[0].Buffer = Marshal.StringToHGlobalUni(privilegeName);
                userRights[0].Length = (UInt16)(privilegeName.Length * UnicodeEncoding.CharSize);
                userRights[0].MaximumLength = (UInt16)((privilegeName.Length + 1) * UnicodeEncoding.CharSize);

                uint res = 0;
                if (remove)
                    res = LsaRemoveAccountRights(policyHandle, sid, false, userRights, 1);
                else
                    res = LsaAddAccountRights(policyHandle, sid, userRights, 1);

                var winErrorCode = LsaNtStatusToWinError(res);
                if (winErrorCode != 0)
                    throw new Win32Exception((int)winErrorCode, $"LsaAddAccountRights failed: {winErrorCode}");
            }
            finally
            {
                LsaClose(policyHandle);
                FreeSid(sid);
            }
        }
        #endregion 
    }

    // Local security rights managed by the Local Security Authority
    public class LocalSecurityAuthorityRights
    {
        #region Constants
        // Log on as a service right
        public const string LogonAsService = "SeServiceLogonRight";
        // Log on as a batch job right
        public const string LogonAsBatchJob = "SeBatchLogonRight";
        // Interactive log on right
        public const string InteractiveLogon = "SeInteractiveLogonRight";
        // Network log on right
        public const string NetworkLogon = "SeNetworkLogonRight";
        // Generate security audit logs right
        public const string GenerateSecurityAudits = "SeAuditPrivilege";
        #endregion
    }

    /* added wrapper for PowerShell */
    public class InteractiveUser : IDisposable
    {
        #region Variables & Properties
        private bool IsLocallyDomainUser { get; set; }
        private bool IsLocalUser { get; set; }
        private NTAccount AccountInfo { get; set; }
        private string CurrentRight { get; set; }
        private string AccountName { get; set; }
        private LocalSecurityAuthorityController Controller = new LocalSecurityAuthorityController();
        #endregion

        #region Constructor
        public InteractiveUser(NTAccount accountInfo, bool IsQlikDesktop)
        {
            try
            {
                if (accountInfo == null && IsQlikDesktop == true)
                    return;

                if (String.IsNullOrEmpty(CurrentRight))
                    CurrentRight = LocalSecurityAuthorityRights.InteractiveLogon;

                AccountInfo = accountInfo;

                IsLocalUser = IsLocalWinUser();
                if (!IsLocalUser)
                {
                    var results = Controller.GetRights(AccountInfo?.Value);
                    if (!results.Contains(CurrentRight))
                    {
                        Controller.SetRight(AccountInfo?.Value, CurrentRight, false);
                        IsLocallyDomainUser = false;
                    }
                    else
                        IsLocallyDomainUser = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"The user \"{AccountName}\" has too few rights.", ex);
            }
        }

        public void Dispose()
        {
            if (!IsLocallyDomainUser && !IsLocalUser)
            {
                Controller.SetRight(AccountInfo.Value, CurrentRight, true);
            }
        }
        #endregion

        #region Private Methods
        private bool IsLocalWinUser()
        {
            var strMachineName = Environment.MachineName.ToUpperInvariant();
            AccountName = WindowsIdentity.GetCurrent().Name.ToUpperInvariant();
            return AccountName.Contains(strMachineName);
        }
        #endregion
    }
}