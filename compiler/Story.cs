using System.Text.RegularExpressions;
using Lua;
using Lua.Standard;

namespace Codex;

public class Story {
  public Dictionary<string, Section> Sections { get; }
  internal IRunner? runner;
  private readonly static LuaState State = LuaState.Create();
  public string Title { get; internal set; } = "Untitled Story";
  internal string globalLua = "";

  internal Story() {
    Sections = new Dictionary<string, Section>();
    State.OpenMathLibrary();
    State.OpenStringLibrary();
    State.OpenTableLibrary();
    State.OpenStandardLibraries();

    // Add helper functions
    _ = State.DoStringAsync("""
      function dice(sides, count, modifier)
        local total = 0
        for i = 1, count do
          total = total + math.floor(math.random(1, sides+1))
        end
        return total + modifier
      end 

      function d(sides)
        return dice(sides, 1, 0)
      end

      temp = {}
    """).Result;

    State.Environment["notify"] = new LuaFunction(async (context, ct) => {
      var arg0 = context.GetArgument<string>(0);
      runner?.Notify(parseText(arg0));
      return 0;
    });
  }

  public void Run(IRunner runner, string startingSectionId = "start") {
    this.runner = runner;
    if (Sections.TryGetValue(startingSectionId, out var startSection)) {
      this.runner.GotoSection(startSection);
    } else {
      throw new InvalidOperationException($"Story does not have a '{startingSectionId}' section.");
    }
  }

  internal async Task addSectionAsync(Section section) {
    Sections[section.Id] = section;

    // Create a Lua table for each section to hold its persistent variables
    await State.DoStringAsync($"section_{section.Id} = {{}}");
  }

  internal static async Task<LuaValue[]> runLua(string luaCode) {
    try {
      return await State.DoStringAsync(luaCode);
    } catch (Exception ex) {
      throw new CompileException($"Lua Error: {ex.Message}");
    }
  }

  internal static string parseText(string text) {
    var result = text;

    // Scan for {varname} patterns var names can be a-Z, 0-9, underscore, and dot for section vars
    var varPattern = new Regex(@"\{([a-zA-Z0-9_.]+)\}");
    var matches = varPattern.Matches(result);
    foreach (Match match in matches) {
      var varName = match.Groups[1].Value;
      var varValue = getVar(varName).Result;
      result = result.Replace(match.Value, varValue);
    }

    return result;
  }

  internal static async Task<string> getVar(string name) {
    var res = await Story.runLua($"return {name}");
    var value = res[0].ToString() ?? "";
    return value;
  }
}
