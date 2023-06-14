#region using
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks; 
#endregion

namespace Common
{
    public static class ReflectionHelper
    {
        public static object CreateGenericList(this PropertyInfo pi)
        {
            var listType = typeof(List<>);
            var genericArgs = pi.PropertyType.GetGenericArguments();
            var concreteType = listType.MakeGenericType(genericArgs);
            var newList = Activator.CreateInstance(concreteType);
            return newList;
        }

        public static object CreateCollectionItem(this PropertyInfo pi)
        {
            Type itemType = pi.PropertyType.GetGenericArguments()[0];
            return Activator.CreateInstance(itemType);
        }

        public static bool IsCollection(this Type type)
        {
            return (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                                          type.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                                          type.GetGenericTypeDefinition() == typeof(IList<>) ||
                                          type.GetGenericTypeDefinition() == typeof(List<>) ||
                                          type.GetGenericTypeDefinition() == typeof(Dictionary<,>))) ||

                   type.IsArray ||

                   type.IsSubclassOfRawGeneric(typeof(KeyedCollection<,>)) ||                   
                   type.IsSubclassOfRawGeneric(typeof(ReadOnlyCollection<>)) ||
                   type.IsSubclassOfRawGeneric(typeof(List<>)) ||
                   type.IsSubclassOfRawGeneric(typeof(BindingList<>));
        }

        public static Type CollectionElementType(this Type type)
        {
            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                                type.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                                type.GetGenericTypeDefinition() == typeof(IList<>) ||
                                type.GetGenericTypeDefinition() == typeof(List<>)))
                return type.GetGenericArguments().First();

            if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                return type.GetGenericArguments().Skip(1).First();

            if (type.IsArray)
                return type.GetElementType();

            if (type.IsSubclassOfRawGeneric(typeof(KeyedCollection<,>)))
                return type.GenericBaseClass().GetGenericArguments().Skip(1).First();

            if (type.IsSubclassOfRawGeneric(typeof(ReadOnlyCollection<>)) ||
                type.IsSubclassOfRawGeneric(typeof(List<>)) ||
                type.IsSubclassOfRawGeneric(typeof(BindingList<>)))
                return type.GenericBaseClass().GetGenericArguments().First();

            return null;
        }

        public static bool HasStringIndexer(this Type type)
        {
             return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>)) ||
                    type.IsSubclassOfRawGeneric(typeof(KeyedCollection<,>));
        }

        public static IDictionary GetDictionary(this object obj)
        {
            Type type = obj.GetType();

             if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                 return obj as IDictionary;

             if(type.IsSubclassOfRawGeneric(typeof(KeyedCollection<,>)))
                 return type.GetProperty("Dictionary", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(obj) as IDictionary;

            return null;
        }

        public static bool IsNullableType(this Type type)
        {
            return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        public static Type NullableArgument(this Type type)
        {
            if (type.IsNullableType())
                return type.GetGenericArguments()[0];

            return null;
        }

        public static Type EkipConnectType(string typeName)
        {
            string fullTypeName = string.Format("EkipConnect.Models.{0}", typeName);
            return Type.GetType(fullTypeName, false);
        }

        public static bool IsSubclassOrEqual(this Type type, Type baseType)
        {
            return type.IsSubclassOf(baseType) || type == baseType;
        }

        public static bool IsSubclassOfRawGeneric(this Type type, Type generic)
        {
            while (type != null && type != typeof(object))
            {
                var cur = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                if (generic == cur)
                {
                    return true;
                }
                type = type.BaseType;
            }
            return false;
        }

        public static Type GenericBaseClass(this Type type)
        {
            while (type != null && type != typeof(object))
            {
                if (type.IsGenericType)
                    return type;

                type = type.BaseType;
            }

            return null;
        }

        public static bool IsNumericType(this Type type)
        {
            if (type == null)
            {
                return false;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                case TypeCode.Object:
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        return IsNumericType(Nullable.GetUnderlyingType(type));
                    }
                    return false;
            }
            return false;
        }

        public static bool DeclaringTypeValid(this Type type)
        {
            return type != typeof(object) &&
                    type.Name != "DeviceBindableBase" &&
                    type.Name != "ViewModelBase" &&
                    type.Name != "BreakerViewModel";
                    //type != typeof(DeviceBindableBase) &&
                    //type != typeof(ViewModelBase) &&
                    //type != typeof(BreakerViewModel);
        }
    }
}
