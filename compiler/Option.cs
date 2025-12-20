using Lua;

namespace Codex;

public class GotoOption : OptionBase {
  public GotoOption(string id, string text, string targetSectionId) : base(id, text, targetSectionId) { }

  public override void Execute(Story story, Section currentSection) {
    // If targetId is "restart", reset the story state
    if (targetId == "restart") {
      story.runner?.Restart();
      return;
    }

    if (!story.Sections.ContainsKey(targetId)) {
      throw new InvalidOperationException($"Target section '{targetId}' does not exist in the story.");
    }

    if (!string.IsNullOrWhiteSpace(runLua)) {
      _ = Story.runLua(runLua).Result;
    }

    // Call the post_option function to check for overrides 
    var checkResult = Story.runLua("""
      if type(post_option) == "function" then
        return post_option()
      else
        return nil
      end
    """).Result;

    if (checkResult != null && checkResult.Length > 0 && checkResult[0].Type == LuaValueType.String) {
      var overrideTarget = checkResult[0].ToString();
      if (!string.IsNullOrWhiteSpace(overrideTarget) && story.Sections.ContainsKey(overrideTarget)) {
        targetId = overrideTarget;
      }
    }

    if (notifyMessage != null && notifyMessage.Length > 0) {
      story.runner?.Notify(Story.parseText(notifyMessage));
    }

    story.runner?.GotoSection(story.Sections[targetId]);
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

    var result = Story.runLua($"return {ifCondition}").Result;
    if (result != null && result.Length > 0 && result[0].Type == LuaValueType.Boolean) {
      bool boolResult = result[0].ToBoolean();
      return boolResult;
    }

    return false;
  }
}