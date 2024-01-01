using System.Net.Sockets;
using System.Text;

const string IPServer = "192.168.1.104";
const int ServerPort = 25565;
int sizeN = -1;
Status curStatus = Status.SendingSize;

Socket ClientSocket = new Socket
       (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

Console.Title = "Client";
ConnectToServer();
RequestLoop();
Exit();


void ConnectToServer()
{
    int attempts = 0;

    while (!ClientSocket.Connected)
    {
        try
        {
            attempts++;
            Console.WriteLine("Connection attempt " + attempts);
            ClientSocket.Connect(IPServer, ServerPort);
        }
        catch (SocketException)
        {
            Console.Clear();
        }
    }

    Console.Clear();
    Console.WriteLine("Connected");
}

void RequestLoop()
{
    Console.WriteLine(@"Type ""exit"" any time to disconnect from server");

    while (true)
    {
        SendRequest();
        ReceiveResponse();
    }
}

void Exit()
{
    SendStop();
    ClientSocket.Shutdown(SocketShutdown.Both);
    ClientSocket.Close();
    Environment.Exit(0);
}

void SendRequest()
{
    switch (curStatus)
    {
        case Status.SendingSize:
            Console.WriteLine("Send matrix size n");
            break;

        case Status.SendingArray:
            Console.WriteLine($"Send array {sizeN}*{sizeN} = {sizeN * sizeN} int digits and {sizeN} digits of vector");
            break;
        case Status.GettingResult:
            Console.WriteLine($"Write anything to request server for result");
            break;
    }

    string request = Console.ReadLine();

    if (request == "exit")
    {
        Exit();
        return;
    }
    if (curStatus == Status.GettingResult)
    {
        SendInts(new int[] { 1 });
        return;
    }
    int[] toSend = request.Split(" ").Select(x => Int32.Parse(x)).ToArray();


    switch (curStatus)
    {
        case Status.SendingSize:
            sizeN = toSend[0];
            curStatus = Status.SendingArray;
            break;
        case Status.SendingArray:
            curStatus = Status.GettingResult;
            break;
    }

    SendInts(toSend);
}


void SendInts(int[] data)
{
    byte[] buffer = new byte[data.Length * sizeof(int)];
    int counter = 0;
    foreach (int i in data)
    {
        byte[]? newDat = BitConverter.GetBytes(data[counter]);
        Array.Copy(newDat, 0, buffer, counter * sizeof(int), newDat.Length);
        counter++;
    }
    ClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
}

void SendStop()
{
    ClientSocket.Send(new byte[1], 0, 1, SocketFlags.None);
}

void ReceiveResponse()
{
    var buffer = new byte[4096]; //to recive any message
    int received = ClientSocket.Receive(buffer, SocketFlags.None);
    if (received == 0) return;
    var data = new byte[received];
    Array.Copy(buffer, data, received);
    string text = Encoding.ASCII.GetString(data);
    Console.WriteLine(text);
}

public enum Status { connectionEstablished, sendingCommand, waitingResult };