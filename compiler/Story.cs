namespace Codex;

public class Story {
  public Dictionary<string, Section> Sections { get; internal set; }
  internal IRunner? runner;

  internal Story() {
    Sections = new Dictionary<string, Section>();
  }

  public static Story FromSource(string source) {
    var compiler = new Compiler();
    return compiler.Compile(source);
  }

  public void Run(IRunner runner) {
    this.runner = runner;
    if (Sections.TryGetValue("start", out var startSection)) {
      this.runner.GotoSection(startSection);
    } else {
      throw new InvalidOperationException("Story does not have a 'start' section.");
    }
  }
}
