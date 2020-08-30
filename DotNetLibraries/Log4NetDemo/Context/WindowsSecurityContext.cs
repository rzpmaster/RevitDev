using Log4NetDemo.Core.Data;
using Log4NetDemo.Core.Interface;
using System;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Log4NetDemo.Context
{
    public class WindowsSecurityContext : SecurityContext, IOptionHandler
    {
        public enum ImpersonationMode
        {
            /// <summary>
            /// Impersonate a user using the credentials supplied
            /// </summary>
            User,

            /// <summary>
            /// Revert this the thread to the credentials of the process
            /// </summary>
            Process
        }

        public WindowsSecurityContext()
        {
        }

        public ImpersonationMode Credentials
        {
            get { return m_impersonationMode; }
            set { m_impersonationMode = value; }
        }

        public string UserName
        {
            get { return m_userName; }
            set { m_userName = value; }
        }

        public string DomainName
        {
            get { return m_domainName; }
            set { m_domainName = value; }
        }

        public string Password
        {
            set { m_password = value; }
        }

        #region IOptionHandler Members

        public void ActivateOptions()
        {
            if (m_impersonationMode == ImpersonationMode.User)
            {
                if (m_userName == null) throw new ArgumentNullException("m_userName");
                if (m_domainName == null) throw new ArgumentNullException("m_domainName");
                if (m_password == null) throw new ArgumentNullException("m_password");

                m_identity = LogonUser(m_userName, m_domainName, m_password);
            }
        }

        [System.Security.SecuritySafeCritical]
        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand, UnmanagedCode = true)]
        private static WindowsIdentity LogonUser(string userName, string domainName, string password)
        {
            const int LOGON32_PROVIDER_DEFAULT = 0;
            //This parameter causes LogonUser to create a primary token.
            const int LOGON32_LOGON_INTERACTIVE = 2;

            // Call LogonUser to obtain a handle to an access token.
            IntPtr tokenHandle = IntPtr.Zero;
            if (!LogonUser(userName, domainName, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, ref tokenHandle))
            {
                NativeError error = NativeError.GetLastError();
                throw new Exception("Failed to LogonUser [" + userName + "] in Domain [" + domainName + "]. Error: " + error.ToString());
            }

            const int SecurityImpersonation = 2;
            IntPtr dupeTokenHandle = IntPtr.Zero;
            if (!DuplicateToken(tokenHandle, SecurityImpersonation, ref dupeTokenHandle))
            {
                NativeError error = NativeError.GetLastError();
                if (tokenHandle != IntPtr.Zero)
                {
                    CloseHandle(tokenHandle);
                }
                throw new Exception("Failed to DuplicateToken after LogonUser. Error: " + error.ToString());
            }

            WindowsIdentity identity = new WindowsIdentity(dupeTokenHandle);

            // Free the tokens.
            if (dupeTokenHandle != IntPtr.Zero)
            {
                CloseHandle(dupeTokenHandle);
            }
            if (tokenHandle != IntPtr.Zero)
            {
                CloseHandle(tokenHandle);
            }

            return identity;
        }
        #endregion

        #region SecurityContext Members

        public override IDisposable Impersonate(object state)
        {
            if (m_impersonationMode == ImpersonationMode.User)
            {
                if (m_identity != null)
                {
                    return new DisposableImpersonationContext(m_identity.Impersonate());
                }
            }
            else if (m_impersonationMode == ImpersonationMode.Process)
            {
                // Impersonate(0) will revert to the process credentials
                return new DisposableImpersonationContext(WindowsIdentity.Impersonate(IntPtr.Zero));
            }
            return null;
        }

        #endregion

        #region Win32 Methods

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private extern static bool CloseHandle(IntPtr handle);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private extern static bool DuplicateToken(IntPtr ExistingTokenHandle, int SECURITY_IMPERSONATION_LEVEL, ref IntPtr DuplicateTokenHandle);

        #endregion

        #region DisposableImpersonationContext

        private sealed class DisposableImpersonationContext : IDisposable
        {
            private readonly WindowsImpersonationContext m_impersonationContext;

            public DisposableImpersonationContext(WindowsImpersonationContext impersonationContext)
            {
                m_impersonationContext = impersonationContext;
            }

            public void Dispose()
            {
                m_impersonationContext.Undo();
            }
        }

        #endregion

        private ImpersonationMode m_impersonationMode = ImpersonationMode.User;
        private string m_userName;
        private string m_domainName = Environment.MachineName;
        private string m_password;
        private WindowsIdentity m_identity;
    }
}
