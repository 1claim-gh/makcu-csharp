using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utilities;


namespace Mouse
{
    public enum MouseButton : int
    {
        Left = 1,
        Right = 2,
        Middle = 3,
        mouse4 = 4,
        mouse5 = 5
    }
    class device
    {
        private static byte[] change_cmd = { 0xDE, 0xAD, 0x05, 0x00, 0xA5, 0x00, 0x09, 0x3D, 0x00 };
        public static bool connected = false;
        private static SerialPort port = null;
        private static Thread button_inputs;
        public static string version = "";
        private static bool runReader = false;
        public static Dictionary<int, bool> bState { get; private set; }
        private static HashSet<byte> validBytes = new HashSet<byte>
        {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
            0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15,
            0x16, 0x17, 0x19, 0x1F
        };

        private static Random r = new Random();
        public static void connect(string com)
        {
            if(port == null)
                port = new SerialPort(com, 115200, Parity.None, 8, StopBits.One);
            try
            {
                port.Open();
                if (!port.IsOpen)
                    return;

                Thread.Sleep(150);
                port.Write(change_cmd, 0, change_cmd.Length);
                port.BaseStream.Flush();
                port.BaudRate = 4000000;
                GetVersion();
                Thread.Sleep(150);
                Console.WriteLine($"[+] Device connected to {port.PortName} at {port.BaudRate} baudrate");
                port.Write("km.buttons(1)\r\n");
                port.Write("km.echo(0)\r\n");
                port.DiscardInBuffer();
                start_listening();
                
                bState = new Dictionary<int, bool>();
                for (int i = 1; i <= 5; i++)
                    bState[i] = false;
                connected = true;
            }
            catch (Exception ex)
            {
                connected = false;
                Console.WriteLine($"[-] Device failed to connect. {ex.ToString()}");
            }
        }

        public static void disconnect()
        {
            if(!connected)
                return;

            Console.WriteLine("[!] Closing port...");
            runReader = false;
            port.Write("km.buttons(0)\r\n");
            Thread.Sleep(10);//Allow time for command to be sent
            port.BaseStream.Flush();
            port.Close();
            if (!port.IsOpen)
                Console.WriteLine("[!] Port terminated successfully");
        }

        public static async void reconnect_device(string com)
        {
            disconnect();
            await Task.Delay(200);
            if(!port.IsOpen)
                port.Open();
            Console.WriteLine("[+] Reconnected to device.");
        }
        
        public static void GetVersion()
        {
            port.Write("km.version()\r");
            Thread.Sleep(100);
            version = port.ReadLine();
        }

        public static void move(int x, int y)
        {
            if (!connected)
                return;

            port.Write($"km.move({x}, {y})\r");
            port.BaseStream.FlushAsync();
        }

        public static void move_smooth(int x, int y, int segments)
        {
            if (!connected)
                return;

            port.Write($"km.move({x}, {y}, {segments})\r");
            port.BaseStream.FlushAsync();
        }

        public static void move_bezier(int x, int y, int segments, int ctrl_x, int ctrl_y)
        {
            if (!connected)
                return;

            port.Write($"km.move({x}, {y}, {segments}, {ctrl_x}, {ctrl_y})\r");
            port.BaseStream.FlushAsync();
        }

        public static void mouse_wheel(int delta)
        {
            if (!connected)
                return;

            port.Write($"km.wheel({delta})\r");
            port.BaseStream.FlushAsync();
        }

        public static void lock_axis(string axis, int bit)
        {
            if (!connected)
                return;

            port.Write($"km.lock_m{axis}({bit})\r");
            port.BaseStream.FlushAsync();
        }

        public static void click(string button, int ms_delay, int click_delay = 0)
        {
            if (!connected)
                return;

            int time = r.Next(10, 100); //use this to randomize press time
            Thread.Sleep(click_delay);
            port.Write($"km.{button}(1)\r");
            Thread.Sleep(time);
            port.Write($"km.{button}(0)\r");
            port.BaseStream.FlushAsync();
            Thread.Sleep(ms_delay);
        }

        public static void press(MouseButton button, int press)
        {
            if(!connected)
                return;

            string cmd = $"km.{MouseButtonToString(button)}({press})\r";
            port.Write(cmd);
            port.BaseStream.FlushAsync();
        }
        public static void start_listening()
        {
            Thread.Sleep(500); //Allow time for cleanup
            runReader = true;
            button_inputs = new Thread(read_buttons);
            button_inputs.IsBackground = true;
            button_inputs.Start();
        }

        public static async void read_buttons()
        {
            await Task.Run(() =>
            {
                Console.WriteLine("[+] Listening to device.");
                while (runReader)
                {
                    if (!connected)
                    {
                        Thread.Sleep(1000);
                        connected = port.IsOpen;
                        continue;
                    }
                    try
                    {
                        if (port.BytesToRead > 0)
                        {
                            int data = port.ReadByte();
                            if (!validBytes.Contains((byte)data))
                                continue;

                            byte b = (byte)data;

                            for (int i = 1; i < 6; i++)
                                bState[i] = (b & 1 << i - 1) != 0;

                            port.DiscardInBuffer();
                        }
                    }
                    catch (Exception ex)
                    {
                        connected = false;
                    }
                }
                
            });
        }

        public static bool button_pressed(MouseButton button)
        {
            if (!connected)
                return false;

            return bState[(int)button];
        }

        public static async void lock_button(MouseButton button, int bit)
        {
            if (!connected)
                return;

            string cmd = "";
            await Task.Delay(1);
            switch(button)
            {
                case MouseButton.Left:
                    cmd = $"km.lock_ml({bit})\r";
                    break;
                case MouseButton.Right:
                    cmd = $"km.lock_mr({bit})\r";
                    break;
                case MouseButton.Middle:
                    cmd = $"km.lock_mm({bit})\r";
                    break;
                case MouseButton.mouse4:
                    cmd = $"km.lock_ms1({bit})\r";
                    break;
                case MouseButton.mouse5:
                    cmd = $"km.lock_ms2({bit})\r";
                    break;
            }
            port.Write(cmd);
            await port.BaseStream.FlushAsync();
        }

        public static int MouseButtonToInt(MouseButton button)
        {
            return (int)button;
        }

        public static MouseButton IntToMouseButton(int button)
        {
            return (MouseButton)button;
        }

        public static string MouseButtonToString(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return "left";
                case MouseButton.Right:
                    return "right";
                case MouseButton.Middle:
                    return "middle";
                case MouseButton.mouse4:
                    return "ms1";
                case MouseButton.mouse5:
                    return "ms2";
            }
            return "left";
        }

        public static void setMouseSerial(string serial)
        {
            if (!connected)
                return;

            port.Write($"km.serial({serial})\r");
        }

        public static void resetMouseSerial()
        {
            if (!connected)
                return;

            port.Write("km.serial(0)\r");
        }

        public static void unlock_all_buttons()
        {
            if(port.IsOpen)
            {
                port.Write($"km.lock_ml(0)\r");
                port.Write($"km.lock_mr(0)\r");
                port.Write($"km.lock_mm(0)\r");
                port.Write($"km.lock_ms1(0)\r");
                port.Write($"km.lock_ms2(0)\r");
            }
        }
    }
}


