namespace QlikConnectorPSExecute
{
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security.AccessControl;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;
    #endregion

    //SOURCE CODE FROM: http://stackoverflow.com/questions/677874/starting-a-process-with-credentials-from-a-windows-service
    public class WindowsAccess
    {
        #region DLL-Import
        // All the code to manipulate a security object is available in .NET framework,
        // but its API tries to be type-safe and handle-safe, enforcing a special implementation
        // (to an otherwise generic WinAPI) for each handle type. This is to make sure
        // only a correct set of permissions can be set for corresponding object types and
        // mainly that handles do not leak.
        // Hence the AccessRule and the NativeObjectSecurity classes are abstract.
        // This is the simplest possible implementation that yet allows us to make use
        // of the existing .NET implementation, sparing necessity to
        // P/Invoke the underlying WinAPI.

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetProcessWindowStation();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetThreadDesktop(int dwThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int GetCurrentThreadId();
        #endregion

        public static int WindowStationAllAccess { get; private set; } = 0x000f037f;
        public static int DesktopRightsAllAccess { get; private set; } = 0x000f01ff;

        #region Static Methods
        public static int GrantAccessToWindowStation(NTAccount accountInfo, int accessMask)
        {
            return GrantAccess(accountInfo, GetProcessWindowStation(), accessMask);
        }

        public static int GrantAccessToDesktop(NTAccount accountInfo, int accessMask)
        {
            return GrantAccess(accountInfo, GetThreadDesktop(GetCurrentThreadId()), accessMask);
        }

        private static int GrantAccess(NTAccount accountInfo, IntPtr handle, int accessMask)
        {
            if (accessMask < 0)
                return -1;

            SafeHandle safeHandle = new NoopSafeHandle(handle);
            var security = new GenericSecurity(false, ResourceType.WindowObject, safeHandle, AccessControlSections.Access);
            var ruels = security.GetAccessRules(true, false, typeof(NTAccount));

            var username = accountInfo.Value;
            if (!username.Contains("\\"))
                username = $"{Environment.MachineName}\\{username}";

            var userResult = ruels.Cast<GrantAccessRule>().SingleOrDefault(r =>
                             r.IdentityReference.Value == username && accessMask == r.PublicAccessMask);

            if (userResult == null)
            {
                userResult = ruels.Cast<GrantAccessRule>().SingleOrDefault(r =>
                             r.IdentityReference.Value == username);

                var rule = new GrantAccessRule(accountInfo, accessMask, AccessControlType.Allow);
                security.AddAccessRule(rule);
                security.Persist(safeHandle, AccessControlSections.Access);

                if (userResult != null)
                {
                    return userResult.PublicAccessMask;
                }
            }

            return -1;
        }
        #endregion

        private class GenericSecurity : NativeObjectSecurity
        {
            public GenericSecurity(
                bool isContainer, ResourceType resType, SafeHandle objectHandle,
                AccessControlSections sectionsRequested)
                : base(isContainer, resType, objectHandle, sectionsRequested) { }

            new public void Persist(SafeHandle handle, AccessControlSections includeSections)
            {
                base.Persist(handle, includeSections);
            }

            new public void AddAccessRule(AccessRule rule)
            {
                base.AddAccessRule(rule);
            }

            new public bool RemoveAccessRule(AccessRule rule)
            {
                return base.RemoveAccessRule(rule);
            }

            new public AuthorizationRuleCollection GetAccessRules(bool includeExplicit, bool includeInherited, Type targetType)
            {
                return base.GetAccessRules(includeExplicit, includeInherited, targetType);
            }

            #region NativeObjectSecurity Abstract Method Overrides
            public override Type AccessRightType
            {
                get { throw new NotImplementedException(); }
            }

            public override AccessRule AccessRuleFactory(
                System.Security.Principal.IdentityReference identityReference,
                int accessMask, bool isInherited, InheritanceFlags inheritanceFlags,
                PropagationFlags propagationFlags, AccessControlType type)
            {
                return new GrantAccessRule(identityReference, accessMask, isInherited, inheritanceFlags, propagationFlags, type);
            }

            public override Type AccessRuleType
            {
                get { return typeof(AccessRule); }
            }

            public override AuditRule AuditRuleFactory(
                System.Security.Principal.IdentityReference identityReference, int accessMask,
                bool isInherited, InheritanceFlags inheritanceFlags,
                PropagationFlags propagationFlags, AuditFlags flags)
            {
                throw new NotImplementedException();
            }

            public override Type AuditRuleType
            {
                get { return typeof(AuditRule); }
            }
            #endregion
        }

        // Handles returned by GetProcessWindowStation and GetThreadDesktop should not be closed
        private class NoopSafeHandle : SafeHandle
        {
            public NoopSafeHandle(IntPtr handle) :
                base(handle, false) {}

            public override bool IsInvalid
            {
                get { return false; }
            }

            protected override bool ReleaseHandle()
            {
                return true;
            }
        }
    }

    public class GrantAccessRule : AccessRule
    {
        public GrantAccessRule(IdentityReference identity, int accessMask, bool isInherited,
                                 InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags,
                                 AccessControlType type) :
                                 base(identity, accessMask, isInherited,
                                      inheritanceFlags, propagationFlags, type) { }

        public GrantAccessRule(
           IdentityReference identity, int accessMask, AccessControlType type) :
            base(identity, accessMask, false, InheritanceFlags.None,
                 PropagationFlags.None, type) {}

        public int PublicAccessMask
        {
            get { return base.AccessMask; }
        }
    }
}
