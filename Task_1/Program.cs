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
        static Queue<int> rngQueue = new Queue<int>();
        static string path = @"...\...\OddNumbers.txt";
        static void Main(string[] args)
        {
            //Creating new threads and starting them accordingly
            new Thread(new ThreadStart(PopulateMatrix)).Start();
            new Thread(new ThreadStart(GenerateRandomNumbers)).Start();
            new Thread(new ThreadStart(WriteOddNumbersToFile)).Start();
            new Thread(new ThreadStart(ReadOddNumbersFromFile)).Start();
            Console.ReadLine();
        }
        /// <summary>
        /// Method for first thread
        /// </summary>
        public static void PopulateMatrix()
        {
            //Lock ensures only one thread can have access
            lock (l)
            {   
                //Initializing matrix
                matrix = new int[100, 100];
                //Putting thread to wait until random numbers are generted
                Monitor.Wait(l);
                //After the random numbers are generated we add them to the matrix
                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                        matrix[i, j] = rngQueue.Dequeue();
                }
                //Pulse signals other waiting threads that the lock is released
                Monitor.Pulse(l);
            }          
        }
        /// <summary>
        /// Method for second thread
        /// </summary>
        public static void GenerateRandomNumbers()
        {
            //Lock ensures only one thread can have access
            lock (l)
            {
                //We add random numbers to queue 10000 times
                for (int i = 0; i < 10000; i++)
                    rngQueue.Enqueue(rnd.Next(10, 99));
                //We use pulse to notify other waiting threads that the lock is released
                Monitor.Pulse(l);
            }
        }
        /// <summary>
        /// Method for third thread
        /// </summary>
        public static void WriteOddNumbersToFile()
        {
            //Lock ensures only one thread can have access
            lock (l)
            {                
                //Checking if matrix is fully populated, if it isn't this thread will be forced to wait until it gets fully populated
                while (matrix[matrix.GetLength(0)-1, matrix.GetLength(1)-1] == 0)
                    Monitor.Wait(l);              
                List<int> tempList = new List<int>();
                //Checking numbers in matrix, if they are odd they will be added to temp list
                foreach (var num in matrix)
                {
                    if (num % 2 == 1)
                        tempList.Add(num);
                }
                //Copying numbers from temp list to an array
                oddNumbers = tempList.ToArray();
                //If file doesn't exist, it will be created
                if (File.Exists(path) == false)
                    File.Create(path).Close();
                sw = new StreamWriter(path);
                //Using streamwriter to write odd numbers from array to file
                using (sw)
                {
                    foreach (var num in oddNumbers)
                        sw.WriteLine(num);
                }
                //Pulsing to notify waiting threads that the lock is released
                Monitor.Pulse(l);
            }
        }
        /// <summary>
        /// Method for fourth thread
        /// </summary>
        public static void ReadOddNumbersFromFile()
        {   
            //Lock ensures only one thread can have access
            lock (l)
            {
                //Checking if file is empty, if it is, the thread has to wait
                while (new FileInfo(path).Length == 0)
                    Monitor.Wait(l);
                sr = new StreamReader(path);
                //If file doesn't exist, it's created (to avoid reading from non existant file)
                if (File.Exists(path) == false)
                    File.Create(path).Close();               
                using (sr)
                {
                    string line;
                    //While there are lines to be read, the loop will go on, writing each line to console
                    while ((line = sr.ReadLine()) != null)
                        Console.WriteLine(line);
                }
            }
        }
    }
}
