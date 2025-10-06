#region using

using System.Runtime.Serialization.Json;
using System.Text.Json;

#endregion

namespace Diginsight.Components;

#region enum SerializationMode
internal enum SerializationMode
{
    Binary,
    XML,
    DataContract,
    Json
}
#endregion
internal class SerializationHelper
{
    static readonly private JsonSerializerOptions defaultOptions;
    
    #region .ctor
    static SerializationHelper()
    {
        defaultOptions = new JsonSerializerOptions
        {
            IgnoreReadOnlyProperties = true,
            //IgnoreNullValues = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            WriteIndented = true
        };
    } 
    #endregion

    #region SerializeJson<T>(T obj)
    /// <summary>Serializza l'oggetto in una stringa xml (DataContractSerializer)</summary>
    /// <typeparam name='T'>Tipo dell'oggetto da serializzare</typeparam>
    /// <param name='obj'>Oggetto da setializzare</param>
    /// <returns>String contenente l'xml con l'oggetto serializzato</returns>
    public static string SerializeJson<T>(T obj)
    {
        var jsonString = JsonSerializer.Serialize(obj, defaultOptions);
        return jsonString;
    }
    #endregion
    #region SerializeJson(Type t, T obj)
    /// <summary>Serializza l'oggetto in una stringa xml (DataContractSerializer)</summary>
    /// <typeparam name='T'>Tipo dell'oggetto da serializzare</typeparam>
    /// <param name='obj'>Oggetto da setializzare</param>
    /// <returns>String contenente l'xml con l'oggetto serializzato</returns>
    public static string SerializeJson(Type t, object obj)
    {
        if (t == null) { t = obj?.GetType(); }
        using (var stream = new MemoryStream())
        {
            string xml = null;
            var serializer = new DataContractJsonSerializer(t);
            serializer.WriteObject(stream, obj);
            stream.Seek(0, SeekOrigin.Begin);

            using (var reader = new StreamReader(stream))
            {
                xml = reader.ReadToEnd();
                return xml;
            }
        }
    }
    #endregion
    #region DeserializeJson<T>(string xml)
    /// <summary>Deserializza l'oggetto da una stringa xml (DataContractSerializer)</summary>
    /// <typeparam name='T'>Tipo dell'oggetto da deserializzare</typeparam>
    /// <param name='xml'>Xml da cui ricostruire l'oggetto</param>
    /// <returns>Oggetto del tipo specificato</returns>
    public static T DeserializeJson<T>(string xml)
    {
        var serializer = new DataContractJsonSerializer(typeof(T));
        using (var stream = new MemoryStream())
        {
            using (var writer = new StreamWriter(stream))
            {
                T obj = default(T);
                writer.Write(xml);
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                obj = (T)serializer.ReadObject(stream);
                return obj;
            }
        }
    }
    #endregion
    #region DeserializeJson(Type t, string json)
    /// <summary>Deserializza l'oggetto da una stringa xml (DataContractSerializer)</summary>
    /// <typeparam name='T'>Tipo dell'oggetto da deserializzare</typeparam>
    /// <param name='xml'>Xml da cui ricostruire l'oggetto</param>
    /// <returns>Oggetto del tipo specificato</returns>
    public static object DeserializeJson(Type t, string json)
    {
        var serializer = new DataContractJsonSerializer(t);
        using (var stream = new MemoryStream())
        {
            using (var writer = new StreamWriter(stream))
            {
                object obj = null;
                writer.Write(json);
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                obj = serializer.ReadObject(stream);
                return obj;
            }
        }
    }
    #endregion

    #region SerializeJson<T>(T obj)
    public static string SerializeJsonObject(object obj)
    {
        return JsonSerializer.Serialize(obj, defaultOptions);
    }

