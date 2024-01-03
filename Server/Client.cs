using InvertedIndexLib;
using System.Net.Sockets;
using System.Text;

namespace IndexServer
{
    internal class Client
    {
        private string _received;
        private byte[] _buffer;
        private InvertedIndexKeeper _index;
        public int RecievedCount;

        public Socket Socket { get; set; }
        public Status CurStatus
        {
            get { return _curStatus; }
            set
            {
                switch (value)
                {
                    case Status.pendingCommand:
                        _curStatus = Status.pendingCommand;
                        _buffer = new byte[4096];
                        break;
                    case Status.waitingResult:
                        Array.Resize(ref _buffer, RecievedCount);
                        _received = Encoding.ASCII.GetString(_buffer);
                        _curStatus = Status.waitingResult;
                        string command = _received.Split()[0];
                        string param;
                        Command parsedCommand;
                        if (!Enum.TryParse(command, true, out parsedCommand))
                        {
                            parsedCommand = Command.Unknown;
                        };
                        switch (parsedCommand)
                        {
                            case Command.Save:
                                Result = null;
                                Console.WriteLine(command + " command was recieved");
                                param = _received.Split(' ', 2)[1];
                                _index.Save(param);
                                break;
                            case Command.Load:
                                Result = null;
                                Console.WriteLine(command + " command was recieved");
                                param = _received.Split(' ', 2)[1];
                                _index.Load(param);
                                break;
                            case Command.AddDoc:
                                Console.WriteLine(command + " command was recieved");
                                param = _received.Split(' ', 2)[1];
                                _index.AddDoc(param);
                                Result = null;
                                break;
                            case Command.GetByWord:
                                Console.WriteLine(command + " command was recieved");
                                param = _received.Split(' ', 2)[1];
                                Result = _index.GetByWord(param);
                                break;
                            case Command.GetByQuery:
                                Console.WriteLine(command + " command was recieved");
                                param = _received.Split(' ', 2)[1];
                                Result = _index.GetByQuery(param);
                                break;
                            case Command.Disconnect:
                                Console.WriteLine(command + " command was recieved");
                                CurStatus = Status.Disconnect;
                                break;
                            default:
                                Console.WriteLine("Incorrect command was recieved");
                                Result = null;
                                CurStatus = Status.pendingCommand;
                                break;
                        }
                        break;
                    case Status.Disconnect:
                        _curStatus = Status.Disconnect;
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
            set
            {
                _buffer = value;
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
            CurStatus = Status.pendingCommand;
        }
    }
    public enum Status { pendingCommand, waitingResult, Disconnect };
    public enum Command { Save, Load, AddDoc, GetByWord, GetByQuery, Disconnect, Unknown};
}
