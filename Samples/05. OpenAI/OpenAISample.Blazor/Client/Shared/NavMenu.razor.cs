#region using
using Common;
using Common.Core.Blazor;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using BlazorPro.BlazorSize;
#endregion

namespace OpenAISampleBlazor.Client.Shared
{
    public partial class NavMenu : ComponentBase, IDisposable
    {
        [Parameter] public EventCallback<string> OnClick { get; set; }
        [Inject] protected ILogger<NavMenu> _logger { get; set; }
        [Inject] protected BrowserService Service { get; set; }

        private bool collapseNavMenu = false;

        #region .ctor
        public NavMenu()
        {
            using var scope = _logger.BeginMethodScope();

        }
        #endregion

        protected override void OnAfterRender(bool firstRender)
        {
            using var scope = _logger.BeginMethodScope();

            if (firstRender)
            {
            }
        }

        private async void ToggleNavMenu()
        {
            using var scope = _logger.BeginMethodScope();

            collapseNavMenu = !collapseNavMenu;
            await OnClick.InvokeAsync("");
        }
        private async void ToggleNavMenuSmall()
        {
            using var scope = _logger.BeginMethodScope();

            var dimension = await Service.GetDimensions();
            var width = dimension.Width;
            scope.LogDebug($"Width:{width}");

            if (width > 641) { return; }
            collapseNavMenu = !collapseNavMenu;
            await OnClick.InvokeAsync("");
        }

        void IDisposable.Dispose()
        {
            using var scope = _logger.BeginMethodScope();

        }
    }
}
