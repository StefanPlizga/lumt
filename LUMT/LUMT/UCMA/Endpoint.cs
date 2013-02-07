// DISCLAIMER
// This source code is provided as a sample. It is licensed "as-is."
// You bear the risk of using it. The contributors give no express warranties, 
// guarantees or conditions. See MS-PL license description for more details.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Rtc.Signaling;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.ContactsGroups;
using Microsoft.Rtc.Collaboration.Presence;
using System.Threading;

namespace LUMT.UCMA
{
    class Endpoint
    {
        private UserEndpoint user;
        private CollaborationPlatform collabPlatform;
        private List<Contact> userContacts = new List<Contact>();
        private List<Group> userGroups = new List<Group>();
        private List<ContainerMembership> userContainerMemberships = new List<ContainerMembership>();
        private PresenceCategoryWithMetaData alertNotification;

        private ManualResetEvent waitForContactListUpdate = new ManualResetEvent(false);
        private ManualResetEvent waitForGroupDeletion = new ManualResetEvent(false);
        private ManualResetEvent waitForContainerMembershipsUpdate = new ManualResetEvent(false);
        private ManualResetEvent waitForAlertNotificationUpdate = new ManualResetEvent(false);

        public UserEndpoint User
        {
            get { return user; }
        }
        
        public List<Contact> UserContacts
        {
            get { return userContacts; }
        }

        public List<Group> UserGroups
        {
            get { return userGroups; }
        }

        public List<ContainerMembership> UserContainerMemberships
        {
            get { return userContainerMemberships; }
        }

        public PresenceCategoryWithMetaData AlertNotification
        {
            get { return alertNotification; }
        }

        public Endpoint(CollaborationPlatform platform)
        {
            collabPlatform = platform;
        }

        public void InitializeUserEndpoint(string userURI)
        {
            // Initialize the User Endpoint if never initialized
            if (user == null)
            {
                CreateUserEndpoint(userURI);
                SubscribeToContactList();
            }
            else
            {
                // If User Endpoint is already initialized but for another user, do initialization
                if (user.OwnerUri.ToLowerInvariant() != userURI.ToLowerInvariant())
                {
                    CreateUserEndpoint(userURI);
                    SubscribeToContactList();
                }
                else
                {
                    // Otherwise, only refresh contact list
                    RefreshContactList();
                }
            }
        }

        public void CreateUserEndpoint(string userURI)
        {
            UserEndpointSettings settings = new UserEndpointSettings(userURI);

            user = new UserEndpoint(collabPlatform, settings);
            user.EndEstablish(user.BeginEstablish(null, null));
            if (user.State != LocalEndpointState.Established)
            {
                throw new Exception("Local endpoint state is not established");
            }

            alertNotification = null;
        }

        void ContactGroupServices_NotificationReceived(object sender, ContactGroupNotificationEventArgs e)
        {
            if (e.IsFullNotification)
            {
                user.ContactGroupServices.NotificationReceived -= ContactGroupServices_NotificationReceived;

                userContacts.Clear();
                userGroups.Clear();

                for (int i = 0; i < e.Contacts.Count; i++)
                {
                    userContacts.Add(e.Contacts[i].Item);
                }

                for (int i = 0; i < e.Groups.Count; i++)
                {
                    userGroups.Add(e.Groups[i].Item);
                }

                waitForContactListUpdate.Set();
            }

        }

        private void SubscribeToContactList()
        {
            if (user.ContactGroupServices.CurrentState != CollaborationSubscriptionState.Subscribed)
            {
                waitForContactListUpdate.Reset();
                user.ContactGroupServices.NotificationReceived += new EventHandler<ContactGroupNotificationEventArgs>(ContactGroupServices_NotificationReceived);
                user.ContactGroupServices.EndSubscribe(user.ContactGroupServices.BeginSubscribe(null, null));
                waitForContactListUpdate.WaitOne();
            }
            else
            {
                throw new Exception("Collaboration subscription state must not be subscribed at this time");
            }
        }

        public void RefreshContactList()
        {
            waitForContactListUpdate.Reset();
            user.ContactGroupServices.NotificationReceived += new EventHandler<ContactGroupNotificationEventArgs>(ContactGroupServices_NotificationReceived);
            user.ContactGroupServices.EndRefresh(user.ContactGroupServices.BeginRefresh(null, null));
            waitForContactListUpdate.WaitOne();
        }

        void LocalOwnerPresence_ContainerNotificationReceived(object sender, ContainerNotificationEventArgs e)
        {
            user.LocalOwnerPresence.ContainerNotificationReceived -= LocalOwnerPresence_ContainerNotificationReceived;

            userContainerMemberships.Clear();

            foreach (ContainerMembership containerMembership in e.ContainerList)
            {
                userContainerMemberships.Add(containerMembership);
            }

            waitForContainerMembershipsUpdate.Set();
        }

        public void SubscribeToContainerMemberships()
        {
            if (user.LocalOwnerPresence.CurrentState != CollaborationSubscriptionState.Subscribed)
            {
                waitForContainerMembershipsUpdate.Reset();
                user.LocalOwnerPresence.ContainerNotificationReceived += new EventHandler<ContainerNotificationEventArgs>(LocalOwnerPresence_ContainerNotificationReceived);
                user.LocalOwnerPresence.EndSubscribe(user.LocalOwnerPresence.BeginSubscribe(null, null));
                waitForContainerMembershipsUpdate.WaitOne();
            }
            else
            {
                throw new Exception("Collaboration subscription state is not subscribed");
            }
        }

        /// <summary>
        /// Used to get LocalOwnerPresence.CurrentState set to Subscribed
        /// As no presence status is required in this app, no event is registered
        /// </summary>
        public void SubscribeToPresence()
        {
            if (user.LocalOwnerPresence.CurrentState != CollaborationSubscriptionState.Subscribed)
            {
                waitForAlertNotificationUpdate.Reset();
                user.LocalOwnerPresence.PresenceNotificationReceived += new EventHandler<LocalPresentityNotificationEventArgs>(LocalOwnerPresence_PresenceNotificationReceived);
                user.LocalOwnerPresence.EndSubscribe(user.LocalOwnerPresence.BeginSubscribe(null, null));
                waitForAlertNotificationUpdate.WaitOne();
            }
            else
            {
                throw new Exception("Collaboration subscription state is not subscribed");
            }
        }

        void LocalOwnerPresence_PresenceNotificationReceived(object sender, LocalPresentityNotificationEventArgs e)
        {
            user.LocalOwnerPresence.PresenceNotificationReceived -= LocalOwnerPresence_PresenceNotificationReceived;

            foreach (PresenceCategoryWithMetaData category in e.SelfUsageCategories)
            {
                if (category.Name.ToLowerInvariant() == "alerts")
                {
                    alertNotification = category;
                    break;
                }
            }

            waitForAlertNotificationUpdate.Set();
        }

    }
}
