#region using
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml;
#endregion

namespace Common
{
    #region ABCActivator
    public static class ABCActivator
    {
        #region const
        private const string S_FORMATSPECIFIER_SERIALIZATIONFILENAME = @"Type={0},Name={1}.json";
        #endregion
        #region internal state
        #endregion

        #region Persist<T, Surr>(T t, string instanceName, string location)
        public static async Task<Exception> Persist<T, S, StateProvider>(T t, string instanceName, string location)
            where T : class, new()
            where StateProvider : IClassStateProvider<T, S>, new()
        {
            Exception ret = null;

            var stateProvider = new StateProvider();
            ret = await Persist<T, S>(t, instanceName, location, stateProvider);
            return ret;
        }
        #endregion
        #region Persist<T>(string instanceName, string location, IClassStateProvider<T, S> stateProvider)
        public static async Task<Exception> Persist<T, S>(T instance, string instanceName, string location, IClassStateProvider<T, S> stateProvider)
            where T : class, new()
        {
            Exception ret = null;
            if (instance == null) { return null; }
            try
            {
                location = location != null ? ExpandEnvironmentVariables(location) : null;
                if (string.IsNullOrEmpty(location) || !Path.IsPathRooted(location))
                {
                    var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
                    string company = ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCompanyAttribute), false)).Company;
                    string title = ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyTitleAttribute), false)).Title;
                    location = $@"%LOCALAPPDATA%\{company}\{title}\{location}";
                    location = ExpandEnvironmentVariables(location);
                }
                Uri uriLocation = new Uri(location);

                Directory.CreateDirectory(uriLocation.LocalPath);
                Uri uriFile = new Uri(uriLocation, string.Format(S_FORMATSPECIFIER_SERIALIZATIONFILENAME, typeof(T).FullName, instanceName));

                stateProvider.Attch(instance, default(S));
                var state = stateProvider.GetState();

                var serializeOptions = new JsonSerializerOptions { WriteIndented = true };
                Stream stream = null;
                using (stream = File.Create(uriFile.LocalPath))
                {
                    await JsonSerializer.SerializeAsync(stream, state, serializeOptions);
                }
            }
            catch (XmlException ex)
            {
                // Logger.Exception(ex); ret = ex;
            }
            catch (SerializationException _)
            {
                // Logger.Exception(ex); ret = ex;
            }
            catch (Exception ex)
            {
                // Logger.Exception(ex); ret = ex;
            }
            return ret;
        }
        #endregion

        #region RestoreState<T, StateProvider>(T t, string instanceName, string location)
        public static async Task<Exception> RestoreState<T, S, StateProvider>(T t, string instanceName, string location)
            where T : class, new()
            where StateProvider : ClassStateProvider<T, S>, new()
        {
            Exception ret = null;
            List<Type> knownTypes = new List<Type>();
            
            var stateProvider = new StateProvider();
            ret = await RestoreState<T, S, StateProvider>(t, instanceName, location, stateProvider);
            return ret;
        }
        #endregion
        #region RestoreState<T>(T t, string instanceName, string location, IClassStateProvider<T, S> stateProvider)
        public static async Task<Exception> RestoreState<T, S, StateProvider>(T t, string instanceName, string location, IClassStateProvider<T, S> stateProvider)
            where T : class, new()
        {
            Exception ret = null;
            try
            {
                location = location != null ? ExpandEnvironmentVariables(location) : null;
                if (string.IsNullOrEmpty(location) || !Path.IsPathRooted(location))
                {
                    var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
                    string company = ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCompanyAttribute), false)).Company;
                    string title = ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyTitleAttribute), false)).Title;
                    location = $@"%LOCALAPPDATA%\{company}\{title}\{location}";
                    location = ExpandEnvironmentVariables(location);
                }
                Uri uriLocation = new Uri(location);
                Uri uriFile = new Uri(uriLocation, string.Format(S_FORMATSPECIFIER_SERIALIZATIONFILENAME, typeof(T).FullName, instanceName));
                ret = await RestoreState<T, S, StateProvider>(t, uriFile, stateProvider);
            }
            catch (Exception ex)
            {
                // Logger.Exception(ex); ret = ex;
            }

            return ret;
        }
        #endregion
        #region RestoreState<T>(T t, Uri uriFile, IClassStateProvider<T, S> stateProvider)
        public static async Task<Exception> RestoreState<T, S, StateProvider>(T t, Uri uriFile, IClassStateProvider<T, S> stateProvider)
            where T : class, new()
        {
            Exception ret = null;
            try
            {
                Stream stream = null;
                if (File.Exists(uriFile.LocalPath)) { stream = File.OpenRead(uriFile.LocalPath); }
                if (stream == null) { return ret = new ApplicationException(string.Format("State file '{0}' was not found", uriFile.LocalPath)); }

                var serializeOptions = new JsonSerializerOptions();
                using (stream)
                {
                    var state = (S)await JsonSerializer.DeserializeAsync(stream, typeof(S), serializeOptions);
                    
                    stateProvider.Attch(t, state);
                    stateProvider.RestoreState();
                }
            }
            catch (XmlException ex)
            {
                // Logger.Exception(ex); ret = ex;
            }
            catch (SerializationException ex)
            {
                // Logger.Exception(ex); ret = ex;
            }
            catch (Exception ex)
            {
                // Logger.Exception(ex); ret = ex;
            }
            if (t == null) { t = new T(); }
            return ret;
        }
        #endregion

        #region ExpandEnvironmentVariables
        public static string ExpandEnvironmentVariables(string s)
        {
            string expanded = Environment.ExpandEnvironmentVariables(s);
            if (expanded.IndexOf("%LOCALAPPDATA%") >= 0) { expanded = expanded.Replace("%LOCALAPPDATA%", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)); }
            return expanded;
        }
        #endregion
    }
    #endregion
}
