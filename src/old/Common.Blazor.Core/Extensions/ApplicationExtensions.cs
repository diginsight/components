using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Core.Blazor
{
    //public static class ApplicationExtensions
    //{
    //    private static readonly string ROLEMAN_APPLICATION_ORDER = "EkipConnect, Ekip Connect Mobile, -, Backoffice Application, Role Manager, Notifications Manager, Configurations Manager, -";

    //    public static IList<Application> GetMenuApplications(this IList<Application> pthis)
    //    {
    //        if (pthis == null) { return new List<Application>(); }

    //        var roleManagerApplications = new List<Application>(pthis);
    //        var menuApplications = new List<Application>();

    //        var rolemanagerApplicationsOrder = ROLEMAN_APPLICATION_ORDER.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    //        rolemanagerApplicationsOrder.ToList().ForEach(appName =>
    //        {
    //            if (appName == "-") { menuApplications.Add(new Application() { Description = "-" }); return; }
    //            var app = roleManagerApplications.FirstOrDefault(a => a.Description == appName);
    //            menuApplications.Add(app);
    //            roleManagerApplications.Remove(app);
    //        });

    //        menuApplications.AddRange(roleManagerApplications);

    //        return menuApplications;
    //    }
    //    public static IList<Application> GetSortedApplications(this IList<Application> pthis)
    //    {
    //        if (pthis == null) { return new List<Application>(); }

    //        var roleManagerApplications = new List<Application>(pthis);
    //        var sortedApplications = new List<Application>();

    //        var rolemanagerApplicationsOrder = ROLEMAN_APPLICATION_ORDER.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    //        rolemanagerApplicationsOrder.ToList().ForEach(appName =>
    //        {
    //            if (appName == "-") { /*menuApplications.Add(new Application() { Description = "-" });*/ return; }
    //            var app = roleManagerApplications.FirstOrDefault(a => a.Description == appName);
    //            sortedApplications.Add(app);
    //            roleManagerApplications.Remove(app);
    //        });

    //        sortedApplications.AddRange(roleManagerApplications);

    //        return sortedApplications;
    //    }

    //}
}
