using Common;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Core.Blazor
{
    public class ComponentHelper
    {
        public static Type T = typeof(ComponentHelper);
        protected ILogger<ComponentHelper> _logger { get; set; }

        public static RenderFragment CreateComponent(Type t) => builder =>
        {
            builder.OpenComponent(0, t);
            //builder.AddAttribute(1, "Title", "User ");
            builder.CloseComponent();
        };
        public static RenderFragment CreateComponent(Type t, IDictionary<string, object> parameters)
        {
            using (var scope = TraceLogger.BeginMethodScope(T))
            {
                var renderFragment = new RenderFragment(builder =>
                {
                    using (var scope = TraceLogger.BeginNamedScope(T, "RenderComponent")) 
                    {
                        builder.OpenComponent(0, t);

                        int i = 0;
                        parameters.ToList().ForEach(p =>
                        {
                            i++;
                            builder.AddAttribute(i, p.Key, p.Value);
                        });

                        builder.CloseComponent();
                    }
                });
                return renderFragment;
            }
        }

    }
}
