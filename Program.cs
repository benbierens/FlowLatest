using FlowLatest;

public class Program
{
    public static int Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Please provide a path");
            return 1;
        }

        var root = args[0];
        if (!Directory.Exists(root))
        {
            Console.WriteLine(root + " is not a path.");
            return 1;
        }

        var flow = new Flow(root, line =>
        {
            Console.WriteLine(line);
        });

        flow.Run();

        return 0;
    }
}
