using System;
using System.Net.Http;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

public partial class WasmEntry {
  public static async Task Main() {
    Console.WriteLine("WASM Entry Point Initialized");

    // In WASM, use HttpClient to fetch files from wwwroot
    // Need to get the base URL from the browser
    var baseUrl = GetBaseUrl();
    using var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
    var source = await httpClient.GetStringAsync("stories/demo-story.yaml");

    Console.WriteLine("Story file loaded.", source);

    var compiler = new Codex.Compiler();
    var story = await compiler.Compile(source);
    var runner = new WebRunner(story);

    // Start the story
    story.Run(runner);
  }

  [JSImport("globalThis.location.origin.toString", "")]
  private static partial string GetBaseUrl();
}
