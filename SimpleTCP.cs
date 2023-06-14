using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace STCP
{
    class SimpleTCP
    {
        TCPType tcpType;
        string serverIP = "127.0.0.1";
        int portNo = 5000;
        public Action<NetworkStream, RSACryptoServiceProvider, RSACryptoServiceProvider> Connected { get; set; }
        public Action ConnectionFailed { get; set; }
        public SimpleTCP(TCPType tcpType, string serverIP = "127.0.0.1", int portNo = 5000)
        {
            this.serverIP = serverIP;
            this.portNo = portNo;
            this.tcpType = tcpType;
        }
        public void Start()
        {
            if (tcpType == TCPType.Server)
            {
                _ = StartHost();
            }
            if (tcpType == TCPType.Client)
            {
                StartClient();
            }
        }

        private void StartClient()
        {
            try
            {
                TcpClient client = new TcpClient(serverIP, portNo);
                _ = HandleConnectionFromClient(client);
            }
            catch (Exception)
            {
                ConnectionFailed();
            }

        }
        private async Task StartHost()
        {
            await Task.Run(() =>
            {
                IPAddress localAdd = IPAddress.Parse(serverIP);
                TcpListener listener = new TcpListener(localAdd, portNo);
                Console.WriteLine("Listening...");
                listener.Start();

                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    _ = HandleConnectionFromServer(client);
                }
            });
        }
        private async Task HandleConnectionFromServer(TcpClient client)
        {

            await Task.Run(() =>
            {
                NetworkStream nwStream = client.GetStream();
                RSACryptoBundle cryptoBundleReceive = CreateRSACryptoServiceProvider();

                SendText(nwStream, cryptoBundleReceive.pubKeyString);
                string sendPublicKey = ReadText(nwStream);
                var cryptoSender = CreateRSACryptoServiceProviderFromString(sendPublicKey);
                Connected(nwStream, cryptoSender, cryptoBundleReceive.provider);
            });
        }
        private async Task HandleConnectionFromClient(TcpClient client)
        {
            await Task.Run(() =>
            {
                NetworkStream nwStream = client.GetStream();
                RSACryptoBundle cryptoBundleReceive = CreateRSACryptoServiceProvider();

                string sendPublicKey = ReadText(nwStream);
                var cryptoSender = CreateRSACryptoServiceProviderFromString(sendPublicKey);

                SendText(nwStream, cryptoBundleReceive.pubKeyString);


                Console.WriteLine("Connected");
                Connected(nwStream, cryptoSender, cryptoBundleReceive.provider);
            });
        }
        private RSACryptoBundle CreateRSACryptoServiceProvider()
        {
            //lets take a new CSP with a new 2048 bit rsa key pair
            var csp = new RSACryptoServiceProvider(2048);

            //how to get the private key
            //var privKey = csp.ExportParameters(true);

            //and the public key ...
            var pubKey = csp.ExportParameters(false);

            //converting the public key into a string representation
            string pubKeyString;
            {
                //we need some buffer
                var sw = new System.IO.StringWriter();
                //we need a serializer
                var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                //serialize the key into the stream
                xs.Serialize(sw, pubKey);
                //get the string from the stream
                pubKeyString = sw.ToString();
            }
            return new RSACryptoBundle() { provider = csp, pubKeyString = pubKeyString };
        }
        private RSACryptoServiceProvider CreateRSACryptoServiceProviderFromString(string pubKeyString)
        {
            //converting it back

            //get a stream from the string
            var sr = new System.IO.StringReader(pubKeyString);
            //we need a deserializer
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            //get the object back from the stream
            var pubKey = (RSAParameters)xs.Deserialize(sr);


            //conversion for the private key is no black magic either ... omitted

            //we have a public key ... let's get a new csp and load that key
            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters(pubKey);
            return csp;
        }
        private void SendText(NetworkStream networkStream, string text)
        {
            Send(networkStream, Encoding.UTF8.GetBytes(text));
        }
        public void SendText(NetworkStream networkStream, RSACryptoServiceProvider sender, string text)
        {
            Send(networkStream, sender, Encoding.UTF8.GetBytes(text));
        }
        private void Send(NetworkStream networkStream, byte[] buffer, int offset = 0, int size = 0)
        {
            if (size == 0)
            {
                size = buffer.Length;
            }
            networkStream.Write(buffer, offset, size);
        }
        public void Send(NetworkStream networkStream, RSACryptoServiceProvider sender, byte[] buffer, int offset = 0, int size = 0)
        {
            if (size == 0)
            {
                size = buffer.Length;
            }
            var bytesCypherText = sender.Encrypt(buffer.Skip(offset).Take(size).ToArray(), false);
            networkStream.Write(bytesCypherText, 0, bytesCypherText.Length);
        }
        private string ReadText(NetworkStream networkStream)
        {
            byte[] buffer = Read(networkStream);
            return Encoding.UTF8.GetString(buffer);
        }
        public string ReadText(NetworkStream networkStream, RSACryptoServiceProvider receiver)
        {
            byte[] buffer = Read(networkStream, receiver);
            return Encoding.UTF8.GetString(buffer);
        }
        private byte[] Read(NetworkStream networkStream)
        {
            bool loop = true;
            int i = 0, recivedLength = 0, endLength = 0;
            List<byte> endBuffer = new List<byte>();
            byte[] buffer = new byte[65535];
            while (loop)
            {
                recivedLength = networkStream.Read(buffer, i * buffer.Length, buffer.Length);
                if (recivedLength != buffer.Length)
                    loop = false;
                endBuffer.AddRange(buffer);
                endLength += recivedLength;
                i++;
            }
            return endBuffer.Take(endLength).ToArray();
        }
        public byte[] Read(NetworkStream networkStream, RSACryptoServiceProvider receiver)
        {
            bool loop = true;
            int i = 0, recivedLength = 0, endLength = 0;
            List<byte> endBuffer = new List<byte>();
            byte[] buffer = new byte[65535];
            while (loop)
            {
                recivedLength = networkStream.Read(buffer, i * buffer.Length, buffer.Length);
                if (recivedLength != buffer.Length)
                    loop = false;
                endBuffer.AddRange(buffer);
                endLength += recivedLength;
                i++;
            }
            return receiver.Decrypt(endBuffer.Take(endLength).ToArray(), false);
        }
    }
    enum TCPType
    {
        Server,
        Client
    }
    public class RSACryptoBundle
    {
        public RSACryptoServiceProvider provider;
        public string pubKeyString;
    }
}
