using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace IndexServer
{
    internal class Server
    {
        private int _backlogCount;
        private readonly ConcurrentDictionary<Client, Client> _clients;
        private readonly string _adress;
        private readonly int _port;
        private readonly Socket _serverSocket;

        public Server(string adress, int port, int backlogCount = 3)
        {
            _backlogCount = backlogCount;
            _clients = new ConcurrentDictionary<Client, Client>();
            _adress = adress;
            _port = port;
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var ep = new IPEndPoint(IPAddress.Parse(_adress), _port);
            _serverSocket.Bind(ep);
            _serverSocket.Listen(_backlogCount);
            Console.WriteLine("Server setup complete");
        }

        private void AcceptCallback(IAsyncResult AR)
        {
            Socket socket;
            socket = _serverSocket.EndAccept(AR);
            var client = new Client(socket);
            do { } while (!_clients.TryAdd(client, client));
            socket.BeginReceive(client.Buff, 0, client.BufferSize, SocketFlags.None, ReceiveCallback, client);
            Console.WriteLine("Client connected");
            _serverSocket.BeginAccept(AcceptCallback, null);
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            Client curClient = (Client)AR.AsyncState;
            int received;

            try
            {
                received = curClient.Socket.EndReceive(AR);
            }
            catch (SocketException)
            {
                Console.WriteLine("Client forcefully disconnected");
                curClient.Socket.Close();
                do { } while (_clients.TryRemove(curClient, out curClient));
                return;
            }
            if (received == 1)
            {
                curClient.Socket.Shutdown(SocketShutdown.Both);
                curClient.Socket.Close();
                do { } while (_clients.TryRemove(curClient, out curClient));
                Console.WriteLine("Client disconnected");
                return;
            }


            byte[] recBuf = new byte[received];
            Array.Copy(curClient.Buff, recBuf, received);
            byte[] toSend;
            switch (curClient.CurStatus)
            {
                case Status.pendingArrSize:
                    curClient.CurStatus = Status.pendingArrData;

                    Console.WriteLine($"Recieved {curClient.n} array size");
                    toSend = Encoding.ASCII.GetBytes($"Recieved length of array: {curClient.n}, enter data of this array");
                    curClient.Socket.Send(toSend);
                    break;
                case Status.pendingArrData:
                    curClient.CurStatus = Status.waitingForCalculatedAnswer;
                    Console.WriteLine("Starting calculation");
                    curClient.StartCalculation();
                    toSend = Encoding.ASCII.GetBytes("Starting calculation");
                    curClient.Socket.Send(toSend);
                    break;
                case Status.waitingForCalculatedAnswer:
                    Console.WriteLine("Attemp to get result");
                    var res = curClient.Result;
                    if (res.Length == 0)
                    {
                        toSend = Encoding.ASCII.GetBytes("Still bisy");
                    }
                    else{
                        StringBuilder sb = new();
                        for (int i =0; i<res.Length; i++)
                        {
                            sb.Append(res[i, 0]).Append(' ');
                        }
                        toSend = Encoding.ASCII.GetBytes($"Your result is: {sb}");
                    }
                    
                    curClient.Socket.Send(toSend);
                    break;
            }
            curClient.Socket.BeginReceive(curClient.Buff, 0, curClient.BufferSize, SocketFlags.None, ReceiveCallback, curClient);
        }
        public void Stop()
        {
            foreach (var client in _clients)
            {
                client.Value.Socket.Shutdown(SocketShutdown.Both);
                client.Value.Socket.Close();
            }
            _serverSocket.Close();
            Console.WriteLine("Server closed");
        }

        public void Start()
        {
            _serverSocket.BeginAccept(AcceptCallback, null);
            Console.WriteLine("Server started");
        }

    }
}
