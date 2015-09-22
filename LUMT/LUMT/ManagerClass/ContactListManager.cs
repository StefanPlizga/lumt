// DISCLAIMER
// This source code is provided as a sample. It is licensed "as-is."
// You bear the risk of using it. The contributors give no express warranties, 
// guarantees or conditions. See MS-PL license description for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Rtc.Collaboration.Presence;
using Microsoft.Rtc.Collaboration.ContactsGroups;
using LUMT.UCMA;
using LUMT.ItemClass;
using LUMT.Enum;

namespace LUMT.ManagerClass
{
    class ContactListManager
    {
        Endpoint userEndpoint;

        public ContactListManager(Endpoint endpoint)
        {
            userEndpoint = endpoint;
        }

        public void AddContacts(string userUri, List<ContactItem> contactsUriList, string contactsGroupName)
        {
            userEndpoint.InitializeUserEndpoint(userUri);

            List<string> contactsGroupList = new List<string>();
            // A contact is always member of the default group "~"
            contactsGroupList.Add("~");

            if ((contactsGroupName.Length > 0) && (contactsGroupName != "~"))
            {
                contactsGroupList.Add(contactsGroupName);
            }

            if (userEndpoint.User.ContactGroupServices.CurrentState == CollaborationSubscriptionState.Subscribed)
            {
                AddContactsToList(contactsUriList);
                AddGroupsToList(contactsGroupList);
                AssignContactsToGroups(contactsUriList, contactsGroupList);
            }
            else
            {
                throw new Exception("Collaboration subscription state is not subscribed");
            }
        }

        private void AddContactsToList(List<ContactItem> contactItemList)
        {
            // Add contacts that are not already in the contact list
            foreach (ContactItem contactItem in contactItemList)
            {
                switch (contactItem.ContactType)
                {
                    case ContactType.Contact:
                        // Avoid adding the contact to the same user
                        if (contactItem.ContactUri.ToLowerInvariant() != userEndpoint.User.OwnerUri.ToLowerInvariant())
                        {
                            int i = 0;
                            while (userEndpoint.UserContacts.Count > i)
                            {
                                if (userEndpoint.UserContacts[i].Uri.ToLowerInvariant() != contactItem.ContactUri.ToLowerInvariant())
                                {
                                    i++;
                                }
                                else
                                {
                                    break;
                                }
                            }

                            if (i >= userEndpoint.UserContacts.Count) // No match
                            {
                                if (userEndpoint.User.ContactGroupServices.CurrentState == CollaborationSubscriptionState.Subscribed)
                                {
                                    userEndpoint.User.ContactGroupServices.EndAddContact(userEndpoint.User.ContactGroupServices.BeginAddContact(contactItem.ContactUri, null, null));
                                    userEndpoint.RefreshContactList();
                                }
                                else
                                {
                                    throw new Exception("Collaboration subscription state is not subscribed");
                                }
                            }
                        }
                        break;

                    case ContactType.DistributionList:
                        // DL is specified as a contact in the ContactsFile but has to be processed as a special group inside the contact list
                        AddDLToList(contactItem.ContactUri);
                        break;

                    default:
                        break;
                }
            }
        }

        private void AddGroupsToList(List<string> contactsGroupList)
        {
            // add groups that are not already in the contact list
            foreach (string contactsGroupName in contactsGroupList)
            {
                int i = 0;
                while (userEndpoint.UserGroups.Count > i)
                {
                    if (userEndpoint.UserGroups[i].Name.ToLowerInvariant() != contactsGroupName.ToLowerInvariant())
                    {
                        i++;
                    }
                    else
                    {
                        break;
                    }

                }

                if (i >= userEndpoint.UserGroups.Count) // No match
                {
                    if (userEndpoint.User.ContactGroupServices.CurrentState == CollaborationSubscriptionState.Subscribed)
                    {
                        // "Pinned Contacts" are "Favorites" in Lync. They have special xml data that must be set at group creation time.
                        if (contactsGroupName.ToLowerInvariant() == "pinned contacts")
                        {
                            userEndpoint.User.ContactGroupServices.EndAddGroup(userEndpoint.User.ContactGroupServices.BeginAddGroup(contactsGroupName, "<groupExtension groupType=\"pinnedGroup\"><email/></groupExtension>", null, null));
                        }
                        else
                        {
                            userEndpoint.User.ContactGroupServices.EndAddGroup(userEndpoint.User.ContactGroupServices.BeginAddGroup(contactsGroupName, "", null, null));
                        }
                        userEndpoint.RefreshContactList();
                    }
                    else
                    {
                        throw new Exception("Collaboration subscription state is not subscribed");
                    }
                }
            }
        }

