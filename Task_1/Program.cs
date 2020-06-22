using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

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
        static string path = @"...\...\OddNumbers.txt";
        static void Main(string[] args)
        {
            new Thread(new ThreadStart(PopulateMatrix)).Start();
            new Thread(new ThreadStart(GenerateRandomNumbers)).Start();
            new Thread(new ThreadStart(WriteOddNumbersToFile)).Start();
            new Thread(new ThreadStart(ReadOddNumbersFromFile)).Start();
            Console.ReadLine();
        }
        public static void PopulateMatrix()
        {
            lock (l)
            {                
                matrix = new int[100, 100];
                Monitor.Wait(l);
                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                        matrix[i, j] = rngQueue.Dequeue();
                }
                Monitor.Pulse(l);
            }          
        }
        public static void GenerateRandomNumbers()
        {
            lock (l)
            {
                for (int i = 0; i < 10000; i++)
                    rngQueue.Enqueue(rnd.Next(10, 99));
                Monitor.Pulse(l);
            }
        }
        public static void WriteOddNumbersToFile()
        {
            lock (l)
            {                
                while (matrix[matrix.GetLength(0)-1, matrix.GetLength(1)-1] == 0)
                    Monitor.Wait(l);              
                List<int> tempList = new List<int>();
                foreach (var num in matrix)
                {
                    if (num % 2 == 1)
                        tempList.Add(num);
                }
                oddNumbers = tempList.ToArray();
                if (File.Exists(path) == false)
                    File.Create(path).Close();
                sw = new StreamWriter(path);
                using (sw)
                {
                    foreach (var num in oddNumbers)
                        sw.WriteLine(num);
                }
                Monitor.Pulse(l);
            }
        }
        public static void ReadOddNumbersFromFile()
        {   
            lock (l)
            {
                while (new FileInfo(path).Length == 0)
                {
                    Monitor.Wait(l);
                }
                sr = new StreamReader(path);
                if (File.Exists(path) == false)
                    File.Create(path).Close();               
                using (sr)
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                        Console.WriteLine(line);
                }
            }
        }
    }
}
