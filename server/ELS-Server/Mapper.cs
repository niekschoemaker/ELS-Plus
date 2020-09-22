using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace ELS_Server
{
    public static class Mapper
    {
        public static void Map(ExpandoObject source, object destination)
        {
            source = source ?? throw new ArgumentNullException(nameof(source));
            destination = destination ?? throw new ArgumentNullException(nameof(destination));

            string normalizeName(string name) => name.ToLowerInvariant();

            IDictionary<string, object> dict = source;
            var type = destination.GetType();

            var setters = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.GetSetMethod() != null)
                .ToDictionary(p => normalizeName(p.Name));

            foreach (var item in dict)
            {
                if (setters.TryGetValue(normalizeName(item.Key), out var setter))
                {
                    var value = setter.PropertyType.ChangeType(item.Value);
                    setter.SetValue(destination, value);
                }
            }
        }

    }

    public static class TypeExtension
    {
        public static bool IsNullable(this Type type)
        {
            type = type ?? throw new ArgumentNullException(nameof(type));
            return type.IsGenericType
                && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
        }

        public static bool IsNullAssignable(this Type type)
        {
            type = type ?? throw new ArgumentNullException(nameof(type));
            return type.IsNullable() || !type.IsValueType;
        }

        public static object ChangeType(this Type type, object instance)
        {
            type = type ?? throw new ArgumentNullException(nameof(type));
            if (instance == null)
            {
                if (!type.IsNullAssignable())
                {
                    throw new InvalidCastException($"{type.FullName} is not null-assignable");
                }
                return null;
            }
            if (type.IsNullable())
            {
                type = Nullable.GetUnderlyingType(type);
            }
            return Convert.ChangeType(instance, type);
        }
    }

}
