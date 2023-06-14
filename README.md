# SimpleTCP<br>
A Script To Simplify and SpeedUp TCP APP Development<br>
Hello World Example:<br>
Server:<br>

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
    
Client:<br>

        static void Main(string[] args)
        {
            simpleTCP = new SimpleTCP(TCPType.Client);
            simpleTCP.Connected = Connected;
            simpleTCP.ConnectionFailed = ConnectionFailed;
            simpleTCP.Start();
            Console.WriteLine("Press any key to exit");
            Console.ReadKey(true);
        }
        public static void Connected(NetworkStream networkStream, RSACryptoServiceProvider sender, RSACryptoServiceProvider reciever)
        {
            string recived = simpleTCP.ReadText(networkStream,reciever);
            Console.WriteLine(recived);
            simpleTCP.SendText(networkStream, sender,recived);
        }
        public static void ConnectionFailed()
        {
            simpleTCP.Start();
        }

