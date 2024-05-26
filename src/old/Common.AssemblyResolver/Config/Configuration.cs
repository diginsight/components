using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Reflection;

namespace Common.Configuration
{
    //[Serializable()]
    public class bindingRedirect
    {
        [XmlAttribute("oldVersion")]
        public string oldVersion { get; set; }

        [XmlAttribute("newVersion")]
        public string newVersion { get; set; }
    }

    //[Serializable()]
    public class assemblyIdentity
    {
        [XmlAttribute("name")]
        public string name { get; set; }

        [XmlAttribute("publicKeyToken")]
        public string publicKeyToken { get; set; }

        [XmlAttribute("culture")]
        public string culture { get; set; }
    }

    //[XmlElementAttribute("dependentAssembly")]
    public class dependentAssembly
    {
        [XmlElementAttribute("assemblyIdentity")]
        public assemblyIdentity assemblyIdentity { get; set; }

        [XmlElementAttribute("bindingRedirect")]
        public bindingRedirect bindingRedirect { get; set; }

        [XmlIgnore]
        public Assembly assembly { get; set; }
    }

    [XmlRootAttribute("assemblyBinding", Namespace = "", IsNullable = false)]
    public class assemblyBinding
    {
        [XmlArray("dependentAssemblies", Form = XmlSchemaForm.None)]
        [XmlArrayItem("dependentAssembly", typeof(dependentAssembly))]
        public dependentAssembly[] dependentAssemblies { get; set; }
    }
}
