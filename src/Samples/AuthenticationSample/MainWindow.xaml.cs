#region using
using Diginsight.CAOptions;
using Diginsight.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using OpenTelemetry.Resources;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Metrics = System.Collections.Generic.Dictionary<string, object>;
using Window = System.Windows.Window; // $$$
using AuthenticationToken = Microsoft.Datasync.Client.AuthenticationToken;
using AuthenticationSample.Service;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using Azure.Identity;
using Azure.Core;
#endregion

namespace AuthenticationSample
{
    /// <summary>Interaction logic for MainWindow.xaml</summary>
    public partial class MainWindow : Window
    {
        static Type T = typeof(MainWindow);
        private ILogger<MainWindow> logger;
        private readonly IClassAwareOptionsMonitor<AppSettingsOptions> appSettingsOptionsMonitor;
        private readonly IClassAwareOptionsMonitor<FeatureFlagOptions> featureFlagOptionsMonitor;
        private readonly IClassAwareOptionsMonitor<AzureKeyVaultOptions> azureKeyVaultOptionsMonitor;
        private readonly IClassAwareOptionsMonitor<AzureAdOptions> azureAdOptionsMonitor;

        private string GetScope([CallerMemberName] string memberName = "") { return memberName; }

        public static IPublicClientApplication IdentityClient { get; set; }
        #region AuthenticationResult
        public AuthenticationResult AuthenticationResult
        {
            get { return (AuthenticationResult)GetValue(AuthenticationResultProperty); }
            set { SetValue(AuthenticationResultProperty, value); }
        }
        public static readonly DependencyProperty AuthenticationResultProperty = DependencyProperty.Register("AuthenticationResult", typeof(AuthenticationResult), typeof(MainWindow), new PropertyMetadata());
        #endregion
        #region ClaimsPrincipal
        public ClaimsPrincipal ClaimsPrincipal
        {
            get { return (ClaimsPrincipal)GetValue(ClaimsPrincipalProperty); }
            set { SetValue(ClaimsPrincipalProperty, value); }
        }
        public static readonly DependencyProperty ClaimsPrincipalProperty = DependencyProperty.Register("ClaimsPrincipal", typeof(ClaimsPrincipal), typeof(MainWindow), new PropertyMetadata());
        #endregion

        #region .ctor
        static MainWindow()
        {
            var host = App.Host;


        }
        public MainWindow(ILogger<MainWindow> logger,
                          IClassAwareOptionsMonitor<AppSettingsOptions> appSettingsOptionsMonitor,
                          IClassAwareOptionsMonitor<FeatureFlagOptions> featureFlagOptionsMonitor,
                          IClassAwareOptionsMonitor<AzureKeyVaultOptions> azureKeyVaultOptionsMonitor,
                          IClassAwareOptionsMonitor<AzureAdOptions> azureAdOptionsMonitor)
        {
            this.logger = logger;
            using var activity = App.ActivitySource.StartMethodActivity(logger, new { logger });

            this.featureFlagOptionsMonitor = featureFlagOptionsMonitor;
            this.appSettingsOptionsMonitor = appSettingsOptionsMonitor;
            this.azureKeyVaultOptionsMonitor = azureKeyVaultOptionsMonitor;
            this.azureAdOptionsMonitor = azureAdOptionsMonitor;

            var clientId = azureAdOptionsMonitor.CurrentValue.ClientId;
            var tenantId = azureAdOptionsMonitor.CurrentValue.TenantId;
            var redirectUri = azureAdOptionsMonitor.CurrentValue.RedirectUri;

            var app = PublicClientApplicationBuilder
                        .Create(clientId)
                        .WithAuthority(AzureCloudInstance.AzurePublic, tenantId)
                        .WithRedirectUri(redirectUri)
                        .Build();
            IdentityClient = app;

            InitializeComponent();
        } 
        #endregion

        private void MainWindow_Initialized(object sender, EventArgs e)
        {
            using var activity = App.ActivitySource.StartMethodActivity(logger, new { sender, e });


        }


        int i = 0;
        private async void btnRun_Click(object sender, RoutedEventArgs e)
        {
            using var activity = App.ActivitySource.StartMethodActivity(logger, new { sender, e });

            try
            {

                // call AuthenticationSampleApi with token
                //var token = this.AuthenticationResult.AccessToken;

                //var api = new AuthenticationSampleApi(token);
                var credentialOptions = new DefaultAzureCredentialOptions { SharedTokenCacheUsername = this.AuthenticationResult.ClaimsPrincipal.Identity.Name, ExcludeInteractiveBrowserCredential = false, ExcludeSharedTokenCacheCredential = false, ExcludeAzureCliCredential = false, ExcludeEnvironmentCredential = true, ExcludeManagedIdentityCredential = true, ExcludeVisualStudioCodeCredential = true, ExcludeVisualStudioCredential = true };
                var credential = new DefaultAzureCredential(credentialOptions);
                //var credentialOptions = new SharedTokenCacheCredentialOptions() { };
                //var credential = new SharedTokenCacheCredential();

                // create credential from this.AuthenticationResult Access token


                var service = new RestSharpService(credential);
                var res = await service.Get("https://localhost:7213/api/Plants/getplants", null, null);


            }
            catch (Exception _) { }
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            using var activity = App.ActivitySource.StartMethodActivity(logger, new { sender, e });

            try
            {
                string[] scopes = Constants.Scopes;

                AuthenticationResult result = await GetAuthenticationToken();
                this.AuthenticationResult = result;
                this.ClaimsPrincipal = result.ClaimsPrincipal;
                var identity = this.ClaimsPrincipal.Identity; // getClaim name

                
            }
            catch (Exception _) { }
        }

        public async Task<AuthenticationResult> GetAuthenticationToken()
        {
            using var activity = App.ActivitySource.StartMethodActivity(logger);

            var accounts = await IdentityClient.GetAccountsAsync();
            AuthenticationResult? result = null;
            try
            {
                result = await IdentityClient.AcquireTokenSilent(Constants.Scopes, accounts.FirstOrDefault())
                    .ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                result = await IdentityClient
                    .AcquireTokenInteractive(Constants.Scopes)
                    .ExecuteAsync();
            }
            catch (Exception ex)
            {
                // Display the error text - probably as a pop-up
                Debug.WriteLine($"Error: Authentication failed: {ex.Message}");
            }

            var name = result?.ClaimsPrincipal?.Claims?.FirstOrDefault(c => c.Type == "name")?.Value;
            return result;
        }

    }
}
