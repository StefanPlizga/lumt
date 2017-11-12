# Version History
* 1.0.0.0: 
	* Original version.
* 1.1.0.0: 
	* Details added to log file when exceptions occur in order to better understand why LUMT is not working.
	* Solution upgraded to Visual Studio 2013.
* 1.2.0.0:
	* Support for "Favorites" group added.
* 1.3.0.0:
	* Enhanced error logging has been added.
* 1.4.0.0:
	* Additional error logging has been added.
	* Lync for Server 2013 version.
	* Imported from CodePlex (http://lumt.codeplex.com).
* 1.5.0.0:
	* Skype for Business Server 2015 version (UCMA 5.0).
	* Solution upgraded to Visual Studio 2017.

# Help
## Description
LUMT tool can be used to manage contacts, ACEs, privacy settings or alert notification settings for Skype for Business Server 2015 users.

**Important: _The software and source code are provided as samples_. They are licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. See MS-PL license description for more details.**

## Usage
`LUMT.exe /Mode:<LUMTMode> /UsersFile:<UsersFileName> [/ContactsFile:<ContactsFileName>] [/ContactsGroup:<ContactsGroupName>] [/ACLFile:<ACLFileName>] [/PrivacyPreference:<Privacy>] [/Delete]`

## Generic Parameter List
_Mode_ | Settings to manage with LUMT
---- | ----------------------------
| | _Contact_: LUMT is used to add or remove contacts for Skype for Business Server 2015 users.
| | _ACL_: LUMT is used to add or remove ACEs on Skype for Business Server 2015 users.
| | _Privacy_: LUMT is used to manage privacy settings for Skype for Business Server 2015 users.
| | _Alerts_: LUMT is used to manage alert notification settings for Skype for Business Server 2015 users.
_UsersFile_ | Path to the text file containing user SIP addresses beginning with sip:

_LUMT can be executed in one mode at a time. It is not possible to run with multiple modes during the same execution cycle._

## Contact Mode Parameter List
Parameter | Description
--------- | -----------
| _ContactsFile_ | Path to the text file containing contact SIP addresses beginning with sip: or distribution list addresses beginning with smtp:
| _ContactsGroup_ | Optional. Name of the group to add contacts. This parameter is ignored when the _Delete_ switch is used. Note: in order to add contacts to the "Favorites" group, specify "Pinned Contacts" as the group name.
| _Delete_ | Optional. Delete contacts from the user's contact list instead of adding them.

_Note: It is not possible to manage contact list if the contact list provider is Unified Contact Store (Exchange Server 2013)._

## ACL Mode Parameter List
Parameter | Description
--------- | -----------
| _ACLFile_ | Path to the text file containing ACEs for users.
| _Delete_ | Optional. Delete ACEs from the user's contact list instead of adding them. When the _Delete_ switch is used, the 

_RelationshipLevel_ parameter is ignored: this means ACES specified are removed no matter what relationship they currently have.

## Privacy Mode Parameter List
Parameter | Description
--------- | -----------
| _PrivacyPreference_ | Privacy setting to set for the user. Valid values are _Private_, _Public_ or _Default_.

## Alerts Mode Parameter List
Parameter | Description
--------- | -----------
| _NotifyAdd_ | Alert display setting to set for the user when someone adds him or her to the cocntact list. Valid values are _Yes_ or _No_.
| _NotifyWhenDND_ | Alert display setting to set fgor the user when his or her status is set to DND. Valid values are _AllAlerts_, _AlertsFromWorkgroup_ or _NoAlerts_.

_Note: Either NotifyAdd, NotifyWhenDND or both parameters can be specified in the Alerts mode._

# Examples
`LUMT /Mode:Contact /UsersFile:Users.txt /ContactsFile:Contacts.txt`  
`LUMT /Mode:Contact /UsersFile:Users.txt /ContactsFile:Contacts.txt /ContactsGroup:Support`  
`LUMT /Mode:Contact /UsersFile:Users.txt /ContactsFile:Contacts.txt /Delete`  
`LUMT /Mode:ACL /UsersFile:Users.txt /ACLFile:ACL.txt`  
`LUMT /Mode:ACL /UsersFile:Users.txt /ACLFile:ACL.txt /Delete`  
`LUMT /Mode:Privacy /UsersFile:Users.txt /PricacyPreference:Private`  
`LUMT /Mode:Privacy /UsersFile:Users.txt /PricacyPreference:Public`  
`LUMT /Mode:Privacy /UsersFile:Users.txt /PricacyPreference:Default`  
`LUMT /Mode:Alerts /UsersFile:Users.txt /NotifyAdd:No`  
`LUMT /Mode:Alerts /UsersFile:Users.txt /NotifyWhenDND:AlertsFromWorkgroup`  
`LUMT /Mode:Alerts /UsersFile:Users.txt /NotifyAdd:Yes /NotifyWhenDND:NoAlerts`  

# Input File Requirements
## UsersFile requirements
Every line must contain only 1 SIP user. Format must be _sip:user@domain.com_

## ContactsFile requirements
Every line must contain only 1 contact. Contact can be one of the following types:

Parameter | Description
--------- | -----------
| SIP user | Format must be _sip:user@domain.com_
| Distribution list | Format must be _smtp:dl@domain.com_

## ACLFile requirements
Every line must contain only 1 ACE. ACE input format is _ACEType RelationshipLevel UserOrDomain_
Each value must be separated by a single white space character.

_ACEType_ value can be:

Value | Description
----- | -----------
| _User_ | ACE applies to a SIP user in the contact list
| _Domain_ | ACE applies to a SIP domain in the contact list
| _Company_ | ACE applies to SIP users who are part of the same company
| _FederatedDomains_ | ACE applies to SIP users who are part of all federated companies
| _PublicDomains_ | ACE applies to SIP users who are part of public IM domains

_RelationshipLevel_ value can be:

Value | Description
----- | -----------
| _Personal_ | Friend and Family relationship level
| _Workgroup_ | Workgroup relationship level
| _Colleagues_ | Colleagues (or Company) relationship level
| _External_ | External relationship level
| _Blocked_ | Blocked relationship level

_UserOrDomain_ value can be:

Value | Description
----- | -----------
| If _ACEType_ is _User_ | SIP user to which apply the ACL. Format must be _sip:user@domain.com_
| If _ACEType_ is _Domain_ | Domain to which apply the ACL. Format must be _domain.com_
| If _ACEType_ is _Company_, _FederatedDomains_ or _PublicDomains_ | Don't specify any value

# Setup
Lync User Management Tool **MUST NOT RUN** on an existing Skype for Business Server 2015 role. It **MUST** run on an application server used for UCMA 5.0 applications (_the same server could be used for other UCMA applications_).

In order for LUMT to work, a trusted application in needed in Skype for Business Server 2015 Topology. There are 3 steps that must be done:
* Create a Skype for Business Server 2015 Trusted Application Pool with:
`New-CsTrustedApplicationPool -Id <ServerFqdn> -Registrar <RegistrarPoolFqdn> -Site <SiteNumber>`

Parameter | Description
--------- | -----------
| ServerFqdn | FQDN of the server that will host LUMT
| RegistrarPoolFqdn | FQDN of the pool that the Trusted Application Pool is related
| SiteNumber | Site ID in Skype for Business Server 2015 Topology. The Site ID can be retrieved with the Get-CsSite cmdlet

* Create a Trusted Application with:
`New-CsTrustedApplication -ApplicationId LUMT –TrustedApplicationPoolFqdn <ServerFqdn> -Port <PortNumber>`

Parameter | Description
--------- | -----------
| ServerFqdn | Same FQDN as used for the New-CsTrustedApplicationPool cmdlet
| PortNumber | Any non-used port number above 1024, for instance 12345

* Enable Skype for Business Server 2015 Topology with:
`Enable-CsTopology`

Then, Skype for Business Server 2015 binaries must be installed on the server by using the Skype for Business Server 2015 Deployment Wizard. The following steps must be performed:
* Install or Update Skype for Business Server 2015 
* Install Local Configuration Store 
* Request, Install or Assign Certificates 
* Start Services

It is now possible to run LUMT from this server. 

# Required DNS Records
In case LUMT does not start and you get error “Automatic server discovery for the given sip user uri failed” in the log file, make sure to have at least one of the following records in place that resolves to your Skype for Business Server 2015 pool:
* SRV:	`_sipinternaltls._tcp.<domain>`
* SRV:	`_sip._tls.<domain>`
* A:	`sipinternal.<domain>`
* A:	`sip.<domain>`
