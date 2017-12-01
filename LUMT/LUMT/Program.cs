// DISCLAIMER
// This source code is provided as a sample. It is licensed "as-is."
// You bear the risk of using it. The contributors give no express warranties, 
// guarantees or conditions. See MS-PL license description for more details.

using System;
using LUMT.App;
using LUMT.Misc;

namespace LUMT
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                if ( (args[0].ToLowerInvariant() == "/help") || (args[0].ToLowerInvariant() == "/?") )
                {
                    ShowHelp();
                    return;
                }

                if (args[0].ToLowerInvariant() == "/fullhelp")
                {
                    ShowHelp("", true);
                    return;
                }
            }

            if ((args.Length < 2) || (args.Length > 5))
            {
                ShowHelp("Wrong number of arguments.");
                return;
            }

            bool executeLUMT = false;

            string mode = GetParamValue("Mode", args);
            string usersFile = GetParamValue("UsersFile", args);
            string contactsFile = GetParamValue("ContactsFile", args);
            string contactsGroup = GetParamValue("ContactsGroup", args);
            string privacyPreference = GetParamValue("PrivacyPreference", args);
            string notifyAdd = GetParamValue("NotifyAdd", args);
            string notifyWhenDND = GetParamValue("NotifyWhenDND", args);
            string aclFile = GetParamValue("ACLFile", args);
            bool delete = GetParamPresence("Delete", args);

            switch (mode.ToLowerInvariant())
            {
                case "":
                    ShowHelp("Invalid argument - Mode parameter not specified.");
                    break;
                case "contact":
                    if (ValidateParameters(args, new string[] {"Mode", "UsersFile", "ContactsFile", "ContactsGroup"}, new string[] {"Delete"}))
                    {
                        // Check for mandatory arguments
                        if ( (usersFile.Length > 0) && (contactsFile.Length > 0) )
                        {
                            executeLUMT = true;
                        }
                        else
                        {
                            ShowHelp("Invalid argument - UsersFile or ContactsFile parameter not specified.");
                        }
                    }
                    else
                    {                        
                        ShowHelp("One or more arguments are invalid.");
                    }
                    break;
                case "acl":
                     if (ValidateParameters(args, new string[] {"Mode", "UsersFile", "ACLFile"}, new string[] {"Delete"}))
                    {
                        // Check for mandatory arguments
                        if ( (usersFile.Length > 0) && (aclFile.Length > 0) )
                        {
                            executeLUMT = true;
                        }
                        else
                        {
                            ShowHelp("Invalid argument - UsersFile or ACEsFile parameter not specified.");
                        }
                    }
                    else
                    {                        
                        ShowHelp("One or more arguments are invalid.");
                    }
                    break;
                case "privacy":
                    if (ValidateParameters(args, new string[] {"Mode", "UsersFile", "PrivacyPreference"}, new string[] {""}))
                    {
                        // Check for mandatory arguments
                        if ( (usersFile.Length > 0) && (privacyPreference.Length > 0) )
                        {
                            executeLUMT = true;
                            switch (privacyPreference.ToLowerInvariant())
                            {
                                case "default":
                                    break;
                                case "private":
                                    break;
                                case "public":
                                    break;
                                default:
                                    executeLUMT = false;
                                    ShowHelp("Incorrect value for PrivacyPreference parameter. Value can be Default, Private or Public.");
                                    break;
                            }
                        }
                        else
                        {
                            ShowHelp("Invalid argument - UsersFile or PrivacyPreference parameter not specified.");
                        }
                    }
                    else
                    {                        
                        ShowHelp("One or more arguments are invalid.");
                    }
                    break;
                case "alerts":
                    if (ValidateParameters(args, new string[] { "Mode", "UsersFile", "NotifyAdd", "NotifyWhenDND" }, new string[] { "" }))
                    {
                        // Check for mandatory arguments
                        if ((usersFile.Length > 0) && ( (notifyAdd.Length > 0) || (notifyWhenDND.Length > 0) ) )
                        {
                            executeLUMT = true;

                            if (notifyAdd.Length > 0)
                            {
                                switch (notifyAdd.ToLowerInvariant())
                                {
                                    case "yes":
                                        break;
                                    case "no":
                                        break;
                                    default:
                                        executeLUMT = false;
                                        ShowHelp("Incorrect value for NotifyAdd parameter. Value can be Yes or No.");
                                        break;
                                }
                            }


                            if (notifyWhenDND.Length > 0)
                            {
                                switch (notifyWhenDND.ToLowerInvariant())
                                {
                                    case "noalerts":
                                        break;
                                    case "allalerts":
                                        break;
                                    case "alertsfromworkgroup":
                                        break;
                                    default:
                                        executeLUMT = false;
                                        ShowHelp("Incorrect value for NotifyWhenDND parameter. Value can be AllAlerts, AlertsFromWorkgroup or NoAlerts.");
                                        break;
                                }
                            }
                        }
                        else
                        {
                            ShowHelp("Invalid argument - UsersFile, NotifyAdd or NotifyWhenDND parameter not specified.");
                        }
                    }
                    else
                    {
                        ShowHelp("One or more arguments are invalid.");
                    }
                    break;
                default:
                    ShowHelp("Invalid argument - Invalid value specified for Mode parameter.");
                    break;
            }

            if (executeLUMT)
            {
                LumtApp lumtApp = new LumtApp();
                lumtApp.InitializeEnvironment();

                switch (mode.ToLowerInvariant())
                {
                    case "contact":
                        lumtApp.ProcessContacts(usersFile, contactsFile, contactsGroup, delete);
                        break;
                    case "acl":
                        lumtApp.ProcessACEs(usersFile, aclFile, delete);
                        break;
                    case "privacy":
                        lumtApp.ProcessPrivacy(usersFile, privacyPreference);
                        break;
                    case "alerts":
                        lumtApp.ProcessAlertNotifications(usersFile, notifyAdd, notifyWhenDND);
                        break;
                    default:
                        // Should never happen
                        break;
                }

                lumtApp.CleanupEnvironment();
            }
        }

        public static void CloseApplicationOnError()
        {
            Console.WriteLine("Review the log file for more details on the error.");
            Log.LogFileClose();
            Environment.Exit(1);
        }

        private static bool ValidateParameters(string[] args, string[] paramsWithValue, string[] paramsWithoutValue)
        {
            foreach (string arg in args)
            {
                bool argFound = false;

                foreach (string param in paramsWithValue)
                {
                    string paramFull = "/" + param + ":";

                    if (arg.ToLowerInvariant().StartsWith(paramFull.ToLowerInvariant()))
                    {
                        if (arg.Length > paramFull.Length)
                        {
                            argFound = true;
                            break;
                        }
                    }
                }

                if (!argFound)
                {
                    foreach (string param in paramsWithoutValue)
                    {
                        string paramFull = "/" + param;

                        if (arg.ToLowerInvariant() == paramFull.ToLowerInvariant())
                        {
                            argFound = true;
                            break;
                        }
                    }
                }

                if (!argFound)
                {
                    return false;
                }

            }

            return true;
        }

        private static string GetParamValue(string paramName, string[] args)
        {
            string paramValue = "";

            paramName = "/" + paramName + ":";

            foreach (string arg in args)
            {
                if (arg.ToLowerInvariant().StartsWith(paramName.ToLowerInvariant()))
                {
                    if (arg.Length > paramName.Length)
                    {
                        paramValue = arg.Substring(paramName.Length);
                    }
                    break;
                }
            }

            return paramValue;
        }

        private static bool GetParamPresence(string paramName, string[] args)
        {
            bool paramPresent = false;

            paramName = "/" + paramName;

            foreach (string arg in args)
            {
                if (arg.ToLowerInvariant() == paramName.ToLowerInvariant())
                {
                    paramPresent = true;
                    break;
                }
            }

            return paramPresent;
        }   

        private static void ShowHelp(string customMessage = "", bool fullHelp = false)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            if (customMessage.Length > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(customMessage);
                Console.WriteLine();
                Console.ForegroundColor = originalColor;
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(String.Format("{0} {1} - Help", LumtGlobals.ApplicationFullName, version));
            Console.ForegroundColor = originalColor;
            Console.WriteLine("");
            Console.WriteLine("Description");
            Console.WriteLine(String.Format("    {0} tool can be used to manage contacts, ACEs, privacy settings or alert\n    notification settings for Skype for Business Server 2015 users.", LumtGlobals.ApplicationShortName));
            Console.WriteLine("");
            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("DISCLAIMER");
            Console.WriteLine("This source code is provided as a sample. It is licensed \"as-is.\"");
            Console.WriteLine("You bear the risk of using it. The contributors give no express warranties,");
            Console.WriteLine("guarantees or conditions. See MS-PL license description for more details.");
            Console.ForegroundColor = originalColor;
            Console.WriteLine("");
            Console.WriteLine("");
           
            Console.WriteLine("Usage");
            Console.WriteLine(String.Format("    {0}.exe /Mode:<Mode> /UsersFile:<UsersFileName>", LumtGlobals.ApplicationShortName));
            Console.WriteLine("    [/ContactsFile:<ContactsFileName>] [/ContactsGroup:<ContactsGroupName>]");
            Console.WriteLine("    [/ACLFile:<ACLFileName>] [/Delete] [/PrivacyPreference:<Privacy>]");
            Console.WriteLine("    [/NotifyAdd:<NotifyAddToContactList>] [/NotifyWhenDND:<DisplayWhenDND>]");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Generic Parameter List");
            Console.WriteLine(String.Format("    Mode\t\tSettings to manage with {0}. Available options are:", LumtGlobals.ApplicationShortName));
            Console.WriteLine(String.Format("\t\t\t- Contact: {0} is used to add or remove contacts for\n\t\t\t  Skype for Business Server users.", LumtGlobals.ApplicationShortName));
            Console.WriteLine(String.Format("\t\t\t- ACL: {0} is used to add or remove ACEs on Lync\n\t\t\t  Server users.", LumtGlobals.ApplicationShortName));
            Console.WriteLine(String.Format("\t\t\t- Privacy: {0} is used to manage privacy settings for\n\t\t\t  Skype for Business Server users.", LumtGlobals.ApplicationShortName));
            Console.WriteLine(String.Format("\t\t\t- Alerts: {0} is used to manage alert notification\n\t\t\t  settings for Skype for Business Server users.", LumtGlobals.ApplicationShortName));
            Console.WriteLine("");
            Console.WriteLine("    UsersFile\t\tPath to the text file containing user SIP addresses\n\t\t\tbeginning with sip:");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Contact Mode Parameter List");
            Console.WriteLine("    ContactsFile\tPath to the text file containing contact SIP addresses\n\t\t\tbeginning with sip: or distribution list addresses\n\t\t\tbeginning with smtp:");
            Console.WriteLine("");
            Console.WriteLine("    ContactsGroup\tOptional. Name of the group to add contacts. This\n\t\t\tparameter is ignored when the Delete switch is used.");
            Console.WriteLine("");
            Console.WriteLine("    Delete\t\tOptional. Delete contacts from the user's contact list\n\t\t\tinstead of adding them.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("ACL Mode Parameter List");
            Console.WriteLine(String.Format("    ACLFile\t\tPath to the text file containing ACEs for users.\n\t\t\tRead Full Help ({0}.exe /FullHelp) in order to see\n\t\t\tinput file requirements.", LumtGlobals.ApplicationShortName));
            Console.WriteLine("");
            Console.WriteLine("    Delete\t\tOptional. Delete ACEs from the user's contact list\n\t\t\tinstead of adding them. When the Delete switch is used,\n\t\t\tthe RelationshipLevel level parameter is ignored: this\n\t\t\tmeans ACES specified are removed no matter what\n\t\t\trelationship they currently have.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Privacy Mode Parameter List");
            Console.WriteLine("    PrivacyPreference\tPrivacy setting to set for the user. Valid\n\t\t\tvalues are Private, Public or Default.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Alerts Mode Parameter List");
            Console.WriteLine("    NotifyAdd\t\tAlert display setting to set for the user when someone\n\t\t\tadds him or her to the cocntact list. Valid values are\n\t\t\tYes or No.");
            Console.WriteLine("    NotifyWhenDND\tAlert display setting to set fgor the user when his or\n\t\t\ther status is set to DND. Valid values are AllAlerts,\n\t\t\tAlertsFromWorkgroup or NoAlerts.");
            Console.WriteLine("");
            Console.WriteLine("Note: Either NotifyAdd, NotifyWhenDND or both parameters can be specified in\n\t\t\tthe Alerts mode.");
            Console.WriteLine("");

            if (fullHelp)
            {
                Console.WriteLine("");
                Console.WriteLine("Examples");
                Console.WriteLine(String.Format("    {0} /Mode:Contact /UsersFile:Users.txt /ContactsFile:Contacts.txt", LumtGlobals.ApplicationShortName));
                Console.WriteLine(String.Format("    {0} /Mode:Contact /UsersFile:Users.txt /ContactsFile:Contacts.txt\n         /ContactsGroup:Support", LumtGlobals.ApplicationShortName));
                Console.WriteLine(String.Format("    {0} /Mode:Contact /UsersFile:Users.txt /ContactsFile:Contacts.txt /Delete", LumtGlobals.ApplicationShortName));
                Console.WriteLine(String.Format("    {0} /Mode:ACL /UsersFile:Users.txt /ACLFile:ACL.txt", LumtGlobals.ApplicationShortName));
                Console.WriteLine(String.Format("    {0} /Mode:ACL /UsersFile:Users.txt /ACLFile:ACL.txt /Delete", LumtGlobals.ApplicationShortName));
                Console.WriteLine(String.Format("    {0} /Mode:Privacy /UsersFile:Users.txt /PrivacyPreference:Private", LumtGlobals.ApplicationShortName));
                Console.WriteLine(String.Format("    {0} /Mode:Privacy /UsersFile:Users.txt /PrivacyPreference:Public", LumtGlobals.ApplicationShortName));
                Console.WriteLine(String.Format("    {0} /Mode:Privacy /UsersFile:Users.txt /PrivacyPreference:Default", LumtGlobals.ApplicationShortName));
                Console.WriteLine(String.Format("    {0} /Mode:Alerts /UsersFile:Users.txt /NotifyAdd:No", LumtGlobals.ApplicationShortName));
                Console.WriteLine(String.Format("    {0} /Mode:Alerts /UsersFile:Users.txt /NotifyWhenDND:AlertsFromWorkgroup", LumtGlobals.ApplicationShortName));
                Console.WriteLine(String.Format("    {0} /Mode:Alerts /UsersFile:Users.txt /NotifyAdd:Yes /NotifyWhenDND:NoAlerts", LumtGlobals.ApplicationShortName));
                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("UsersFile requirements");
                Console.WriteLine("    Every line must contain only 1 SIP user. Format must be sip:user@domain.com");
                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("ContactsFile requirements");
                Console.WriteLine("    Every line must contain only 1 contact. Contact can be 1 of 2 types:");
                Console.WriteLine("    - SIP user: format must be sip:user@domain.com");
                Console.WriteLine("    - Distribution list: format must be smtp:dl@domain.com");
                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("ACLFile requirements");
                Console.WriteLine("    Every line must contain only 1 ACE. ACE input format is:");
                Console.WriteLine("    ACEType RelationshipLevel UserOrDomain");
                Console.WriteLine("    Each value must be separated by a single white space character.");
                Console.WriteLine("");
                Console.WriteLine("    ACEType value can be:");
                Console.WriteLine("    - User: ACE applies to a SIP user in the contact list");
                Console.WriteLine("    - Domain: ACE applies to a SIP domain in the contact list");
                Console.WriteLine("    - Company: ACE applies to SIP users who are part of the same company");
                Console.WriteLine("    - FederatedDomains: ACE applies to SIP users who are part of all federated\n      companies");
                Console.WriteLine("    - PublicDomains: ACE applies to SIP users who are part of public IM domains");
                Console.WriteLine("");
                Console.WriteLine("    RelationshipLevel value can be:");
                Console.WriteLine("    - Personal: Friend and Family relationship level");
                Console.WriteLine("    - Workgroup: Workgroup relationship level");
                Console.WriteLine("    - Colleagues: Colleagues (or Company) relationship level");
                Console.WriteLine("    - External: External relationship level");
                Console.WriteLine("    - Blocked: Blocked relationship level");
                Console.WriteLine("");
                Console.WriteLine("    UserOrDomain value can be:");
                Console.WriteLine("    - If ACEType is User: SIP user to which apply the ACL. Format must be\n      sip:user@domain.com");
                Console.WriteLine("    - If ACEType is Domain: domain to which apply the ACL. Format must be\n      domain.com");
                Console.WriteLine("    - If ACEType is Company, FederatedDomains or PublicDomains, don't specify\n      any value");
                Console.WriteLine("");
            }
            else
            {
                Console.WriteLine(String.Format("For Full Help, execute {0}.exe /FullHelp", LumtGlobals.ApplicationShortName));
                Console.WriteLine("");
            }
        }

    }
}
