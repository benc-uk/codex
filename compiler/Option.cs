using Lua;

namespace Codex;

public class GotoOption : OptionBase {
  public GotoOption(string id, string text, string targetSectionId) : base(id, text, targetSectionId) { }

  public override void Execute(Story story, Section currentSection) {
    if (!story.Sections.ContainsKey(targetId)) {
      throw new InvalidOperationException($"Target section '{targetId}' does not exist in the story.");
    }

    var targetSection = story.Sections[targetId];
    if (!string.IsNullOrWhiteSpace(runLua)) {
      _ = Story.RunLua(runLua).Result;
    }

    if (notifyMessage != null && notifyMessage.Length > 0) {
      story.runner?.Notify(currentSection.parseText(notifyMessage));
    }

    story.runner?.GotoSection(targetSection);
  }
}

// ============================================================================================

public interface IOption {
  public string Id { get; }
  public string Text { get; }

  public void Execute(Story story, Section currentSection);
  internal bool IsAvailable();
}

// ============================================================================================

public abstract class OptionBase : IOption {
  public string Id { get; }
  public string Text { get; }

  protected string targetId;

  internal string ifCondition = "";

  internal string runLua = "";

  internal string? notifyMessage = null;

  protected OptionBase(string id, string text, string target) {
    Id = id;
    Text = text;
    targetId = target;
  }

  public abstract void Execute(Story story, Section currentSection);

  public virtual bool IsAvailable() {
    if (string.IsNullOrWhiteSpace(ifCondition)) {
      return true;
    }

    var result = Story.RunLua($"return {ifCondition}").Result;
    if (result != null && result.Length > 0 && result[0].Type == LuaValueType.Boolean) {
      bool boolResult = result[0].ToBoolean();
      return boolResult;
    }

    return false;
  }
}