    /// <summary>Serializza l'oggetto in una stringa xml (DataContractSerializer)</summary>
    /// <typeparam name='T'>Tipo dell'oggetto da serializzare</typeparam>
    /// <param name='obj'>Oggetto da setializzare</param>
    /// <returns>String contenente l'xml con l'oggetto serializzato</returns>
    public static string SerializeJsonObject<T>(T obj)
    {
        return JsonSerializer.Serialize<T>(obj, defaultOptions);
    }
    #endregion
    #region SerializeJsonObject(Type t, T obj)
    /// <summary>Serializza l'oggetto in una stringa xml (DataContractSerializer)</summary>
    /// <typeparam name='T'>Tipo dell'oggetto da serializzare</typeparam>
    /// <param name='obj'>Oggetto da setializzare</param>
    /// <returns>String contenente l'xml con l'oggetto serializzato</returns>
    public static string SerializeJsonObject(Type t, object obj)
    {
        return JsonSerializer.Serialize(obj, t);
    }
    #endregion
    #region DeserializeJsonObject<T>(string json)
    /// <summary>Deserializza l'oggetto da una stringa xml (DataContractSerializer)</summary>
    /// <typeparam name='T'>Tipo dell'oggetto da deserializzare</typeparam>
    /// <param name='xml'>Xml da cui ricostruire l'oggetto</param>
    /// <returns>Oggetto del tipo specificato</returns>
    public static T DeserializeJsonObject<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json);
    }
    #endregion
    #region DeserializeJsonObject(Type t, string json)
    /// <summary>Deserializza l'oggetto da una stringa xml (DataContractSerializer)</summary>
    /// <typeparam name='T'>Tipo dell'oggetto da deserializzare</typeparam>
    /// <param name='xml'>Xml da cui ricostruire l'oggetto</param>
    /// <returns>Oggetto del tipo specificato</returns>
    public static object DeserializeJsonObject(Type t, string json)
    {
        return JsonSerializer.Deserialize(json, t, defaultOptions);
    }
    #endregion

    #region SerializeJsonDataContract<T>(T obj)
    /// <summary>Serializza l'oggetto in una stringa xml (DataContractSerializer)</summary>
    /// <typeparam name='T'>Tipo dell'oggetto da serializzare</typeparam>
    /// <param name='obj'>Oggetto da setializzare</param>
    /// <returns>String contenente l'xml con l'oggetto serializzato</returns>
    public static string SerializeJsonDataContract<T>(T obj)
    {
        using (var stream = new MemoryStream())
        {
            string xml = null;
            var serializer = new DataContractJsonSerializer(typeof(T));
            serializer.WriteObject(stream, obj);
            stream.Seek(0, SeekOrigin.Begin);

            using (var reader = new StreamReader(stream))
            {
                xml = reader.ReadToEnd();
                return xml;
            }
        }
    }
    #endregion
    #region SerializeJsonDataContract(Type t, T obj)
    /// <summary>Serializza l'oggetto in una stringa xml (DataContractSerializer)</summary>
    /// <typeparam name='T'>Tipo dell'oggetto da serializzare</typeparam>
    /// <param name='obj'>Oggetto da setializzare</param>
    /// <returns>String contenente l'xml con l'oggetto serializzato</returns>
    public static string SerializeJsonDataContract(Type t, object obj)
    {
        if (t == null) { t = obj?.GetType(); }
        using (var stream = new MemoryStream())
        {
            string xml = null;
            var serializer = new DataContractJsonSerializer(t);
            serializer.WriteObject(stream, obj);
            stream.Seek(0, SeekOrigin.Begin);

            using (var reader = new StreamReader(stream))
            {
                xml = reader.ReadToEnd();
                return xml;
            }
        }
    }
    #endregion
    #region DeserializeJsonDataContract<T>(string xml)
    /// <summary>Deserializza l'oggetto da una stringa xml (DataContractSerializer)</summary>
    /// <typeparam name='T'>Tipo dell'oggetto da deserializzare</typeparam>
    /// <param name='xml'>Xml da cui ricostruire l'oggetto</param>
    /// <returns>Oggetto del tipo specificato</returns>
    public static T DeserializeJsonDataContract<T>(string xml)
    {
        var serializer = new DataContractJsonSerializer(typeof(T));
        using (var stream = new MemoryStream())
        {
            using (var writer = new StreamWriter(stream))
            {
                T obj = default(T);
                writer.Write(xml);
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                obj = (T)serializer.ReadObject(stream);
                return obj;
            }
        }
    }
    #endregion
    #region DeserializeJsonDataContract(Type t, string json)
    /// <summary>Deserializza l'oggetto da una stringa xml (DataContractSerializer)</summary>
    /// <typeparam name='T'>Tipo dell'oggetto da deserializzare</typeparam>
    /// <param name='xml'>Xml da cui ricostruire l'oggetto</param>
    /// <returns>Oggetto del tipo specificato</returns>
    public static object DeserializeJsonDataContract(Type t, string json)
    {
        var serializer = new DataContractJsonSerializer(t);
        using (var stream = new MemoryStream())
        {
            using (var writer = new StreamWriter(stream))
            {
                object obj = null;
                writer.Write(json);
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                obj = serializer.ReadObject(stream);
                return obj;
            }
        }
    }
    #endregion

    #region CloneBySerializing<T>(T original)
    /// <summary>Clona un oggetto serializzandone il contenuto</summary>
    /// <typeparam name="T">Tipo dell'oggetto da clonare</typeparam>
    /// <param name="original">Oggetto da clonare</param>
    /// <returns>Oggetto clonato</returns>
    public static T CloneBySerializing<T>(T original) { return (T)CloneBySerializingInternal(original, SerializationMode.Binary); }
    #endregion
    #region CloneBySerializing<T>(T original, SerializationMode mode)
    public static T CloneBySerializing<T>(T original, SerializationMode mode) { return (T)CloneBySerializingInternal(original, mode); }
    #endregion
    #region CloneBySerializingInternal(object original, SerializationMode mode)
    /// <summary>Clona un oggetto serializzandone il contenuto</summary>
    /// <param name="original">Oggetto da clonare</param>
    /// <returns>Oggetto clonato</returns>
    internal static Object CloneBySerializingInternal(object original, SerializationMode mode)
    {
        if (original == null) { return null; }

        if (original == null) { return null; }
        var t = original.GetType();
        var json = SerializeJson(t, original);
        return DeserializeJson(t, json);

        //switch (mode)
        //{
        //    //case SerializationMode.DataContract:
        //    //    {
        //    //        using (var stream = new MemoryStream())
        //    //        {
        //    //            var serializer = new NetDataContractSerializer();
        //    //            serializer.Serialize(stream, original);
        //    //            stream.Seek(0, SeekOrigin.Begin);
        //    //            return serializer.Deserialize(stream);
        //    //        }
        //    //    }
        //    //case SerializationMode.Json:
        //    //    {
        //    //        if (original == null) { return null; }
        //    //        var t = original.GetType();
        //    //        var json = SerializeJson(t, original);
        //    //        return DeserializeJson(t, json);
        //    //    }
        //    //default:
        //    //    {
        //    //        using (var stream = new MemoryStream())
        //    //        {
        //    //            var formatter = new BinaryFormatter();
        //    //            formatter.Context = new StreamingContext(StreamingContextStates.Clone);
        //    //            formatter.Serialize(stream, original);
        //    //            stream.Position = 0;
        //    //            return formatter.Deserialize(stream);
        //    //        }
        //    //    }
        //}
    }
    #endregion
}
