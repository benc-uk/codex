using System.Text.Json.Nodes;
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

      function contains(container, item)
        for _, v in pairs(container) do
          if v == item then
            return true
          end
        end
        return false
      end

      function insert(container, item)
        table.insert(container, item)
      end

      function remove(container, item)
        for i, v in pairs(container) do
          if v == item then
            table.remove(bag, i)
            break
          end
        end
      end

      function remove_all(container, item)
        local i = 1
        while i <= #container do
          if container[i] == item then
            table.remove(container, i)
          else
            i = i + 1
          end
        end
      end

      function count(container, item)
        local count = 0
        for _, v in pairs(container) do
          if v == item then
            count = count + 1
          end
        end
        return count
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

  public Dictionary<string, JsonNode?> GetGlobals() {
    var globals = new Dictionary<string, JsonNode?>();
    var luaGlobals = State.Environment;

    foreach (var kvp in luaGlobals) {
      var ok = kvp.Value.TryRead<string>(out var strVal);
      if (ok) {
        globals[kvp.Key.ToString()] = JsonValue.Create(strVal);
        continue;
      }
      ok = kvp.Value.TryRead<double>(out var numVal);
      if (ok) {
        globals[kvp.Key.ToString()] = JsonValue.Create(numVal);
        continue;
      }
      ok = kvp.Value.TryRead<bool>(out var boolVal);
      if (ok) {
        globals[kvp.Key.ToString()] = JsonValue.Create(boolVal);
        continue;
      }
      ok = kvp.Value.TryRead<LuaTable>(out var tableVal);
      if (ok) {
        // Convert LuaTable to JsonArray
        var jsonArray = new JsonArray();
        for (var i = 1; i <= tableVal.ArrayLength; i++) {
          var val = tableVal[i];
          if (val != LuaValue.Nil) {
            var itemOk = val.TryRead<string>(out var itemStr);
            if (itemOk) {
              jsonArray.Add(JsonValue.Create(itemStr));
            }
          }
        }
        globals[kvp.Key.ToString()] = jsonArray;
        continue;
      }
    }

    return globals;
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
