using System;
using System.Net.Http;
using System.Threading.Tasks;

public partial class WasmEntry
{
  public static async Task Main(string[] args)
  {
    Console.WriteLine("Codex WASM entry point initialized.");

    if (args.Length < 1)
    {
      Console.WriteLine("Please provide the story URL as a command-line argument.");
      return;
    }

    var storyUrl = args[0];
    Console.WriteLine($"Loading story from URL: {storyUrl}");

    using var httpClient = new HttpClient();
    var source = await httpClient.GetStringAsync(storyUrl);

    Console.WriteLine($"Story file loaded, length: {source.Length} characters.");

    var compiler = new Codex.Compiler();
    var story = await compiler.Compile(source);
    Console.WriteLine($"Story '{story.Title}' compiled with {story.Sections.Count} sections.");
    var runner = new WebRunner(story);

    runner.Notify($"Welcome to '{story.Title}'!");

    // Start the story
    story.Run(runner);
  }
}
