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

            //var redis = new RedisCacheService("192.168.1.188:6379,password=12345678", 2);
            //var redis = new RedisCacheService("47.105.33.30:8900,password=12345678", 2);
            var redis = new RedisCacheService("127.0.0.1:7000");

            var a = redis.Get<int>("a");
            var b = a;
            //var b= redis.Add("a", a+1);
            var res = redis.Get("a");

            Console.WriteLine("{0} {1}", b, JsonConvert.SerializeObject(res));
            while (true)
            {
                Console.Read();
            }
        }
    }
}
