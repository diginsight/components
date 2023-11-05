#region using
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace Common.Core.Blazor
{
    public interface IApplicationState: INotifyPropertyChanged {
        IHost Host { get; set; }
        JsonSerializerSettings JsonSerializerSettings { get; set; }
        JsonSerializerOptions JsonSerializerOptions { get; set; }
        //IDictionary Properties { get; }
    }

    public class ApplicationStateBase : EntityBase, IApplicationState
    {
        public static ApplicationStateBase Current;
        
        #region IHost
        public IHost Host
        {
            get { return GetValue(() => Host); }
            set { SetValue(() => Host, value); }
        }
        #endregion

        #region JsonSerializerSettings
        public JsonSerializerSettings JsonSerializerSettings
        {
            get { return GetValue(() => JsonSerializerSettings); }
            set { SetValue(() => JsonSerializerSettings, value); }
        }
        #endregion
        #region JsonSerializerOptions
        public JsonSerializerOptions JsonSerializerOptions
        {
            get { return GetValue(() => JsonSerializerOptions); }
            set { SetValue(() => JsonSerializerOptions, value); }
        }
        #endregion
    }
    public interface IBlazorApplicationState : IApplicationState, INotifyPropertyChanged
    {
        CommandManager CommandManager { get; set; }
    }
    public class BlazorApplicationStateBase : EntityBase, IApplicationState
    {
        public static BlazorApplicationStateBase Current;

        #region IHost
        public IHost Host
        {
            get { return GetValue(() => Host); }
            set { SetValue(() => Host, value); }
        }
        #endregion
        #region CommandManager
        public CommandManager CommandManager
        {
            get { return GetValue(() => CommandManager); }
            set { SetValue(() => CommandManager, value); }
        }
        #endregion

        #region JsonSerializerSettings
        public JsonSerializerSettings JsonSerializerSettings
        {
            get { return GetValue(() => JsonSerializerSettings); }
            set { SetValue(() => JsonSerializerSettings, value); }
        }
        #endregion
        #region JsonSerializerOptions
        public JsonSerializerOptions JsonSerializerOptions
        {
            get { return GetValue(() => JsonSerializerOptions); }
            set { SetValue(() => JsonSerializerOptions, value); }
        }
        #endregion
    }
}
