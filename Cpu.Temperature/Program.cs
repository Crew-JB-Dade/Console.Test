//// See https://aka.ms/new-console-template for more information
//using System.Diagnostics;
//using System.Management;

//Console.WriteLine("Hello, World!");
//////Debug.WriteLine("Hello, World!");


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using System.Diagnostics;
using PlaLibrary;

namespace ConsoleApplication1
{
    class Program
    {

        protected static PerformanceCounter CPUCounter;
        protected static PerformanceCounter memoryCounter;


        static void Main(string[] args)
        {
            // initialize objects
            CPUCounter = new PerformanceCounter();
            memoryCounter = new PerformanceCounter();

            // assign parameters
            CPUCounter.CategoryName = "Processor";
            CPUCounter.CounterName = "% Processor Time";
            CPUCounter.InstanceName = "_Total";

            memoryCounter.CategoryName = "Memory";
            memoryCounter.CounterName = "Available MBytes";

            while (true)
            {
                // invoke the following monitoring methods
                getCpuUsage();
                getMemory();
                System.Threading.Thread.Sleep(500);
            }
        }

        // prints the value of total processor usage time
        public static void getCpuUsage()
        {
            Console.WriteLine(CPUCounter.NextValue() + "%");
        }

        // prints the value of available memory
        public static void getMemory()
        {
            Console.WriteLine(memoryCounter.NextValue() + "MB");
        }
    }
}