using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BoillerSerialComm
{
    internal class Example
    {
        private static EventWaitHandle ewh;
        private static long threadCount = 0;

        private static EventWaitHandle clearCount = new EventWaitHandle(false, EventResetMode.AutoReset);

        [MTAThread]
        public static void Main1()
        {
            // Create an AutoReset EventWaitHandle. 
            //
            ewh = new EventWaitHandle(false, EventResetMode.AutoReset);

            // Create and start five numbered threads. Use the 
            // ParameterizedThreadStart delegate, so the thread 
            // number can be passed as an argument to the Start  
            // method. 
            for (int i = 0; i <= 4; i++)
            {
                Thread t = new Thread(
                    new ParameterizedThreadStart(ThreadProc)
                    );
                t.Start(i);
            }

            // Wait until all the threads have started and blocked. 
            // When multiple threads use a 64-bit value on a 32-bit 
            // system, you must access the value through the 
            // Interlocked class to guarantee thread safety. 
            // 
            while (Interlocked.Read(ref threadCount) < 5)
            {
                Thread.Sleep(500);
            }

            // Release one thread each time the user presses ENTER, 
            // until all threads have been released. 
            // 
            while (Interlocked.Read(ref threadCount) > 0)
            {
                Console.WriteLine("Press ENTER to release a waiting thread.");
                Console.ReadLine();

                // SignalAndWait signals the EventWaitHandle, which 
                // releases exactly one thread before resetting,  
                // because it was created with AutoReset mode.  
                // SignalAndWait then blocks on clearCount, to  
                // allow the signaled thread to decrement the count 
                // before looping again. 
                //
                WaitHandle.SignalAndWait(ewh, clearCount);
            }
            Console.WriteLine();

            // Create a ManualReset EventWaitHandle. 
            //
            ewh = new EventWaitHandle(false, EventResetMode.ManualReset);

            // Create and start five more numbered threads. 
            // 
            for (int i = 0; i <= 4; i++)
            {
                Thread t = new Thread(
                    new ParameterizedThreadStart(ThreadProc)
                    );
                t.Start(i);
            }

            // Wait until all the threads have started and blocked. 
            // 
            while (Interlocked.Read(ref threadCount) < 5)
            {
                Thread.Sleep(500);
            }

            // Because the EventWaitHandle was created with 
            // ManualReset mode, signaling it releases all the 
            // waiting threads. 
            //
            Console.WriteLine("Press ENTER to release the waiting threads.");
            Console.ReadLine();
            ewh.Set();


        }

        public static void ThreadProc(object data)
        {
            int index = (int)data;

            Console.WriteLine("Thread {0} blocks.", data);
            // Increment the count of blocked threads.
            Interlocked.Increment(ref threadCount);

            // Wait on the EventWaitHandle.
            ewh.WaitOne();

            Console.WriteLine("Thread {0} exits.", data);
            // Decrement the count of blocked threads.
            Interlocked.Decrement(ref threadCount);

            // After signaling ewh, the main thread blocks on 
            // clearCount until the signaled thread has  
            // decremented the count. Signal it now. 
            //
            clearCount.Set();
        }

    }
}
