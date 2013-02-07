// DISCLAIMER
// This source code is provided as a sample. It is licensed "as-is."
// You bear the risk of using it. The contributors give no express warranties, 
// guarantees or conditions. See MS-PL license description for more details.

using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.Rtc.Collaboration.Presence;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Signaling;
using LUMT.UCMA;
using LUMT.ItemClass;
using LUMT.Enum;

namespace LUMT.ManagerClass
{
    class ACLManager
    {
        Endpoint userEndpoint;

        public ACLManager(Endpoint endpoint)
        {
            userEndpoint = endpoint;
        }

        public void AddACEs(string userUri, List<ACEItem> aceItemList)
        {
            userEndpoint.InitializeUserEndpoint(userUri);
            userEndpoint.SubscribeToContainerMemberships();

            Dictionary<int, ContainerUpdateOperation> operations = new Dictionary<int, ContainerUpdateOperation>();

            foreach (ACEItem aceItem in aceItemList)
            {
                switch (aceItem.ACEType)
                {
                    case ACEType.User:
                        // Avoid adding an ACE in the contact list for for the current user endpoint itselft
                        if (aceItem.ItemValue.ToLowerInvariant() != userEndpoint.User.OwnerUri.ToLowerInvariant())
                        {
                            if (UserACEExistsInOtherContainerMemberships(aceItem.ItemValue, aceItem.PresenceRelationshipLevel))
                            {
                                operations = CreateDeleteUserACEOperation(operations, aceItem);
                            }
                            operations = CreateAddUserACEOperation(operations, aceItem);
                        }
                        break;

                    case ACEType.Company:
                        if (SourceNetworkACEExistsInOtherContainerMemberships(SourceNetwork.SameEnterprise, aceItem.PresenceRelationshipLevel))
                        {
                            operations = CreateDeleteSourceNetworkACEOperation(operations, SourceNetwork.SameEnterprise);
                        }
                        operations = CreateAddSourceNetworkACEOperation(operations, SourceNetwork.SameEnterprise, aceItem);
                        break;

                    case ACEType.Domain:
                        if (DomainACEExistsInOtherContainerMemberships(aceItem.ItemValue, aceItem.PresenceRelationshipLevel))
                        {
                            operations = CreateDeleteDomainACEOperation(operations, aceItem);
                        }
                        operations = CreateAddDomainACEOperation(operations, aceItem);
                        break;

                    case ACEType.FederatedDomains:
                        if (SourceNetworkACEExistsInOtherContainerMemberships(SourceNetwork.Federated, aceItem.PresenceRelationshipLevel))
                        {
                            operations = CreateDeleteSourceNetworkACEOperation(operations, SourceNetwork.Federated);
                        }
                        operations = CreateAddSourceNetworkACEOperation(operations, SourceNetwork.Federated, aceItem);
                        break;

                    case ACEType.PublicDomains:
                        if (SourceNetworkACEExistsInOtherContainerMemberships(SourceNetwork.PublicCloud, aceItem.PresenceRelationshipLevel))
                        {
                            operations = CreateDeleteSourceNetworkACEOperation(operations, SourceNetwork.PublicCloud);
                        }
                        operations = CreateAddSourceNetworkACEOperation(operations, SourceNetwork.PublicCloud, aceItem);
                        break;

                    default:
                        // Should never happen
                        break;
                }
            }

            if (operations.Count > 0)
            {
                if (userEndpoint.User.LocalOwnerPresence.CurrentState == CollaborationSubscriptionState.Subscribed)
                {
                    userEndpoint.User.LocalOwnerPresence.EndUpdateContainerMembership(userEndpoint.User.LocalOwnerPresence.BeginUpdateContainerMembership(operations.Values, null, null));
                }
                else
                {
                    throw new Exception("Collaboration subscription state is not subscribed");
                }
            }
        }

        private Dictionary<int, ContainerUpdateOperation> CreateAddUserACEOperation(Dictionary<int, ContainerUpdateOperation> operations, ACEItem aceItem)
        {
            foreach (ContainerMembership container in userEndpoint.UserContainerMemberships)
            {
                if (container.ContainerId == (Int32)aceItem.PresenceRelationshipLevel)
                {
                    bool aceFound = false;

                    foreach (RealTimeAddress user in container.AllowedSubscribers)
                    {
                        if (user.Uri.ToLowerInvariant() == aceItem.ItemValue.ToLowerInvariant())
                        {
                            aceFound = true;
                            break;
                        }
                    }

                    if (!aceFound)
                    {
                        if (operations.ContainsKey(container.ContainerId))
                        {
                            operations[container.ContainerId].AddUri(aceItem.ItemValue);
                        }
                        else
                        {
                            ContainerUpdateOperation operation = new ContainerUpdateOperation(container.ContainerId);
                            operation.AddUri(aceItem.ItemValue);
                            operations.Add(container.ContainerId, operation);
                        }
                    }

                    break;
                }
            }

            return operations;
        }

