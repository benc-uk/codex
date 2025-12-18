using Lua;

namespace Codex;

public class Story {
  public Dictionary<string, Section> Sections { get; }
  internal IRunner? runner;
  internal static LuaState State = LuaState.Create();

  internal Story() {
    Sections = new Dictionary<string, Section>();
  }

  public void Run(IRunner runner) {
    this.runner = runner;
    if (Sections.TryGetValue("start", out var startSection)) {
      this.runner.GotoSection(startSection);
    } else {
      throw new InvalidOperationException("Story does not have a 'start' section.");
    }
  }

  internal async Task AddSectionAsync(Section section) {
    Sections[section.Id] = section;

    // Add a corresponding table in Lua state for this section
    await State.DoStringAsync($"{section.Id} = {{}}");
    foreach (var kvp in section.InitialVars) {
      var value = kvp.Value;

      // If the var value is a dice expression, evaluate it
      if (kvp.Value.StartsWith("[") && kvp.Value.EndsWith("]")) {
        var expression = kvp.Value[1..^1];
        value = "" + Dice.Evaluate(expression);
      }

      await State.DoStringAsync($"{section.Id}.{kvp.Key} = {value}");
    }
  }
}
