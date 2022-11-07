using System;
using System.IO.Ports;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace SerialHandler
{
    class SerialThread
    {
        // serialport status 
        public int status;
        // serialport address
        public string address;
        // serialport connection boolean
        public bool connectedToSerial;
        // Action method list to imitate a thread
        private static LinkedList<Action> list = new LinkedList<Action>();

        StreamWriter log;
        // Create a new SerialPort object with default settings.

        Thread backgroundThread = new Thread(new ThreadStart(threadHandler));

        //List<byte> bytes = new List<byte>();

        static SerialPort _serialPort; 
        public SerialThread(string address) {
            string strLogText = "Server Thread has started.";

            // Create a writer and open the file:
            // check if the log file exists
            if (!File.Exists("logfile.txt"))
            {
                // Create new file to log
                log = new StreamWriter("logfile.txt");
            }
            else
            {
                // Loading file to write
                log = File.AppendText("logfile.txt");
            }

            // Write to the file:
            // Log the current date
            log.WriteLine(DateTime.Now);
            // Text to log
            log.WriteLine(strLogText);
            log.WriteLine("Connecting to "+ address);
            try {
                status = connect(address);
                connectedToSerial = true;

            } catch (Exception e) 
            {
                connectedToSerial = true;
                status = 100;
                log.WriteLine("Connecting to " + e);
            }
            backgroundThread.Start();
            // Close the stream:
            log.Close();
        }

        private async void DataReceivedHandlerAsync(
                         object sender,
                         SerialDataReceivedEventArgs e)
        {
            try
            {
                int count = _serialPort.BytesToRead;
                //string returnMessage = "";
                byte[] bytePayload = new byte[16];

                if (count == 16)
                {
                    Console.WriteLine("count :{0}", count);
                    for (int i = 0; i < 16; i++)
                    {
                        int intReturnASCII = _serialPort.ReadByte();
                        Console.WriteLine(intReturnASCII);
                        count--;
                        bytePayload[15 - i] = (byte)intReturnASCII;
                    }

                    await VerifyAsync(bytePayload);
                }
                else {
                    Console.WriteLine("Split");
                }
                //Console.WriteLine("Executed " + returnMessage);
            }
            catch (Exception err)
            {

                Console.WriteLine("Failed");
                Console.WriteLine(err);

            }
          

        }
        private async Task VerifyAsync(byte[] bytePayload) {


            foreach (var item in bytePayload)
            {
                Console.WriteLine(item);
            }

            int dec1 = bytePayload[1];

            int dec2 = bytePayload[12];
            int value = dec1 ^ dec2;
            bool isVal;
            if (value == 192 || value == 193)
            {
                isVal = true;
                Console.WriteLine("Working:{0}", value);
            }
            else
            {
                isVal = false;

                Console.WriteLine("NOT Working:{0} {1} {2}", value, dec1, dec2);
            }
            Console.WriteLine(value);
            var requestXor = new
            {
                status = isVal,
            };

            var json = JsonConvert.SerializeObject(requestXor);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var url = "http://localhost:5000/serial";
            using var client = new HttpClient();


            try
            {
                var response = await client.PostAsync(url, data);

                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine(result);

                //bytes.Clear();
            }
            catch (Exception err)
            {

                //bytes.Clear();
                Console.WriteLine("Failed to send a request");
            }
        }
        //Connect to serial port logic
        public int connect(string address)
        {
            this.address = address;
            try
            {

                _serialPort = new SerialPort(address, 115200, Parity.None, 8, StopBits.One);
                _serialPort.Open();
                _serialPort.DiscardOutBuffer();
                _serialPort.DiscardInBuffer();
                _serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandlerAsync);

                connectedToSerial = true;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                connectedToSerial = false;
                throw;
            }
            // Add connection logic here
            if (connectedToSerial)
            {

                return 0;
            }
            else
            {
                return 1;
            }
        }
        //adding a command to the serial command thread

        void OnScan(object sender, SerialDataReceivedEventArgs args)
        {
            SerialPort port = sender as SerialPort;

            string line = port.ReadExisting();
            // etc
        }
        public static string SetPortName(string defaultPortName)
        {
            string portName;

            Console.WriteLine("Available Ports:");
            foreach (string s in SerialPort.GetPortNames())
            {
                Console.WriteLine("   {0}", s);
            }

            Console.WriteLine("Enter COM port value (Default: {0}): ", defaultPortName);
            portName = defaultPortName;

            return portName;
        }
        // Display BaudRate values and prompt user to enter a value.
        public static int SetPortBaudRate(int defaultPortBaudRate)
        {
            string baudRate;

            Console.WriteLine("Baud Rate(default:{0}): ", defaultPortBaudRate);

            baudRate = defaultPortBaudRate.ToString();

            return int.Parse(baudRate);
        }

        // Display PortParity values and prompt user to enter a value.
        public static Parity SetPortParity(Parity defaultPortParity)
        {
            string parity;

             parity = defaultPortParity.ToString();

            return (Parity)Enum.Parse(typeof(Parity), parity, true);
        }
        // Display DataBits values and prompt user to enter a value.
        public static int SetPortDataBits(int defaultPortDataBits)
        {
            string dataBits;

            Console.WriteLine("Enter DataBits value (Default: {0}): ", defaultPortDataBits);
            dataBits = Console.ReadLine();


            dataBits = defaultPortDataBits.ToString();

            return int.Parse(dataBits.ToUpperInvariant());
        }

        public static void threadHandler() {
            while (true)
                if (list.Any())
                {
                    list.First.Value.Invoke();
                    list.RemoveFirst();
                }
        }
        // Display StopBits values and prompt user to enter a value.
        public static StopBits SetPortStopBits(StopBits defaultPortStopBits)
        {
            string stopBits;

            Console.WriteLine("StopBits value (None is not supported and \n" +
             "raises an ArgumentOutOfRangeException. \n (Default: {0}):", defaultPortStopBits.ToString());

            stopBits = defaultPortStopBits.ToString();

            return (StopBits)Enum.Parse(typeof(StopBits), stopBits, true);
        }
        public static Handshake SetPortHandshake(Handshake defaultPortHandshake)
        {
            string handshake;

            Console.WriteLine("Handshake value (Default: {0}):", defaultPortHandshake.ToString());

            handshake = defaultPortHandshake.ToString();

            return (Handshake)Enum.Parse(typeof(Handshake), handshake, true);
        }

        public void serialCommand(string command, int type,int delay) {
            switch (type) {
                case 1:
                    list.AddLast(() => execSerial(command, delay));
                    break;
                case 3:
                    execSerial(command, delay);
                    break;
            }
        }

        public byte[] xorCalculation(string text, string xorVal)
        {
            Console.WriteLine("{0} is the xorVal1", xorVal);
            int dec1 = Convert.ToInt32(text, 16) - 1;
            Console.WriteLine(dec1);
            int dec2 = Convert.ToInt32(xorVal, 16);
            Console.WriteLine("{0} is the xorVal2", dec2);
            int xorResponse = 0;
            if (text.EndsWith("0"))
            {
                xorResponse = dec1 ^ dec2;
                xorResponse = xorResponse - 2;
            }
            else
            {
                xorResponse = dec1 ^ dec2;
            }


            string hexValue = xorResponse.ToString("X");
            Console.WriteLine(hexValue);
            int payload = int.Parse(text) - 1;
            text = payload.ToString();

            if (dec1 >= 16)
                text = "58 59 C2 " + text + " 80 00 00 00 00 00 00 00 00 00 ";
            else
                text = "58 59 C2 0" + text + " 80 00 00 00 00 00 00 00 00 00 ";
            if (xorResponse >= 16) { text = text + hexValue + " 54"; } else { text = text + "0" + hexValue + " 54"; }
            string[] hexValuesSplit = text.Split(' ');
            List<byte> byteList = new List<byte>();
            byte[] byteArray = new byte[16];
            Console.WriteLine(text);
            foreach (string hex in hexValuesSplit)
            {
                // Convert the number expressed in base-16 to an integer.
                byte value = Convert.ToByte(hex, 16);
                // Get the character corresponding to the integral value.
                //  byteArray[counter] = value;
                //counter++;

                byteList.Add(value);
                //  Console.WriteLine("   Converted '{0}' to {1}.{2}", hex, (byte)value);
            }
            byteArray = byteList.ToArray();

            return byteArray;
        }
        //"58 59 C2 01 80 00 00 00 00 00 00 00 00 00 42 54";

        public void testSerialResponse()
        {
            string text = "58 59 C2 11 03 00 00 00 00 00 00 00 00 00 D1 54";
            string[] hexValuesSplit = text.Split(' ');
            List<byte> byteList = new List<byte>();
            byte[] byteArray = new byte[16];
            Console.WriteLine(text);
            foreach (string hex in hexValuesSplit)
            {
                // Convert the number expressed in base-16 to an integer.
                byte val = Convert.ToByte(hex, 16);
                // Get the character corresponding to the integral value.
                //  byteArray[counter] = value;
                //counter++;

                byteList.Add(val);
                //  Console.WriteLine("   Converted '{0}' to {1}.{2}", hex, (byte)value);
            }
            byteArray = byteList.ToArray();
            Console.WriteLine("SerialPort Said :");
            string a = String.Join(" ", byteArray);
            Console.WriteLine("Byte Array is: " + a);

            int dec1 = Convert.ToInt32(byteArray[3].ToString(), 10);

            Console.WriteLine(dec1 + byteArray[3].ToString());

            int dec2 = Convert.ToInt32(byteArray[14].ToString(), 10);
            Console.WriteLine(dec2 + byteArray[14].ToString());
            int value = dec1 ^ dec2;

            Console.WriteLine(value);





        }

        //sending command before sending command
        public void execSerial(string command, int time)
        {
            Console.WriteLine("data :{0}", command);

            byte[] byteToSend = xorCalculation(command, "43");
            string returnMessage = "";
            try
            {
                //_serialPort.Open();
                int count = _serialPort.BytesToRead;
                Console.WriteLine(byteToSend.Length.ToString(), byteToSend);
                _serialPort.Write(byteToSend, 0, byteToSend.Length);

                Thread.Sleep(time);
                Console.WriteLine("Waiting");
                while (count > 0)
                {
                    int intReturnASCII = _serialPort.ReadByte();
                    returnMessage = returnMessage + Convert.ToChar(intReturnASCII);
                    count--;
                    Console.WriteLine("Executed " + returnMessage);
                }
                //_serialPort.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }


            // Thread.Sleep(time);

        }
    }

}

