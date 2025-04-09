// OrderSystem.csproj
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace OrderSystem
{
    [Serializable]
    [XmlRoot("OrderDetails")]
    public class OrderDetails
    {
        [XmlElement("Product")]
        public string ProductName { get; set; }
        
        [XmlElement("UnitPrice")]
        public decimal Price { get; set; }
        
        [XmlElement("Quantity")]
        public int Quantity { get; set; }

        public override bool Equals(object obj) => 
            obj is OrderDetails details && ProductName == details.ProductName;

        public override string ToString() => 
            $"{ProductName} x{Quantity} @{Price:C}";
    }

    [Serializable]
    [XmlRoot("Order")]
    public class Order
    {
        [XmlAttribute("ID")]
        public string OrderId { get; set; }
        
        [XmlElement("Customer")]
        public string Customer { get; set; }
        
        [XmlArray("Items")]
        [XmlArrayItem("Item")]
        public List<OrderDetails> Details { get; set; } = new List<OrderDetails>();

        [XmlIgnore]
        public decimal TotalAmount => Details.Sum(d => d.Price * d.Quantity);

        public override bool Equals(object obj) => 
            obj is Order order && OrderId == order.OrderId;

        public override string ToString() => 
            $"Order {OrderId}\nCustomer: {Customer}\nTotal: {TotalAmount:C}\n" +
            string.Join("\n", Details);
    }

    public class OrderService
    {
        private readonly List<Order> _orders = new List<Order>();

        public void AddOrder(Order order)
        {
            if (_orders.Any(o => o.OrderId == order.OrderId))
                throw new ArgumentException("订单已存在");
            _orders.Add(order);
        }

        public void RemoveOrder(string orderId)
        {
            var order = _orders.FirstOrDefault(o => o.OrderId == orderId) ?? 
                throw new KeyNotFoundException("订单不存在");
            _orders.Remove(order);
        }

        public IEnumerable<Order> QueryOrders(Func<Order, bool> predicate) => 
            _orders.Where(predicate).OrderByDescending(o => o.TotalAmount);

        public void Export(string filePath)
        {
            var serializer = new XmlSerializer(typeof(List<Order>));
            using var writer = new StreamWriter(filePath);
            serializer.Serialize(writer, _orders);
        }

        public void Import(string filePath)
        {
            var serializer = new XmlSerializer(typeof(List<Order>));
            using var reader = new StreamReader(filePath);
            if (serializer.Deserialize(reader) is List<Order> imported)
            {
                _orders.Clear();
                _orders.AddRange(imported);
            }
        }
    }

    // 测试类与主程序共存
    public static class OrderServiceTests
    {
        public static void RunAllTests()
        {
            TestAddOrder();
            TestRemoveOrder();
            TestImportExport();
            Console.WriteLine("所有测试通过！");
        }

        private static void TestAddOrder()
        {
            var service = new OrderService();
            service.AddOrder(new Order { OrderId = "TEST001" });
            Assert(service.QueryOrders(o => true).Count() == 1, "添加订单测试失败");
        }

        private static void TestRemoveOrder()
        {
            var service = new OrderService();
            service.AddOrder(new Order { OrderId = "TEST002" });
            service.RemoveOrder("TEST002");
            Assert(service.QueryOrders(o => true).Count() == 0, "删除订单测试失败");
        }

        private static void TestImportExport()
        {
            var service = new OrderService();
            service.AddOrder(new Order { OrderId = "TEST003" });
            
            var tempFile = Path.GetTempFileName();
            service.Export(tempFile);
            
            var newService = new OrderService();
            newService.Import(tempFile);
            
            Assert(newService.QueryOrders(o => true).Count() == 1, "导入导出测试失败");
            File.Delete(tempFile);
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // 运行测试
            try
            {
                OrderServiceTests.RunAllTests();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试失败: {ex.Message}");
                return;
            }

            // 示例使用
            var service = new OrderService();
            service.AddOrder(new Order 
            {
                OrderId = "20231001",
                Customer = "张三",
                Details = new List<OrderDetails>
                {
                    new OrderDetails { ProductName = "手机", Price = 2999m, Quantity = 1 }
                }
            });
            
            service.Export("orders.xml");
            Console.WriteLine("订单已导出到 orders.xml");
        }
    }
}
