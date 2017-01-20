using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Igorious.StardewValley.DynamicApi2.Extensions
{
    public static class ReflectionExtensions
    {
        public static Type Intersect(this Type firstType, Type secondType)
        {
            var counts = GetTypeHierarchy(firstType).Concat(GetTypeHierarchy(secondType))
                .GroupBy(t => t)
                .ToDictionary(g => g.Key, g => g.Count());

            return GetTypeHierarchy(firstType).First(t => counts[t] == 2);
        }

        public static IEnumerable<FieldInfo> GetAllFields(this Type type)
        {
            if (type == null) return Enumerable.Empty<FieldInfo>();

            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                                       BindingFlags.Static | BindingFlags.Instance |
                                       BindingFlags.DeclaredOnly;
            return type.GetFields(flags).Concat(GetAllFields(type.BaseType));
        }

        public static T GetField<T>(this object o, string fieldName) where T : class
        {
            return GetField<T>(o, o.GetType(), fieldName);
        }

        public static void SetField<T>(this object o, string fieldName, T value) where T : class
        {
            var fieldInfo = GetFieldInfo(o.GetType(), fieldName);
            fieldInfo?.SetValue(o, value);
        }

        public static bool IsImplementGenericInterface(this Type t, Type genericInterface)
        {
            return t.GetInterfaces().Where(i => i.IsGenericType)
                .Any(i => i.GetGenericTypeDefinition() == genericInterface);
        }

        private static IEnumerable<Type> GetTypeHierarchy(Type type)
        {
            do
            {
                yield return type;
                type = type.BaseType;
            } while (type != null);
        }

        private static T GetField<T>(object o, Type type, string fieldName) where T : class
        {
            return GetFieldInfo(type, fieldName)?.GetValue(o) as T;
        }

        private static FieldInfo GetFieldInfo(Type type, string fieldName)
        {
            while (type != typeof(object))
            {
                var fieldInfo = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (fieldInfo != null) return fieldInfo;
                type = type.BaseType;
            }
            return null;
        }
    }
}