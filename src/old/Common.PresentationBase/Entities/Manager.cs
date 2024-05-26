//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;

//namespace Common
//{
//    public class UserProfile : EntityBase
//    {
//        public string[] BusinessPhones { get; set; }
//        public string DisplayName { get; set; }
//        public string GivenName { get; set; }
//        public string JobTitle { get; set; }
//        public string Mail { get; set; }
//        public string MobilePhone { get; set; }
//        public string OfficeLocation { get; set; }
//        public string PreferredLanguage { get; set; }
//        public string Surname { get; set; }
//        public string UserPrincipalName { get; set; }
//        public string Id { get; set; }

//    }

//    public class GroupsProfile
//    {
//        public string email { get; set; }
//        public Group[] value { get; set; }
//    }

//    public class Group
//    {
//        public string id { get; set; }
//        public object deletedDateTime { get; set; }
//        public object classification { get; set; }
//        public DateTime createdDateTime { get; set; }
//        public object[] creationOptions { get; set; }
//        public string description { get; set; }
//        public string displayName { get; set; }
//        public object expirationDateTime { get; set; }
//        public string[] groupTypes { get; set; }
//        public object isAssignableToRole { get; set; }
//        public string mail { get; set; }
//        public bool mailEnabled { get; set; }
//        public string mailNickname { get; set; }
//        public object membershipRule { get; set; }
//        public object membershipRuleProcessingState { get; set; }
//        public object onPremisesDomainName { get; set; }
//        public object onPremisesLastSyncDateTime { get; set; }
//        public object onPremisesNetBiosName { get; set; }
//        public object onPremisesSamAccountName { get; set; }
//        public object onPremisesSecurityIdentifier { get; set; }
//        public object onPremisesSyncEnabled { get; set; }
//        public object preferredDataLocation { get; set; }
//        public object preferredLanguage { get; set; }
//        public string[] proxyAddresses { get; set; }
//        public DateTime renewedDateTime { get; set; }
//        public object[] resourceBehaviorOptions { get; set; }
//        public object[] resourceProvisioningOptions { get; set; }
//        public bool securityEnabled { get; set; }
//        public string securityIdentifier { get; set; }
//        public object theme { get; set; }
//        public string visibility { get; set; }
//        public object[] onPremisesProvisioningErrors { get; set; }
//    }

//    public class MemberGroups
//    {
//        public string[] value { get; set; }
//    }

//    public class MembersGroup
//    {
//        public Value[] value { get; set; }
//    }

//    public class Value
//    {
//        public string id { get; set; }
//        public object deletedDateTime { get; set; }
//        public object classification { get; set; }
//        public DateTime createdDateTime { get; set; }
//        public object[] creationOptions { get; set; }
//        public object description { get; set; }
//        public string displayName { get; set; }
//        public object expirationDateTime { get; set; }
//        public object[] groupTypes { get; set; }
//        public object isAssignableToRole { get; set; }
//        public string mail { get; set; }
//        public bool mailEnabled { get; set; }
//        public string mailNickname { get; set; }
//        public object membershipRule { get; set; }
//        public object membershipRuleProcessingState { get; set; }
//        public object onPremisesDomainName { get; set; }
//        public object onPremisesLastSyncDateTime { get; set; }
//        public object onPremisesNetBiosName { get; set; }
//        public object onPremisesSamAccountName { get; set; }
//        public object onPremisesSecurityIdentifier { get; set; }
//        public object onPremisesSyncEnabled { get; set; }
//        public object preferredDataLocation { get; set; }
//        public object preferredLanguage { get; set; }
//        public string[] proxyAddresses { get; set; }
//        public DateTime renewedDateTime { get; set; }
//        public object[] resourceBehaviorOptions { get; set; }
//        public object[] resourceProvisioningOptions { get; set; }
//        public bool securityEnabled { get; set; }
//        public string securityIdentifier { get; set; }
//        public object theme { get; set; }
//        public object visibility { get; set; }
//        public object[] onPremisesProvisioningErrors { get; set; }
//        public Member[] members { get; set; }
//    }

