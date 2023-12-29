using InvertedIndexLib;

using (var index = new InvertedIndexKeeper(6))
{
    //index.AddText("hello", "1");
    index.AddText("This, is a sample: document.", "2");
    //index.AddText("hello cat", "3");
    //index.AddText("hello hhh", "4");
    //index.AddText("hello cat", "5");
    //index.AddText("big cat", "6");
    while (index.IsBusy){
        Console.WriteLine("busy");
    }
    Console.WriteLine(index.GetByWord("hello").Count());
    Console.WriteLine(index.GetByWord("world").Count());
    Console.WriteLine(index.GetByWord("cat").Count());
    Console.WriteLine(index.GetByWord("ff").Count());
    Console.WriteLine(index.GetByQuery("hello cat").Count());
}
