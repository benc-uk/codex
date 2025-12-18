using Lua;

namespace Codex;

public class GotoOption : OptionBase {
  public GotoOption(string id, string text, string targetSectionId) : base(id, text, targetSectionId) { }

  public override void Execute(Story story, Section currentSection) {
    if (!story.Sections.ContainsKey(TargetId)) {
      throw new InvalidOperationException($"Target section '{TargetId}' does not exist in the story.");
    }

    var targetSection = story.Sections[TargetId];
    if (!string.IsNullOrWhiteSpace(runLua)) {
      Story.State.DoStringAsync($"section = {currentSection.Id}");
      _ = Story.State.DoStringAsync(runLua).Result;
    }

    story.runner?.GotoSection(targetSection);
  }
}

public interface IOption {
  public string Id { get; }
  public string Text { get; }

  public void Execute(Story story, Section currentSection);
  internal bool IsAvailable(Section section);
}

public abstract class OptionBase : IOption {
  public string Id { get; }
  public string Text { get; }

  protected string TargetId;

  internal string ifCondition = "";

  internal string runLua = "";

  protected OptionBase(string id, string text, string targetId) {
    Id = id;
    Text = text;
    TargetId = targetId;
  }

  public abstract void Execute(Story story, Section currentSection);

  public virtual bool IsAvailable(Section section) {
    if (string.IsNullOrWhiteSpace(ifCondition)) {
      return true;
    }

    _ = Story.State.DoStringAsync($"section = {section.Id}").Result;
    var result = Story.State.DoStringAsync($"return {ifCondition}").Result;
    if (result != null && result.Length > 0 && result[0].Type == LuaValueType.Boolean) {
      bool boolResult = result[0].ToBoolean();
      return boolResult;
    }

    return false;
  }
}