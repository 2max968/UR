using System;
using System.Runtime.InteropServices;

namespace Assets.Scripts
{
    internal static class URUtil
    {
        public static T ArrayToStruct<T>(byte[] array, int offset) where T : struct
        {
            int length = Marshal.SizeOf<T>();
            int minLength = Math.Min(length, array.Length - offset);
            IntPtr nativeArray = Marshal.AllocHGlobal(length);
            Marshal.Copy(array, offset, nativeArray, minLength);
            T structure = Marshal.PtrToStructure<T>(nativeArray);
            Marshal.FreeHGlobal(nativeArray);
            return structure;
        }

        public static int ArrayToInt32(byte[] array, int offset)
        {
            return (array[offset + 0] << 24) | (array[offset + 1] << 16) | (array[offset + 2] << 8) | (array[offset + 3] << 0);
        }
    }

    public struct NetworkDouble
    {
        public byte B0;
        public byte B1;
        public byte B2;
        public byte B3;
        public byte B4;
        public byte B5;
        public byte B6;
        public byte B7;

        public double AsDouble => BitConverter.ToDouble(new[] { B7, B6, B5, B4, B3, B2, B1, B0 }, 0);

        public static implicit operator double(NetworkDouble dbl) => dbl.AsDouble;

        public override string ToString()
        {
            return AsDouble.ToString();
        }
    }

    public struct NetworkFloat
    {
        public byte B0;
        public byte B1;
        public byte B2;
        public byte B3;

        public float AsFloat => BitConverter.ToSingle(new[] { B3, B2, B1, B0 }, 0);

        public static implicit operator float(NetworkFloat flt) => flt.AsFloat;

        public override string ToString()
        {
            return AsFloat.ToString();
        }
    }

    public struct NetworkBool
    {
        public byte B0;

        public bool AsBool => B0 != 0;

        public static implicit operator bool(NetworkBool b) => b.AsBool;

        public override string ToString()
        {
            return AsBool.ToString();
        }
    }

    public struct NetworkInt32
    {
        public byte B0;
        public byte B1;
        public byte B2;
        public byte B3;

        public int AsInt => BitConverter.ToInt32(new[] { B3, B2, B1, B0 }, 0);

        public static implicit operator int(NetworkInt32 i) => i.AsInt;

        public override string ToString()
        {
            return AsInt.ToString();
        }
    }
    
    public struct NetworkInt64
    {
        public byte B0;
        public byte B1;
        public byte B2;
        public byte B3;
        public byte B4;
        public byte B5;
        public byte B6;
        public byte B7;

        public long AsInt => BitConverter.ToInt64(new[] { B7, B6, B5, B4, B3, B2, B1, B0 }, 0);

        public static implicit operator long(NetworkInt64 i) => i.AsInt;

        public override string ToString()
        {
            return AsInt.ToString();
        }
    }

    public struct NetworkDouble6
    {
        public NetworkDouble D1;
        public NetworkDouble D2;
        public NetworkDouble D3;
        public NetworkDouble D4;
        public NetworkDouble D5;
        public NetworkDouble D6;
    }
}