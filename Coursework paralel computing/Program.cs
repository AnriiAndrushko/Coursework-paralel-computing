using InvertedIndexLib;
using IndexServer;

using (var index = new InvertedIndexKeeper(6))
{

    const string IP = "192.168.1.104";
    const int PORT = 25565;

    Console.Title = "Server";
    var server = new Server(IP, PORT);
    server.Start();
    Console.ReadLine();
    server.Stop();
}
