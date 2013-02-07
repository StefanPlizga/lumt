// DISCLAIMER
// This source code is provided as a sample. It is licensed "as-is."
// You bear the risk of using it. The contributors give no express warranties, 
// guarantees or conditions. See MS-PL license description for more details.

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Rtc.Collaboration.Presence;
using LUMT.ItemClass;
using LUMT.Enum;

namespace LUMT.Misc
{
    class FileAccess
    {
        public static List<string> GetUsersFileData(string usersFilePath)
        {
            List<string> usersList = new List<string>();

            TextReader usersFile;

            usersFile = new StreamReader(usersFilePath);
            string userSipAddress;

            int line = 1;

            while ( (userSipAddress = usersFile.ReadLine()) != null)
            {
                // Ignore empty lines
                userSipAddress = userSipAddress.Trim();

                if (userSipAddress.Length > 0)
                {
                    if (IsValidUserEntry(userSipAddress))
                    {
                        usersList.Add(userSipAddress);
                    }
                    else
                    {
                        throw new Exception(String.Format("Users file content is incorrect at line {0}. Only SIP: entries are supported.", line));
                    }
                }

                line++;
            }

            return usersList;
        }

        public static List<ContactItem> GetContactsFileData(string contactsFilePath)
        {
            List<ContactItem> contactsList = new List<ContactItem>();

            TextReader contactsFile;

            contactsFile = new StreamReader(contactsFilePath);
            string contactSipAddress;
            ContactItem contactItem = null;

            int line = 1;

            while ((contactSipAddress = contactsFile.ReadLine()) != null)
            {
                contactSipAddress = contactSipAddress.Trim();

                if (contactSipAddress.Trim().Length > 0)
                {
                    if (IsValidContactEntry(contactSipAddress))
                    {
                        string[] contactInfo = contactSipAddress.Split(':');

                        if (contactInfo.Length == 2)
                        {
                            switch (contactInfo[0].ToLowerInvariant())
                            {
                                case "sip":
                                    contactItem = new ContactItem("sip:" + contactInfo[1], ContactType.Contact);
                                    break;
                                case "smtp":
                                    contactItem = new ContactItem(contactInfo[1], ContactType.DistributionList);
                                    break;
                                default:
                                    // should never happen
                                    break;
                            }
                        }
                        else
                        {
                            throw new Exception(String.Format("Contacts file content is incorrect at line {0}. Only SIP: and SMTP: entries are supported.", line));
                        }

                        contactsList.Add(contactItem);
                    }
                    else
                    {
                        throw new Exception(String.Format("Contacts file content is incorrect at line {0}. Only SIP: and SMTP: entries are supported.", line));
                    }
                }

                line++;
            }

            return contactsList;
        }

        private static bool IsValidEntry(bool userEntry, string entry)
        {
            // UserEntry can start with sip:
            // ContactEntry can start with sip: (contact) or smtp: (distribution list)
            string regExPattern = (userEntry?"^sip":"^(sip|smtp)") + @":[\w.-]+@[\w.-]+\.[a-z]+$";
            
            Regex regex = new Regex(regExPattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(entry);
        }

        private static bool IsValidUserEntry(string entry)
        {
            return IsValidEntry(true, entry);
        }

        private static bool IsValidContactEntry(string entry)
        {
            return IsValidEntry(false, entry);
        }

        private static bool IsValidDomainEntry(string entry)
        {
            string regExPattern = @"^[\w.-]+\.[a-z]+$";

            Regex regex = new Regex(regExPattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(entry);
        }
        
        public static List<ACEItem> GetACLFileData(string aclFilePath)
        {
            List<ACEItem> aceList = new List<ACEItem>();

            TextReader aclFile;

            aclFile = new StreamReader(aclFilePath);

            string aceLine;
            ACEItem aceItem = null;

            int line = 1;

            while ((aceLine = aclFile.ReadLine()) != null)
            {
                aceLine = aceLine.Trim();

                if (aceLine.Trim().Length > 0)
                {
                    string[] aceDetails = aceLine.Split(' ');

                    if ((aceDetails.Length == 2) || (aceDetails.Length == 3))
                    {
                        string userOrDomainValue = "";
                        ACEType aceType;
                        PresenceRelationshipLevel presenceRelationshipLevel;

                        switch (aceDetails[0].ToLowerInvariant())
                        {
                            case "user":
                                if (aceDetails.Length != 3)
                                {
                                    throw new Exception(String.Format("Error at line {0}: an ACE with type User must have 3 values.", line));
                                }
                                aceType = ACEType.User;

                                if (IsValidUserEntry(aceDetails[2]))
                                {
                                    userOrDomainValue = aceDetails[2];
                                }
                                else
                                {
                                    throw new Exception(String.Format("ACL file content is incorrect at line {0}. Only SIP: entries are supported for ACE of type User.", line));
                                }

                                break;
                            case "domain":
                                if (aceDetails.Length != 3)
                                {
                                    throw new Exception(String.Format("Error at line {0}: an ACE with type Domain must have 3 values.", line));
                                }

                                if (IsValidDomainEntry(aceDetails[2]))
                                {
                                    userOrDomainValue = aceDetails[2];
                                }
                                else
                                {
                                    throw new Exception(String.Format("ACL file content is incorrect at line {0}. Invalid value for ACE of type Domain.", line));
                                }

                                aceType = ACEType.Domain;
                                break;
                            case "company":
                                if (aceDetails.Length != 2)
                                {
                                    throw new Exception(String.Format("Error at line {0}: an ACE with type Company must have 2 values.", line));
                                }
                                aceType = ACEType.Company;
                                break;
                            case "publicdomains":
                                if (aceDetails.Length != 2)
                                {
                                    throw new Exception(String.Format("Error at line {0}: an ACE with type PublicDomains must have 2 values.", line));
                                }
                                aceType = ACEType.PublicDomains;
                                break;
                            case "federateddomains":
                                if (aceDetails.Length != 2)
                                {
                                    throw new Exception(String.Format("Error at line {0}: an ACE with type FederatedDomains must have 2 values.", line));
                                }
                                aceType = ACEType.FederatedDomains;
                                break;
                            default:
                                throw new Exception(String.Format("Error at line {0}: incorrect value for ACE type. Value can be User, Domain, Company, FederatedDomains or PublicDomains.", line));
                        }

                        switch (aceDetails[1].ToLowerInvariant())
                        {
                            case "personal":
                                presenceRelationshipLevel = PresenceRelationshipLevel.Personal;
                                break;
                            case "workgroup":
                                presenceRelationshipLevel = PresenceRelationshipLevel.Workgroup;
                                break;
                            case "colleagues":
                                presenceRelationshipLevel = PresenceRelationshipLevel.Colleagues;
                                break;
                            case "external":
                                presenceRelationshipLevel = PresenceRelationshipLevel.External;
                                break;
                            case "blocked":
                                presenceRelationshipLevel = PresenceRelationshipLevel.Blocked;
                                break;
                            default:
                                throw new Exception(String.Format("Error at line {0}: incorrect value for Relationship Level. Value can be Personal, Workgroup, Colleagues, External or Blocked.", line));
                        }

                        // If here, it does mean values are correct because no Exception was thrown
                        aceItem = new ACEItem(aceType, userOrDomainValue, presenceRelationshipLevel);
                        aceList.Add(aceItem);
                    }
                    else
                    {
                        throw new Exception(String.Format("ACL file content is incorrect at line {0}. Every line must have 2 or 3 values.", line));
                    }
                }

                line++;
            }

            return aceList;
        }
    }
}
