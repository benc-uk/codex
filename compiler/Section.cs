using System.Text.RegularExpressions;
using Lua;

namespace Codex;

public class Section {
  public string Id { get; }
  public string Text {
    internal set;
    get {
      // Handle variable interpolation
      var result = field;

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
  internal Dictionary<string, IOption> Options;

  // Vars are the starting variables for this section, initial string values only!
  internal Dictionary<string, string> InitialVars = new Dictionary<string, string>();

  internal string runLua = "";

  internal Section(string id, string text) {
    Id = id;
    Text = text;
    Options = new Dictionary<string, IOption>();
  }

  public IReadOnlyDictionary<string, IOption> GetOptions() {
    // Only return options that are available
    var availableOptions = new Dictionary<string, IOption>();
    foreach (var kvp in Options) {
      if (kvp.Value.IsAvailable(this)) {
        availableOptions[kvp.Key] = kvp.Value;
      }
    }

    return availableOptions;
  }

  public IOption? GetOption(string optionId) {
    if (Options.ContainsKey(optionId) && Options[optionId].IsAvailable(this)) {
      return Options[optionId];
    }
    return null;
  }

  public async Task<string> GetVar(string name) {
    await Story.State.DoStringAsync($"section = {Id}");
    var res = await Story.State.DoStringAsync($"return {name}");
    return res[0].ToString() ?? "";
  }

  // run the section's runLua code before displaying
  public async Task Run() {
    if (!string.IsNullOrWhiteSpace(runLua)) {
      await Story.State.DoStringAsync($"section = {Id}");
      await Story.State.DoStringAsync(runLua);
    }
  }
}
