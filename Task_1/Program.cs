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
        static StreamReader sr;
        public static int[,] matrix;
        public static int[] oddNumbers;
        static readonly Random rnd = new Random();
        readonly static object l = new object();
        readonly static object l2 = new object();
        static Queue<int> rngQueue = new Queue<int>();
        static string path = ".../.../OddNumbers.txt";
        static void Main(string[] args)
        {
            Thread t1 = new Thread(ThreadOneTask);
            Thread t2 = new Thread(ThreadTwoTask);            
            Thread t3 = new Thread(ThreadThreeTask);
            Thread t4 = new Thread(ThreadFourTask);
            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();
            t3.Start();
            t4.Start();
            Console.ReadLine();
        }
        public static void ThreadOneTask()
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
                Monitor.Pulse(l);
                Console.WriteLine("Method 1 done");
            }          
        }
        public static void ThreadTwoTask()
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
        public static void ThreadThreeTask()
        {
            lock (l2)
            {                
                Console.WriteLine("Thread 3 starting");
                List<int> tempList = new List<int>();
                foreach (var num in matrix)
                {
                    if (num % 2 == 1)
                    {
                        tempList.Add(num);
                    }
                }
                oddNumbers = tempList.ToArray();
                Console.WriteLine("thread 3 odd number array created ");
                if (File.Exists(path) == false)
                    File.Create(path).Close();
                sw = new StreamWriter(path);
                using (sw)
                {
                    foreach (var num in oddNumbers)
                    {
                        sw.WriteLine();
                    }
                }
                Console.WriteLine("thread 3 pulsing");
                Monitor.Pulse(l2);
            }
        }
        public static void ThreadFourTask()
        {
            Console.WriteLine("Thread 4 cannot enter");
            lock (l2)
            {
                Console.WriteLine("Thread 4 entered");
                //Monitor.Wait(l);
                
                if (File.Exists(path) == false)
                    File.Create(path).Close();
                sr = new StreamReader(path);
                using (sr)
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                        Console.WriteLine(line);
                }
                Console.WriteLine("Thread 4 done");
            }
        }
    }
}
