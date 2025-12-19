using System.Text.RegularExpressions;
using Lua;

namespace Codex;

public class Section {
  public string Id { get; }
  public string Text {
    internal set;
    get => parseText(field);
  }
  internal string? title = null;
  public string? Title {
    get => title;
  }
  internal Dictionary<string, IOption> options;
  internal string runLua = "";

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

  public async Task<string> GetVar(string name) {
    var res = await Story.RunLua($"return {name}");
    var value = res[0].ToString() ?? "";
    return value;
  }

  // Run the section's runLua code before displaying
  public void Start() {
    // NOTE: 
    // temp is reset before each section run to avoid leftover state
    // The section table is a link to the section specific variables
    Story.RunLua($"temp = {{}}; section = section_{Id} or {{}}; {runLua}").Wait();
  }

  internal string parseText(string text) {
    var result = text;

    // Scan for {varname} patterns var names can be a-Z, 0-9, underscore, and dot for section vars
    var varPattern = new Regex(@"\{([a-zA-Z0-9_.]+)\}");
    var matches = varPattern.Matches(result);
    foreach (Match match in matches) {
      var varName = match.Groups[1].Value;
      var varValue = GetVar(varName).Result;
      result = result.Replace(match.Value, varValue);
    }

    return result;
  }
}
