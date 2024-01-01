using System.Diagnostics;

Process p = new Process();

p.StartInfo.UseShellExecute = false;
p.StartInfo.RedirectStandardOutput = true;
p.StartInfo.FileName = "E:\\repos\\ParalelComputing\\Coursework paralel computing\\ConsoleClient\\bin\\Debug\\net6.0\\ConsoleClient.exe";
p.StartInfo.RedirectStandardInput = true;

p.Start();
p.StandardInput.WriteLine("AddDoc text.txt");
Thread.Sleep(1000);
p.StandardInput.WriteLine("GetByWord hello");
p.StandardInput.WriteLine("Disconnect");
string output = p.StandardOutput.ReadToEnd();
Console.WriteLine(output);
p.WaitForExit();
Console.ReadLine();