using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace Common
{
    internal sealed class SerializationHelper
    {
        #region .ctor
        private SerializationHelper() { }
        #endregion

        #region Clone
        /// <summary>Clona un oggetto serializzandone il contenuto</summary>
        /// <typeparam name="T">Tipo dell'oggetto da clonare</typeparam>
        /// <param name="original">Oggetto da clonare</param>
        /// <returns>Oggetto clonato</returns>
        public static T Clone<T>(T original)
        {
            if (original == null || original.Equals(default(T))) { return original; }
            T copy = default(T);
            DataContractSerializer serializer = new DataContractSerializer(typeof(T));
            using (MemoryStream stream = new MemoryStream())
            {
                serializer.WriteObject(stream, original);
                stream.Seek(0, SeekOrigin.Begin);
                copy = (T)serializer.ReadObject(stream);
            }
            return copy;
        }
        #endregion
        #region Serialize
        /// <summary>Serialize l'oggetto in una stringa xml (NetDataContractSerializer)</summary>
        /// <param name="obj">Oggetto da setializzare</param>
        /// <returns>String contenente l'xml con l'oggetto serializzato</returns>
        public static string Serialize(object obj)
        {
            if (obj == null) { return null; }
            string xml = null;
            DataContractSerializer serializer = new DataContractSerializer(obj.GetType());
            using (MemoryStream stream = new MemoryStream())
            {
                serializer.WriteObject(stream, obj);
                stream.Seek(0, SeekOrigin.Begin);
                using (StreamReader reader = new StreamReader(stream))
                {
                    xml = reader.ReadToEnd();
                }
            }
            return xml;
        }
        #endregion
        #region SerializeXml
        /// <summary>Serialize l'oggetto in una stringa xml (NetDataContractSerializer)</summary>
        /// <param name="obj">Oggetto da setializzare</param>
        /// <returns>String contenente l'xml con l'oggetto serializzato</returns>
        public static string SerializeXml(object obj, XmlRootAttribute rootAttribute, XmlSerializerNamespaces xns)
        {
            if (obj == null) { return null; }
            string xml = null;

            var serializer = new XmlSerializer(obj.GetType(), rootAttribute);
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, obj, xns);
                stream.Seek(0, SeekOrigin.Begin);
                using (StreamReader reader = new StreamReader(stream))
                {
                    xml = reader.ReadToEnd();
                }
            }
            return xml;
        }
        #endregion

        #region Deserialize
        /// <summary>Deserialize l'oggetto da una stringa xml (NetDataContractSerializer)</summary>
        /// <param name="xml">Xml da cui ricostruire l'oggetto</param>
        /// <returns>Oggetto del tipo specificato</returns>
        public static T Deserialize<T>(string xml) where T : class
        {
            if (xml == null) { return null; }
            object obj = null;
            DataContractSerializer serializer = new DataContractSerializer(typeof(T));
            using (MemoryStream stream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(xml);
                    writer.Flush();
                    stream.Seek(0, SeekOrigin.Begin);
                    obj = serializer.ReadObject(stream);
                }
            }
            return obj as T;
        }
        #endregion
        #region DeserializeXml
        /// <summary>Deserialize l'oggetto da una stringa xml (NetDataContractSerializer)</summary>
        /// <param name="xml">Xml da cui ricostruire l'oggetto</param>
        /// <returns>Oggetto del tipo specificato</returns>
        public static object DeserializeXml(Type t, string xml, XmlRootAttribute rootAttribute, XmlSerializerNamespaces xns)
        {
            object obj = null;

            var serializer = new XmlSerializer(t, rootAttribute);
            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                {
                    writer.Write(xml);
                    writer.Flush();
                }
                stream.Seek(0, SeekOrigin.Begin);
                obj = serializer.Deserialize(stream);
            }
            return obj;
        }
        #endregion
    }
}
