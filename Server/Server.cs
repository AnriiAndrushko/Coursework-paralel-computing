﻿using System.Net.Sockets;
using System.Net;
using System.Text;
using InvertedIndexLib;
using InvertedIndexLib.TechClasses;

namespace IndexServer
{
    internal class Server
    {
        private int _backlogCount;
        private readonly HashSet<Client> _clients;
        private readonly string _adress;
        private readonly int _port;
        private readonly Socket _serverSocket;
        private readonly InvertedIndexKeeper _index;
        public event NotifyCompleted TasksCompleted;
        private void OnTasksCompleted()
        {
            TasksCompleted?.Invoke();
        }
        public Server(string adress, int port, int backlogCount = 6, int threadCount = 6, bool useOwnQueue = false)
        {
            _index = new InvertedIndexKeeper(threadCount, useOwnQueue);
            _backlogCount = backlogCount;
            _clients = new HashSet<Client>();
            _adress = adress;
            _port = port;
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var ep = new IPEndPoint(IPAddress.Parse(_adress), _port);
            _serverSocket.Bind(ep);
            _serverSocket.Listen(_backlogCount);
            //Console.WriteLine("Server setup complete");
            _index.TasksCompleted += OnTasksCompleted;
        }

        private void AcceptCallback(IAsyncResult AR)
        {
            Socket socket = null;
            try
            {
                socket = _serverSocket.EndAccept(AR);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            var client = new Client(socket, _index);
            _clients.Add(client);
            socket.BeginReceive(client.Buff, 0, client.BufferSize, SocketFlags.None, ReceiveCallback, client);
            Console.WriteLine("Client connected");
            _serverSocket.BeginAccept(AcceptCallback, null);
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            Client curClient = (Client)AR.AsyncState;
            int amountReceivedBytes;
            try
            {
                amountReceivedBytes = curClient.Socket.EndReceive(AR);
            }
            catch (SocketException)
            {
                Console.WriteLine("Client forcefully disconnected");
                curClient.Socket.Close();
                _clients.Remove(curClient);
                return;
            }
            curClient.RecievedCount = amountReceivedBytes;
            byte[] toSend;
            switch (curClient.CurStatus)
            {
                case Status.pendingCommand:
                    curClient.CurStatus = Status.waitingResult;

                    var res = curClient.Result;
                    if (res == null)
                    {
                        toSend = Encoding.ASCII.GetBytes("No result for this command");
                    }
                    else
                    {
                        StringBuilder sb = new();
                        foreach (var path in res)
                        {
                            sb.AppendLine(path);
                        }
                        if (res.Count() == 0)
                        {
                            sb.AppendLine("Nothing found");
                        }
                        toSend = Encoding.ASCII.GetBytes(sb.ToString());

                    }
                    try
                    {
                        curClient.Socket.Send(toSend);
                    }
                    catch (SocketException)
                    {
                        Console.WriteLine("Client forcefully disconnected");
                        return;
                    }
                    
                    if (curClient.CurStatus == Status.Disconnect)
                    {
                        curClient.Socket.Shutdown(SocketShutdown.Both);
                        curClient.Socket.Close();
                        _clients.Remove(curClient);
                        Console.WriteLine("Client disconnected");
                        return;
                    }
                    curClient.CurStatus = Status.pendingCommand;
                    break;
            }
            curClient.Socket.BeginReceive(curClient.Buff, 0, curClient.BufferSize, SocketFlags.None, ReceiveCallback, curClient);
        }
        public void Stop()
        {
            foreach (var client in _clients)
            {
                client.Socket.Shutdown(SocketShutdown.Both);
                client.Socket.Close();
            }
            _serverSocket.Close();
            //Console.WriteLine("Server closed");
            _index.Dispose();
        }

        public void Start()
        {
            _serverSocket.BeginAccept(AcceptCallback, null);
            Console.WriteLine("Server started");
        }

        public string LocalExecute(string command)
        {
            string cmd = command.Split()[0];
            string param;
            IEnumerable<string> res;
            Command parsedCommand;
            if (!Enum.TryParse(cmd, true, out parsedCommand))
            {
                parsedCommand = Command.Unknown;
            };
            switch (parsedCommand)
            {
                case Command.Save:
                    param = command.Split(' ', 2)[1];
                    _index.Save(param);
                    return cmd + " command was recieved";
                case Command.Load:
                    param = command.Split(' ', 2)[1];
                    _index.Load(param);
                    return cmd + " command was recieved";
                case Command.AddDoc:
                    param = command.Split(' ', 2)[1];
                    _index.AddDoc(param);
                    return cmd + " command was recieved";
                case Command.GetByWord:
                    param = command.Split(' ', 2)[1];
                    res = _index.GetByWord(param);
                    break;
                case Command.GetByQuery:
                    param = command.Split(' ', 2)[1];
                    res = _index.GetByQuery(param);
                    break;
                default:
                    return "Incorrect command was recieved";

            }
            return res.Aggregate(new StringBuilder(), (sb, x) => sb.Append(x).Append(" ")).ToString();
        }
    }

}