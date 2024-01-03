using IndexServer;
using System.Diagnostics;

const string IP = "192.168.1.104";
const int PORT = 25565;

Console.WriteLine("Run test or normal server? test\\server");
bool answer = Console.ReadLine().Equals("test");
bool useOwnDictionary = false;

if (answer) {
    Console.WriteLine("Use own dictionary? y/n");
    useOwnDictionary = Console.ReadLine().Equals("y");
}



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
List<string> path = new List<string>() { "F:\\dataset\\aclImdb\\my\\test\\neg", 
                                         "F:\\dataset\\aclImdb\\my\\test\\pos",
                                         "F:\\dataset\\aclImdb\\my\\train\\unsup", 
                                         "F:\\dataset\\aclImdb\\my\\train\\pos", 
                                         "F:\\dataset\\aclImdb\\my\\train\\neg" };
List<string> files = new List<string>();

foreach (string pathItem in path)
{
    foreach (var file in Directory.GetFiles(pathItem))
    {
        files.Add(file);
    }
}

ManualResetEvent isDoneMarker = new ManualResetEvent(false);
int maxThread = 30;
int aproxCount = 20;

double[] time = new double[maxThread];

for (int i = 0; i< aproxCount; i++)
{
    for (int threadCount = 1; threadCount <= maxThread; threadCount++)
    {
        Console.WriteLine("Thread count: " + threadCount);

        var server = new Server(IP, PORT, 0, threadCount, useOwnDictionary);

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
        time[threadCount-1] += elapsedTime.TotalMilliseconds;
        //if (threadCount == maxThread) { server.LocalExecute("Save taskIndex"); }
        server.Stop();
        server.TasksCompleted -= done;
    }
}
for (int i = 0; i<maxThread; i++)
{
    time[i] /= aproxCount;
    Console.Write($"{time[i].ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)}, ");
}


Console.ReadLine();

void done(){
    isDoneMarker.Set();
}