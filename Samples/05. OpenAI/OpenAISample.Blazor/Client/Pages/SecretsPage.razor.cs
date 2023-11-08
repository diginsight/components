#region using
using Common;
using Common.Core.Blazor;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#endregion

namespace OpenAISampleBlazor.Client.Pages
{
    public partial class SecretsPage : ComponentBaseCore
    {
        [Inject] protected ILogger<SecretsPage> _logger { get; set; }
        [Inject] protected NavigationManager NavManager { get; set; }
        [Inject] protected IApplicationState ApplicationState { get; set; }
        //[Inject] protected ServerManager Server { get; set; }

        //#region Secrets
        //public IEnumerable<SecretProperties> Secrets
        //{
        //    get { return (IEnumerable<SecretProperties>)GetValue(SecretsProperty); }
        //    set { SetValue(SecretsProperty, value); }
        //}
        //public static readonly DependencyProperty SecretsProperty = DependencyProperty.Register("Secrets", typeof(IEnumerable<SecretProperties>), typeof(MainControl), new PropertyMetadata(null));
        //#endregion
        #region ApplicationsAll
        public IList<Secret> ApplicationsAll
        {
            get { return GetValue(() => ApplicationsAll); }
            set { SetValue(() => ApplicationsAll, value); }
        }
        #endregion
        #region Applications
        public IList<Secret> Applications
        {
            get { return GetValue(() => Applications); }
            set { SetValue(() => Applications, value); }
        }
        #endregion
        #region Application
        public Secret Application
        {
            get { return GetValue(() => Application); }
            set { SetValue(() => Application, value, ApplicationChanged); }
        }
        private void ApplicationChanged(object source, EventArgs args)
        {
            using (var scope = _logger.BeginMethodScope())
            {
                var application = this.Application;
                if (application == null || application.IsEnabled == false) { return; }

                var applicationState = this.ApplicationState as ApplicationState;
                //applicationState.RoleManagerApplication = this.Application;

                //if (application != null && ApplicationRolesAll != null)
                //{
                //    ApplicationRoles = (from ar in ApplicationRolesAll
                //                        where ar.ApplicationId == application.ApplicationId
                //                        select ar)?.ToArray();
                //}
                //else { ApplicationRoles = new ApplicationRole[0]; }
            }
        }
        #endregion

        //#region ApplicationRolesAll
        //public IList<ApplicationRole> ApplicationRolesAll
        //{
        //    get { return GetValue(() => ApplicationRolesAll); }
        //    set { SetValue(() => ApplicationRolesAll, value); }
        //}
        //#endregion
        //#region ApplicationRoles
        //public IList<ApplicationRole> ApplicationRoles
        //{
        //    get { return GetValue(() => ApplicationRoles); }
        //    set { SetValue(() => ApplicationRoles, value); }
        //}
        //#endregion
        //#region ApplicationRole
        //public ApplicationRole ApplicationRole
        //{
        //    get { return GetValue(() => ApplicationRole); }
        //    set { SetValue(() => ApplicationRole, value, ApplicationRoleChanged); }
        //}
        //private void ApplicationRoleChanged(object source, EventArgs args)
        //{
        //    using (var scope = _logger.BeginMethodScope())
        //    {
        //        //var user = this.ApplicationRole;
        //    }
        //}
        //#endregion

        protected override async Task OnInitializedAsync()
        {
            using (var scope = _logger.BeginMethodScope())
            {
                ApplicationState.PropertyChanged += ApplicationState_PropertyChanged;
                //scope.LogDebug($"Http.BaseAddress: {Http.BaseAddress}");

                var applicationState = this.ApplicationState as ApplicationState;
                //this.ApplicationsAll = applicationState.RoleManagerApplications;
                //this.Applications = GetSortedApplications(applicationState.RoleManagerApplications);

                //var options = ApplicationState.JsonSerializerSettings;
                //ApplicationsAll = await Server.GetAsync<IList<Application>>("Applications"); // ?$filter=isEnabled eq true
                //scope.LogDebug(new { ApplicationsAll });

                //applicationState.RoleManagerApplications = GetSortedApplications(ApplicationsAll);
            }
        }
        #region ApplicationState_PropertyChanged
        private void ApplicationState_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            using (var scope = _logger.BeginMethodScope(new { e.PropertyName }))
            {
                var applicationState = this.ApplicationState as ApplicationState;
                //if (e.PropertyName == "RoleManagerApplication") { this.Application = applicationState.RoleManagerApplication; StateHasChanged(); }
                //if (e.PropertyName == "RoleManagerApplications") { this.Applications = applicationState.RoleManagerApplications; StateHasChanged(); }
                //if (e.PropertyName == "SelectedApplication") { StateHasChanged(); }
            }
        }
        #endregion


        private void AddApplication()
        {
            using (var scope = _logger.BeginMethodScope())
            {
                //var fragment = ComponentHelper.CreateComponent(typeof(RLM_ApplicationDetailPage), new Dictionary<string, object>() {
                //    { "Application", new Application() { Description = "New application", ApplicationId = Guid.NewGuid().ToString().ToUpperInvariant() } },
                //    { "Applications", this.Applications }
                //});
                //scope.LogDebug($"ComponentHelper.CreateComponent(typeof(RLM_ApplicationDetailPage)) returned {fragment}");
                //var component = new LayoutComponent() { Id = $"appDetail{Guid.NewGuid()}", Location = "page", OuterClasses = "pt-5", InnerClasses = "align-self-center", RenderFragment = fragment };

                //WebCommands.AddItem.Execute(new CommandArgs(this, component));
            }
        }
        private void SelectApplication(Secret app)
        {
            using (var scope = _logger.BeginMethodScope(new { app }))
            {
                //this.Application = app;
            }
        }
        private void EditApplication(Secret app)
        {
            //using (var scope = _logger.BeginMethodScope(new { app }))
            //{
            //    var fragment = ComponentHelper.CreateComponent(typeof(RLM_ApplicationDetailPage), new Dictionary<string, object>() {
            //        { "Application", app },
            //        { "Applications", this.Applications }
            //    });
            //    scope.LogDebug($"ComponentHelper.CreateComponent(typeof(RLM_ApplicationDetailPage)) returned {fragment}");
            //    var component = new LayoutComponent() { Id = $"appDetail{Guid.NewGuid()}", Location = "page", OuterClasses = "pt-5", InnerClasses = "align-self-center", RenderFragment = fragment };

            //    WebCommands.AddItem.Execute(new CommandArgs(this, component));
            //}
        }
        private void DeleteApplication(Secret app)
        {
            using var scope = _logger.BeginMethodScope(new { app });
            
        }
        private void NavigateUsers()
        {
            using var scope = _logger.BeginMethodScope();
            NavManager.NavigateTo("/rlm_applicationroleuserpage");
        }
        private void NavigateRoles()
        {
            using var scope = _logger.BeginMethodScope();
            NavManager.NavigateTo("/rlm_rolespage");

        }

        public IList<Secret> GetSortedApplications(IList<Secret> apps)
        {
            //if (apps == null) { return new List<Application>(); }
            //return ApplicationExtensions.GetSortedApplications(apps); 
            return null;
        }
    }

    public partial class Secret
    {
        public Secret()
        {
        }

        public string ApplicationId { get; set; }
        public string Description { get; set; }
        public string Owner { get; set; }
        public bool? RolesRequireApproval { get; set; }
        public bool? IsEnabled { get; set; }
        public string Modifiedby { get; set; }
        public DateTime? Modified { get; set; }
    }

}
