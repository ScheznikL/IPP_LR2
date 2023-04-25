using System;
using System.Threading;
using System.Diagnostics;
using System.Security.AccessControl;

namespace IPP_LR2_CSH
{
    class Program
    {
        private static int _userAmount;
        private static int _chosenVarianrOfTask = 1;
        private static bool _enterFlag = false;
        static void Main(string[] args)
        {            
            if (_chosenVarianrOfTask == 0)
            {
                Alternative.StartThreads();
            }
            else
            {
                #region var 2
                const string semaphoreName = "SemaphoreExample2";
                Semaphore _pool = null;
                bool doesNotExist = false;
                bool unauthorized = false;

                CheckOnExistingSemaphore(semaphoreName, ref _pool, ref doesNotExist, ref unauthorized);
                if (doesNotExist)
                {
                    CreateSemaphore(semaphoreName, ref _pool);
                }
                else if (unauthorized)
                {
                    AutorizeAndReOpenSemaphore(semaphoreName, ref _pool);
                }
                TryEnterSemaphore(ref _pool);
            }
            #endregion
            KillAllProcesses();
        }

        private static void TryEnterSemaphore(ref Semaphore _pool)
        {
            try
            {
                _enterFlag = _pool.WaitOne(150);
                if (!_enterFlag)
                {
                    Environment.Exit(0);
                }
                Console.WriteLine("Entered the semaphore.");
                Console.WriteLine("Press the Enter key to exit.");

                Console.ReadLine();
                _pool.Release(); 
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("Unauthorized access: {0}", ex.Message);
            }
        }
        private static void AutorizeAndReOpenSemaphore(string semaphoreName, ref Semaphore _pool)
        {
            try
            {
                _pool = Semaphore.OpenExisting(
                    semaphoreName,
                    SemaphoreRights.ReadPermissions
                        | SemaphoreRights.ChangePermissions);

                SemaphoreSecurity semSec = _pool.GetAccessControl();

                string user = Environment.UserDomainName + "\\"
                    + Environment.UserName;

                SemaphoreAccessRule rule = new SemaphoreAccessRule(
                    user,
                    SemaphoreRights.Synchronize | SemaphoreRights.Modify,
                    AccessControlType.Deny);
                semSec.RemoveAccessRule(rule);

                rule = new SemaphoreAccessRule(user,
                     SemaphoreRights.Synchronize | SemaphoreRights.Modify,
                     AccessControlType.Allow);
                semSec.AddAccessRule(rule);

                _pool.SetAccessControl(semSec);

                Console.WriteLine("Updated semaphore security.");

                _pool = Semaphore.OpenExisting(semaphoreName, SemaphoreRights.Synchronize
                 | SemaphoreRights.Modify);
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("Unable to change permissions: {0}", ex.Message);
                return;
            }
        }
        private static void CreateSemaphore(string semaphoreName, ref Semaphore _pool)
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
            bool semaphoreWasCreated = false;
            _pool = new Semaphore(0, _userAmount, semaphoreName,
                out semaphoreWasCreated);
            _pool.Release(_userAmount);
            if (semaphoreWasCreated)
            {
                Console.WriteLine("Created the semaphore.");
            }
            else
            {
                Console.WriteLine("Unable to create the semaphore.");
                return;
            }
        }
        private static void CheckOnExistingSemaphore(string semaphoreName, ref Semaphore _pool, ref bool doesNotExist, ref bool unauthorized)
        {
            try
            {
                _pool = Semaphore.OpenExisting(semaphoreName, SemaphoreRights.Synchronize
                | SemaphoreRights.Modify);
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                Console.WriteLine("Semaphore does not exist.");
                doesNotExist = true;
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("Unauthorized access: {0}", ex.Message);
                unauthorized = true;
            }
        }
        static void KillAllProcesses()
        {
            Console.WriteLine("Type s to stop all proceses.");
            if (Console.ReadLine().ToLower() == "s")
            {
                var processes = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
                foreach (var pr in processes)
                {
                    if (pr.Id != Process.GetCurrentProcess().Id)
                    {
                        pr.Kill();
                    }
                }
            }
        }

    }
}


