// See https://aka.ms/new-console-template for more information
using System;

class Program
{
    static void Main()
    {
        Console.WriteLine("你好！我是黄义健");
        Console.Write("请输入下限: ");
        int lowerLimit = int.Parse(Console.ReadLine());

        Console.Write("请输入上限: ");
        int upperLimit = int.Parse(Console.ReadLine());

        PrintPrimes(lowerLimit, upperLimit);
    }

    static void PrintPrimes(int lowerLimit, int upperLimit)
    {
        int count = 0;

        for (int number = lowerLimit; number <= upperLimit; number++)
        {
            if (IsPrime(number))
            {
                Console.Write(number + " ");
                count++;

                // 每10个数换行
                if (count % 10 == 0)
                {
                    Console.WriteLine();
                }
            }
        }

        // 如果最后一行没有换行，补一个换行
        if (count % 10 != 0)
        {
            Console.WriteLine();
        }
    }

    static bool IsPrime(int number)
    {
        if (number < 2) return false;
        for (int i = 2; i <= Math.Sqrt(number); i++)
        {
            if (number % i == 0) return false;
        }
        return true;
    }
}