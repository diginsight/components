//#region using
//using Common.Properties;
//using Common;
//using Microsoft.InformationProtectionAndControl;
//using Microsoft.Office.Core;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Windows.Forms; 
//#endregion

//namespace Common
//{
//    public static class AipExtensionMethods
//    {
//        public static Type T = typeof(AipExtensionMethods);

//        public static TemplateInfo template { get; private set; }

//        public static bool IsInternal(this String user)
//        {
//            using (var sec = TraceManager.GetCodeSection(T, new { user = user.GetLogString() }))
//            {
//                foreach (string dominio in Resources.Domini.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
//                {
//                    if (user.EndsWith(dominio, StringComparison.InvariantCultureIgnoreCase))
//                    {
//                        return true;
//                    }
//                }
//            }
//            return false;
//        }

//        public static List<UserRights> GetPolicyUserRights(this TemplateInfo template)
//        {
//            using (var sec = TraceManager.GetCodeSection(T, new { template = template.GetLogString() }))
//            {
//                if (template == null)
//                {
//                    throw new InvalidOperationException("Impossibile trovare il template per estrarre gli User Rights");
//                }

//                var licenseHandle = SafeNativeMethods.IpcCreateLicenseFromTemplateId(template.TemplateId);
//                return SafeNativeMethods.IpcGetLicenseUserRightsList(licenseHandle).ToList();
//            }
//        }

//        public static bool IsOwner(this Microsoft.Office.Core.UserPermission permission)
//        {
//            using (var sec = TraceManager.GetCodeSection(T, new { permission = permission.GetLogString() }))
//            {
//                return (permission.Permission == 64);
//            }
//        }

//        public static TemplateInfo GetTemplate(this Permission permission)
//        {
//            using (var sec = TraceManager.GetCodeSection(T, new { permission = permission.GetLogString() }))
//            {
//                // Word.Document wordDoc = permission.Parent;
//                // Word.Document wordDoc = permission.Parent;
//                String sensitivity = null;

//                if (permission.PermissionFromPolicy)
//                {
//                    sensitivity = permission.PolicyName.Replace(" -", "");
//                }
//                else
//                {
//                    sensitivity = GetSensitivityFromProperty(permission);
//                }

//                if (template == null || !template.Name.Replace(" -", "").Equals(sensitivity))
//                {
//                    var templates = SafeNativeMethods.IpcGetTemplateList(null, false, false, false, true, null, null, null);
//                    template = templates.FirstOrDefault(t => t.Name.Replace(" -", "") == sensitivity);
//                }
//            }

//            return template;
//        }

//        public static string GetSensitivityFromProperty(this Permission permission)
//        {
//            using (var sec = TraceManager.GetCodeSection(T, new { permission = permission.GetLogString() }))
//            {
//                var ret = string.Empty;
//                var properties = permission.Parent.CustomDocumentProperties;
//                sec.Debug(new { properties = LogStringExtensions.GetLogString(properties) });
                
//                foreach (var property in properties)
//                {
//                    sec.Debug($"{property.Name}: {property.Value}");
//                    if (property.Name.Equals("Sensitivity"))
//                    {
//                        ret = property.Value; sec.Result = ret;
//                        return ret;
//                    }
//                }

//                sec.Result = ret;
//                return ret;
//            }
//        }

//        public static List<UserRights> GetPolicyUserRights(this String templateId)
//        {
//            using (var sec = TraceManager.GetCodeSection(T, new { templateId = templateId.GetLogString() }))
//            {
//                //var templates = SafeNativeMethods.IpcGetTemplateList(null, false, false, false, true, null, null, null);
//                //var template = templates.FirstOrDefault(t => t.TemplateId == permission.PolicyName);

//                //if (template == null)
//                //{
//                //    throw new InvalidOperationException("Impossibile trovare il template per estrarre gli User Rights");
//                //}

//                var licenseHandle = SafeNativeMethods.IpcCreateLicenseFromTemplateId(templateId);
//                return SafeNativeMethods.IpcGetLicenseUserRightsList(licenseHandle).ToList();
//            }
//        }

