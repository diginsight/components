#region using
using Common;
using Common.Core.Blazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#endregion

namespace OpenAISampleBlazor.Client.Shared
{
    public partial class MainLayout : LayoutComponentBaseCore
    {
        private readonly string[] BACKOFFICEAPPLICATIONNAMES = new[] { "Backoffice Application", "Role Manager", "Notifications Manager", "Configurations Manager" };
        private readonly string BACKOFFICEAPPLICATIONNAME = "Backoffice Application";

        [Inject] protected ILogger<MainLayout> _logger { get; set; }
        [Inject] protected BrowserService Service { get; set; }
        [Inject] IApplicationState ApplicationState { get; set; }
        //[Inject] protected ServerManager Server { get; set; }
        [CascadingParameter] private Task<AuthenticationState> AuthenticationStateTask { get; set; }


        //public IList<Application> Applications
        //{
        //    get { return GetValue(() => Applications); }
        //    set { SetValue(() => Applications, value); }
        //}
        //public Application Application
        //{
        //    get { return GetValue(() => Application); }
        //    set { SetValue(() => Application, value); }
        //}

        //public IList<Application> RoleManagerApplications
        //{
        //    get { return GetValue(() => RoleManagerApplications); }
        //    set { SetValue(() => RoleManagerApplications, value); }
        //}
        //public Application RoleManagerApplication
        //{
        //    get { return GetValue(() => RoleManagerApplication); }
        //    set { SetValue(() => RoleManagerApplication, value); }
        //}

        public IList<LayoutComponent> Components
        {
            get { return GetValue(() => Components); }
            set { SetValue(() => Components, value); }
        }

        private bool collapseNavMenu = false;

        protected override async Task OnInitializedAsync()
        {
            using (var scope = _logger.BeginMethodScope(new { BaseAddress = Http?.BaseAddress?.ToString() }))
            {
                _logger.LogDebug("Prova");

                ApplicationState.PropertyChanged += ApplicationState_PropertyChanged;
                this.Components = new List<LayoutComponent>();

                var applicationState = ApplicationState as ApplicationState;
                var commandManager = applicationState.CommandManager;

                commandManager.CommandBindings.Clear();
                commandManager.CommandBindings.Add(new CommandBinding(WebCommands.ClearItems, ClearItemsExecuted, ClearItemsCanExecute));
                commandManager.CommandBindings.Add(new CommandBinding(WebCommands.AddItem, AddItemExecuted, AddItemCanExecute));
                commandManager.CommandBindings.Add(new CommandBinding(WebCommands.RemoveItem, RemoveItemExecuted, RemoveItemCanExecute));
                scope.LogDebug(new { commandManager.CommandBindings });

                var user = (await AuthenticationStateTask)?.User;
                scope.LogDebug(new { user });

                scope.LogDebug($"Http.BaseAddress: {Http.BaseAddress?.AbsolutePath?.GetLogString()}");
                
                //var serverApplications = await Server.GetAsync<IList<Application>>("Applications?$filter=isEnabled eq true");
                //scope.LogDebug(new { serverApplications });

                //this.RoleManagerApplications = serverApplications;
                //applicationState.RoleManagerApplications = serverApplications;
                //applicationState.Applications = serverApplications.Where(a => BACKOFFICEAPPLICATIONNAMES.Contains(a.Description, StringComparer.InvariantCultureIgnoreCase)).ToList();
                //applicationState.Application = serverApplications.FirstOrDefault(a => BACKOFFICEAPPLICATIONNAME.Equals(a.Description, StringComparison.InvariantCultureIgnoreCase));

                //if (applicationState.RoleManagerApplications != null && applicationState.RoleManagerApplication == null)
                //{
                //    applicationState.RoleManagerApplication = applicationState.RoleManagerApplications.FirstOrDefault(a => a.Description.StartsWith("Ekip", StringComparison.InvariantCultureIgnoreCase));
                //}
                //scope.LogDebug(new { applicationState.RoleManagerApplication });
            }
        }
        #region ApplicationState_PropertyChanged
        private void ApplicationState_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            using (var scope = _logger.BeginMethodScope(new { e.PropertyName }))
            {
                var applicationState = this.ApplicationState as ApplicationState;
                //if (e.PropertyName == "RoleManagerApplication") { this.RoleManagerApplication = applicationState.RoleManagerApplication; StateHasChanged(); }
                //if (e.PropertyName == "Application") { this.Application = applicationState.Application; StateHasChanged(); }
                
                //if (e.PropertyName == "SelectedApplication") { StateHasChanged(); }
            }
        }
        #endregion

        private async void ToggleNavMenu()
        {
            using (var scope = _logger.BeginMethodScope())
            {
                collapseNavMenu = !collapseNavMenu;
            }
        }
        private async void SelectApplication(string applicationName)
        {
            using (var scope = _logger.BeginMethodScope(new { applicationName }))
            {
                //var appShortName = applicationName?.Trim().Replace(" ", "");
                //var application = RoleManagerApplications.FirstOrDefault(a => a.Description.Equals(applicationName, StringComparison.InvariantCultureIgnoreCase));
                //if (application != null)
                //{
                //    //ApplicationState.SelectedApplication = applicationName;
                //    var applicationState = ApplicationState as ApplicationState;
                //    applicationState.RoleManagerApplication = this.RoleManagerApplication = application;
                //}
            }
        }


        private void ClearItemsCanExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = true; }
        private void ClearItemsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            using (var scope = _logger.BeginMethodScope(new { sender, e.Parameter }))
            {
                this.Components.Clear(); StateHasChanged();
            }
        }
        private void AddItemCanExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = true; }
        private void AddItemExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            using (var scope = _logger.BeginMethodScope(new { sender, e.Parameter }))
            {
                var component = e.Parameter as LayoutComponent;
                if (component == null)
                {
                    var fragment = e.Parameter as RenderFragment;
                    component = new LayoutComponent() { Id = "", RenderFragment = fragment, OuterClasses = "pt-5", InnerClasses = "align-self-center" };
                }
                if (component == null) return;

                this.Components.Add(component); StateHasChanged();
            }
        }
        private void RemoveItemCanExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = true; }
        private void RemoveItemExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            using (var scope = _logger.BeginMethodScope(new { sender, e.Parameter }))
            {
                var component = e.Parameter as RenderFragment;
                this.Components.Remove(this.Components.LastOrDefault()); StateHasChanged();
            }
        }

        //public IList<Application> GetMenuApplications(IList<Application> apps)
        //{
        //    if (apps == null) { return new List<Application>(); }
        //    return ApplicationExtensions.GetMenuApplications(apps); ;
        //}
    }
}
