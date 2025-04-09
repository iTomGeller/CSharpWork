using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IntegratedSolution
{
    // ================== d1 订单管理类定义 ==================
    public class OrderDetails
    {
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public override bool Equals(object obj) => 
            obj is OrderDetails details && ProductName == details.ProductName;

        public override int GetHashCode() => ProductName.GetHashCode();

        public override string ToString() => 
            $"{ProductName} x{Quantity} @{Price:C}";
    }

    public class Order
    {
        public string OrderId { get; set; }
        public string Customer { get; set; }
        public List<OrderDetails> Details { get; } = new List<OrderDetails>();
        public decimal TotalAmount => Details.Sum(d => d.Price * d.Quantity);

        public override bool Equals(object obj) => 
            obj is Order order && OrderId == order.OrderId;

        public override int GetHashCode() => OrderId.GetHashCode();

        public override string ToString() => 
            $"Order {OrderId}\nCustomer: {Customer}\nTotal: {TotalAmount:C}\n" +
            string.Join("\n", Details);
    }

    public class OrderService
    {
        public List<Order> Orders { get; } = new List<Order>();

        public void AddOrder(Order order)
        {
            if (Orders.Contains(order))
                throw new ArgumentException("订单已存在");
            Orders.Add(order);
        }

        public void RemoveOrder(string orderId)
        {
            var order = Orders.FirstOrDefault(o => o.OrderId == orderId) ?? 
                throw new KeyNotFoundException("订单不存在");
            Orders.Remove(order);
        }

        public void UpdateOrder(Order newOrder)
        {
            var index = Orders.FindIndex(o => o.OrderId == newOrder.OrderId);
            if (index == -1) throw new KeyNotFoundException("订单不存在");
            Orders[index] = newOrder;
        }

        public IEnumerable<Order> QueryOrders(Func<Order, bool> predicate) => 
            Orders.Where(predicate).OrderByDescending(o => o.TotalAmount);

        public void SortOrders(Comparison<Order> comparison = null) => 
            Orders.Sort(comparison ?? ((x, y) => x.OrderId.CompareTo(y.OrderId)));
    }

    // ================== d3 Person类定义 ==================
    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }

        public Person() { }
        public Person(string name, int age) { Name = name; Age = age; }

        public override string ToString() => $"{Name} ({Age}岁)";
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("========== 订单管理系统测试 ==========");
            TestOrderSystem();

            Console.WriteLine("\n========== 随机数处理测试 ==========");
            TestRandomNumbers();

            Console.WriteLine("\n========== 反射创建对象测试 ==========");
            TestReflection();
        }

        static void TestOrderSystem()
        {
            var service = new OrderService();
            
            // 创建测试订单
            var order1 = new Order { OrderId = "2023001", Customer = "张三" };
            order1.Details.AddRange(new[] {
                new OrderDetails { ProductName = "手机", Price = 2999m, Quantity = 1 },
                new OrderDetails { ProductName = "耳机", Price = 399m, Quantity = 2 }
            });

            try
            {
                service.AddOrder(order1);
                Console.WriteLine("添加订单成功");
                
                // 查询测试
                var results = service.QueryOrders(o => o.TotalAmount > 3000);
                Console.WriteLine("\n高价值订单查询结果：");
                foreach (var o in results)
                    Console.WriteLine(o + "\n");

                // 删除测试（故意使用错误ID触发异常）
                service.RemoveOrder("2023002");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"操作异常：{ex.Message}");
            }
        }

        static void TestRandomNumbers()
        {
            var rand = new Random();
            var numbers = Enumerable.Range(0, 100)
                           .Select(_ => rand.Next(0, 1001))
                           .ToList();

            var sorted = numbers.OrderByDescending(n => n).ToList();
            Console.WriteLine($"前10个最大数：{string.Join(", ", sorted.Take(10))}");
            Console.WriteLine($"总和：{sorted.Sum()}，平均值：{sorted.Average():F2}");
        }

        static void TestReflection()
        {
            // 使用无参构造函数
            Type type = typeof(Person);
            object obj1 = Activator.CreateInstance(type);
            Console.WriteLine($"默认创建：{obj1}");

            // 使用有参构造函数
            ConstructorInfo ctor = type.GetConstructor(new[] { typeof(string), typeof(int) });
            object obj2 = ctor.Invoke(new object[] { "李四", 28 });
            Console.WriteLine($"反射创建：{obj2}");
        }
    }
}
