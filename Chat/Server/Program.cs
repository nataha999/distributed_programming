using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Server;

class Program
{
    private static List<string> _history = new List<string>();
    public static void StartListening(int port)
    {
        // Разрешение сетевых имён

        // Привязываем сокет ко всем интерфейсам на текущей машинe
        IPAddress ipAddress = IPAddress.Any;

        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

        // CREATE
        Socket listener = new Socket(
            ipAddress.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp);

        try
        {
            // BIND
            listener.Bind(localEndPoint);

            // LISTEN
            listener.Listen(10);

            while (true)
            {
                Console.WriteLine("Ожидание соединения клиента...");
                // ACCEPT
                Socket handler = listener.Accept();

                Console.WriteLine("Получение данных...");
                byte[] buf = new byte[1024];
                string data = null;

                while (true)
                {
                    // RECEIVE
                    int bytesRec = handler.Receive(buf);

                    data += Encoding.UTF8.GetString(buf, 0, bytesRec);
                    if (data.IndexOf("<EOF>") > -1)
                    {
                        break;
                    }
                }
                data = data.Substring(0, data.Length - "<EOF>".Length);

                _history.Add(data);

                Console.WriteLine("Полученный текст: {0}", data);

                // Отправляем текст обратно клиенту
                string sendText = null;
                foreach(var i in _history)
                    sendText += i + "\n";

                sendText += "<EOF>";
                byte[] msg = Encoding.UTF8.GetBytes(sendText);

                // SEND
                handler.Send(msg);

                // RELEASE
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    static void Main(string[] args)
    {
        Console.WriteLine("Запуск сервера...");
        StartListening(Int32.Parse(args[0]));

        Console.WriteLine("\nНажмите ENTER чтобы выйти...");
        Console.Read();
    }
}