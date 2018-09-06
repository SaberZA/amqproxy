using System;
using amqp_sharp_bridge;

namespace amqp_sharp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Usage();
                return;
            }

            var iphe = System.Net.Dns.GetHostEntry(args[2]);

            var b = new BridgeAcceptor(
                int.Parse(args[1]),
                new System.Net.IPEndPoint(iphe.AddressList[0], int.Parse(args[1]))
            );

//            b.TDSMessageReceived += new TDSMessageReceivedDelegate(b_TDSMessageReceived);
//            b.TDSPacketReceived += new TDSPacketReceivedDelegate(b_TDSPacketReceived);
//            b.ConnectionAccepted += new ConnectionAcceptedDelegate(b_ConnectionAccepted);
//            b.ConnectionDisconnected += new ConnectionDisconnectedDelegate(b_ConnectionClosed);

            b.Start();

            Console.WriteLine("Press enter to kill this process...");
            Console.ReadLine();

            b.Stop();
        }
        
        static string FormatDateTime()
        {
            return DateTime.Now.ToString("yyyyMMdd HH:mm:ss.ffffff");

        }

        static void Usage()
        {
            Console.WriteLine(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + " <listen address> <port> <amqp server address>");
        }
    }
}