//        public static void ApplyUserRights(this Permission permission, IEnumerable<UserRights> userRightsList)
//        {
//            using (var sec = TraceManager.GetCodeSection(T, new { permission = permission.GetLogString(), userRightsList = userRightsList.GetLogString() }))
//            {
//                if (permission is null)
//                {
//                    throw new ArgumentNullException(nameof(permission));
//                }

//                if (userRightsList is null)
//                {
//                    throw new ArgumentNullException(nameof(userRightsList));
//                }

//                permission.RemoveAll();
//                permission.Enabled = true;
//                foreach (var userRight in userRightsList.Where(r => r.UserIdType != UserIdType.IpcUser))
//                {
//                    permission.Add(userRight.UserId, userRight.GetMsoPermission());
//                }
//            }
//        }

//        public static UserRights FindByUserId(this IEnumerable<UserRights> userRightsList, string userId)
//        {
//            using (var sec = TraceManager.GetCodeSection(T, new { userRightsList = userRightsList.GetLogString(), userId }))
//            {
//                if (userRightsList is null)
//                {
//                    throw new ArgumentNullException(nameof(userRightsList));
//                }

//                if (string.IsNullOrEmpty(userId))
//                {
//                    throw new ArgumentException("message", nameof(userId));
//                }

//                return userRightsList.FirstOrDefault(r => r.UserId == userId);
//            }
//        }

//        public static List<UserRights> CloneUserRights(this UserRights userRights, IEnumerable<string> newPrincipalIds)
//        {
//            using (var sec = TraceManager.GetCodeSection(T, new { userRights = userRights.GetLogString(), newPrincipalIds = newPrincipalIds.GetLogString() }))
//            {
//                if (userRights is null)
//                {
//                    throw new ArgumentNullException(nameof(userRights));
//                }

//                if (newPrincipalIds is null)
//                {
//                    throw new ArgumentNullException(nameof(newPrincipalIds));
//                }

//                return newPrincipalIds
//                    .Select(id => CloneUserRights(userRights, id))
//                    .ToList();
//            }
//        }

//        public static UserRights CloneUserRights(this UserRights userRights, string newPrincipalId)
//        {
//            using (var sec = TraceManager.GetCodeSection(T, new { userRights = userRights.GetLogString(), newPrincipalId }))
//            {
//                if (userRights is null)
//                {
//                    throw new ArgumentNullException(nameof(userRights));
//                }

//                if (string.IsNullOrEmpty(newPrincipalId))
//                {
//                    throw new ArgumentNullException(nameof(newPrincipalId));
//                }

//                return new UserRights(userRights.UserIdType, newPrincipalId, userRights.Rights);
//            }
//        }

//        public static MsoPermission GetMsoPermission(this UserRights userRights)
//        {
//            using (var sec = TraceManager.GetCodeSection(T, new { userRights = userRights.GetLogString() }))
//            {
//                if (userRights is null)
//                {
//                    throw new ArgumentNullException(nameof(userRights));
//                }

//                var msoPermission = (MsoPermission)0;
//                foreach (var right in userRights.Rights)
//                {
//                    switch (right)
//                    {
//                        case "VIEW":
//                            msoPermission |= MsoPermission.msoPermissionView;
//                            break;
//                        case "OBJMODEL":
//                            msoPermission |= MsoPermission.msoPermissionObjModel;
//                            break;
//                        case "PRINT":
//                            msoPermission |= MsoPermission.msoPermissionPrint;
//                            break;
//                        case "EDIT":
//                        case "DOCEDIT":
//                        case "SAVE":
//                            msoPermission |= (MsoPermission)6;
//                            break;
//                        case "EXTRACT":
//                            msoPermission |= MsoPermission.msoPermissionExtract;
//                            break;
//                        case "OWNER":
//                            msoPermission |= MsoPermission.msoPermissionFullControl;
//                            break;
//                        default:
//                            // Skipping unsupported Office permission;
//                            break;
//                    }
//                }

//                return msoPermission;
//            }
//        }
//    }
//}
