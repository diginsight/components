#region using
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace Common.Core.Blazor
{
    // LayoutComponent
    //   id
    //   RenderFragment
    //   Location main, nav, page
    //   OuterClasses eg. pt-0 pt-5
    //   InnerClasses eg. align-self-center
    public class LayoutComponent: EntityBase
    {
        #region Id
        public string Id
        {
            get { return GetValue(() => Id); }
            set { SetValue(() => Id, value); }
        }
        #endregion
        #region RenderFragment
        public RenderFragment RenderFragment
        {
            get { return GetValue(() => RenderFragment); }
            set { SetValue(() => RenderFragment, value); }
        }
        #endregion
        #region Location
        public string Location
        {
            get { return GetValue(() => Location); }
            set { SetValue(() => Location, value); }
        }
        #endregion
        #region OuterClasses
        public string OuterClasses
        {
            get { return GetValue(() => OuterClasses); }
            set { SetValue(() => OuterClasses, value); }
        }
        #endregion
        #region InnerClasses
        public string InnerClasses
        {
            get { return GetValue(() => InnerClasses); }
            set { SetValue(() => InnerClasses, value); }
        }
        #endregion
    }
}
