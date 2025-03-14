using System;
using System.Collections.Generic;

// ================== Problem C1 ==================
public class ArrayCalculator
{
    public static void Calculate(int[] arr)
    {
        if (arr == null || arr.Length == 0)
        {
            Console.WriteLine("数组为空");
            return;
        }

        int max = arr[0], min = arr[0], sum = 0;
        foreach (int num in arr)
        {
            max = Math.Max(max, num);
            min = Math.Min(min, num);
            sum += num;
        }
        double avg = (double)sum / arr.Length;

        Console.WriteLine($"最大值: {max}, 最小值: {min}, 平均值: {avg:F2}, 总和: {sum}");
    }
}

// ================== Problem C2 & C3 ==================
public abstract class Shape
{
    public abstract double CalculateArea();
    public abstract bool IsValid();
}

public class Rectangle : Shape
{
    public double Width { get; set; }
    public double Height { get; set; }

    public Rectangle(double w, double h)
    {
        Width = w;
        Height = h;
    }

    public override double CalculateArea() => Width * Height;
    public override bool IsValid() => Width > 0 && Height > 0;
}

public class Square : Shape
{
    public double Side { get; set; }

    public Square(double s) => Side = s;

    public override double CalculateArea() => Side * Side;
    public override bool IsValid() => Side > 0;
}

public class Triangle : Shape
{
    public double A { get; set; }
    public double B { get; set; }
    public double C { get; set; }

    public Triangle(double a, double b, double c)
    {
        A = a;
        B = b;
        C = c;
    }

    public override double CalculateArea()
    {
        if (!IsValid()) return 0;
        double p = (A + B + C) / 2;
        return Math.Sqrt(p * (p - A) * (p - B) * (p - C));
    }

    public override bool IsValid() => 
        A > 0 && B > 0 && C > 0 &&
        A + B > C && A + C > B && B + C > A;
}

public class ShapeFactory
{
    private static Random rand = new Random();

    public static Shape CreateShape()
    {
        int type = rand.Next(3);
        switch (type)
        {
            case 0: // Rectangle
                return new Rectangle(rand.Next(1, 10), rand.Next(1, 10));
            case 1: // Square
                return new Square(rand.Next(1, 10));
            case 2: // Triangle
                double a = rand.Next(1, 5);
                double b = rand.Next(1, 5);
                double c = rand.Next(Math.Max((int)a, (int)b), (int)(a + b));
                return new Triangle(a, b, c);
            default:
                throw new InvalidOperationException();
        }
    }
}

// ================== Problem C4 ==================
public class LinkedListNode<T>
{
    public T Value { get; set; }
    public LinkedListNode<T> Next { get; set; }

    public LinkedListNode(T value) => Value = value;
}

public class LinkedList<T>
{
    private LinkedListNode<T> head;

    public void Add(T value)
    {
        var newNode = new LinkedListNode<T>(value);
        if (head == null) head = newNode;
        else
        {
            var current = head;
            while (current.Next != null)
                current = current.Next;
            current.Next = newNode;
        }
    }

    public void ForEach(Action<T> action)
    {
        var current = head;
        while (current != null)
        {
            action(current.Value);
            current = current.Next;
        }
    }
}

// ================== Problem C5 ==================
public class AlarmClock
{
    public DateTime AlarmTime { get; set; }
    public event EventHandler Tick;
    public event EventHandler Alarm;

    public void Start()
    {
        new Thread(() =>
        {
            while (true)
            {
                Tick?.Invoke(this, EventArgs.Empty);
                if (DateTime.Now >= AlarmTime)
                    Alarm?.Invoke(this, EventArgs.Empty);
                Thread.Sleep(1000);
            }
        }).Start();
    }
}

// ================== Main Program ==================
class Program
{
    static void Main()
    {
        // C1测试
        int[] arr = { 3, 1, 4, 1, 5, 9, 2, 6 };
        Console.WriteLine("C1结果:");
        ArrayCalculator.Calculate(arr);

        // C3测试
        Console.WriteLine("\nC3结果:");
        double totalArea = 0;
        for (int i = 0; i < 10; i++)
        {
            var shape = ShapeFactory.CreateShape();
            totalArea += shape.CalculateArea();
        }
        Console.WriteLine($"总面积: {totalArea:F2}");

        // C4测试
        Console.WriteLine("\nC4结果:");
        var list = new LinkedList<int>();
        list.Add(3); list.Add(1); list.Add(4); list.Add(2);
        
        Console.Write("元素：");
        list.ForEach(x => Console.Write(x + " "));
        
        int sum = 0, max = int.MinValue, min = int.MaxValue;
        list.ForEach(x => {
            sum += x;
            max = Math.Max(max, x);
            min = Math.Min(min, x);
        });
        Console.WriteLine($"\n总和: {sum}, 最大值: {max}, 最小值: {min}");

        // C5测试
        Console.WriteLine("\nC5结果（运行5秒）:");
        var clock = new AlarmClock
        {
            AlarmTime = DateTime.Now.AddSeconds(3)
        };
        clock.Tick += (s, e) => Console.WriteLine($"Tick: {DateTime.Now:T}");
        clock.Alarm += (s, e) => Console.WriteLine("ALARM!!!");
        clock.Start();
        Thread.Sleep(5000);
    }
}
