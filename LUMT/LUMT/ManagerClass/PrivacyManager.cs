// DISCLAIMER
// This source code is provided as a sample. It is licensed "as-is."
// You bear the risk of using it. The contributors give no express warranties, 
// guarantees or conditions. See MS-PL license description for more details.

using System;
using System.Text;
using Microsoft.Rtc.Collaboration.Presence;
using LUMT.UCMA;

namespace LUMT.ManagerClass
{
    class PrivacyManager
    {
        Endpoint userEndpoint;

        public PrivacyManager(Endpoint endpoint)
        {
            userEndpoint = endpoint;
        }

        public void SetPrivacyPreference(string userUri, PrivacyModePreference privacyMode)
        {
            userEndpoint.InitializeUserEndpoint(userUri);
            userEndpoint.SubscribeToPresence();

            if (userEndpoint.User.ContactGroupServices.CurrentState == CollaborationSubscriptionState.Subscribed)
            {
                userEndpoint.User.PresenceServices.EndUpdatePrivacyPreference(userEndpoint.User.PresenceServices.BeginUpdatePrivacyPreference(privacyMode, null, null));
            }
            else
            {
                throw new Exception("Collaboration subscription state is not subscribed");
            }
        }


    }
}
