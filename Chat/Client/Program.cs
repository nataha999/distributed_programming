using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Client;

class Program
{
    public static void StartClient(string host, int port, string message)
    {
        try
        {
            // Разрешение сетевых имён
            IPAddress ipAddress = IPAddress.Loopback;
            //IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            //IPAddress ipAddress = ipHostInfo.AddressList[0];

            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

            // CREATE
            Socket sender = new Socket(
                ipAddress.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            try
            {
                // CONNECT
                sender.Connect(remoteEP);

                Console.WriteLine("Удалённый адрес подключения сокета: {0}",
                    sender.RemoteEndPoint.ToString());

                // SEND
                int bytesSent = sender.Send(Encoding.UTF8.GetBytes(message + "<EOF>"));

                // RECEIVE
                byte[] buf = new byte[1024];
                string data = null;
                while (true)
                {
                    int bytesRec = sender.Receive(buf);

                    data += Encoding.UTF8.GetString(buf, 0, bytesRec);
                    if (data.IndexOf("<EOF>") > -1)
                    {
                        break;
                    }
                }

                data = data.Substring(0, data.Length - "<EOF>".Length);
                Console.Write(data);

                // RELEASE
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    static void Main(string[] args)
    {
        if (args.Length != 3)
        {
            Console.WriteLine("Usage: dotnet run <URL> <PORT> <Message>");
        }
        else if (args[2].Length == 0)
        {
            Console.WriteLine("Message can't be empty");
        }
        else
        {
            StartClient(args[0], Int32.Parse(args[1]), args[2]);
        }
    }
}