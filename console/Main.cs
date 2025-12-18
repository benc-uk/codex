using Codex;

public class CodexConsoleApp
{
  public static void Main(string[] args)
  {
    if (args.Length < 1)
    {
      Console.WriteLine("Usage: testharness <story-file.yaml>");
      return;
    }

    var source = File.ReadAllText(args[0]);
    var compiler = new Compiler();
    var story = compiler.Compile(source).Result;

    var runner = new ConsoleRunner(story);

    // Block and run the story
    story.Run(runner);
  }
}