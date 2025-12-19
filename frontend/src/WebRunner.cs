#nullable enable

using System;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using Codex;

public partial class WebRunner : IRunner {
  private static WebRunner? _instance;
  Story story;
  public Section? currentSection { get; set; }

  public WebRunner(Story story) {
    this.story = story;
    _instance = this;
  }

  public void GotoSection(Section section) {
    // Run any section Lua code
    section.Start();
    currentSection = section;
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


}
