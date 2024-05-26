#region using
using Azure.ResourceManager.Resources;
using Common;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Button = System.Windows.Controls.Button;
#endregion

namespace Common
{
    public partial class LogstringHelper
    {
        public static string ToLogStringInternal(System.Windows.Window pthis)
        {
            if (pthis == null) { return null; }
            string logString = $"{{Window:{{Name:{pthis.Name},ActualHeight:{pthis.ActualHeight},ActualWidth:{pthis.ActualWidth},AllowsTransparency:{pthis.AllowsTransparency},Background:{pthis.Background},Width:{pthis.Width},Height:{pthis.Height}}}}}";
            return logString;
        }
        public static string ToLogStringInternal(PropertyChangedEventArgs pthis)
        {
            if (pthis == null) { return null; }
            string logString = $"{{PropertyChangedEventArgs:{{PropertyName:{pthis.PropertyName}}}}}";
            return logString;
        }
        public static string ToLogStringInternal(ExecutedRoutedEventArgs pthis)
        {
            if (pthis == null) { return null; }
            string logString = $"{{ExecutedRoutedEventArgs:{{Parameter:Command:{pthis.Command.GetLogString()},{pthis.Parameter.GetLogString()},RoutedEvent:{pthis.RoutedEvent.GetLogString()},Source:{pthis.Source},OriginalSource:{pthis.OriginalSource}}}}}";
            return logString;
        }
        public static string ToLogStringInternal(RoutedUICommand pthis)
        {
            if (pthis == null) { return null; }
            string logString = $"{{RoutedUICommand:{{Name:{pthis.Name},Text:{pthis.Text},OwnerType:{pthis.OwnerType},InputGestures:{pthis.InputGestures.GetLogString()}}}}}";
            return logString;
        }
        public static string ToLogStringInternal(Button pthis)
        {
            if (pthis == null) { return null; }
            string logString = $"{{Button:{{Name:{pthis.Name},Content:{pthis.Content.GetLogString()}}}}}";
            return logString;
        }
        public static string ToLogStringInternal(Thread pthis)
        {
            if (pthis == null) { return null; }
            string logString = $"{{Button:{{Name:{pthis.Name},ManagedThreadId:{pthis.ManagedThreadId},ApartmentState:{pthis.ApartmentState},ThreadState:{pthis.ThreadState},IsAlive:{pthis.IsAlive},IsBackground:{pthis.IsBackground},IsThreadPoolThread:{pthis.IsThreadPoolThread},Priority:{pthis.Priority},CurrentCulture:{pthis.CurrentCulture},CurrentUICulture:{pthis.CurrentUICulture}}}}}";
            return logString;
        }
        public static string ToLogStringInternal(Microsoft.Graph.Models.Application pthis)
        {
            if (pthis == null) { return null; }
            string logString = $"{{Application:{{Id:{pthis.Id},AppId:{pthis.AppId},DisplayName:{pthis.DisplayName},Description:{pthis.Description},PublisherDomain:{pthis.PublisherDomain},SignInAudience:{pthis.SignInAudience},Owners:{pthis.Owners.GetLogString()}}}}}";
            return logString;
        }
        
        public static string ToLogStringInternal(Identity pthis)
        {
            if (pthis == null) { return null; }
            string logString = $"{{Identity:{{Name:{pthis.Name},Email:{pthis.Email},Upn:{pthis.Upn},Manager:{pthis.Manager}}}}}";
            return logString;
        }
        public static string ToLogStringInternal(TokenCacheNotificationArgs pthis)
        {
            if (pthis == null) { return null; }
            string logString = $"{{TokenCacheNotificationArgs:{{Account:{pthis.Account.GetLogString()},HasStateChanged:{pthis.HasStateChanged},RequestScopes:{pthis.RequestScopes.GetLogString()},SuggestedCacheExpiry:{pthis.SuggestedCacheExpiry},SuggestedCacheKey:{pthis.SuggestedCacheKey},TelemetryData:{pthis.TelemetryData},TokenCache:{pthis.TokenCache.GetLogString()},ClientId:{pthis.ClientId},PiiLoggingEnabled:{pthis.PiiLoggingEnabled},CorrelationId:{pthis.CorrelationId}}}}}";
            return logString;
        }
        public static string ToLogStringInternal(TenantData pthis)
        {
            if (pthis == null) { return null; }
            string logString = $"{{TenantData:{{DisplayName:{pthis.DisplayName},DefaultDomain:{pthis.DefaultDomain},Id:{pthis.Id},TenantId:{pthis.TenantId},TenantType:{pthis.TenantType},TenantCategory:{pthis.TenantCategory},Country:{pthis.Country},CountryCode:{pthis.CountryCode},TenantBrandingLogoUri:{pthis.TenantBrandingLogoUri}}}}}";
            return logString;
        }
        public static string ToLogStringInternal(TenantResource pthis)
        {
            if (pthis == null) { return null; }
            string logString = $"{{TenantData:{{Id:{pthis.Id},Data:{pthis.Data.GetLogString()}}}}}";
            return logString;
        }
    }
}
