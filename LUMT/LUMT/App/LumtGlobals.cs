// DISCLAIMER
// This source code is provided as a sample. It is licensed "as-is."
// You bear the risk of using it. The contributors give no express warranties, 
// guarantees or conditions. See MS-PL license description for more details.

using System;
using System.Text;

namespace LUMT.App
{
    public static class LumtGlobals
    {
        const string applicationFullName = "Lync User Management Tool";
        const string applicationShortName = "LUMT";
        const string applicationUserAgent = "LUMT";
        const string trustedApplicationID = "lumt";

        public static string ApplicationFullName
        {
            get { return applicationFullName; }
        }

        public static string ApplicationShortName
        {
            get { return applicationShortName; }
        }

        public static string ApplicationUserAgent
        {
            get { return applicationUserAgent; }
        }

        public static string TrustedApplicationID
        {
            get { return trustedApplicationID; }
        } 

    }
}
