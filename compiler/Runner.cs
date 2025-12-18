namespace Codex;

// Interface for external runners of programs
public interface IRunner {
  public Section? currentSection { get; set; }

  void GotoSection(Section section);
}