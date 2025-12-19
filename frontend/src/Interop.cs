
using System.Runtime.InteropServices.JavaScript;

public static partial class Interop {
  [JSImport("renderSection", "codex")]
  public static partial void RenderSection(string id, string text, string title, string[] optionIds, string[] optionTexts);

  [JSImport("notify", "codex")]
  public static partial void Notify(string message);

  [JSImport("globalThis.location.origin.toString", "")]
  public static partial string GetBaseUrl();
}