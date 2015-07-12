// DISCLAIMER
// This source code is provided as a sample. It is licensed "as-is."
// You bear the risk of using it. The contributors give no express warranties, 
// guarantees or conditions. See MS-PL license description for more details.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Rtc.Collaboration.Presence;
using LUMT.UCMA;
using LUMT.Misc;
using LUMT.ItemClass;
using LUMT.ManagerClass;
using LUMT.Enum;
using Microsoft.Rtc.Signaling;

namespace LUMT.App
{
    class LumtApp
    {
        private Platform ucmaPlatform;
        private Endpoint ucmaEndpoint;

        public void InitializeEnvironment()
        {
            Log.LogFileInit();
            ucmaPlatform = new Platform();
        }

        public void CleanupEnvironment()
        {
            ucmaPlatform.ShutdownPlatform();
            Log.LogFileClose();
        }

        public void ProcessContacts(string usersFile, string contactsFile, string contactsGroup, bool deletionMode)
        {
            List<string> usersList = new List<string>();
            List<ContactItem> contactsList = new List<ContactItem>();

            try
            {
                usersList = FileAccess.GetUsersFileData(usersFile);
            }
            catch (Exception ex)
            {
                Log.WriteLogEntry("ERROR", String.Format("Error while reading Users file: {0}", ex.Message), "Error while reading Users file.");
                CleanupEnvironment();
                Program.CloseApplicationOnError();
            }

            try
            {
                contactsList = FileAccess.GetContactsFileData(contactsFile);
            }
            catch (Exception ex)
            {
                Log.WriteLogEntry("ERROR", String.Format("Error while reading Contacts file: {0}", ex.Message), "Error while reading Contacts file.");
                CleanupEnvironment();
                Program.CloseApplicationOnError();
            }

            if ((contactsList.Count > 0) && (usersList.Count > 0))
            {
                ucmaPlatform.StartupPlatform();
                ucmaEndpoint = new Endpoint(ucmaPlatform.CollabPlatform);
                ContactListManager contactListManager = new ContactListManager(ucmaEndpoint);

                foreach (string user in usersList)
                {
                    Log.WriteLogEntry("INFO", String.Format("Processing user {0}...", user.ToLowerInvariant()), String.Format("Processing user {0}...", user.ToLowerInvariant()));

                    try
                    {
                        if (!deletionMode)
                        {
                            contactListManager.AddContacts(user, contactsList, contactsGroup);
                        }
                        else
                        {
                            contactListManager.RemoveContacts(user, contactsList);
                        }
                    }
                    catch (PublishSubscribeException pex)
                    {
                        // If contact list provide is UCS (Exchange Server 2013)
                        if (pex.DiagnosticInformation.ErrorCode == 2164)
                        {
                            Log.WriteLogEntry("WARNING", String.Format("Error while processing user {0}: User's contact list is stored in Unified Contact Store. It can't be managed by {1}", user.ToLowerInvariant(), LumtGlobals.ApplicationShortName), String.Format("Error while processing user {0}", user.ToLowerInvariant()));
                        }
                        else
                        {
                            Log.WriteLogEntry("ERROR", String.Format("Error while processing user {0}: {1}. Inner Exception: {2}", user.ToLowerInvariant(), pex.Message, (pex.InnerException == null ? "N/A" : pex.InnerException.Message)), String.Format("Error while processing user {0}", user.ToLowerInvariant()));
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLogEntry("ERROR", String.Format("Error while processing user {0}: {1}. Inner Exception: {2}", user.ToLowerInvariant(), ex.Message, (ex.InnerException == null ? "N/A" : ex.InnerException.Message)), String.Format("Error while processing user {0}", user.ToLowerInvariant()));
                    }
                }
            }
            else
            {
                Log.WriteLogEntry("WARNING", "Users or Contacts file is empty or invalid. No action will be performed.");
            }
        }

        public void ProcessACEs(string usersFile, string aclFile, bool deletionMode)
        {
            List<string> usersList = new List<string>();
            List<ACEItem> aceList = new List<ACEItem>();

            try
            {
                usersList = FileAccess.GetUsersFileData(usersFile);
            }
            catch (Exception ex)
            {
                Log.WriteLogEntry("ERROR", String.Format("Error while reading Users file: {0}", ex.Message), "Error while reading Users file.");
                CleanupEnvironment();
                Program.CloseApplicationOnError();
            }

            try
            {
                aceList = FileAccess.GetACLFileData(aclFile);
            }
            catch (Exception ex)
            {
                Log.WriteLogEntry("ERROR", String.Format("Error while reading ACL file: {0}", ex.Message), "Error while reading ACL file.");
                CleanupEnvironment();
                Program.CloseApplicationOnError();
            }

            if ((aceList.Count > 0) && (usersList.Count > 0))
            {
                ucmaPlatform.StartupPlatform();
                ucmaEndpoint = new Endpoint(ucmaPlatform.CollabPlatform);
                ACLManager aclManager = new ACLManager(ucmaEndpoint);

                foreach (string user in usersList)
                {
                    Log.WriteLogEntry("INFO", String.Format("Processing user {0}...", user.ToLowerInvariant()), String.Format("Processing user {0}...", user.ToLowerInvariant()));

                    try
                    {
                        if (!deletionMode)
                        {
                            aclManager.AddACEs(user, aceList);
                        }
                        else
                        {
                            aclManager.RemoveACEs(user, aceList);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLogEntry("ERROR", String.Format("Error while processing user {0}: {1}", user.ToLowerInvariant(), ex.Message), String.Format("Error while processing user {0}", user.ToLowerInvariant()));
                    }
                }
            }
            else
            {
                Log.WriteLogEntry("WARNING", "Users or ACL file is empty or invalid. No action will be performed.");
            }
        }

        public void ProcessPrivacy(string usersFile, string privacyPreference)
        {
            List<string> usersList = new List<string>();

            try
            {
                usersList = FileAccess.GetUsersFileData(usersFile);
            }
            catch (Exception ex)
            {
                Log.WriteLogEntry("ERROR", String.Format("Error while reading Users file: {0}", ex.Message), "Error while reading Users file.");
                CleanupEnvironment();
                Program.CloseApplicationOnError();
            }

            PrivacyModePreference userPrivacyPreference = PrivacyModePreference.None;

            switch (privacyPreference.ToLowerInvariant())
            {
                case "default":
                    userPrivacyPreference = PrivacyModePreference.None;
                    break;
                case "private":
                    userPrivacyPreference = PrivacyModePreference.Private;
                    break;
                case "public":
                    userPrivacyPreference = PrivacyModePreference.Standard;
                    break;
                default:
                    // Should not happen
                    Log.WriteLogEntry("ERROR", "Incorrect value for PrivacyPreference parameter.");
                    CleanupEnvironment();
                    Program.CloseApplicationOnError();
                    break;
            }

            if (usersList.Count > 0)
            {
                ucmaPlatform.StartupPlatform();
                ucmaEndpoint = new Endpoint(ucmaPlatform.CollabPlatform);
                PrivacyManager privacyManager = new PrivacyManager(ucmaEndpoint);

                foreach (string user in usersList)
                {
                    Log.WriteLogEntry("INFO", String.Format("Processing user {0}...", user.ToLowerInvariant()), String.Format("Processing user {0}...", user.ToLowerInvariant()));

                    try
                    {
                        privacyManager.SetPrivacyPreference(user, userPrivacyPreference);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLogEntry("ERROR", String.Format("Error while processing user {0}: {1}", user.ToLowerInvariant(), ex.Message), String.Format("Error while processing user {0}", user.ToLowerInvariant()));
                    }
                }
            }
            else
            {
                Log.WriteLogEntry("WARNING", "Users file is empty or invalid. No action will be performed.");
            }
        }

        public void ProcessAlertNotifications(string usersFile, string alertWhenAddToContactListNotification, string alertWhenDNDNotification)
        {
            List<string> usersList = new List<string>();

            try
            {
                usersList = FileAccess.GetUsersFileData(usersFile);
            }
            catch (Exception ex)
            {
                Log.WriteLogEntry("ERROR", String.Format("Error while reading Users file: {0}", ex.Message), "Error while reading Users file.");
                CleanupEnvironment();
                Program.CloseApplicationOnError();
            }

            NotifyAdditionToContactListType notifyAdditionToContactList = NotifyAdditionToContactListType.NoChange;
            switch (alertWhenAddToContactListNotification.ToLowerInvariant())
            {
                case "":
                    notifyAdditionToContactList = NotifyAdditionToContactListType.NoChange;
                    break;
                case "yes":
                    notifyAdditionToContactList = NotifyAdditionToContactListType.Yes;
                    break;
                case "no":
                    notifyAdditionToContactList = NotifyAdditionToContactListType.No;
                    break;
                default:
                    // Should not happen
                    Log.WriteLogEntry("ERROR", "Incorrect value for NotifyAdd parameter.");
                    CleanupEnvironment();
                    Program.CloseApplicationOnError();
                    break;
            }

            AlertsWhenDoNotDisturbType alertsWhenDoNotDisturb = AlertsWhenDoNotDisturbType.NoChange;
            switch (alertWhenDNDNotification.ToLowerInvariant())
            {
                case "":
                    alertsWhenDoNotDisturb = AlertsWhenDoNotDisturbType.NoChange;
                    break;
                case "noalerts":
                    alertsWhenDoNotDisturb = AlertsWhenDoNotDisturbType.NoAlerts;
                    break;
                case "allalerts":
                    alertsWhenDoNotDisturb = AlertsWhenDoNotDisturbType.DisplayAllAlerts;
                    break;
                case "alertsfromworkgroup":
                    alertsWhenDoNotDisturb = AlertsWhenDoNotDisturbType.DisplayAlertsFromHighPresence;
                    break;
                default:
                    // Should not happen
                    Log.WriteLogEntry("ERROR", "Incorrect value for NotifyWhenDND parameter.");
                    CleanupEnvironment();
                    Program.CloseApplicationOnError();
                    break;
            }

            if (usersList.Count > 0)
            {
                ucmaPlatform.StartupPlatform();
                ucmaEndpoint = new Endpoint(ucmaPlatform.CollabPlatform);
                AlertsManager alertsManager = new AlertsManager(ucmaEndpoint);

                foreach (string user in usersList)
                {
                    Log.WriteLogEntry("INFO", String.Format("Processing user {0}...", user.ToLowerInvariant()), String.Format("Processing user {0}...", user.ToLowerInvariant()));

                    try
                    {
                        alertsManager.SetAlertNotificationSetting(user, notifyAdditionToContactList, alertsWhenDoNotDisturb);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLogEntry("ERROR", String.Format("Error while processing user {0}: {1}", user.ToLowerInvariant(), ex.Message), String.Format("Error while processing user {0}", user.ToLowerInvariant()));
                    }
                }
            }
            else
            {
                Log.WriteLogEntry("WARNING", "Users file is empty or invalid. No action will be performed.");
            }

        }
    }
}