//    public class Member
//    {
//        public string odatatype { get; set; }
//        public string id { get; set; }
//        public object deletedDateTime { get; set; }
//        public bool accountEnabled { get; set; }
//        public object ageGroup { get; set; }
//        public string[] businessPhones { get; set; }
//        public string city { get; set; }
//        public object companyName { get; set; }
//        public object consentProvidedForMinor { get; set; }
//        public string country { get; set; }
//        public DateTime createdDateTime { get; set; }
//        public object creationType { get; set; }
//        public string department { get; set; }
//        public string displayName { get; set; }
//        public object employeeId { get; set; }
//        public object externalUserState { get; set; }
//        public object externalUserStateChangeDateTime { get; set; }
//        public object faxNumber { get; set; }
//        public string givenName { get; set; }
//        public string jobTitle { get; set; }
//        public object legalAgeGroupClassification { get; set; }
//        public string mail { get; set; }
//        public string mailNickname { get; set; }
//        public object mobilePhone { get; set; }
//        public object onPremisesDistinguishedName { get; set; }
//        public object onPremisesDomainName { get; set; }
//        public object onPremisesImmutableId { get; set; }
//        public object onPremisesLastSyncDateTime { get; set; }
//        public object onPremisesSecurityIdentifier { get; set; }
//        public object onPremisesSamAccountName { get; set; }
//        public object onPremisesSyncEnabled { get; set; }
//        public object onPremisesUserPrincipalName { get; set; }
//        public object[] otherMails { get; set; }
//        public object passwordPolicies { get; set; }
//        public object passwordProfile { get; set; }
//        public string officeLocation { get; set; }
//        public string postalCode { get; set; }
//        public string preferredLanguage { get; set; }
//        public string[] proxyAddresses { get; set; }
//        public DateTime refreshTokensValidFromDateTime { get; set; }
//        public string[] imAddresses { get; set; }
//        public object isResourceAccount { get; set; }
//        public object showInAddressList { get; set; }
//        public DateTime signInSessionsValidFromDateTime { get; set; }
//        public string state { get; set; }
//        public string streetAddress { get; set; }
//        public string surname { get; set; }
//        public string usageLocation { get; set; }
//        public string userPrincipalName { get; set; }
//        public string userType { get; set; }
//        public Assignedlicens[] assignedLicenses { get; set; }
//        public Assignedplan[] assignedPlans { get; set; }
//        public IdentityInfo[] identities { get; set; }
//        public object[] onPremisesProvisioningErrors { get; set; }
//        public Onpremisesextensionattributes onPremisesExtensionAttributes { get; set; }
//        public Provisionedplan[] provisionedPlans { get; set; }
//    }

//    public class Onpremisesextensionattributes
//    {
//        public object extensionAttribute1 { get; set; }
//        public object extensionAttribute2 { get; set; }
//        public object extensionAttribute3 { get; set; }
//        public object extensionAttribute4 { get; set; }
//        public object extensionAttribute5 { get; set; }
//        public object extensionAttribute6 { get; set; }
//        public object extensionAttribute7 { get; set; }
//        public object extensionAttribute8 { get; set; }
//        public object extensionAttribute9 { get; set; }
//        public object extensionAttribute10 { get; set; }
//        public object extensionAttribute11 { get; set; }
//        public object extensionAttribute12 { get; set; }
//        public object extensionAttribute13 { get; set; }
//        public object extensionAttribute14 { get; set; }
//        public object extensionAttribute15 { get; set; }
//    }

//    public class Assignedlicens
//    {
//        public string[] disabledPlans { get; set; }
//        public string skuId { get; set; }
//    }

//    public class Assignedplan
//    {
//        public DateTime assignedDateTime { get; set; }
//        public string capabilityStatus { get; set; }
//        public string service { get; set; }
//        public string servicePlanId { get; set; }
//    }

//    public class IdentityInfo
//    {
//        public string signInType { get; set; }
//        public string issuer { get; set; }
//        public string issuerAssignedId { get; set; }
//    }

//    public class Provisionedplan
//    {
//        public string capabilityStatus { get; set; }
//        public string provisioningStatus { get; set; }
//        public string service { get; set; }
//    }

//}
