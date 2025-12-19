using Lua;
using Lua.Standard;

namespace Codex;

public class Story {
  public Dictionary<string, Section> Sections { get; }
  internal IRunner? runner;
  private static LuaState State = LuaState.Create();
  public string Title { get; internal set; } = "Untitled Story";

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
  }

  public void Run(IRunner runner, string startingSectionId = "start") {
    this.runner = runner;
    if (Sections.TryGetValue(startingSectionId, out var startSection)) {
      this.runner.GotoSection(startSection);
    } else {
      throw new InvalidOperationException($"Story does not have a '{startingSectionId}' section.");
    }
  }

  internal async Task AddSectionAsync(Section section) {
    Sections[section.Id] = section;

    // Create a Lua table for each section to hold its persistent variables
    await State.DoStringAsync($"section_{section.Id} = {{}}");
  }

  internal static async Task<LuaValue[]> RunLua(string luaCode) {
    var res = await State.DoStringAsync(luaCode);
    return res;
  }
}
