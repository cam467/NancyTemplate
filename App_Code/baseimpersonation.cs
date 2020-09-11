namespace ApiBase.Globals
{
    using System;
    using System.Security.Principal;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Web;
    
    public class ImpersonateUser
    {
        public const int LOGON32_LOGON_INTERACTIVE = 2;
        public const int LOGON32_LOGON_BATCH = 4;
        public const int LOGON32_TYPE_NEW_CREDENTIALS = 9;
        public const int LOGON32_PROVIDER_DEFAULT = 0;
        WindowsImpersonationContext impersonationContext;

        [DllImport("advapi32.dll")]
        public static extern int LogonUserA(String lpszUserName,
            String lpszDomain,
            String lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            ref IntPtr phToken);
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int DuplicateToken(IntPtr hToken,
            int impersonationLevel,
            ref IntPtr hNewToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool RevertToSelf();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool CloseHandle(IntPtr handle);

        public bool impersonateLoggedinUser()
        {
            try
            {
                impersonationContext = ((System.Security.Principal.WindowsIdentity)HttpContext.Current.User.Identity).Impersonate();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool impersonateValidUser(String userName, String domain, String password)
        {
            WindowsIdentity tempWindowsIdentity;
            IntPtr token = IntPtr.Zero;
            IntPtr tokenDuplicate = IntPtr.Zero;

            if (RevertToSelf())
            {
                if (LogonUserA(userName, domain, password, LOGON32_LOGON_BATCH, LOGON32_PROVIDER_DEFAULT, ref token) != 0
                    || LogonUserA(userName, domain, password, LOGON32_TYPE_NEW_CREDENTIALS, LOGON32_PROVIDER_DEFAULT, ref token) != 0)
                {
                    if (DuplicateToken(token, 2, ref tokenDuplicate) != 0)
                    {
                        tempWindowsIdentity = new WindowsIdentity(tokenDuplicate);
                        impersonationContext = tempWindowsIdentity.Impersonate();
                        if (impersonationContext != null)
                        {
                            CloseHandle(token);
                            CloseHandle(tokenDuplicate);
                            return true;
                        }
                    }
                }
            }
            if (token != IntPtr.Zero)
                CloseHandle(token);
            if (tokenDuplicate != IntPtr.Zero)
                CloseHandle(tokenDuplicate);
            return false;
        }

        public void undoImpersonation()
        {
            impersonationContext.Undo();
        }
    }
}