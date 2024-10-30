using System;
using System.Collections.Generic;
using System.Management;
using System.Threading;

namespace USB_Device_Monitor
{
    internal class Program
    {
        /// <summary>
        /// Monitors USB devices for anything plugged in or unplugged.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("USB device change tracker v1.0.3 http://nordquist.cc");
            Console.WriteLine("Copyright 2024 Jacob Nordquist\r\n\r\nPermission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:\r\n\r\nThe above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.\r\n\r\nTHE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.");
            var usbDevices = GetUSBDevices();
            List<USBDeviceInfo> lastUsbDevices = GetUSBDevices();
            char[] loadWheel = new char[] { '-', '\\', '|', '/' };
            int loadIndex = 0;

            int cursorPos = 2;

            while (true)
            {
                lastUsbDevices = usbDevices;
                usbDevices = GetUSBDevices();

                var pluggedIn = new List<USBDeviceInfo>();

                foreach (var d in usbDevices)
                {
                    bool found = false;
                    foreach(var last in lastUsbDevices)
                    {
                        if (d.DeviceID == last.DeviceID)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        pluggedIn.Add(d);
                    }
                }

                var unplugged = new List<USBDeviceInfo>();
                foreach (var d in lastUsbDevices)
                {
                    bool found = false;
                    foreach (var last in usbDevices)
                    {
                        if (d.DeviceID == last.DeviceID)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        unplugged.Add(d);
                    }
                }

                Console.ForegroundColor = ConsoleColor.Red;
                foreach (var usbDevice in unplugged)
                {
                    Console.WriteLine("Removed Device:" + consolePrintout(usbDevice));
                }
                Console.ForegroundColor = ConsoleColor.Green;
                foreach (var usbDevice in pluggedIn)
                {
                    Console.WriteLine($"New Device:" + consolePrintout(usbDevice));
                }
                Thread.Sleep(100);
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                if (cursorPos == Console.CursorTop)
                {
                    Console.SetCursorPosition(21, Console.CursorTop-1);
                }
                else
                {
                    Console.Write("Waiting for change...");
                    
                    cursorPos = Console.CursorTop+1;
                }
                Console.WriteLine(loadWheel[loadIndex]);
                loadIndex++;
                if (loadIndex + 1 > loadWheel.Length)
                {
                    loadIndex = 0;
                }
            }
        }
        static string consolePrintout(USBDeviceInfo device)
        {
            return $"\n\tDevice ID:\t{device.DeviceID}" +
                    BuildHWID(device.HardwareID) +
                    $"\n\tManufacturer:\t{device.Name}" +
                    $"\n\tName:\t\t{device.Name}" +
                    $"\n\tDescription:\t{device.Description}";
                   
        }
        static string BuildHWID(string[] id)
        {
            if(id != null && id.Length > 0)
            {
                string returnString = $"\n\tHWID:\t\t{id[0]}";
                for (int i = 1; i < id.Length; i++)
                {
                    returnString += "\n\t\t\t" + id[i];
                }
                return returnString;
            }
            return "\n\tHWID: NULL";
        }

        static List<USBDeviceInfo> GetUSBDevices()
        {
            List<USBDeviceInfo> devices = new List<USBDeviceInfo>();

            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_PnPEntity"))
                collection = searcher.Get();

            foreach (var device in collection)
            {
                devices.Add(new USBDeviceInfo(
                    (string)device.GetPropertyValue("DeviceID"),
                    (string[])device.GetPropertyValue("HardwareID"),
                    (string)device.GetPropertyValue("Manufacturer"),
                    (string)device.GetPropertyValue("Name"),
                    (string)device.GetPropertyValue("Description")
                ));
            }

            collection.Dispose();
            return devices;
        }
    }

    class USBDeviceInfo
    {
        public USBDeviceInfo(string deviceID, string[] hardwareID, string Manufacturer, string name, string description)
        {
            this.DeviceID = deviceID;
            this.HardwareID = hardwareID;
            this.Manufacturer = Manufacturer;
            this.Description = description;
            this.Name = name;
            
        }
        public string DeviceID { get; private set; }
        public string[] HardwareID { get; private set; }
        public string Manufacturer { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
    }
}
