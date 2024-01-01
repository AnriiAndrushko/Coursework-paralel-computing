using IndexServer;

const int threadCount = 6;
const int backlog = threadCount;

const string IP = "192.168.1.104";
const int PORT = 25565;

Console.Title = "Server";
var server = new Server(IP, PORT, backlog, threadCount);
server.Start();
Console.ReadLine();
server.Stop();