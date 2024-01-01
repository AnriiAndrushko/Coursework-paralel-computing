﻿using System.Net.Sockets;
using System.Text;

const string IPServer = "192.168.1.104";
const int ServerPort = 25565;
int sizeN = -1;
Status curStatus = Status.sendingCommand;

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
    Console.WriteLine(@"Type ""Disconnect"" any time to disconnect from server");

    while (true)
    {
        SendRequest();
        ReceiveResponse();
    }
}

void Exit()
{
    ClientSocket.Shutdown(SocketShutdown.Both);
    ClientSocket.Close();
    Environment.Exit(0);
}

void SendRequest()
{
    switch (curStatus)
    {
        case Status.sendingCommand:
            Console.WriteLine("Send one of this command:\n" +
                "Save path - save index to provided path\n" +
                "Load path - load index from provided path\n" +
                "AddDoc path - adds doc from provided path\n" +
                "GetByWord word - get all dock name from index by word\n" +
                "GetByQuery query - get all dock name from index by query\n" +
                "Disconnect - disconnect from server");
            break;
    }

    string input = Console.ReadLine();

    string command = input.Split()[0];
    Command parsedCommand;
    if (!Enum.TryParse(command, true, out parsedCommand))
    {
        parsedCommand = Command.Unknown;
    };
    switch (parsedCommand)
    {
        case Command.Save:
            break;
        case Command.Load:
            break;
        case Command.AddDoc:
            break;
        case Command.GetByWord:
            break;
        case Command.GetByQuery:
            break;
        case Command.Disconnect:
            SendCommand(command);
            Exit();
            return;
        default:
            Console.WriteLine("Incorrect command");
            SendCommand(command);//this should not be here in real app, but i need to send incorrect command to test that server correctly handle it
            return;
    }

    SendCommand(command);
}


void SendCommand(string data)
{
    byte[] buffer = Encoding.ASCII.GetBytes(data); ;
    ClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
}

void ReceiveResponse()
{
    var buffer = new byte[4096];
    int received = ClientSocket.Receive(buffer, SocketFlags.None);
    if (received == 0) return;
    var data = new byte[received];
    Array.Copy(buffer, data, received);
    string text = Encoding.ASCII.GetString(data);
    Console.WriteLine(text);
}

public enum Status { sendingCommand };
public enum Command { Save, Load, AddDoc, GetByWord, GetByQuery, Disconnect, Unknown };