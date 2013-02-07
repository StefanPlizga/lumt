// DISCLAIMER
// This source code is provided as a sample. It is licensed "as-is."
// You bear the risk of using it. The contributors give no express warranties, 
// guarantees or conditions. See MS-PL license description for more details.

using System;
using System.Text;
using Microsoft.Rtc.Collaboration.Presence;
using System.Xml;
using System.Collections.Generic;
using LUMT.UCMA;
using LUMT.Enum;

namespace LUMT.ManagerClass
{
    class AlertsManager
    {
        Endpoint userEndpoint;

        public AlertsManager(Endpoint endpoint)
        {
            userEndpoint = endpoint;
        }

        public void SetAlertNotificationSetting(string userUri, NotifyAdditionToContactListType notifyAdditionToContactList, AlertsWhenDoNotDisturbType alertsWhenDoNotDisturb)
        {
            const string defaultDataXml = "<alerts xmlns=\"http://schemas.microsoft.com/2006/09/sip/options/alerts\"></alerts>";
            const string alertsXmlns = "http://schemas.microsoft.com/2006/09/sip/options/alerts";

            if ((notifyAdditionToContactList != NotifyAdditionToContactListType.NoChange) || (alertsWhenDoNotDisturb != AlertsWhenDoNotDisturbType.NoChange))
            {
                string newDataXml = "";

                userEndpoint.InitializeUserEndpoint(userUri);
                userEndpoint.SubscribeToPresence();

                if (userEndpoint.User.ContactGroupServices.CurrentState == CollaborationSubscriptionState.Subscribed)
                {
                    XmlDocument doc = new XmlDocument();

                    if (userEndpoint.AlertNotification != null)
                    {
                        string dataXml = userEndpoint.AlertNotification.Category.GetCategoryDataXml();
                        doc.LoadXml(dataXml);

                        string xmlns = doc.DocumentElement.Attributes["xmlns"].Value;
                        if (xmlns != alertsXmlns)
                        {
                            doc.LoadXml(defaultDataXml);
                        }
                    }
                    else
                    {
                        doc.LoadXml(defaultDataXml);
                    }

                    doc = SetNotifyAdditionToContactListSetting(doc, notifyAdditionToContactList, alertsXmlns);
                    doc = SetAlertsWhenDoNotDisturbSetting(doc, alertsWhenDoNotDisturb, alertsXmlns);

                    newDataXml = doc.InnerXml;
                }
                else
                {
                    throw new Exception("Collaboration subscription state is not subscribed");
                }

                PresenceCategory cat = new CustomPresenceCategory("alerts", newDataXml);
                List<PresenceCategory> catCollection = new List<PresenceCategory>();
                catCollection.Add(cat);
                userEndpoint.User.LocalOwnerPresence.EndPublishPresence(userEndpoint.User.LocalOwnerPresence.BeginPublishPresence(catCollection, null, null));
            }
        }

        private XmlDocument SetNotifyAdditionToContactListSetting(XmlDocument xmlDoc, NotifyAdditionToContactListType notifyAdditionToContactList, string xmlns)
        {
            if (notifyAdditionToContactList == NotifyAdditionToContactListType.NoChange)
            {
                return xmlDoc;
            }

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("alerts", xmlns);
            XmlNode notifyAdditionToContactListNode = xmlDoc.SelectSingleNode("//alerts:notifyAdditionToContactList", nsmgr);

            if (notifyAdditionToContactListNode != null)
            {
                if ((notifyAdditionToContactListNode.InnerText.ToLowerInvariant() == "false") && (notifyAdditionToContactList == NotifyAdditionToContactListType.Yes))
                {
                    // Category "alerts" does exist with value of False -> notifyAdditionToContactList node will be modified with value of True
                    xmlDoc.DocumentElement.RemoveChild(notifyAdditionToContactListNode);
                    XmlNode newNode = xmlDoc.CreateNode(XmlNodeType.Element, "notifyAdditionToContactList", xmlns);
                    newNode.InnerText = "true";
                    xmlDoc.DocumentElement.AppendChild(newNode);
                }

                if ((notifyAdditionToContactListNode.InnerText.ToLowerInvariant() == "true") && (notifyAdditionToContactList == NotifyAdditionToContactListType.No))
                {
                    // Category "alerts" does exist with value of True -> notifyAdditionToContactList node will be modified with value of False
                    xmlDoc.DocumentElement.RemoveChild(notifyAdditionToContactListNode);
                    XmlNode newNode = xmlDoc.CreateNode(XmlNodeType.Element, "notifyAdditionToContactList", xmlns);
                    newNode.InnerText = "false";
                    xmlDoc.DocumentElement.AppendChild(newNode);
                }
            }
            else
            {
                // notifyAdditionToContactList node does not exist -> it will be created
                XmlNode newNode = xmlDoc.CreateNode(XmlNodeType.Element, "notifyAdditionToContactList", xmlns);
                if (notifyAdditionToContactList == NotifyAdditionToContactListType.Yes)
                {
                    newNode.InnerText = "true";
                }
                else
                {
                    newNode.InnerText = "false";
                }
                xmlDoc.DocumentElement.AppendChild(newNode);
            }

            return xmlDoc;
        }