        private bool UserACEExistsInOtherContainerMemberships(string userUri, PresenceRelationshipLevel containerIdToExcludeFromSearch)
        {
            bool aceExists = false;

            foreach (ContainerMembership container in userEndpoint.UserContainerMemberships)
            {
                if (container.ContainerId != (Int32)containerIdToExcludeFromSearch)
                {
                    foreach (RealTimeAddress user in container.AllowedSubscribers)
                    {
                        if (user.Uri.ToLowerInvariant() == userUri.ToLowerInvariant())
                        {
                            aceExists = true;
                            break;
                        }
                    }
                }
            }

            return aceExists;
        }

        private Dictionary<int, ContainerUpdateOperation> CreateDeleteUserACEOperation(Dictionary<int, ContainerUpdateOperation> operations, ACEItem aceItem)
        {
            foreach (ContainerMembership container in userEndpoint.UserContainerMemberships)
            {
                bool aceFound = false;

                foreach (RealTimeAddress user in container.AllowedSubscribers)
                {
                    if (user.Uri.ToLowerInvariant() == aceItem.ItemValue.ToLowerInvariant())
                    {
                        aceFound = true;
                        break;
                    }
                }

                if (aceFound)
                {
                    if (operations.ContainsKey(container.ContainerId))
                    {
                        operations[container.ContainerId].DeleteUri(aceItem.ItemValue);
                    }
                    else
                    {
                        ContainerUpdateOperation operation = new ContainerUpdateOperation(container.ContainerId);
                        operation.DeleteUri(aceItem.ItemValue);
                        operations.Add(container.ContainerId, operation);
                    }
                    break;
                }
            }

            return operations;
        }

        private Dictionary<int, ContainerUpdateOperation> CreateAddSourceNetworkACEOperation(Dictionary<int, ContainerUpdateOperation> operations, SourceNetwork sourceNetwork, ACEItem aceItem)
        {
            foreach (ContainerMembership container in userEndpoint.UserContainerMemberships)
            {
                if (container.ContainerId == (Int32)aceItem.PresenceRelationshipLevel)
                {
                    bool itemFound = false;

                    if (((Int32)container.AllowedSourceNetworks & (Int32)sourceNetwork) == (Int32)sourceNetwork)
                    {
                        itemFound = true;
                    }

                    if (!itemFound)
                    {
                        if (operations.ContainsKey(container.ContainerId))
                        {
                            operations[container.ContainerId].AddSourceNetwork(sourceNetwork);
                        }
                        else
                        {
                            ContainerUpdateOperation operation = new ContainerUpdateOperation(container.ContainerId);
                            operation.AddSourceNetwork(sourceNetwork);
                            operations.Add(container.ContainerId, operation);
                        }
                    }

                    break;
                }
            }

            return operations;
        }

        private bool SourceNetworkACEExistsInOtherContainerMemberships(SourceNetwork sourceNetwork, PresenceRelationshipLevel containerIdToExcludeFromSearch)
        {
            bool aceExists = false;

            foreach (ContainerMembership container in userEndpoint.UserContainerMemberships)
            {
                if (container.ContainerId != (Int32)containerIdToExcludeFromSearch)
                {
                    if (((Int32)container.AllowedSourceNetworks & (Int32)sourceNetwork) == (Int32)sourceNetwork)
                    {
                        aceExists = true;
                        break;
                    }
                }
            }

            return aceExists;
        }

        private Dictionary<int, ContainerUpdateOperation> CreateDeleteSourceNetworkACEOperation(Dictionary<int, ContainerUpdateOperation> operations, SourceNetwork sourceNetwork)
        {
            foreach (ContainerMembership container in userEndpoint.UserContainerMemberships)
            {
                bool aceFound = false;

                if (((Int32)container.AllowedSourceNetworks & (Int32)sourceNetwork) == (Int32)sourceNetwork)
                {
                    aceFound = true;
                }

                if (aceFound)
                {
                    if (operations.ContainsKey(container.ContainerId))
                    {
                        operations[container.ContainerId].DeleteSourceNetwork(sourceNetwork);
                    }
                    else
                    {
                        ContainerUpdateOperation operation = new ContainerUpdateOperation(container.ContainerId);
                        operation.DeleteSourceNetwork(sourceNetwork);
                        operations.Add(container.ContainerId, operation);
                    }
                    break;
                }
            }

            return operations;
        }

