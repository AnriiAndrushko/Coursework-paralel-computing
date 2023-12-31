using System.Net.Sockets;

namespace IndexServer
{
    internal class Client
    {
        private int[,] _arr;
        private int[] _vecor;
        public Socket Socket { get; set; }
        public Status CurStatus
        {
            get { return _curStatus; }
            set
            {
                switch (value)
                {
                    case Status.pendingArrData:
                        _curStatus = Status.pendingArrData;
                        n = BitConverter.ToInt32(_buffer, 0);
                        _buffer = new byte[BufferSize];
                        _arr = new int[n, n];
                        _vecor = new int[n];
                        break;
                    case Status.pendingArrSize:
                        _curStatus = Status.pendingArrSize;
                        n = -1;
                        _buffer = new byte[BufferSize];
                        break;
                    case Status.waitingForCalculatedAnswer:
                        int x=0, y=0;
                        int matLength = _buffer.Length * 2 / 3;
                        for (int i = 0; i< matLength; i+= sizeof(int))
                        {
                            _arr[y,x] = BitConverter.ToInt32(_buffer, i);
                            x++;
                            if (x>=n)
                            {
                                x = 0;
                                y++;
                            }
                        }
                        x = 0;
                        for (int i = _buffer.Length * 2 / 3; i < _buffer.Length; i += sizeof(int))
                        {
                            _vecor[x] = BitConverter.ToInt32(_buffer, i);
                            x++;
                        }
                        _curStatus = Status.waitingForCalculatedAnswer;
                        break;
                }
            }
        }
        public Status _curStatus;

        public int n;
        public byte[] Buff {
            get
            {
                return _buffer;
            }
        }
        private byte[] _buffer;

        Task<int[,]> _task;
        public int[,] Result
        {
            get
            {
                if (!_task.IsCompleted)
                {
                    return new int[0,0];
                }
                return _task.Result;
            }
        }

        public int BufferSize { get
            {
                return CurStatus == Status.pendingArrSize ? 
                    sizeof(int) * 1 : sizeof(int) * (n * n + n);
            }
        }

        public Client(Socket socket)
        {
            _task = new Task<int[,]>(worker);
            Socket = socket;
            CurStatus = Status.pendingArrSize;
        }

        public void StartCalculation()
        {
            if (CurStatus != Status.waitingForCalculatedAnswer)
            {
                throw new Exception("Cant start calculation without data");
            }
            _task.Start();
        }

        private int[,] worker()
        {
            int rA = _arr.GetLength(0);
            int cA = _arr.GetLength(1);
            int cB = 1;
            int temp = 0;
            int[,] toReturn = new int[rA, cB];

            for (int i = 0; i < rA; i++)
            {
                for (int j = 0; j < cB; j++)
                {
                    temp = 0;
                    for (int k = 0; k < cA; k++)
                    {
                        temp += _arr[i, k] * _vecor[k];
                    }
                    toReturn[i, j] = temp;
                }
            }


            Thread.Sleep(10_000); //simulate work
            return toReturn;
        }

    }
    public enum Status { pendingArrSize, pendingArrData, waitingForCalculatedAnswer };
}
