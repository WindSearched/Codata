using System.Collections;
using System.Reflection;
using CdPlugin;
using Codata.scripts.classes;
using MoonSharp.Interpreter;

namespace Codata.scripts;

public static class Tools
{
    public static Info info => Center.info;
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

        public static List<string> GetFieldsString(object obj, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public)
        {
            if (obj == null)
                return null;

            FieldInfo[] fields = obj.GetType()
                .GetFields(flags);

            return fields.Select(field => field.Name).Select(value => value?.ToString() ?? "null").ToList();
        }
    }
    public static class ImageTools
    {
        public static bool IsFullyTransparent(Image image)
        {
            using (Bitmap bmp = new Bitmap(image))
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        Color pixel = bmp.GetPixel(x, y);

                        if (pixel.A != 0)
                        {
                            Program.Log(pixel.A);
                            return false; // 有一个像素不是透明
                        }
                    }
                }
            }

            return true;
        }
    }
    public static class Strings
    {
        /// <summary>
        /// detect last index of cutter char, and split str in two part
        /// </summary>
        public static (string s1, string s2) SplitInLastIndexOf(string str, string cutter)
        {
            int ind = str.LastIndexOf(cutter);
            return new(str.Substring(0, ind + 1), str.Substring(ind + 1));
        }

        public static List<string> SplitAndRemoveRepeatedEmpties(string str, string cutter)
        {
            var s = str.Split(cutter);
            var l = new List<string>();
            bool isLeading = true;

            foreach (var ind in s)
            {
                if (ind != "")
                {
                    isLeading = false;
                    l.Add(ind);
                }
                else if (!isLeading && l[^1] != "")
                {
                    l.Add(ind);
                }
            }

            if (l.Count == 0 && s.Length > 0) l.Add("");

            return l;
        }
        public static string[] SplitAndRemoveEmpties(string str, string cutter)
        {
            var s = str.Split(cutter);
            return s.Where(x => x!= "").ToArray();
        }
    }
    public static class KeyCtrl
    {
        public static int GetLineOfInsertPosition() => Program.form.rtb.GetLineFromCharIndex(Program.form.rtb.SelectionStart);
        public static bool SearchLineInForm(LineCommand line, out int commandLine)
        {
            var l = GetLineOfInsertPosition();
            bool b = line.CanUse(l);
            commandLine = b ? l : 0;
            return b;
        }

        public static void ShowCommandLine(int line)
        {
            var f = Program.form.rtb;
            if(f.Lines.Length == 0 || line < 0 || line > f.Lines.Length) return;
            int start = f.GetFirstCharIndexFromLine(line);
            int length = f.Lines[line].Length;
            f.Select(start, length);
        }

        /// <returns>if the line is selected return false</returns>
        public static bool TryShowLine(int line)
        {
            var f =  Program.form.rtb;
            if(f.Lines.Length == 0 || line < 0 || line > f.Lines.Length) return false;
            int start = f.GetFirstCharIndexFromLine(line);
            int length = f.Lines[line].Length;

            if (start == f.SelectionStart && length == f.SelectionLength) return false;
            f.Select(start, length);
            return true;
        }
        /// <summary>
        /// register to line command with absolute index
        /// </summary>
        /// <param name="lcmd"></param>
        /// <param name="blocks"></param>
        public static void Register(LineCommand lcmd, List<CommandLine> blocks)
        {
            if(blocks is null) return;
            var f = Program.form.rtb;
            var l = f.Lines.Length - 1;
            blocks.ForEach(x => x.line += l);

            for (int i = 0; i < blocks.Count; i++)
            {
                lcmd.Reg(blocks[i].line + l , blocks[i].cmd);
            }
        }
    }

    /// <summary>
    /// log only the debug setting is active
    /// </summary>
    public static void DebugLog(object obj)
    {
        if(info.debug)
            Program.Log("[Debug]" + obj);
    }
    public static void SetAfterConfirm(Closure closure) => Program.afterConfirm.Set(closure);
    public static void SetAfterConfirm(Action act) => Program.afterConfirm.Set(act);
}