        private Dictionary<int, ContainerUpdateOperation> CreateAddDomainACEOperation(Dictionary<int, ContainerUpdateOperation> operations, ACEItem aceItem)
        {
            foreach (ContainerMembership container in userEndpoint.UserContainerMemberships)
            {
                if (container.ContainerId == (Int32)aceItem.PresenceRelationshipLevel)
                {
                    bool aceFound = false;

                    foreach (string domain in container.AllowedSipDomains)
                    {
                        if (domain.ToLowerInvariant() == aceItem.ItemValue.ToLowerInvariant())
                        {
                            aceFound = true;
                            break;
                        }
                    }

                    if (!aceFound)
                    {
                        if (operations.ContainsKey(container.ContainerId))
                        {
                            operations[container.ContainerId].AddDomain(aceItem.ItemValue);
                        }
                        else
                        {
                            ContainerUpdateOperation operation = new ContainerUpdateOperation(container.ContainerId);
                            operation.AddDomain(aceItem.ItemValue);
                            operations.Add(container.ContainerId, operation);
                        }
                    }

                    break;
                }
            }

            return operations;
        }

        private bool DomainACEExistsInOtherContainerMemberships(string sipDomain, PresenceRelationshipLevel containerIdToExcludeFromSearch)
        {
            bool aceExists = false;

            foreach (ContainerMembership container in userEndpoint.UserContainerMemberships)
            {
                if (container.ContainerId != (Int32)containerIdToExcludeFromSearch)
                {
                    foreach (string domain in container.AllowedSipDomains)
                    {
                        if (domain.ToLowerInvariant() == sipDomain.ToLowerInvariant())
                        {
                            aceExists = true;
                            break;
                        }
                    }
                }
            }

            return aceExists;
        }

        private Dictionary<int, ContainerUpdateOperation> CreateDeleteDomainACEOperation(Dictionary<int, ContainerUpdateOperation> operations, ACEItem aceItem)
        {
            foreach (ContainerMembership container in userEndpoint.UserContainerMemberships)
            {
                bool aceFound = false;

                foreach (string domain in container.AllowedSipDomains)
                {
                    if (domain.ToLowerInvariant() == aceItem.ItemValue.ToLowerInvariant())
                    {
                        aceFound = true;
                        break;
                    }
                }

                if (aceFound)
                {
                    if (operations.ContainsKey(container.ContainerId))
                    {
                        operations[container.ContainerId].DeleteDomain(aceItem.ItemValue);
                    }
                    else
                    {
                        ContainerUpdateOperation operation = new ContainerUpdateOperation(container.ContainerId);
                        operation.DeleteDomain(aceItem.ItemValue);
                        operations.Add(container.ContainerId, operation);
                    }
                    break;
                }
            }

            return operations;
        }

        public void RemoveACEs(string userUri, List<ACEItem> aceItemList)
        {
            userEndpoint.InitializeUserEndpoint(userUri);
            userEndpoint.SubscribeToContainerMemberships();

            Dictionary<int, ContainerUpdateOperation> operations = new Dictionary<int, ContainerUpdateOperation>();

            foreach (ACEItem aceItem in aceItemList)
            {
                switch (aceItem.ACEType)
                {
                    case ACEType.User:
                        operations = CreateDeleteUserACEOperation(operations, aceItem);
                        break;

                    case ACEType.Company:
                        // Company should always be in a Container Membership - Default is Colleagues
                        operations = CreateDeleteSourceNetworkACEOperation(operations, SourceNetwork.SameEnterprise);

                        // Company ACE is added to the Colleagues container. It does equals to reset it to default
                        ACEItem colleaguesACEItem = new ACEItem(ACEType.Company, "", PresenceRelationshipLevel.Colleagues);
                        operations = CreateAddSourceNetworkACEOperation(operations, SourceNetwork.SameEnterprise, colleaguesACEItem);
                        break;

                    case ACEType.Domain:
                        operations = CreateDeleteDomainACEOperation(operations, aceItem);
                        break;

                    case ACEType.FederatedDomains:
                        // Company should always be in a Container Membership - Default is External
                        operations = CreateDeleteSourceNetworkACEOperation(operations, SourceNetwork.Federated);

                        // Federated ACE is added to the External container. It does equals to reset it to default
                        ACEItem externalACEItem = new ACEItem(ACEType.FederatedDomains, "", PresenceRelationshipLevel.External);
                        operations = CreateAddSourceNetworkACEOperation(operations, SourceNetwork.Federated, externalACEItem);
                        break;

                    case ACEType.PublicDomains:
                        operations = CreateDeleteSourceNetworkACEOperation(operations, SourceNetwork.PublicCloud);
                        break;

                    default:
                        // Should never happen
                        break;
                }
            }

            if (operations.Count > 0)
            {
                if (userEndpoint.User.LocalOwnerPresence.CurrentState == CollaborationSubscriptionState.Subscribed)
                {
                    userEndpoint.User.LocalOwnerPresence.EndUpdateContainerMembership(userEndpoint.User.LocalOwnerPresence.BeginUpdateContainerMembership(operations.Values, null, null));
                }
                else
                {
                    throw new Exception("Collaboration subscription state is not subscribed");
                }
            }
        }

    }
}
