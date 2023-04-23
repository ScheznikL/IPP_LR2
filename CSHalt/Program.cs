/*Організувати синхронізацію за допомогою семафорів для наступної задачі. 
Після запуску основної програми користувач задає максимальне число можливих копій цієї програми. 
Після чого, починає процес запуску програм кілька разів. 
Якщо ліміт програм вичерпаний, наступна програма автоматично завершує роботу.*/
using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;

namespace IPP_LR2_CSH
{
    class Program
    {
        private static Semaphore _pool;
        private static List<Process> listOfKilledProceses = new List<Process>();
        private static List<Process> listOfAllProcess = new List<Process>();
        private static int _padding;
        private static int _userAmount;

        static void Main(string[] args)
        {
            //Console.SetWindowPosition(0, 0);
            Console.WriteLine("Enter max amount of copy of this program.");
            try
            {
                _userAmount = Convert.ToInt32(Console.ReadLine());
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            //try                
            //{
            //    Semaphore.OpenExisting("semaphoreLrIPP");
            //}
            //catch(WaitHandleCannotBeOpenedException e)
            //{
            //    Console.WriteLine(e.Message);
            //    _pool= new Semaphore()
            //}
            //Console.WriteLine("Enter number of times that program will be of copy of this program.");
            //try
            //{
            //    _userAmount = Convert.ToInt32(Console.ReadLine());
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //    return;
            //}


            // Create a semaphore that can satisfy up to three
            // concurrent requests. Use an initial count of zero,
            // so that the entire semaphore count is initially
            // owned by the main program thread.
            //
            _pool = new Semaphore(initialCount: 0, maximumCount: _userAmount);
            
            // Create and start five numbered threads. 
            //
            for (int i = 1; i < 11; i++)
            {
                Thread t = new Thread(Worker);
                t.IsBackground = true;
                t.Name = i.ToString();
                // Start the thread, passing the number.
                t.Start(i);
            }

            // Wait for half a second, to allow all the
            // threads to start and to block on the semaphore.
            //
            Thread.Sleep(100 * 11);

            // The main thread starts out holding the entire
            // semaphore count. Calling Release(3) brings the 
            // semaphore count back to its maximum value, and
            // allows the waiting threads to enter the semaphore,
            // up to three at a time.
            //
            Console.WriteLine($"Main thread calls Release({_userAmount}).");
            _pool.Release(releaseCount: _userAmount);

            Console.WriteLine("Main thread exits.\nType s to stop all proceses.");
            if (Console.ReadLine() == "s")
            {
                foreach (var proc in listOfAllProcess.Except(listOfKilledProceses))
                {
                    proc.Kill();
                }
            }
        }
        private static void Worker(object num)
        {
            // Each worker thread begins by requesting the
            // semaphore.
            /*Console.WriteLine("Thread {0} begins " +
                "and waits for the semaphore.", num);*/
            _pool.WaitOne();

            // A padding interval to make the output more orderly.
            int padding = Interlocked.Add(ref _padding, 100);

            Console.WriteLine("Thread {0} enters the semaphore.", num);

            // The thread's "work" consists of sleeping for 
            // about a second. Each thread "works" a little 
            // longer, just to make the output more orderly.

            /*  using (*/
            Process myProcess = new Process();/*)*/
                                              // {
                                              // myProcess.StartInfo.UseShellExecute = false;
                                              // You can start any process, HelloWorld is a do-nothing example.
            myProcess.StartInfo.FileName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
            //myProcess.PriorityClass = ProcessPriorityClass.Idle;
            // myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            listOfAllProcess.Add(myProcess);
            //  myProcess.StartInfo.CreateNoWindow = false;
            //myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            if (!myProcess.Start())
            {
                Console.WriteLine($"Thread {num} wasn't starteds.", num);
            }
            // This code assumes the process you are starting will terminate itself.
            // Given that it is started without a window so you cannot terminate it
            // on the desktop, it must terminate itself or you can do it programmatically
            // from this application using the Kill method.

            Thread.Sleep(1000 + padding);
            var previousReleareCount = _pool.Release();
            Console.WriteLine("Thread {0} releases the semaphore.", num);
            Console.WriteLine("Thread {0} previous semaphore count: {1}",
                num, previousReleareCount);
            //previousReleareCount = _pool.Release();
            if (previousReleareCount == 0)
            {
                listOfKilledProceses.Add(myProcess);
                myProcess.Kill();
                // _pool.Release(/*(int)num +*/ 1);
                var th = Thread.CurrentThread;
            }
            //}
        }
    }
}
