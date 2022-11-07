using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace SerialHandler
{
    class Program
    {
        static SerialThread serialPort;
        public static void ShowRequestData(HttpListenerRequest request)
        {
            if (!request.HasEntityBody)
            {
                Console.WriteLine("There is no request with a body");
                return;
            }
            Stream body = request.InputStream;
            Encoding encoding = request.ContentEncoding;
            StreamReader reader = new StreamReader(body, encoding);
            if (request.ContentType != null)
            {
                Console.WriteLine("Request Content-Type {0}", request.ContentType);
            }
            Console.WriteLine("Request Length {0}", request.ContentLength64);

            Console.WriteLine("Data Start :");
            // Convert the data to a string and display it on the console.
            string s = reader.ReadToEnd();
            Console.WriteLine(s);
            Console.WriteLine("Data Ended :");
            body.Close();
            reader.Close();


            var details = JObject.Parse(s);
            Console.WriteLine(string.Concat("Received ", details["command"], " " + details["type"]));

            switch ( (int)details["type"])
            {
                case 0:
                    Console.WriteLine("Selecting to connect with the serialport");
                    serialPort = new SerialThread((string)details["argument"]);
                    break;
                case 1:
                    Console.WriteLine("Selecting to get a qrcode for the product");
                    //PosAPI handling logic here
                    break;
                case 2:
                    Console.WriteLine("test");
                    serialPort.testSerialResponse();
                    break;
                default:
                    if (serialPort != null)
                    {

                        Console.WriteLine("Selecting to add command to the buffer");
                        serialPort.serialCommand((string)details["command"], (int)details["type"], (int)details["delay"]);
                    }
                    else {
                        Console.WriteLine("No Port connected");
                    
                    }

                    break;
            }
            // If you are finished with the request, it should be closed also.
        }
        static void Main(string[] args)
        {
            HttpListener http = null;
            try {
                http = new HttpListener();
                http.Prefixes.Add("http://localhost:8800/");
                http.Start();
                while (true) {
                    Console.WriteLine("waiting for command");
                    HttpListenerContext context = http.GetContext();
                    string msg = $"{{ \"value\": {10} }}";
                    context.Response.ContentLength64 = Encoding.UTF8.GetByteCount(msg);
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    using (Stream stream = context.Response.OutputStream) {
                        ShowRequestData(context.Request);
                        using (StreamWriter writer = new StreamWriter(stream))
                        {
                            writer.Write(msg); 
                        }
                    }

                }
            }
            catch (WebException e )
            {
                Console.WriteLine(e.Status);
            }
        }
    }
}
