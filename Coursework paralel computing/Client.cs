using InvertedIndexLib;
using System;
using System.Net.Sockets;
using System.Text;

namespace IndexServer
{
    internal class Client
    {
        string _received;
        private byte[] _buffer;
        InvertedIndexKeeper _index;

        public Socket Socket { get; set; }
        public Status CurStatus
        {
            get { return _curStatus; }
            set
            {
                switch (value)
                {
                    case Status.connectionEstablished:
                        _curStatus = Status.connectionEstablished;
                        _buffer = new byte[4096];
                        break;
                    case Status.pendingCommand:
                        _curStatus = Status.pendingCommand;
                        _received = Encoding.ASCII.GetString(_buffer);
                        break;
                    case Status.waitingResult:
                        _curStatus = Status.waitingResult;
                        string command = _received.Split()[0];
                        string param = _received.Split(' ', 2)[1];
                        Command parsedCommand;
                        if (!Enum.TryParse(command, true, out parsedCommand))
                        {
                            parsedCommand = Command.Unknown;
                        };
                        switch (parsedCommand)
                        {
                            case Command.Save:
                                _index.Save(param);
                                break;
                            case Command.Load:
                                _index.Load(param);
                                break;
                            case Command.AddDoc:
                                _index.AddDoc(param);
                                break;
                            case Command.GetByWord:
                                Result = _index.GetByWord(param);
                                break;
                            case Command.GetByQuery:
                                Result = _index.GetByQuery(param);
                                break;
                            default:
                                throw new Exception("Unimplemented command");
                                break;
                        }
                        break;
                }
            }
        }
        public Status _curStatus;

        public byte[] Buff {
            get
            {
                return _buffer;
            }
        }
        public IEnumerable<string> Result;

        public int BufferSize { get
            {
                return _buffer.Length;
            }
        }

        public Client(Socket socket, InvertedIndexKeeper index)
        {
            _index = index;
            Socket = socket;
            CurStatus = Status.connectionEstablished;
        }
    }
    public enum Status { connectionEstablished, pendingCommand, waitingResult };
    public enum Command { Save, Load, AddDoc, GetByWord, GetByQuery, Unknown};
}
