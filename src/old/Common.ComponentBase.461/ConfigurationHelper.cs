#region using
//using Newtonsoft.Json;
//using Microsoft.Extensions.Configuration;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;
#endregion

namespace Common
{
    [Flags]
    public enum SettingAccessType
    {
        Default = 0,
        SecretWithCredential = 1,
        SecretWithManagedIdentity = 2,
        Secret = 3
    }
    public static partial class ConfigurationHelper
    {
        #region constants
        private const string E_ARGUMENTNULLEXCEPTION = "argument '{name}' cannot be null";
        private const string S_NAME_PLACEHOLDER = "{name}";
        #endregion
        public static IConfiguration Configuration { get; internal set; }

        static ConfigurationHelper() { }

        public static void Init(IConfiguration configuration)
        {
            ConfigurationHelper.Configuration = configuration;
        }

        #region GetSetting
        public static T GetSetting<T>(string key, T defaultValue = default(T))
        {
            var configurationValue = Configuration.GetValue($"AppSettings:{key}", defaultValue);
            return configurationValue;
        }
        public static async Task<T> GetSettingAsync<T>(string key, SettingAccessType accessType, T defaultValue = default(T))
        {
            var result = default(T);
            if (accessType == SettingAccessType.Default) { result = Configuration.GetValue($"AppSettings:{key}", defaultValue); return result; }

            var configurationValue = GetSetting<string>(key);
            if (configurationValue == null)
            {
                if (accessType.HasFlag(SettingAccessType.SecretWithManagedIdentity)) { try { configurationValue = await GetSecretWithManagedIdentity(key); } catch (Exception ex) { } }
            }
            if (configurationValue == null)
            {
                if (accessType.HasFlag(SettingAccessType.SecretWithCredential)) { try { configurationValue = await GetSecretWithCredentials(key); } catch (Exception ex) { } }
            }
            if (configurationValue == null) { return defaultValue; }

            var converter = TypeDescriptor.GetConverter(typeof(T));
            result = (T)converter.ConvertFrom(configurationValue);
            return result;
        }
        #endregion
        #region GetClassSetting
        public static TRet GetClassSetting<TClass, TRet>(string name, TRet defaultValue = default(TRet), CultureInfo culture = null)
        {
            if (culture == null) { culture = CultureInfo.CurrentCulture; }

            var ret = defaultValue;
            try
            {
                Type type = typeof(TClass); Assembly assembly = type.Assembly;
                var assemblyName = GetConfigName(assembly); var className = GetConfigName(type);
                var specificName = $"{assemblyName}.{className}.{name}";
                var sectionName = $"{className}.{name}";
                var groupName = $"{name}";

                var valueString = GetSetting<string>(specificName, null);
                if (valueString == null) { valueString = GetSetting<string>(sectionName, null); }
                if (valueString == null) { valueString = GetSetting<string>(groupName, null); }
                if (valueString == null) { return ret; }

                var converter = TypeDescriptor.GetConverter(typeof(TRet));
                ret = (TRet)converter.ConvertFrom(null, culture, valueString);
                return ret;
            }
            catch (Exception ex) { TraceManager.Exception(ex); return ret; }
            finally { TraceManager.Debug($"GetClassSetting('{name}') returned '{ret.GetLogString()}'", "config"); }
        }
        #endregion
        #region GetClassSettingAsync
        public static async Task<TRet> GetClassSettingAsync<TClass, TRet>(string name, SettingAccessType accessType, TRet defaultValue = default(TRet), CultureInfo culture = null)
        {
            if (culture == null) { culture = CultureInfo.CurrentCulture; }

            var ret = defaultValue;
            try
            {
                Type type = typeof(TClass); Assembly assembly = type.Assembly;
                var assemblyName = GetConfigName(assembly); var className = GetConfigName(type);
                var specificName = $"{assemblyName}.{className}.{name}";
                var sectionName = $"{className}.{name}";
                var groupName = $"{name}";

                var valueString = await GetSettingAsync<string>(specificName, accessType, null);
                if (valueString == null) { valueString = await GetSettingAsync<string>(sectionName, accessType, null); }
                if (valueString == null) { valueString = await GetSettingAsync<string>(groupName, accessType, null); }
                if (valueString == null) { return ret; }

                var converter = TypeDescriptor.GetConverter(typeof(TRet));
                ret = (TRet)converter.ConvertFrom(null, culture, valueString);
                return ret;
            }
            catch (Exception ex) { TraceManager.Exception(ex); return ret; }
            finally { TraceManager.Debug($"GetClassSetting('{name}') returned '{ret.GetLogString()}'", "config"); }
        }
        #endregion

