// DISCLAIMER
// This source code is provided as a sample. It is licensed "as-is."
// You bear the risk of using it. The contributors give no express warranties, 
// guarantees or conditions. See MS-PL license description for more details.

using System;
using System.Text;
using Microsoft.Rtc.Collaboration;
using LUMT.App;
using LUMT.Misc;

namespace LUMT.UCMA
{
    class Platform
    {
        private CollaborationPlatform collabPlatform;

        public CollaborationPlatform CollabPlatform
        {
            get { return collabPlatform; }
        }

        public void StartupPlatform()
        {
            try
            {
                
                ProvisionedApplicationPlatformSettings settings = new ProvisionedApplicationPlatformSettings(LumtGlobals.ApplicationUserAgent, LumtGlobals.TrustedApplicationID);
                collabPlatform = new CollaborationPlatform(settings);

                Log.WriteLogEntry("INFO", String.Format("Starting {0} platform...", LumtGlobals.ApplicationShortName));
                collabPlatform.EndStartup(collabPlatform.BeginStartup(null, null));
                Log.WriteLogEntry("INFO", String.Format("{0} platform started.", LumtGlobals.ApplicationShortName));
            }
            catch (Exception ex)
            {
                Log.WriteLogEntry("ERROR", String.Format("{0} platform NOT started: {1}. Inner Exception: {2}", LumtGlobals.ApplicationShortName, ex.Message, (ex.InnerException == null ? "N/A" : ex.InnerException.Message)), String.Format("{0} platform NOT started.", LumtGlobals.ApplicationShortName));
                Program.CloseApplicationOnError();
            }
        }

        public void ShutdownPlatform()
        {
            if (collabPlatform != null)
            {
                try
                {
                    Log.WriteLogEntry("INFO", String.Format("Shutting down {0} platform...", LumtGlobals.ApplicationShortName));
                    collabPlatform.EndShutdown(collabPlatform.BeginShutdown(null, null));
                    Log.WriteLogEntry("INFO", String.Format("{0} platform shut down.", LumtGlobals.ApplicationShortName));
                }
                catch (Exception ex)
                {
                    Log.WriteLogEntry("ERROR", String.Format("{0} platform NOT shut down: {1}. Inner Exception: {2}", LumtGlobals.ApplicationShortName, ex.Message, (ex.InnerException == null ? "N/A" : ex.InnerException.Message)));
                }
            }
        }
    }
}
