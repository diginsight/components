#region using
using Common;
//using Common.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace Common
{
    public static class ResourcesHelper
    {
        #region constants
        private const string E_ARGUMENTNULLEXCEPTION = "argument '{name}' cannot be null";
        private const string S_NAME_PLACEHOLDER = "{name}";
        #endregion
        //private ILogger<ResourcesHelper> logger;

        // ResourceManager temp = new ResourceManager("Common.Properties.Resources", typeof(Resources).Assembly);
        // ResourceManager temp = new global::System.Resources.ResourceManager("Common.Properties.Resources", typeof(Resources).Assembly);
        // Common.Properties.Resources_device_132
        static Dictionary<string, ResourceManager> _resources = new Dictionary<string, ResourceManager>();

        public static Dictionary<string, ResourceManager> Resources { get => _resources; set => _resources = value; }

        public static string GetString(string resourceName)
        {
            if (resourceName != null && resourceName.Replace(" ", "").StartsWith("res:"))
            {
                resourceName = resourceName.Replace(" ", "").Replace("res:", "").Trim();

                var resManager = _resources.FirstOrDefault().Value;
                return resManager?.GetString(resourceName, CultureInfo.CurrentUICulture);
            }

            return resourceName;
        }

        #region GetResourceValue
        public static TRet GetResourceValue<TRet>(this object pthis, string name, TRet defaultValue = default(TRet), CultureInfo culture = null) { return GetResourceValue<TRet>(pthis.GetType(), name, defaultValue, culture); }
        public static TRet GetResourceValue<TClass, TRet>(string name, TRet defaultValue = default(TRet), CultureInfo culture = null) { return GetResourceValue<TRet>(typeof(TClass), name, defaultValue, culture); }
        public static TRet GetResourceValue<TRet>(Type t, string name, TRet defaultValue = default(TRet), CultureInfo culture = null) { return GetResourceValue<TRet>(t, name, null, defaultValue, culture); }
        public static TRet GetResourceValue<TRet>(Type t, string name, int? deviceID = null, TRet defaultValue = default(TRet), CultureInfo culture = null)
        {
            if (culture == null) { culture = CultureInfo.CurrentCulture; }

            var ret = defaultValue;
            var valueString = default(string);
            try
            {
                Type type = t; Assembly assembly = type.Assembly;
                var assemblyName = GetConfigName(assembly); var className = GetConfigName(type);
                var specificName = $"{assemblyName}.{className}.{name}";
                var sectionName = $"{className}.{name}";
                var groupName = $"{name}";

                if (valueString == null && deviceID != null) // deviceSpecific
                {
                    var ok = _resources.TryGetValue($"{assemblyName}.Common.Properties.Resources_device_{deviceID}", out var resManagerLocal);
                    if (resManagerLocal != null) { valueString = resManagerLocal.GetString(groupName, culture); }
                }
                if (valueString == null && deviceID != null) // device
                {
                    var ok = _resources.TryGetValue($"{assemblyName}.Common.Properties.Resources_device", out var resManagerLocal);
                    if (resManagerLocal != null) { valueString = resManagerLocal.GetString(groupName, culture); }
                }


                if (valueString == null) // normal
                {
                    var resManager = _resources.FirstOrDefault().Value;
                    valueString = resManager?.GetString(groupName, culture);
                }

                //if (valueString == null)
                //{
                //    var resManager = Common.Properties.Resources.ResourceManager;
                //    valueString = resManager.GetString(sectionName, culture);
                //}
                //if (valueString == null)
                //{
                //    var resManager = Common.Properties.Resources.ResourceManager;
                //    valueString = resManager.GetString(groupName, culture);
                //}

                if (valueString == null) { return ret = defaultValue; }

                var converter = TypeDescriptor.GetConverter(typeof(TRet));
                var result = converter.ConvertFrom(null, culture, valueString);
                ret = (TRet)result;
            }
            finally { TraceManager.Debug($"resource '{name}' (class:{t.Name},device:{deviceID},found:{valueString != null}): {ret}", "resource"); }
            return ret;
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

        #region LoadApplicationResources
        public static void LoadApplicationResources(IEnumerable<int> deviceIDs)
        {
            using (var scope = TraceLogger.BeginMethodScope(typeof(ResourcesHelper), new { deviceIDs }))
            {
                //if (deviceIDs == null) { return; }
                var assembly = Assembly.GetEntryAssembly();
                string[] resourceNames = assembly.GetManifestResourceNames();
                scope.LogDebug(new { resourceNames });

                foreach (string resourceName in resourceNames)
                {
                    var resourceBaseName = resourceName.Substring(0, resourceName.IndexOf(".resources"));
                    ResourceManager resource = new global::System.Resources.ResourceManager(resourceBaseName, assembly);
                    _resources.Add(resourceBaseName, resource); scope.LogDebug($"_resources.Add('{resourceBaseName}', {resource.GetLogString()}) completed");
                }

                // ResourceManager globalResources = new global::System.Resources.ResourceManager("Common.Properties.Resources", typeof(Common.Properties.Resources).Assembly);
                // ResourceManager deviceResources = new global::System.Resources.ResourceManager("Common.Properties.Resources_device", typeof(Common.Properties.Resources).Assembly);
                // foreach (var deviceID in deviceIDs)
                // {
                //     var resourceName = $"Common.Properties.Resources_device_{deviceID}";
                //     if (!_resources.ContainsKey(resourceName))
                //     {
                //         ResourceManager resource = new global::System.Resources.ResourceManager(resourceName, typeof(Common.Properties.Resources).Assembly);
                //         // if (resource!=null)
                //         _resources.Add(resourceName, resource);
                //     }
                // }
            }
        }
        #endregion
    }
}
