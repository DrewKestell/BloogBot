using RaidMemberBot.Mem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RaidMemberBot.ExtensionMethods
{
    internal static class MemExtensions
    {
        internal static string WriteDown4X4Matrix(this IntPtr ptr)
        {
            var retList = new List<string[]>();
            for (int i = 0; i < 4; i++)
            {
                var row = new string[4];
                for (int x = 0; x < 4; x++)
                {
                    row[x] = ptr.Add(i * 4 + x * 4 * 4).ReadAs<float>().ToString();
                }
                retList.Add(row);
            }
            return retList.PadElementsInLines(5);
        }

        private static string PadElementsInLines(this List<string[]> lines, int padding = 1)
        {
            // Calculate maximum numbers for each element accross all lines
            var numElements = lines[0].Length;
            var maxValues = new int[numElements];
            for (int i = 0; i < numElements; i++)
            {
                maxValues[i] = lines.Max(x => (x.Length > i + 1 && x[i] != null ? x[i].Length : 0)) + padding;
            }
            var sb = new StringBuilder();
            // Build the output
            bool isFirst = true;
            foreach (var line in lines)
            {
                if (!isFirst)
                {
                    sb.AppendLine();
                }
                isFirst = false;
                for (int i = 0; i < line.Length; i++)
                {
                    var value = line[i];
                    // Append the value with padding of the maximum length of any value for this element
                    if (value != null)
                    {
                        sb.Append(value.PadRight(maxValues[i]));
                    }
                }
            }
            return sb.ToString();
        }

        internal static bool WriteTo<T>(this IntPtr addr, T value) where T : struct
        {
            return Memory.Reader.Write(addr, value);
        }

        internal static bool WriteTo<T>(this uint addr, T value) where T : struct
        {
            return Memory.Reader.Write((IntPtr)addr, value);
        }

        internal static bool WriteTo<T>(this int addr, T value) where T : struct
        {
            return Memory.Reader.Write((IntPtr)addr, value);
        }

        internal static T ReadAs<T>(this IntPtr value) where T : struct
        {
            return value == IntPtr.Zero ? default(T) : Memory.Reader.Read<T>(value);
        }

        internal static T ReadAs<T>(this int value) where T : struct
        {
            return value == 0 ? default(T) : Memory.Reader.Read<T>((IntPtr)value);
        }

        internal static T ReadAs<T>(this uint value) where T : struct
        {
            return value == 0 ? default(T) : Memory.Reader.Read<T>((IntPtr)value);
        }

        internal static string ReadString(this IntPtr value, int length = 512, Encoding encoding = null)
        {
            if (value == IntPtr.Zero) return "";
            if ((int)value < 00401000) return "";
            encoding ??= Encoding.ASCII;
            try
            {
                return Memory.Reader.ReadString(value, encoding, length);
            }
            catch (Exception)
            {
                return "";
            }
        }

        internal static string ReadString(this int value, int length = 512, Encoding encoding = null)
        {
            if (value == 0) return "";
            if (value < 00401000) return "";
            encoding ??= Encoding.ASCII;
            try
            {
                return Memory.Reader.ReadString((IntPtr)value, encoding, length);
            }
            catch (Exception)
            {
                return "";
            }
        }

        internal static string ReadString(this uint value, int length = 512, Encoding encoding = null)
        {
            if (value == 0) return "";
            if ((int)value < 00401000) return "";
            encoding ??= Encoding.ASCII;
            try
            {
                return Memory.Reader.ReadString((IntPtr)value, encoding, length);
            }
            catch (Exception)
            {
                return "";
            }
        }

        internal static IntPtr Add(this IntPtr value, IntPtr toAdd)
        {
            return IntPtr.Add(value, (int)toAdd);
        }

        internal static IntPtr Add(this IntPtr value, int toAdd)
        {
            return IntPtr.Add(value, toAdd);
        }

        internal static IntPtr PointsTo(this IntPtr value)
        {
            return value == IntPtr.Zero ? IntPtr.Zero : Memory.Reader.Read<IntPtr>(value);
        }

        internal static IntPtr PointsTo(this int value)
        {
            return value == 0 ? IntPtr.Zero : Memory.Reader.Read<IntPtr>((IntPtr)value);
        }

        internal static IntPtr PointsTo(this uint value)
        {
            return value == 0 ? IntPtr.Zero : Memory.Reader.Read<IntPtr>((IntPtr)value);
        }
    }
}
