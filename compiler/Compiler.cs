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

        await story.addSectionAsync(section.Result);
      }
    }

    // Top level vars (global vars)
    if (root.Children.TryGetValue(new YamlScalarNode("vars"), out var varsNode)) {
      var varsMap = (YamlMappingNode)varsNode;
      foreach (var (keyNode, valueNode) in varsMap.Children) {
        var varName = ((YamlScalarNode)keyNode).Value!;
        var varValue = ((YamlScalarNode)valueNode).Value!;
        await Story.runLua($"{varName} = {varValue}");
      }
    }

    // title
    if (root.Children.TryGetValue(new YamlScalarNode("title"), out var titleNode)) {
      story.Title = ((YamlScalarNode)titleNode).Value!;
    }

    // init Lua code
    if (root.Children.TryGetValue(new YamlScalarNode("init"), out var initNode)) {
      var initCode = ((YamlScalarNode)initNode).Value!;
      story.globalLua = initCode;
      await Story.runLua(initCode);
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

    // title
    if (node.Children.TryGetValue(new YamlScalarNode("title"), out var titleNode)) {
      section.title = ((YamlScalarNode)titleNode).Value!;
    }

    // options
    if (node.Children.TryGetValue(new YamlScalarNode("options"), out var optionsNode)) {
      var optionsMap = (YamlMappingNode)optionsNode;

      foreach (var (keyNode, valueNode) in optionsMap.Children) {
        var optionId = ((YamlScalarNode)keyNode).Value!;
        var option = ParseOption(optionId, valueNode);
        if (option != null) {
          section.options[optionId] = option;
        }
      }
    }

    // run Lua 
    if (node.Children.TryGetValue(new YamlScalarNode("run"), out var runNode)) {
      section.runLua = ((YamlScalarNode)runNode).Value!;
    }

    // run once Lua
    if (node.Children.TryGetValue(new YamlScalarNode("run_once"), out var runOnceNode)) {
      section.runOnceLua = ((YamlScalarNode)runOnceNode).Value!;
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
      string? ifCondition = null;
      string? runLua = null;
      string? notify = null;

      if (mappingNode.Children.TryGetValue(new YamlScalarNode("text"), out var textNode)) {
        text = ((YamlScalarNode)textNode).Value;
      }

      if (mappingNode.Children.TryGetValue(new YamlScalarNode("goto"), out var gotoNode)) {
        gotoTarget = ((YamlScalarNode)gotoNode).Value;
      }

      if (mappingNode.Children.TryGetValue(new YamlScalarNode("if"), out var ifNode)) {
        ifCondition = ((YamlScalarNode)ifNode).Value;
      }

      if (mappingNode.Children.TryGetValue(new YamlScalarNode("run"), out var runNode)) {
        runLua = ((YamlScalarNode)runNode).Value;
      }

      if (mappingNode.Children.TryGetValue(new YamlScalarNode("notify"), out var notifyNode)) {
        notify = ((YamlScalarNode)notifyNode).Value;
      }

      if (text == null) {
        throw new CompileException($"Option '{id}' must have a 'text' property");
      }

      if (gotoTarget != null) {
        var option = new GotoOption(id, text, gotoTarget);

        option.ifCondition = ifCondition ?? "";
        option.runLua = runLua ?? "";
        option.notifyMessage = notify ?? "";

        return option;
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