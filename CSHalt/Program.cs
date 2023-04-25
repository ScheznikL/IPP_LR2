/*Організувати синхронізацію за допомогою семафорів для наступної задачі. 
Після запуску основної програми користувач задає максимальне число можливих копій цієї програми. 
Після чого, починає процес запуску програм кілька разів. 
Якщо ліміт програм вичерпаний, наступна програма автоматично завершує роботу.*/
using System;
using System.Threading;
using System.Diagnostics;
using System.Security.AccessControl;

namespace IPP_LR2_CSH
{
    class Program
    {
        private static int _userAmount;

        static void Main(string[] args)
        {
            //  Alternative.StartThreads();
            #region var 2
            const string semaphoreName = "SemaphoreExample2";
            Semaphore _pool = null;
            bool doesNotExist = false;
            bool unauthorized = false;

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
            if (doesNotExist)
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
            else if (unauthorized)
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
            bool flag = false;
            try
            {
                flag = _pool.WaitOne(150);
                if (!flag)
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
            KillAllProceses();

        }
        static void KillAllProceses()
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
        #endregion
    }
}


