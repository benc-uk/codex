using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Text.Json.Serialization;

public partial class WasmEntry
{
  public static void Main()
  {
    Console.WriteLine("WASM Entry Point Initialized");

    var optionsJson = createOptions(
     "Look around the cave",
     "Move deeper into the cave",
     "Exit the cave"
   );

    renderSection("cave", @"You stand at the entrance of a dark cave. 
            The air is damp and the sound of dripping water echoes from within. 
            There is moss and lichen growing on the walls, and the faint glimmer of crystals catches your eye. 
            The cave extends into darkness ahead.", optionsJson);
  }

  private static string createOptions(params string[] options)
  {
    var optionsDict = new Dictionary<string, string>();
    for (int i = 0; i < options.Length; i++)
    {
      optionsDict[$"option{i + 1}"] = options[i];
    }
    return JsonSerializer.Serialize(optionsDict, AppJsonContext.Default.DictionaryStringString);
  }

  [JSImport("renderSection", "codex")]
  internal static partial bool renderSection(string id, string desc, string optionsJson);
}

[JsonSerializable(typeof(Dictionary<string, string>))]
internal partial class AppJsonContext : JsonSerializerContext
{
}
