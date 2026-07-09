using System.Collections;
using System.Reflection;

namespace Codata.scripts;

public static class Tools
{
    public static class ReflectionHelper
    {
        public static void SetFieldFromString(object obj, string fieldName, string value)
        {
            FieldInfo field = obj.GetType().GetField(
                fieldName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);

            if (field == null)
                throw new Exception($"字段不存在: {fieldName}");

            object result = ConvertString(value, field.FieldType);

            field.SetValue(obj, result);
        }
        private static object ConvertString(string value, Type type)
        {
            // Nullable<T>
            Type nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null)
            {
                if (string.IsNullOrEmpty(value))
                    return null;

                type = nullableType;
            }


            // string
            if (type == typeof(string))
                return value;


            // 数组
            if (type.IsArray)
            {
                Type elementType = type.GetElementType();

                string[] items = Split(value);

                Array array = Array.CreateInstance(
                    elementType,
                    items.Length);

                for (int i = 0; i < items.Length; i++)
                {
                    array.SetValue(
                        ConvertString(items[i], elementType),
                        i);
                }

                return array;
            }


            // List<T>
            if (type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type elementType = type.GetGenericArguments()[0];

                IList list = (IList)Activator.CreateInstance(type);

                foreach (string item in Split(value))
                {
                    list.Add(
                        ConvertString(item, elementType));
                }

                return list;
            }


            // Enum
            if (type.IsEnum)
                return Enum.Parse(type, value);


            // bool
            if (type == typeof(bool))
                return bool.Parse(value);


            // 基础类型
            if (type.IsPrimitive ||
                type == typeof(decimal))
            {
                return Convert.ChangeType(value, type);
            }


            // DateTime
            if (type == typeof(DateTime))
                return DateTime.Parse(value);


            throw new NotSupportedException(
                $"不支持类型:{type}");
        }
        private static string[] Split(string value)
        {
            return value.Split(
                new[] { ',', ';' },
                StringSplitOptions.RemoveEmptyEntries);
        }
    }
}