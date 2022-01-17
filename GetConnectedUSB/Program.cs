using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

namespace GetConnectedUSB
{

    public class MessageBoxWrapper
    {
        public bool IsOpen { get; set; }

        // give all arguments you want to have for your MSGBox
        public void Show(string messageBoxText)
        {
            IsOpen = true;
            MessageBox.Show(messageBoxText);
            IsOpen = false;
        }
    }

    class Program
    {
        static ManagementEventWatcher insertWatcher = new ManagementEventWatcher();
        static ManagementEventWatcher removeWatcher = new ManagementEventWatcher();

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        static MessageBoxWrapper input1Wrapper = new MessageBoxWrapper();
        static MessageBoxWrapper input2Wrapper = new MessageBoxWrapper();
        static MessageBoxWrapper printer1Wrapper = new MessageBoxWrapper();
        static MessageBoxWrapper printer2Wrapper = new MessageBoxWrapper();

        private static void Watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            
            ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            Console.WriteLine(instance.Properties["name"].Value);
            /*foreach (var property in instance.Properties)
            {
                Console.WriteLine(property.Name + " = " + property.Value);
            }*/
            if (instance.Properties["Name"].Value.ToString() == "USB Input Device" && !input1Wrapper.IsOpen)
            {
                input1Wrapper.Show("A USB input device was connected!");
                //MessageBox.Show("A USB input device was disconnected! \nCheck your Card Reader to ensure it's connected!");
                //Console.WriteLine("Your MagTek device was plugged in.");
            }

            if (instance.Properties["Name"].Value.ToString() == "USB Printing Support" && !printer1Wrapper.IsOpen)
            {
                printer1Wrapper.Show("A USB printer was connected!");
                //MessageBox.Show("A USB input device was disconnected! \nCheck your Card Reader to ensure it's connected!");
                //Console.WriteLine("Your MagTek device was plugged in.");
            }



        }

        private static void Watcher_RemovedArrived(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            //Console.WriteLine(instance.Properties["Description"].Value);
            if (instance.Properties["Name"].Value.ToString() == "USB Input Device" && !input2Wrapper.IsOpen)
            {
                input2Wrapper.Show("A USB input device was disconnected! \nCheck your Card Reader to ensure it's connected!");
                //MessageBox.Show("A USB input device was disconnected! \nCheck your Card Reader to ensure it's connected!");
                //Console.WriteLine("Your MagTek device was plugged in.");
            }

            if (instance.Properties["Name"].Value.ToString() == "USB Printing Support" && !printer2Wrapper.IsOpen)
            {
                printer2Wrapper.Show("A USB printer was disconnected! \nCheck your Printer to ensure it's connected!");
                //MessageBox.Show("A USB input device was disconnected! \nCheck your Card Reader to ensure it's connected!");
                //Console.WriteLine("Your MagTek device was plugged in.");
            }

        }

        static public void removeWatcherRunner()
        {
            WqlEventQuery removeQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent " +
            "WITHIN 2 "
            + "WHERE TargetInstance ISA 'Win32_PnPEntity'");
            //ManagementEventWatcher removeWatcher = new ManagementEventWatcher(removeQuery);
            removeWatcher.EventArrived += new EventArrivedEventHandler(Watcher_RemovedArrived);
            removeWatcher.Query = removeQuery;
            removeWatcher.Start();

            while (true)
            {
                removeWatcher.WaitForNextEvent();
            }
        }

        static public void insertWatcherRunner()
        {
            //ManagementEventWatcher insertWatcher = new ManagementEventWatcher();
            WqlEventQuery query = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent " +
            "WITHIN 2 "
            + "WHERE TargetInstance ISA 'Win32_PnPEntity'");
            insertWatcher.EventArrived += new EventArrivedEventHandler(Watcher_EventArrived);
            insertWatcher.Query = query;
            insertWatcher.Start();

            while (true)
            {
                insertWatcher.WaitForNextEvent();
            }
        }

        public static void Main()
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);

            Thread t = new Thread(removeWatcherRunner);
            t.Start();

            insertWatcherRunner();

        }

    }
}
