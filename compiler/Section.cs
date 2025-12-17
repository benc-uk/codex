namespace Codex;

using Lua;

public class Section {
  public string Id { get; }
  public string Text { get; internal set; }
  internal Dictionary<string, IOption> Options;

  internal LuaState luaState;

  public Section(string id, string text) {
    Id = id;
    Text = text;
    Options = new Dictionary<string, IOption>();
    luaState = LuaState.Create();
  }

  public async Task TestLua() {
    var res = await luaState.DoStringAsync("return searched");
    Console.WriteLine($"Lua test result: {res[0]}");
  }

  public IReadOnlyDictionary<string, IOption> GetOptions() {
    // only return options that are available
    var availableOptions = new Dictionary<string, IOption>();
    foreach (var kvp in Options) {
      if (kvp.Value.IsAvailable(this)) {
        availableOptions[kvp.Key] = kvp.Value;
      }
    }
    return availableOptions;
  }
}
