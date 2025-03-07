// See https://aka.ms/new-console-template for more information
using System;

class Program
{
    static void Main()
    {
        Console.WriteLine("{0,-10} {1,-10} {2,-20} {3,-20}", "类型", "字节数", "最小值", "最大值");
        Console.WriteLine(new string('-', 60));

        PrintTypeInfo<sbyte>();
        PrintTypeInfo<byte>();
        PrintTypeInfo<short>();
        PrintTypeInfo<ushort>();
        PrintTypeInfo<int>();
        PrintTypeInfo<uint>();
        PrintTypeInfo<long>();
        PrintTypeInfo<ulong>();
        PrintTypeInfo<float>();
        PrintTypeInfo<double>();
        PrintTypeInfo<decimal>();
    }

    static void PrintTypeInfo<T>() where T : struct
    {
        Type type = typeof(T);
        Console.WriteLine("{0,-10} {1,-10} {2,-20} {3,-20}", 
            type.Name, 
            System.Runtime.InteropServices.Marshal.SizeOf(type), 
            GetMinValue<T>(), 
            GetMaxValue<T>());
    }

    static T GetMinValue<T>() where T : struct
    {
        return (T)typeof(T).GetField("MinValue").GetValue(null);
    }

    static T GetMaxValue<T>() where T : struct
    {
        return (T)typeof(T).GetField("MaxValue").GetValue(null);
    }
}