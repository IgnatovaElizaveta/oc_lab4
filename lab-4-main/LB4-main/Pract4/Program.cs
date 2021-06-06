using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

namespace Pract4
{
    class Program
    {
        /// <summary>
        /// </summary>
        static readonly int timeSlotDefault = 800;
        /// <summary>
        /// </summary>
        static List<ProcessPlan> processes = new List<ProcessPlan>();
        /// <summary>
        /// </summary>
        static Thread threadControl;
        /// <summary>
        /// </summary>
        static Thread threadTimeSlot;

        /// <summary>
        /// </summary>
        /// <param name="milliseconds"></param>
        static void GoTimer(int milliseconds)
        {
            var timer = Stopwatch.StartNew();
            do
            {
            } while (timer.ElapsedMilliseconds != milliseconds);
            timer.Stop();
        }

        /// <summary>
   
        /// </summary>
        /// <param name="obj"></param>
        static void WaitUntilProcessEnd(object obj)
        {
            ProcessPlan process = (ProcessPlan)obj;
            process.MyProcess.WaitForExit();
            processes.Remove(process);
            Console.WriteLine("Процесс " + Thread.CurrentThread.Name + " закончил работу");
        }

        /// <summary>
 
        /// </summary>
        static void PrintTimeSlots()
        {
            Console.WriteLine("Кванты для потоков");
            foreach (var item in processes)
                Console.WriteLine("Поток " + item.Thread.Name + ": " + item.TimeSlot);
        }


        /// <summary>

        /// </summary>
        static void ChangeTimeSlot()
        {
            // Событие
            while (processes.Count > 0)
            {
                Console.WriteLine("Введите номер потока, для которого хотите увеличить квант времени на 200мс.");
                int iProccess = int.Parse(Console.ReadLine());
                bool isChange = false;
                try
                {
                    foreach (var item in processes)
                    {
                        if (item.Thread.Name == iProccess.ToString())
                        {
                            item.TimeSlot += 200;
                            isChange = true;
                            break;
                        }
                    }
                }
                catch
                {
                    isChange = false;
                }
                if (isChange == false)
                {
                    Console.WriteLine("Такой поток не найден. Возможно, он уже завершен.");
                }
                PrintTimeSlots();
            }
        }

        /// <summary>
     
        /// </summary>
        static void ControlProcess()
        {
            for (int i = 0; processes.Count > 0;)
            {
                int nP = processes.Count;
                if (i > processes.Count - 1)
                    i = 0;
                if (processes[i].IsStarted == false)
                {
                    processes[i].MyProcess.Start();
                    processes[i].Thread.Start(processes[i]);
                    processes[i].IsStarted = true;
                }
                else
                {
                    processes[i].Resume();
                }
                Console.WriteLine("Запущен " + (i + 1) + " процесс");
                GoTimer(processes[i].TimeSlot);
                if (nP == processes.Count)
                {
                    processes[i].Suspend();
                    Console.WriteLine("Приостановлен " + (i + 1) + " процесс");
                    if (processes.Count != 0 && i == processes.Count - 1)
                        i = 0;
                    else
                        i++;
                }
            }
          
            threadTimeSlot.Abort();
            Console.WriteLine("Все процессы заверешены");
        }

        static void Main(string[] args)
        {
            for (int i = 0; i < 3; i++)
            {
                string line = "ConsoleApp" + (i + 1);
                line = "..\\..\\..\\" + line + "\\bin\\Debug\\" + line;
                ProcessPlan processPlan = new ProcessPlan(line);
                processPlan.TimeSlot = timeSlotDefault;
                processPlan.IsStarted = false;
                processPlan.Thread = new Thread(new ParameterizedThreadStart(WaitUntilProcessEnd));
                processPlan.Thread.Name = (i + 1).ToString();
                processes.Add(processPlan);
            }
            PrintTimeSlots();

          
            threadControl = new Thread(new ThreadStart(ControlProcess));
            threadControl.Start();

            threadTimeSlot = new Thread(new ThreadStart(ChangeTimeSlot));
            threadTimeSlot.Start();
        }
    }
}
