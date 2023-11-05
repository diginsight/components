using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Common.Core.Blazor
{
    public class WebCommands
    {
        public static readonly ICommand ClearItems = new WebCommand("ClearItems", "ClearItems", typeof(WebCommands));
        public static readonly ICommand AddItem = new WebCommand("AddItem", "AddItem", typeof(WebCommands));
        public static readonly ICommand RemoveItem = new WebCommand("RemoveItem", "RemoveItem", typeof(WebCommands));
    }
}