        #region GetConfigName
        ///<summary>gets the name of the assembly to be used as a configsection name or a prefix for appettings values.</summary>
        public static string GetConfigName(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly", E_ARGUMENTNULLEXCEPTION.Replace(S_NAME_PLACEHOLDER, "assembly"));

            ConfigurationNameAttribute assemblyConfignameAttribute = (ConfigurationNameAttribute)Attribute.GetCustomAttribute(assembly, typeof(ConfigurationNameAttribute));
            if (assemblyConfignameAttribute != null) { return assemblyConfignameAttribute.Name; } else { return assembly.GetName().Name; }
        }
        ///<summary>gets the name of the configuration section associated with the class.</summary>
        public static string GetConfigName(Type type)
        {
            if (type == null) throw new ArgumentNullException("type", E_ARGUMENTNULLEXCEPTION.Replace(S_NAME_PLACEHOLDER, "type"));

            ConfigurationNameAttribute classConfignameAttribute = (ConfigurationNameAttribute)Attribute.GetCustomAttribute(type, typeof(ConfigurationNameAttribute));
            if (classConfignameAttribute != null) { return classConfignameAttribute.Name; } else { return type.Name; }
        }
        #endregion

        #region GetManagedIdentityToken
        static string GetManagedIdentityToken()
        {
            var request = WebRequest.Create("http://169.254.169.254/metadata/identity/oauth2/token?api-version=2018-02-01&resource=https%3A%2F%2Fvault.azure.net");
            request.Headers.Add("Metadata", "true");
            WebResponse response = request.GetResponse();

            var responseString = String.Empty;
            var token = String.Empty;
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                responseString = reader.ReadToEnd();
            }

            var options = new JsonSerializerOptions { IgnoreReadOnlyProperties = true, IgnoreNullValues = true, ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true, WriteIndented = true };
            var tokenInfo = JsonSerializer.Deserialize<KeyVaultTokenInfo>(responseString, options);
            token = tokenInfo.access_token;

            return token;
        }
        #endregion
        #region GetSecretWithCredentials
        public static async Task<string> GetSecretWithCredentials(string key, TokenCredential credential = null)
        {
            var keyVaultAddress = ConfigurationHelper.GetSetting<string>("KeyVaultAddress", null);
            if (string.IsNullOrEmpty(keyVaultAddress)) { return null; }

            //var tokenCredentials = new TokenCredentials(token);
            //var azureCredentials = new AzureCredentials( tokenCredentials, tokenCredentials, tenantId, AzureEnvironment.AzureGlobalCloud);
            
            if (credential == null) { credential = new DefaultAzureCredential(includeInteractiveCredentials: true); }
            var client = new SecretClient(new Uri(keyVaultAddress), credential); // , new SecretClientOptions()

            var secret = await client.GetSecretAsync(key);

            var value = secret.Value.Value;
            return value;
        }
        #endregion
        #region GetSecretWithManagedIdentity
        public static async Task<string> GetSecretWithManagedIdentity(string key, DefaultAzureCredential credential = null)
        {
            var keyVaultAddress = ConfigurationHelper.GetSetting<string>("KeyVaultAddress", null);
            if (string.IsNullOrEmpty(keyVaultAddress)) { return null; }

            var responseString = String.Empty;
            var token = GetManagedIdentityToken();

            var secretKey = key.Replace('.', '-');
            if (!secretKey.StartsWith(TraceManager.ProcessName)) { secretKey = $"{TraceManager.ProcessName}-{secretKey}"; }
            var kvRequest = WebRequest.Create($@"{keyVaultAddress}/secrets/{secretKey}?api-version=2016-10-01");
            kvRequest.Headers.Add("Authorization", "Bearer " + token);
            var response = kvRequest.GetResponse();
            var value = String.Empty;
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                responseString = reader.ReadToEnd();
            }

            var options = new JsonSerializerOptions { IgnoreReadOnlyProperties = true, IgnoreNullValues = true, ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true, WriteIndented = true };
            var secretInfo = JsonSerializer.Deserialize<KeyVaultSecretInfo>(responseString, options);
            var secret = secretInfo.value;

            return secret;
        }
        #endregion
    }

    #region ConfigurationNameAttribute
    ///<summary>this attribute is used to define a name for an assembly or class to be used in configuration files.</summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class)]
    public sealed class ConfigurationNameAttribute : Attribute
    {
        #region internal state
        private string _name;
        #endregion
        #region construction
        ///<summary>constructs the attribute: gets a reference to the name.</summary>
        public ConfigurationNameAttribute(string name) { _name = name; }
        #endregion
        #region properties
        ///<summary>returns the name to be used for the assembly or the class.</summary>
        public string Name { get { return _name; } set { _name = value; } }
        #endregion
    }
    #endregion
}
