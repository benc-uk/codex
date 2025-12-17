namespace Codex;

public class GotoOption : OptionBase {
  public GotoOption(string id, string text, string targetSectionId)
      : base(id, text, targetSectionId) {
  }

  public override void Execute(Story story) {
    if (!story.Sections.ContainsKey(TargetId)) {
      throw new InvalidOperationException($"Target section '{TargetId}' does not exist in the story.");
    }

    story.runner?.GotoSection(story.Sections[TargetId]);
  }
}

public interface IOption {
  public string Id { get; }
  public string Text { get; }

  public void Execute(Story story);
  internal bool IsAvailable(Section section);
}

public abstract class OptionBase : IOption {
  public string Id { get; }
  public string Text { get; }

  public string TargetId;

  protected OptionBase(string id, string text, string targetId) {
    Id = id;
    Text = text;
    TargetId = targetId;
  }

  public abstract void Execute(Story story);

  public virtual bool IsAvailable(Section section) {
    // by default, options are always available
    return true;
  }
}