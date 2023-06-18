#region using
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Common.Configuration; 
#endregion

namespace Common
{
    public class AssemblyResolver
    {
        static assemblyBinding _assemblyBinding;

        static AssemblyResolver()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (_assemblyBinding == null || _assembly.Binding.dependentAssemblies == null) { return null; }

            var strongNameParts = args.Name.Split(',')?.Select(s => s?.Trim());
            var assemplyName = strongNameParts.FirstOrDefault();
            var dependentAssembly = _assemblyBinding.dependentAssemblies.FirstOrDefault(da => assemplyName.Equals(da.assemblyIdentity.name, StringComparison.InvariantCultureIgnoreCase)); // , StringComparer.InvariantCultureIgnoreCase
            if (dependentAssembly != null)
            {
                var assembly = dependentAssembly.assembly;
                if (assembly == null)
                {
                    assembly = Assembly.Load($"{dependentAssembly.assemblyIdentity.name}, Version={dependentAssembly.bindingRedirect.newVersion}, Culture={dependentAssembly.assemblyIdentity.culture}, PublicKeyToken={dependentAssembly.assemblyIdentity.publicKeyToken}");
                    dependentAssembly.assembly = assembly;
                }
                return dependentAssembly.assembly;
            }

            return null;
        }

        public static void ResolveBindingsFromFile(string path)
        {
            var xns = new XmlSerializerNamespaces();
            xns.Add(string.Empty, string.Empty);

            if (!Path.IsPathRooted(path))
            {
                //var folder = Path.GetDirectoryName(typeof(AssemblyResolver).Assembly.Location);
                //var folder = System.IO.Directory.GetCurrentDirectory();
                var folder = System.AppDomain.CurrentDomain.BaseDirectory.Trim('\\');
                path = $@"{folder}\{path}";
            }
            var s = File.ReadAllText(path);

            var dependentAssemblies = SerializationHelper.DeserializeXml(typeof(dependentAssembly[]), s, new XmlRootAttribute("dependentAssemblies"), xns) as dependentAssembly[];
            _assemblyBinding = new assemblyBinding() { dependentAssemblies = dependentAssemblies };

            _assemblyBinding.dependentAssemblies?.ToList()?.ForEach(da =>
            {
                var version = da.bindingRedirect.newVersion;
                da.assembly = null; //  Assembly.Load($"{da.assemblyIdentity.name}, Version={version}, Culture={da.assemblyIdentity.culture}, PublicKeyToken={da.assemblyIdentity.publicKeyToken}");
            });
        }

        public static void ResolveBindingsFromResource(Assembly assembly, string resourceName)
        {
            var xns = new XmlSerializerNamespaces();
            xns.Add(string.Empty, string.Empty);

            var content = default(string);
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream)) { content = reader.ReadToEnd(); }

            var dependentAssemblies = SerializationHelper.DeserializeXml(typeof(dependentAssembly[]), content, new XmlRootAttribute("dependentAssemblies"), xns) as dependentAssembly[];
            _assemblyBinding = new assemblyBinding() { dependentAssemblies = dependentAssemblies };

            _assemblyBinding.dependentAssemblies?.ToList()?.ForEach(da =>
            {
                var version = da.bindingRedirect.newVersion;
                da.assembly = Assembly.Load($"{da.assemblyIdentity.name}, Version={version}, Culture={da.assemblyIdentity.culture}, PublicKeyToken={da.assemblyIdentity.publicKeyToken}");
            });
        }

        public static void ResolveToAssemblyReferences(Assembly assembly)
        {
            var referenceAssemblies = assembly.GetReferencedAssemblies();
            var dependentAssemblies = new List<dependentAssembly>();

            referenceAssemblies?.ToList()?.ForEach(ra =>
            {
                var da = new dependentAssembly()
                {
                    assemblyIdentity = new assemblyIdentity() { name = ra.Name, culture = ra.CultureName, publicKeyToken = ra.GetPublicKeyToken().ToString() },
                    bindingRedirect = null,
                    assembly = Assembly.Load(ra)
                };
                dependentAssemblies.Add(da);
            });

            _assemblyBinding = new assemblyBinding() { dependentAssemblies = dependentAssemblies.ToArray() };
        }
    }
}
