using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace IPP_LR2_CSH
{
    public static class Alternative
    {
        private static Semaphore _pool = null;
        private static int _padding;
        private static int _userAmount, _userThreadsAmount;
        public static void StartThreads()
        {
            Console.WriteLine("Enter max amount of copy of this program.");
            try
            {
                _userAmount = Convert.ToInt32(Console.ReadLine());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            Console.WriteLine("Enter number of times that program will be of copy of this program.");
            try
            {
                _userThreadsAmount = Convert.ToInt32(Console.ReadLine());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            _pool = new Semaphore(initialCount: 0, maximumCount: _userAmount);

            for (int i = 1; i < _userThreadsAmount + 5; i++)
            {
                Thread t = new Thread(Worker);
                t.IsBackground = true;
                t.Name = i.ToString();
                t.Start(i);
            }
            Thread.Sleep(100 * _userThreadsAmount);
            Console.WriteLine($"Main thread calls Release({_userAmount}).");
            _pool.Release(releaseCount: _userAmount);
        }
        private static void Worker(object num)
        {
            _pool.WaitOne();

            int padding = Interlocked.Add(ref _padding, 100);
            Console.WriteLine("Thread {0} enters the semaphore.", num);
            Process myProcess = new Process();
            myProcess.StartInfo.FileName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
            if (!myProcess.Start())
            {
                Console.WriteLine($"Thread {num} wasn't starteds.", num);
            }
            Thread.Sleep(1000 + padding);
            var previousReleareCount = _pool.Release();
            Console.WriteLine("Thread {0} releases the semaphore.", num);
            Console.WriteLine("Thread {0} previous semaphore count: {1}",
                num, previousReleareCount);
            if (previousReleareCount == 0)
            {
                myProcess.Kill();
                var th = Thread.CurrentThread;
            }
        }
    }
}
