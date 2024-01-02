using IndexServer;
using System.Diagnostics;

const string IP = "192.168.1.104";
const int PORT = 25565;

Console.WriteLine("Run test or normal server? test\\server");
bool answer = Console.ReadLine().Equals("test");

if (!answer)
{
    const int threadCount = 6;
    const int backlog = threadCount;

    Console.Title = "Server";
    var server = new Server(IP, PORT, backlog, threadCount);
    server.Start();
    Console.ReadLine();
    server.Stop();
    return;
}
List<string> path = new List<string>() { "F:\\dataset\\aclImdb\\test\\neg", 
                                         "F:\\dataset\\aclImdb\\test\\pos",
                                         "F:\\dataset\\aclImdb\\train\\unsup", 
                                         "F:\\dataset\\aclImdb\\train\\pos", 
                                         "F:\\dataset\\aclImdb\\train\\neg" };
List<string> files = new List<string>();

foreach (string pathItem in path)
{
    foreach (var file in Directory.GetFiles(pathItem))
    {
        files.Add(file);
    }
}

ManualResetEvent isDoneMarker = new ManualResetEvent(false);
for (int threadCount = 1; threadCount<25; threadCount++)
{
    Console.Write("Thread count: " + threadCount);

    var server = new Server(IP, PORT, 0, threadCount);

    isDoneMarker.Reset();

    server.TasksCompleted += done;

    Stopwatch stopwatch = new Stopwatch();
    stopwatch.Start();

    foreach (var file in files)
    {
        server.LocalExecute("AddDoc " + file);
    }

    isDoneMarker.WaitOne();

    stopwatch.Stop();
    TimeSpan elapsedTime = stopwatch.Elapsed;
    Console.WriteLine($" Time: {elapsedTime.TotalMilliseconds} milliseconds");


    //server.LocalExecute("Save taskIndex");
    server.Stop();
    server.TasksCompleted -= done;
}
Console.ReadLine();

void done(){
    isDoneMarker.Set();
}