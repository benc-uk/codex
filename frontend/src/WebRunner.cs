#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Codex;

[JsonSerializable(typeof(Dictionary<string, JsonNode?>))]
internal partial class WebRunnerJsonContext : JsonSerializerContext { }

public partial class WebRunner : IRunner {
  private static WebRunner? _instance;
  Story story;
  public Section? currentSection { get; set; }

  public WebRunner(Story story) {
    this.story = story;
    _instance = this;
  }

  public void GotoSection(Section section) {
    currentSection = section;

    // Calling start on the section to set it up and run any entry code it has
    section.Start().Wait();

    var optionsList = section.GetOptions();
    var optionsIds = optionsList.Keys.ToArray();
    var optionsTexts = optionsList.Values.Select(o => o.Text).ToArray();

    // Invoke JS to render the section
    Interop.RenderSection(section.Id, section.Text, section.Title, optionsIds, optionsTexts);
  }

  public void Notify(string message) {
    // Invoke JS to show notification
    Interop.Notify(message);
  }

  public void Restart() {
    _instance = null;
    Interop.Restart();
  }

  [JSExport]
  public static void TakeOption(string optionId) {
    if (_instance == null) {
      throw new InvalidOperationException("WebRunner not initialized.");
    }
    if (_instance.currentSection == null) {
      throw new InvalidOperationException("No current section to take option from.");
    }
    var option = _instance.currentSection.GetOption(optionId);
    if (option == null) {
      throw new InvalidOperationException($"Option '{optionId}' not found in section '{_instance.currentSection.Id}'.");
    }
    option.Execute(_instance.story, _instance.currentSection);
  }

  [JSExport]
  public static string GetGlobalsWrapper() {
    if (_instance == null) {
      throw new InvalidOperationException("WebRunner not initialized.");
    }

    return _instance.GetGlobals();
  }

  public string GetGlobals() {
    var globalDict = story.GetGlobals();
    var json = JsonSerializer.Serialize(globalDict, WebRunnerJsonContext.Default.DictionaryStringJsonNode);
    return json;
  }
}