        private void AddDLToList(string dlEmail)
        {
            // add groups that are not already in the contact list
            int i = 0;

            string dlData = String.Format("<groupExtension groupType=\"dg\"><email>{0}</email></groupExtension>", dlEmail);

            while (userEndpoint.UserGroups.Count > i)
            {
                if (userEndpoint.UserGroups[i].GroupData.ToLowerInvariant() != dlData.ToLowerInvariant())
                {
                    i++;
                }
                else
                {
                    break;
                }

            }

            if (i >= userEndpoint.UserGroups.Count) // No match
            {
                if (userEndpoint.User.ContactGroupServices.CurrentState == CollaborationSubscriptionState.Subscribed)
                {
                    userEndpoint.User.ContactGroupServices.EndAddGroup(userEndpoint.User.ContactGroupServices.BeginAddGroup(dlEmail, dlData, null, null));
                    userEndpoint.RefreshContactList();
                }
                else
                {
                    throw new Exception("Collaboration subscription state is not subscribed");
                }
            }
        }

        private void AssignContactsToGroups(List<ContactItem> contactItemList, List<string> contactsGroupList)
        {
            foreach (ContactItem contactItem in contactItemList)
            {
                if (contactItem.ContactType == ContactType.Contact)
                {
                    // Avoid adding the contact to the same user
                    if (contactItem.ContactUri.ToLowerInvariant() != userEndpoint.User.OwnerUri.ToLowerInvariant())
                    {
                        Contact contact = userEndpoint.User.ContactGroupServices.EndGetCachedContact(userEndpoint.User.ContactGroupServices.BeginGetCachedContact(
                            contactItem.ContactUri, null, null));

                        List<int> contactGroups = new List<int>(contact.GroupIds);

                        // Look for the group from the name
                        foreach (string contactsGroupName in contactsGroupList)
                        {
                            int i = 0;
                            while (userEndpoint.UserGroups.Count > i)
                            {
                                if (userEndpoint.UserGroups[i].Name.ToLowerInvariant() != contactsGroupName.ToLowerInvariant())
                                {
                                    i++;
                                }
                                else
                                {
                                    int groupId = userEndpoint.UserGroups[i].GroupId;
                                    if (!contactGroups.Contains(groupId))
                                    {
                                        contactGroups.Add(groupId);
                                    }
                                    break;
                                }

                                if (i >= userEndpoint.UserGroups.Count)
                                {
                                    // Error
                                }
                            }
                        }

                        if (userEndpoint.User.ContactGroupServices.CurrentState == CollaborationSubscriptionState.Subscribed)
                        {
                            // Assign groups to the contact
                            contact.GroupIds = contactGroups.ToArray();
                            userEndpoint.User.ContactGroupServices.EndUpdateContact(userEndpoint.User.ContactGroupServices.BeginUpdateContact(contact, null, null));
                            userEndpoint.RefreshContactList();
                        }
                        else
                        {
                            throw new Exception("Collaboration subscription state is not subscribed");
                        }
                    }
                }
            }
        }

        public void RemoveContacts(string userUri, List<ContactItem> contactItemList)
        {
            userEndpoint.InitializeUserEndpoint(userUri);

            // Ensure the contact is in the contact list
            foreach (ContactItem contactItem in contactItemList)
            {
                switch (contactItem.ContactType)
                {
                    case ContactType.Contact:
                        int i = 0;
                        while (userEndpoint.UserContacts.Count > i)
                        {
                            if (userEndpoint.UserContacts[i].Uri.ToLowerInvariant() != contactItem.ContactUri.ToLowerInvariant())
                            {
                                i++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (i >= userEndpoint.UserContacts.Count)
                        {
                            // Contact is not in the contact list
                            // Do nothing
                        }
                        else
                        {
                            if (userEndpoint.User.ContactGroupServices.CurrentState == CollaborationSubscriptionState.Subscribed)
                            {
                                userEndpoint.User.ContactGroupServices.EndDeleteContact(userEndpoint.User.ContactGroupServices.BeginDeleteContact(contactItem.ContactUri, null, null));
                                userEndpoint.RefreshContactList();
                            }
                            else
                            {
                                throw new Exception("Collaboration subscription state is not subscribed");
                            }
                        }
                        break;

                    case ContactType.DistributionList:
                        RemoveDLFromList(contactItem.ContactUri);
                        break;

                    default:
                        // should never happen
                        break;
                }
            }
        }

        private void RemoveDLFromList(string dlEmail)
        {
            int i = 0;
            int groupIdToRemove = -1;

            string dlData = String.Format("<groupExtension groupType=\"dg\"><email>{0}</email></groupExtension>", dlEmail);

            while (userEndpoint.UserGroups.Count > i)
            {
                if (userEndpoint.UserGroups[i].GroupData.ToLowerInvariant() != dlData.ToLowerInvariant())
                {
                    i++;
                }
                else
                {
                    groupIdToRemove = userEndpoint.UserGroups[i].GroupId;
                    break;
                }
            }

            if (i >= userEndpoint.UserGroups.Count)
            {
                // DL is not in the contact list
                // Do nothing
            }
            else
            {
                if (userEndpoint.User.ContactGroupServices.CurrentState == CollaborationSubscriptionState.Subscribed)
                {
                    userEndpoint.User.ContactGroupServices.EndDeleteGroup(userEndpoint.User.ContactGroupServices.BeginDeleteGroup(groupIdToRemove, null, null));
                    userEndpoint.RefreshContactList();
                }
                else
                {
                    throw new Exception("Collaboration subscription state is not subscribed");
                }
            }
        }

    }
}
