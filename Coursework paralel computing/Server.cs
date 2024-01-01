using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using System.Text;
using InvertedIndexLib;

namespace IndexServer
{
    internal class Server
    {
        private int _backlogCount;
        private readonly ConcurrentDictionary<Client, Client> _clients;
        private readonly string _adress;
        private readonly int _port;
        private readonly Socket _serverSocket;
        private readonly InvertedIndexKeeper _index;

        public Server(string adress, int port, InvertedIndexKeeper index, int backlogCount = 3)
        {
            _index = index;
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
            var client = new Client(socket, _index);
            do { } while (!_clients.TryAdd(client, client));
            socket.BeginReceive(client.Buff, 0, client.BufferSize, SocketFlags.None, ReceiveCallback, client);
            Console.WriteLine("Client connected");
            _serverSocket.BeginAccept(AcceptCallback, null);
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            Client curClient = (Client)AR.AsyncState;
            int amountReceivedBytes;

            if (curClient.CurStatus==Status.Disconnect)
            {
                curClient.Socket.Shutdown(SocketShutdown.Both);
                curClient.Socket.Close();
                do { } while (_clients.TryRemove(curClient, out curClient));
                Console.WriteLine("Client disconnected");
                return;
            }

            try
            {
                amountReceivedBytes = curClient.Socket.EndReceive(AR);
            }
            catch (SocketException)
            {
                Console.WriteLine("Client forcefully disconnected");
                curClient.Socket.Close();
                do { } while (_clients.TryRemove(curClient, out curClient));
                return;
            }
            curClient.RecievedCount = amountReceivedBytes;
            byte[] toSend;
            switch (curClient.CurStatus)
            {
                case Status.pendingCommand:
                    curClient.CurStatus = Status.waitingResult;
                    toSend = Encoding.ASCII.GetBytes("Started your command");
                    curClient.Socket.Send(toSend);
                    break;
                case Status.waitingResult:
                    Console.WriteLine("Attemp to get result");
                    var res = curClient.Result;
                    if (res == null)
                    {
                        toSend = Encoding.ASCII.GetBytes("No result for this command");
                    }
                    else{
                        StringBuilder sb = new();
                        foreach(var path in res)
                        {
                            sb.Append(path).Append(' ');
                        }
                        toSend = Encoding.ASCII.GetBytes($"Your result is:\n {sb}");
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
