using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;

namespace TestClientSocket_console
{
    class Program
    {
        static void Main(string[] args)
        {

            TcpClient client;
            NetworkStream stream;
            String message;
            Int32 port = 13000;
            bool fine = false;

            Console.WriteLine("Client di comunicazione 127.0.0.1:13000");
            Console.WriteLine("Premere un tasto per connettersi...");
            Console.ReadLine();

            try
            {
                client = new TcpClient("127.0.0.1", port);
                stream = client.GetStream();


                // Translate the passed message into ASCII and store it as a Byte array.
                while (!fine)
                {
                    Console.Write(">>> ");
                    message = Console.ReadLine();
                    Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                    // Send the message to the connected TcpServer. 

                    stream.Write(data, 0, data.Length);

                    Console.WriteLine("Sent: {0}", message);

                    // Receive the TcpServer.response.

                    // Buffer to store the response bytes.
                    data = new Byte[256];

                    // String to store the response ASCII representation.
                    String responseData = String.Empty;

                    // Read the first batch of the TcpServer response bytes.
                    Int32 bytes = stream.Read(data, 0, data.Length);
                    responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                    Console.WriteLine("Received: {0}", responseData);
                    if (responseData == "QUIT")
                        fine = true;
                }
                // Close everything.
                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }

            Console.WriteLine("\n Press Enter to continue...");
            Console.Read();


        }
    }
}
