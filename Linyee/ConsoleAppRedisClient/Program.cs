using MemoryCacheService;
using Newtonsoft.Json;
using System;

namespace ConsoleAppRedsiClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var redis = new RedisCacheService("192.168.1.188:6379",2);

            var a = redis.Get<int>("a");
            var b= redis.Add("a", a+1);
            var res= redis.Get("a");

            Console.WriteLine("{0} {1}", b, JsonConvert.SerializeObject(res));
            while (true)
            {
                Console.Read();
            }
        }
    }
}
