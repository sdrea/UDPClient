using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace UDPClient
{
    class UDPClient
    {
        public static string name = "";
        public static bool connected = false;

        static void OnUdpData(IAsyncResult result)
        {
            UdpClient socket = result.AsyncState as UdpClient;
            IPEndPoint source = new IPEndPoint(IPAddress.Any, 51600);
            try
            {
                byte[] message = socket.EndReceive(result, ref source);
                Console.WriteLine(Encoding.Default.GetString(message));
            }
            catch
            {

            }

            socket.BeginReceive(new AsyncCallback(OnUdpData), socket);
        }

        enum UDPCommands //4 bytes
        {
            CONNECT,
            HEARTBEAT,
            MESSAGE
        }

        static void Main(string[] args)
        {
            byte[] message = null;
            IPEndPoint target = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 51600);

            while (true)
            {
                Console.Write("Enter your name (maximum length = 8): ");
                name = Console.ReadLine();
                if (name.Length <= 8) break;
                Console.Write("Name was too long!!!!");
            }

            byte[] UDPName = Encoding.Default.GetBytes(name.PadRight(8));
            byte[] UDPCommand = BitConverter.GetBytes((int)UDPCommands.CONNECT);
            byte[] ConnectMessage = new byte[UDPName.Length + UDPCommand.Length];
            Buffer.BlockCopy(UDPName, 0, ConnectMessage, 0, 8);
            Buffer.BlockCopy(UDPCommand, 0, ConnectMessage, 8, 4);

            UdpClient socket = new UdpClient(0);
            socket.BeginReceive(new AsyncCallback(OnUdpData), socket);

            socket.Send(ConnectMessage, ConnectMessage.Length, target);

            while (true)
            {
                var input = Console.ReadLine();
                if (input == "exit") break;
                UDPName = Encoding.Default.GetBytes(name.PadRight(8));
                UDPCommand = BitConverter.GetBytes((int)UDPCommands.MESSAGE);
                message = Encoding.Default.GetBytes(input);
                byte[] SendMessage = new byte[UDPName.Length + UDPCommand.Length + message.Length];
                Buffer.BlockCopy(UDPName, 0, SendMessage, 0, 8);
                Buffer.BlockCopy(UDPCommand, 0, SendMessage, 8, 4);
                Buffer.BlockCopy(message, 0, SendMessage, 12, message.Length);
                socket.Send(SendMessage, SendMessage.Length, target);
                Console.SetCursorPosition(0, Console.CursorTop - 1);

            }
        }
    }
}
