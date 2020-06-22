using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Task_1
{
    class Program
    {
        static StreamWriter sw;
        public static int[,] matrix;
        public static int[] oddNumbers;
        static readonly Random rnd = new Random();
        readonly static object l = new object();
        static Queue<int> rngQueue = new Queue<int>();
        static readonly string path = ".../.../OddNumbers.txt";
        static void Main(string[] args)
        {
            new Thread(new ThreadStart(MethodOneTask)).Start();
            new Thread(new ThreadStart(MethodTwoTask)).Start();
            Console.ReadLine();
        }
        public static void MethodOneTask()
        {
            lock (l)
            {                
                matrix = new int[100, 100];
                Console.WriteLine("Method 1 generated matrix and waiting");
                Monitor.Wait(l);
                Console.WriteLine("Method 1 resuming");
                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        matrix[i, j] = rngQueue.Dequeue();
                    }
                }
                Console.WriteLine("Method 1 done");
            }          
        }
        public static void MethodTwoTask()
        {
            Console.WriteLine("method 2 cant enter");
            lock (l)
            {
                Console.WriteLine("method 2  entered");
                for (int i = 0; i < 10000; i++)
                {
                    rngQueue.Enqueue(rnd.Next(10, 99));
                }
                Console.WriteLine("method 2 done");
                Monitor.Pulse(l);
            }
        }
        public static void MethodThreeTask()
        {
            lock (l)
            {
                List<int> tempList = new List<int>();
                foreach (var num in matrix)
                {
                    if (num % 2 == 1)
                    {
                        tempList.Add(num);
                    }
                }
                oddNumbers = tempList.ToArray();
                if (!File.Exists(path))
                    File.Create(path).Close();
                sw = new StreamWriter(path, append: true);
                using (sw)
                {
                    foreach (var num in oddNumbers)
                    {
                        sw.WriteLine();
                    }
                }
                Monitor.Pulse(l);
            }            
        }
    }
}
