# SimpleTCP
A Script To Simplify and SpeedUp TCP APP Development

Server Hello World Example
class Program
    {
        static SimpleTCP simpleTCP;
        static void Main(string[] args)
        {
            simpleTCP = new SimpleTCP(TCPType.Server);
            simpleTCP.Connected = Connected;
            simpleTCP.Start();
            Console.WriteLine("Press any key to exit");
            Console.ReadKey(true);
        }
        public static void Connected(NetworkStream networkStream, RSACryptoServiceProvider sender, RSACryptoServiceProvider reciever)
        {
            simpleTCP.SendText(networkStream, sender, "Hello World!");
            Console.WriteLine(simpleTCP.ReadText(networkStream,reciever));
        }
    }
