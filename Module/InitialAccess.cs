using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Management;
using System.IO;


namespace InitialAccess
{
    public class Class1
    {
        //Create a method that will execute precheck
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);

        static void CheckAPI(bool isDebuggerPresent)
        {
            bool ifDebugger_API = false;
            CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref ifDebugger_API);
            Console.WriteLine(string.Format("Debugger Attached: {0}", ifDebugger_API));
            Console.WriteLine("\n");
            if (ifDebugger_API)
            {
                Console.WriteLine("Debugger Attached, Exiting the process.");
                if (!isDebuggerPresent)
                {
                    Environment.Exit(0);
                }
            }
        }

        //--------------------------------------------------------------------------------
        //Create a method that will execute check tick
        static void CheckTick(bool isDebuggerPresent)
        {
            bool ifDebugger_Tick = false;
            //Set pre_check time to count
            int sleep_time = 100;
            int error_time = 30;
            //call system time
            long tickCountBefore = Environment.TickCount;
            //Sleeping pre_check time
            Console.WriteLine("Sleeping for 100ms to check if there is any debugger attached.");
            Thread.Sleep(sleep_time);
            Console.WriteLine("Wake up from sleep.");
            //call system time again
            long tickCountAfter = Environment.TickCount;
            //get actual sleep time 
            long ActualSleepTime = tickCountAfter - tickCountBefore;
            //Compare actual sleep time with pre_check time
            //if actual sleep time is greater than pre_check time + error time, then there is a debugger attached
            if (ActualSleepTime > sleep_time + error_time) { ifDebugger_Tick = true; };
            Console.WriteLine($"Expected sleep duration: {sleep_time} ms");
            Console.WriteLine($"Actual sleep duration: {ActualSleepTime} ms");
            Console.WriteLine(string.Format("Debugger Attached: {0}", ifDebugger_Tick));
            if (ifDebugger_Tick)
            {
                Console.WriteLine("Debugger Attached, Exiting the process.");
                if (!isDebuggerPresent)
                {
                    Environment.Exit(0);
                }
            }
        }

        //Create a method that will execute check process dll
        [DllImport("psapi.dll")]
        private static extern bool EnumProcessModules(IntPtr hProcess, [Out] IntPtr[] lphModule, uint cb, [MarshalAs(UnmanagedType.U4)] out uint lpcbNeeded);

        [DllImport("psapi.dll", CharSet = CharSet.Auto)]
        private static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In][MarshalAs(UnmanagedType.U4)] int nSize);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        private const uint PROCESS_QUERY_INFORMATION = 0x0400;
        private const uint PROCESS_VM_READ = 0x0010;
        static void CheckDll(bool isDebuggerPresent)
        {
            int processId = Process.GetCurrentProcess().Id;
            IntPtr processHandle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, false, processId);

            if (processHandle == IntPtr.Zero)
            {
                Console.WriteLine("Failed to open process for reading.");
                return;
            }

            List<string> moduleNames = new List<string>();
            IntPtr[] moduleHandles = new IntPtr[1024];
            uint cb = (uint)(IntPtr.Size * moduleHandles.Length);
            uint cbNeeded;

            if (EnumProcessModules(processHandle, moduleHandles, cb, out cbNeeded))
            {
                int moduleCount = (int)(cbNeeded / IntPtr.Size);

                for (int i = 0; i < moduleCount; i++)
                {
                    StringBuilder moduleName = new StringBuilder(1024);
                    GetModuleFileNameEx(processHandle, moduleHandles[i], moduleName, moduleName.Capacity);
                    Console.WriteLine(moduleName.ToString());
                    moduleNames.Add(moduleName.ToString().ToLower());
                }
            }
            else
            {
                Console.WriteLine("Failed to enumerate process modules.");
            }

            CloseHandle(processHandle);

            List<string> analysisDlls = new List<string>
        {
            "dbghelp.dll",
            "ntdll.dll",
            "x64dbg.dll",
            "titanengine.dll",
            "scyllahide.dll",
            "dbgcore.dll", // Used by various debuggers
            "vmguestlib.dll", // VMware tools
            "vboxservice.dll", // VirtualBox
            "mscoree.dll", // .NET runtime
            "clr.dll", // .NET runtime
            // Add more known DLLs here
        };

            bool analysisToolDetected = false;
            foreach (string analysisDll in analysisDlls)
            {
                if (moduleNames.Contains(analysisDll))
                {
                    Console.WriteLine($"Analysis tool detected: {analysisDll}");
                    analysisToolDetected = true;
                    if (analysisToolDetected)
                    {
                        Console.WriteLine("Debugger Attached, Exiting the process.");
                        if (!isDebuggerPresent)
                        {
                            Environment.Exit(0);
                        }

                    }
                }
            }

            if (!analysisToolDetected)
            {
                Console.WriteLine("No analysis tools detected.");
            }
        }

        //--------------------------------------------------------------------------------
        //Create a method that will check vm
        static void CheckForVirtualMachine(bool isDebuggerPresent)
        {
            // Keywords to check for indicating a virtual machine environment
            string[] vmKeywords = { "VMware", "VirtualBox", "Virtual", "Hyper-V", "QEMU", "Parallels" };

            // Check Win32_ComputerSystem
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    //print all the properties of the object
                    foreach (var prop in obj.Properties)
                    {
                        Console.WriteLine(prop.Name + ": " + prop.Value);
                    }
                    string manufacturer = obj["Manufacturer"]?.ToString() ?? string.Empty;
                    string model = obj["Model"]?.ToString() ?? string.Empty;

                    Console.WriteLine($"Manufacturer: {manufacturer}");
                    Console.WriteLine($"Model: {model}");

                    foreach (var keyword in vmKeywords)
                    {
                        if (manufacturer.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
                            model.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            Console.WriteLine($"Detected virtual machine environment: {keyword}");
                            Console.WriteLine("Debugger Attached, Exiting the process.");
                            if (!isDebuggerPresent)
                            {
                                Environment.Exit(0);
                            }
                        }
                    }
                }
            }

            // Check Win32_BIOS
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    //print all the properties of the object
                    foreach (var prop in obj.Properties)
                    {
                        Console.WriteLine(prop.Name + ": " + prop.Value);
                    }

                    string manufacturer = obj["Manufacturer"]?.ToString() ?? string.Empty;
                    string version = obj["Version"]?.ToString() ?? string.Empty;

                    Console.WriteLine($"BIOS Manufacturer: {manufacturer}");
                    Console.WriteLine($"BIOS Version: {version}");

                    foreach (var keyword in vmKeywords)
                    {
                        if (manufacturer.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
                            version.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            Console.WriteLine($"Detected virtual machine environment: {keyword}");
                            Console.WriteLine("Debugger Attached, Exiting the process.");
                            if (!isDebuggerPresent)
                            {
                                Environment.Exit(0);
                            }
                        }
                    }
                }
            }

            // Check Win32_BaseBoard
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    //print all the properties of the object
                    foreach (var prop in obj.Properties)
                    {
                        Console.WriteLine(prop.Name + ": " + prop.Value);
                    }
                    string manufacturer = obj["Manufacturer"]?.ToString() ?? string.Empty;
                    string product = obj["Product"]?.ToString() ?? string.Empty;

                    Console.WriteLine($"BaseBoard Manufacturer: {manufacturer}");
                    Console.WriteLine($"BaseBoard Product: {product}");

                    foreach (var keyword in vmKeywords)
                    {
                        if (manufacturer.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
                            product.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            Console.WriteLine($"Detected virtual machine environment: {keyword}");
                            Console.WriteLine("Debugger Attached, Exiting the process.");
                            if (!isDebuggerPresent)
                            {
                                Environment.Exit(0);
                            }
                            return;
                        }
                    }
                }
            }

            Console.WriteLine("No virtual machine environment detected.");
        }

        //--------------------------------------------------------------------------------
        //Create a method that will check ip
        static void CheckIP()
        {
            string url = "http://ip-api.com/json";
            string response = string.Empty;
            using (var client = new System.Net.WebClient())
            {
                response = client.DownloadString(url);
            }
            Console.WriteLine(response);

        }
        public static void First(string args)
        {
            Console.WriteLine("Initial Access executeing with args:"+args);
            bool isDebuggerPresent = true;
            if (args == "test")
            {
                Console.WriteLine("Debug mode");
            }
            else if (args == "normal")
            {
                isDebuggerPresent = false;
                Console.WriteLine("Normal mode");
            }
            else
            {
                Console.WriteLine("Invalid mode");
                return;
            }
            //Step 1: Check if the process is running in debugged mode
            //Call Windows API CheckRemoteDebuggerPresent to check if the process is running in debugged mode
            //If the process is running in debugged mode, exit the process
            Console.WriteLine("Step 1: Check if the process is running in debugged mode");
            CheckAPI(isDebuggerPresent);


            //Step 2: Run two tick in defferent thread, check there is any debugger attached
            Console.WriteLine("\nStep 2: Run two tick in defferent thread, check there is any debugger attached");
            CheckTick(isDebuggerPresent);


            //Step 3: According to process's appended dll to Check if the process is running in a virtual machine or sandbox
            Console.WriteLine("\nStep 3: Check if the process is running in a virtual machine or sandbox");
            CheckDll(isDebuggerPresent);

            //Step 4: Check system info with WMI if the process is running in vmware
            Console.WriteLine("\nStep 4: Check system info with WMI if the process is running in vmware");
            CheckForVirtualMachine(isDebuggerPresent);

            //Step 5: Request ip-aip website to check it’s host or not
            Console.WriteLine("\nStep 5: Request ip-aip website to check it’s host or not");
            CheckIP();


        }

    }
}