        private XmlDocument SetAlertsWhenDoNotDisturbSetting(XmlDocument xmlDoc, AlertsWhenDoNotDisturbType alertsWhenDoNotDisturb, string xmlns)
        {
            if (alertsWhenDoNotDisturb == AlertsWhenDoNotDisturbType.NoChange)
            {
                return xmlDoc;
            }

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("alerts", xmlns);
            XmlNode alertsWhenDoNotDisturbNode = xmlDoc.SelectSingleNode("//alerts:alertsWhenDoNotDisturb", nsmgr);

            if (alertsWhenDoNotDisturbNode != null)
            {
                if ((alertsWhenDoNotDisturb == AlertsWhenDoNotDisturbType.DisplayAllAlerts) && (alertsWhenDoNotDisturbNode.InnerText.ToLowerInvariant() != "displayallalerts"))
                {
                    // alertsWhenDoNotDisturb node will be modified with value of displayAllAlerts
                    xmlDoc.DocumentElement.RemoveChild(alertsWhenDoNotDisturbNode);
                    XmlNode newNode = xmlDoc.CreateNode(XmlNodeType.Element, "alertsWhenDoNotDisturb", xmlns);
                    newNode.InnerText = "displayAllAlerts";
                    xmlDoc.DocumentElement.AppendChild(newNode);
                }

                if ((alertsWhenDoNotDisturb == AlertsWhenDoNotDisturbType.DisplayAlertsFromHighPresence) && (alertsWhenDoNotDisturbNode.InnerText.ToLowerInvariant() != "displayalertsfromhighpresence"))
                {
                    // alertsWhenDoNotDisturb node will be modified with value of displayAlertsFromHighPresence
                    xmlDoc.DocumentElement.RemoveChild(alertsWhenDoNotDisturbNode);
                    XmlNode newNode = xmlDoc.CreateNode(XmlNodeType.Element, "alertsWhenDoNotDisturb", xmlns);
                    newNode.InnerText = "displayAlertsFromHighPresence";
                    xmlDoc.DocumentElement.AppendChild(newNode);
                }

                if ((alertsWhenDoNotDisturb == AlertsWhenDoNotDisturbType.NoAlerts) && (alertsWhenDoNotDisturbNode.InnerText.ToLowerInvariant() != "noalerts"))
                {
                    // alertsWhenDoNotDisturb node will be modified with value of noAlerts
                    xmlDoc.DocumentElement.RemoveChild(alertsWhenDoNotDisturbNode);
                    XmlNode newNode = xmlDoc.CreateNode(XmlNodeType.Element, "alertsWhenDoNotDisturb", xmlns);
                    newNode.InnerText = "noAlerts";
                    xmlDoc.DocumentElement.AppendChild(newNode);
                }
            }
            else
            {
                // alertsWhenDoNotDisturb node does not exist -> it will be created
                XmlNode newNode = xmlDoc.CreateNode(XmlNodeType.Element, "alertsWhenDoNotDisturb", xmlns);
                switch (alertsWhenDoNotDisturb)
                {
                    case AlertsWhenDoNotDisturbType.DisplayAllAlerts:
                        newNode.InnerText = "displayAllAlerts";
                        break;
                    case AlertsWhenDoNotDisturbType.DisplayAlertsFromHighPresence:
                        newNode.InnerText = "displayAlertsFromHighPresence";
                        break;
                    case AlertsWhenDoNotDisturbType.NoAlerts:
                        newNode.InnerText = "noAlerts";
                        break;
                    default:
                        // Should not occur
                        break;
                }

                xmlDoc.DocumentElement.AppendChild(newNode);
            }

            return xmlDoc;
        }
    }
}
