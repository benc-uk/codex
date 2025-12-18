using Lua;
using YamlDotNet.RepresentationModel;

namespace Codex;

public class Compiler {
  public async Task<Story> Compile(string source) {
    var story = new Story();

    var yaml = new YamlStream();
    yaml.Load(new StringReader(source));

    var root = (YamlMappingNode)yaml.Documents[0].RootNode;

    if (root.Children.TryGetValue(new YamlScalarNode("sections"), out var sectionsNode)) {
      var sectionsMap = (YamlMappingNode)sectionsNode;

      foreach (var (keyNode, valueNode) in sectionsMap.Children) {
        var sectionId = ((YamlScalarNode)keyNode).Value!;
        var section = ParseSection(sectionId, (YamlMappingNode)valueNode);

        await story.AddSectionAsync(section.Result);
      }
    }

    // Top level vars (global vars)
    if (root.Children.TryGetValue(new YamlScalarNode("vars"), out var varsNode)) {
      var varsMap = (YamlMappingNode)varsNode;
      foreach (var (keyNode, valueNode) in varsMap.Children) {
        var varName = ((YamlScalarNode)keyNode).Value!;
        var varValue = ((YamlScalarNode)valueNode).Value!;
        // Set in Lua state as global variable
        await Story.State.DoStringAsync($"{varName} = {varValue}");
      }
    }

    return story;
  }

  private async Task<Section> ParseSection(string id, YamlMappingNode node) {
    // If id is named "section" that's a problem and reserved word
    if (id == "section") {
      throw new CompileException("Section ID cannot be 'section' as it is a reserved word.");
    }

    var text = "";

    if (node.Children.TryGetValue(new YamlScalarNode("text"), out var textNode)) {
      text = ((YamlScalarNode)textNode).Value ?? "";
    }

    var section = new Section(id, text);

    // options
    if (node.Children.TryGetValue(new YamlScalarNode("options"), out var optionsNode)) {
      var optionsMap = (YamlMappingNode)optionsNode;

      foreach (var (keyNode, valueNode) in optionsMap.Children) {
        var optionId = ((YamlScalarNode)keyNode).Value!;
        var option = ParseOption(optionId, valueNode);
        if (option != null) {
          section.Options[optionId] = option;
        }
      }
    }

    // vars
    if (node.Children.TryGetValue(new YamlScalarNode("vars"), out var varsNode)) {
      var varsMap = (YamlMappingNode)varsNode;
      foreach (var (keyNode, valueNode) in varsMap.Children) {
        var varName = ((YamlScalarNode)keyNode).Value!;
        var varValue = ((YamlScalarNode)valueNode).Value!;
        section.InitialVars[varName] = varValue;
      }
    }

    // run Lua 
    if (node.Children.TryGetValue(new YamlScalarNode("run"), out var runNode)) {
      var runLua = ((YamlScalarNode)runNode).Value!;
      section.runLua = runLua;
    }

    return section;
  }

  private IOption? ParseOption(string id, YamlNode node) {
    // Short form: option_id: [goto_target, "Option text"]
    if (node is YamlSequenceNode sequenceNode) {
      if (sequenceNode.Children.Count >= 2) {
        var text = ((YamlScalarNode)sequenceNode.Children[0]).Value!;
        var gotoTarget = ((YamlScalarNode)sequenceNode.Children[1]).Value!;
        return new GotoOption(id, text, gotoTarget);
      }
      throw new CompileException($"Short form option '{id}' must have exactly 2 elements: [goto_target, text]");
    }

    // Long form: option_id: { text: "...", goto: "...", trigger: "..." }
    if (node is YamlMappingNode mappingNode) {
      string? text = null;
      string? gotoTarget = null;
      string? triggerTarget = null;
      string? ifCondition = null;
      string? runLua = null;

      if (mappingNode.Children.TryGetValue(new YamlScalarNode("text"), out var textNode)) {
        text = ((YamlScalarNode)textNode).Value;
      }

      if (mappingNode.Children.TryGetValue(new YamlScalarNode("goto"), out var gotoNode)) {
        gotoTarget = ((YamlScalarNode)gotoNode).Value;
      }

      if (mappingNode.Children.TryGetValue(new YamlScalarNode("trigger"), out var triggerNode)) {
        triggerTarget = ((YamlScalarNode)triggerNode).Value;
      }

      if (mappingNode.Children.TryGetValue(new YamlScalarNode("if"), out var ifNode)) {
        ifCondition = ((YamlScalarNode)ifNode).Value;
      }

      if (mappingNode.Children.TryGetValue(new YamlScalarNode("run"), out var runNode)) {
        runLua = ((YamlScalarNode)runNode).Value;
      }

      if (text == null) {
        throw new CompileException($"Option '{id}' must have a 'text' property");
      }

      if (gotoTarget != null) {
        var option = new GotoOption(id, text, gotoTarget);

        option.ifCondition = ifCondition ?? "";
        option.runLua = runLua ?? "";

        return option;
      } else if (triggerTarget != null) {
        // return new TriggerOption(id, text, triggerTarget);
        throw new CompileException($"TriggerOption is not implemented yet");
      } else {
        throw new CompileException($"Option '{id}' must have either 'goto' or 'trigger'");
      }
    }

    throw new CompileException($"Option '{id}' has invalid format");
  }
}

public class CompileException : Exception {
  public CompileException(string message) : base(message) { }
}