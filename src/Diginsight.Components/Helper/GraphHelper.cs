#region using
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace Diginsight.Components;

internal class GraphHelper
{
    #region constants
    private static readonly Type T = typeof(GraphHelper);
    const string CONFIGVALUE_ISREVOKERENALBED = "IsRevokerEnabled"; const bool DEFAULTVALUE_ISREVOKERENALBED = true;
    const string GRAPHAPI_ME = "https://graph.microsoft.com/v1.0/me";
    const string GRAPHAPI_MANAGER = "https://graph.microsoft.com/v1.0/users/{upn}/manager";
    const string GRAPHAPI_DECLASSIFIER_ME = "https://graph.microsoft.com/v1.0/users/{upn}/getMemberGroups";
    const string GRAPHAPI_DECLASSIFIER_GROUPID = "https://graph.microsoft.com/v1.0/groups?$filter=displayName eq 'groupName'";
    const string GRAPHAPI_GROUP = "https://graph.microsoft.com/v1.0/groups/?$filter=mail eq '{upn}'";
    const string GRAPHAPI_GROUP_MEMBER = "https://graph.microsoft.com/v1.0/groups/?$filter=mail eq '{upn}'&$expand=members";
    const string CONFIGVALUE_DATADECLASSGROUPNAME = "GFSCL_DATADECLASS_USERS";
    #endregion
    //private ILogger<GraphHelper> logger;

    //public static async Task<UserProfile> GetMe(HttpClient client = null)
    //{
    //    //using (var scope = TraceLogger.BeginMethodScope(T, new { client }))
    //    //{
    //        // if (client == null) { client = HttpHelper.GetHttpClient("application/json", authResult.AccessToken)}
    //        var response = await client.GetAsync(GRAPHAPI_ME);
    //        if (response.StatusCode == System.Net.HttpStatusCode.OK)
    //        {
    //            var stringContent = await response.Content.ReadAsStringAsync();
    //            scope.LogInformation($"getting user profile", "User");
    //            var userProfile = SerializationHelper.DeserializeJsonObject<UserProfile>(stringContent);
    //            return userProfile;
    //        }
    //    //}
    //    return null;
    //}
    //public static async Task<UserProfile> GetManager(string upn, HttpClient client = null)
    //{
    //    //using (var scope = TraceLogger.BeginMethodScope(T, new { client, upn }))
    //    //{
    //        // if (client == null) { client = HttpHelper.GetHttpClient("application/json", authResult.AccessToken)}
    //        var url = GRAPHAPI_MANAGER.Replace("{upn}", upn);
    //        var response = await client.GetAsync(url);
    //        if (response.StatusCode == System.Net.HttpStatusCode.OK)
    //        {
    //            var stringContent = await response.Content.ReadAsStringAsync();
    //            scope.LogInformation($"getting user's manager", "User");
    //            var userProfile = SerializationHelper.DeserializeJsonObject<UserProfile>(stringContent);

    //            scope.LogDebug(new { userProfile });
    //            return userProfile;
    //        }
    //    //}
    //    return null;
    //}
    //public static async Task<GroupsProfile> GetGroupId(string groupName, HttpClient client = null)
    //{
    //    //using (var scope = TraceLogger.BeginMethodScope(T, new { client, groupName }))
    //    //{
    //        var url = GRAPHAPI_DECLASSIFIER_GROUPID.Replace("groupName", groupName);
    //        var response = await client.GetAsync(url);
    //        if (response.StatusCode == System.Net.HttpStatusCode.OK)
    //        {
    //            var stringContent = await response.Content.ReadAsStringAsync();
    //            scope.LogInformation($"getting groupId", "groupName");
    //            var groupProfile = SerializationHelper.DeserializeJsonObject<GroupsProfile>(stringContent);

    //            return groupProfile;
    //        }
    //    //}
    //    return null;
    //}

    /////<summary>GetGroupProfile starting from an upn (distribution list, security group, etc, etc) returns the group profile</summary>
    /////<param name="upn"></param>
    /////<param name="client></param>
    /////<returns>The group profile for the upn</returns>
    /////<remarks>If mailEnabled=true and securityEnabled=false, we're talking about distribution lists
    /////If mailEnabled=true and securityEnabled=true, we're talking about security groups</remarks>
    //public static async Task<GroupsProfile> GetGroupProfile(string upn, HttpClient client = null)
    //{
    //    //using (var sec = TraceLogger.BeginMethodScope(T, new { client, upn }))
    //    //{
    //        var url = GRAPHAPI_GROUP.Replace("{upn}", upn);

    //        var response = await client.GetAsync(url);
    //        if (response.StatusCode == System.Net.HttpStatusCode.OK)
    //        {
    //            var stringContent = await response.Content.ReadAsStringAsync();
    //            sec.LogInformation($"getting group profile from distribution list, security group, etc, etc", "User");
    //            var groupProfile = SerializationHelper.DeserializeJsonObject<GroupsProfile>(stringContent);

    //            return groupProfile;
    //        }
    //        return null;
    //    //}
    //}

    /////<summary>GetMembersGroup starting from an upn (distribution list, security group, etc, etc) returns the beloging members</summary>
    /////<param name="upn"></param>
    /////<param name="client></param>
    /////<returns>The members belonging to the upn</returns>
    //public static async Task<MembersGroup> GetMembersGroup(string upn, HttpClient client = null)
    //{
    //    //using (var scope = TraceLogger.BeginMethodScope(T, new { client, upn }))
    //    //{
    //        var url = GRAPHAPI_GROUP_MEMBER.Replace("{upn}", upn);

    //        var response = await client.GetAsync(url);
    //        if (response.StatusCode == System.Net.HttpStatusCode.OK)
    //        {
    //            var stringContent = await response.Content.ReadAsStringAsync();
    //            scope.LogInformation($"getting belonging member fron distribution list, security group, etc, etc", "User");
    //            var membersGroup = SerializationHelper.DeserializeJsonObject<MembersGroup>(stringContent);

    //            return membersGroup;
    //        }
    //        return null;
    //    //}
    //}

    //public static async Task<bool> IsDeclassifier(string upn, string groupName, HttpClient client = null)
    //{
    //    //using (var scope = TraceLogger.BeginMethodScope(T, new { client, upn, groupName }))
    //    //{
    //        bool isRevokerEnalbed = ConfigurationHelper.GetClassSetting<GraphHelper, bool>(CONFIGVALUE_ISREVOKERENALBED, DEFAULTVALUE_ISREVOKERENALBED);
    //        if (isRevokerEnalbed == false) { return false; }

    //        // Get group Id from group name
    //        var groupProfile = await GetGroupId(groupName, client);
    //        if (groupProfile.value != null && groupProfile.value.Count() > 0)
    //        {
    //            string groupId = groupProfile.value[0].id;
    //            var url = GRAPHAPI_DECLASSIFIER_ME.Replace("{upn}", upn);
    //            var json = "{\"securityEnabledOnly\":\"false\"}";
    //            var data = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    //            var response = await client.PostAsync(url, data);
    //            if (response.StatusCode == System.Net.HttpStatusCode.OK)
    //            {
    //                var stringContent = await response.Content.ReadAsStringAsync();
    //                scope.LogInformation($"getting user's group in order to know if belong to declassifier group", "User");
    //                var memberOfGroups = SerializationHelper.DeserializeJsonObject<MemberGroups>(stringContent);

    //                return memberOfGroups.value != null && memberOfGroups.value.Contains(groupId);
    //            }
    //            return false;
    //        }
    //        return false;
    //    //}
    //}
}
