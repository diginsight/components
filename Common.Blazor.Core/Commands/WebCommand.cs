#region using
using Common;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
#endregion

namespace Common.Core.Blazor
{
    public static class WebCommandExtensions
    {
        public static void UseCommandManager(this IHost host)
        {
            WebCommand._host = host;
        }
    }
    public class WebCommand : ICommand
    {
        public static Type T = typeof(WebCommand);
        protected ILogger<WebCommand> _logger { get; set; }
        #region internal state
        internal static IHost _host;
        string _text;
        string _name;
        Type _type;
        #endregion

        #region .ctor
        public WebCommand(string text, string name, Type type)
        {
            _text = text;
            _name = name;
            _type = type;
        }
        #endregion

        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter) { return true; }
        public void Execute(object args)
        {
            //using (var scope = TraceLogger.BeginMethodScope(T, new { args }))
                //{
                if (_host?.Services == null) { return; }
                var applicationState = _host.Services.GetService(typeof(IApplicationState)) as BlazorApplicationStateBase;
                if (applicationState == null) { return; }

                var commandBindings = applicationState?.CommandManager?.CommandBindings;
                var binding = commandBindings?.FirstOrDefault(cb => cb.Command == this);
                if (binding == null) { return; }

                var commandArgs = args as CommandArgs;
                var canExecuteArgs = new CanExecuteRoutedEventArgs(this, commandArgs?.Parameter);

                binding.OnCanExecute(commandArgs.Source, canExecuteArgs);
                if (canExecuteArgs.CanExecute)
                {
                    var executedArgs = new ExecutedRoutedEventArgs(this, commandArgs?.Parameter);
                    binding.OnExecuted(commandArgs.Source, executedArgs);
                }
                //else { TraceLogger.LogDebug($"binding: {binding}, canExecuteArgs.CanExecute: {canExecuteArgs.CanExecute}"); }
            //}
        }
    }

    public class CommandArgs : EventArgs
    {
        #region .ctor
        public CommandArgs(ComponentBase source, object parameter)
        {
            this.Source = source;
            this.Parameter = parameter;
        }
        #endregion
        public ComponentBase Source { get; set; }
        public object Parameter { get; set; }
    }

}
