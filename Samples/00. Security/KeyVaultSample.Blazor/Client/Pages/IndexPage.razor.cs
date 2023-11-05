#region using
using Common;
using Common.Core.Blazor;
using KeyVaultSampleBlazor.Client.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
#endregion

namespace KeyVaultSampleBlazor.Client.Pages
{
    public partial class IndexPage : ComponentBaseCore
    {
        private readonly string[] BACKOFFICEAPPLICATIONNAMES = new[] { "Backoffice Application", "Role Manager", "Notifications Manager", "Configurations Manager" };
        private readonly string BACKOFFICEAPPLICATIONNAME = "Backoffice Application";
        private readonly string ROLEMANAGERNAME = "Role Manager";

        [Inject] protected ILogger<IndexPage> _logger { get; set; }
        [Inject] protected NavigationManager NavManager { get; set; }
        [Inject] protected BrowserService Service { get; set; }
        [Inject] IApplicationState ApplicationState { get; set; }
        //[Inject] protected ServerManager Server { get; set; }

        //public Application[] RoleManagerApplications
        //{
        //    get { return GetValue(() => RoleManagerApplications); }
        //    set { SetValue(() => RoleManagerApplications, value); }
        //}
        //public Application RoleManagerApplication
        //{
        //    get { return GetValue(() => RoleManagerApplication); }
        //    set { SetValue(() => RoleManagerApplication, value); }
        //}
        //public IList<LayoutComponent> Components
        //{
        //    get { return GetValue(() => Components); }
        //    set { SetValue(() => Components, value); }
        //}

        private bool collapseNavMenu = false;

        protected override async Task OnInitializedAsync()
        {
            using (var scope = _logger.BeginMethodScope())
            {
                ApplicationState.PropertyChanged += ApplicationState_PropertyChanged;
                //scope.LogDebug(new { RoleManagerApplications });

                var fragment = ComponentHelper.CreateComponent(typeof(NavMenu), new Dictionary<string, object>() { });
                scope.LogDebug($"ComponentHelper.CreateComponent(typeof(NavMenu)) returned {fragment}");
                var component = new LayoutComponent() { Id = $"userDetail{Guid.NewGuid()}", Location = "left", OuterClasses = "", InnerClasses = "", RenderFragment = fragment };

                var applicationState = ApplicationState as ApplicationState;
                //applicationState.Application = applicationState.Applications?.FirstOrDefault(a => BACKOFFICEAPPLICATIONNAME.Equals(a.Description, StringComparison.InvariantCultureIgnoreCase));

                WebCommands.ClearItems.Execute(new CommandArgs(this, null)); scope.LogDebug($"WebCommands.ClearItems.Execute(new CommandArgs(this, null));");
                WebCommands.AddItem.Execute(new CommandArgs(this, component)); scope.LogDebug($"WebCommands.AddItem.Execute(new CommandArgs(this, component));");
                //StateHasChanged(); scope.LogDebug($"StateHasChanged(); completed");
            }
        }
        #region ApplicationState_PropertyChanged
        private void ApplicationState_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            using (var scope = _logger.BeginMethodScope(new { e.PropertyName }))
            {
                var applicationState = this.ApplicationState as ApplicationState;
                //if (e.PropertyName == "RoleManagerApplication") { this.RoleManagerApplication = applicationState.RoleManagerApplication; StateHasChanged(); }
                //if (e.PropertyName == "SelectedApplication") { StateHasChanged(); }
            }
        }
        #endregion

        private async void SelectApplication(string applicationName)
        {
            using (var scope = _logger.BeginMethodScope(new { applicationName }))
            {
                //if (applicationName != null && applicationName.Equals("rolemanager", StringComparison.InvariantCultureIgnoreCase))
                //{
                //    NavManager.NavigateTo("/rlm_homepage");
                //}
            }
        }
    }
}
