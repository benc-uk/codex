using System.Text.RegularExpressions;
using Lua;

namespace Codex;

public class Section {
  public string Id { get; }
  public string Text {
    internal set;
    get => Story.parseText(field);
  }
  internal string? title = null;
  public string? Title {
    get => title;
  }
  internal Dictionary<string, IOption> options;
  internal string runLua = "";
  internal string runOnceLua = "";
  internal int visits = 0;

  internal Section(string id, string text) {
    Id = id;
    Text = text;
    options = new Dictionary<string, IOption>();
  }

  public IReadOnlyDictionary<string, IOption> GetOptions() {
    // Only return options that are available
    var availableOptions = new Dictionary<string, IOption>();
    foreach (var kvp in options) {
      if (kvp.Value.IsAvailable()) {
        availableOptions[kvp.Key] = kvp.Value;
      }
    }

    return availableOptions;
  }

  public IOption? GetOption(string optionId) {
    if (options.ContainsKey(optionId) && options[optionId].IsAvailable()) {
      return options[optionId];
    }
    return null;
  }

  // Run the section's runLua code before displaying
  public async Task Start() {
    visits += 1;

    // temp is reset before each section run to avoid leftover state
    // The section table is a link to the section specific variables which persist
    var code = $$"""
      temp = {}
      section = section_{{Id}}
      section.visits = {{visits}}
      {{runLua}}
    """;

    await Story.runLua(code);

    if (visits == 1 && runOnceLua.Trim() != "") {
      await Story.runLua(runOnceLua);
    }
  }
}
