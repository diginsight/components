using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Animation;

namespace Common
{
    [ValueConversion(typeof(object), typeof(object))]
    public class LocalizeConverter : TransformConverter<object, object>
    {
        public LocalizeConverter()
        {
            this.Trasformation = delegate (object val)
            {
                string valueString = (string)val;
                // string suffix = (string)parameter;
                var ret = this.GetResourceValue<string>(valueString, valueString);

                return ret;
            };
            this.Trasformation2 = delegate (object[] values)
            {
                int i = 0;
                string key = values != null && values.Length > i && values[i] is string ? (string)values[i] : null; i++;
                int? deviceId = values != null && values.Length > i && values[i] is int? ? (int?)values[i] : 0; i++;
                if (deviceId == null) { return key; }

                var ret = ResourcesHelper.GetResourceValue<string>(this.GetType(), key, deviceId, key);
                return ret;
            };
        }
    }


}
