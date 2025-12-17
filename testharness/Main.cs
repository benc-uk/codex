using Codex;

public class ConsoleRunner : IRunner {
  Story story;

  public ConsoleRunner(Story story) {
    this.story = story;
  }

  public void GotoSection(Section section) {
    Console.WriteLine(section.Text);

    section.TestLua().Wait();

    // write the options
    int index = 1;
    var optionsList = section.GetOptions().Values.ToList();
    foreach (var option in optionsList) {
      Console.WriteLine($"{index}. {option.Text}");
      index++;
    }
    // get user choice
    var choice = GetUserChoice(optionsList.Count);
    var selectedOption = optionsList[choice - 1];

    selectedOption.Execute(story);
  }

  // get user input
  public int GetUserChoice(int maxChoice) {
    while (true) {
      Console.Write("Choose an option: ");
      var input = Console.ReadLine();
      if (int.TryParse(input, out var choice)) {
        if (choice >= 1 && choice <= maxChoice) {
          return choice;
        }
      }
      Console.WriteLine("Invalid choice. Please try again.");
    }
  }
}

public class TestHarness {
  public static void Main(string[] args) {
    if (args.Length < 1) {
      Console.WriteLine("Usage: testharness <story-file.yaml>");
      return;
    }

    var source = File.ReadAllText(args[0]);
    var compiler = new Compiler();
    var story = compiler.Compile(source);

    var runner = new ConsoleRunner(story);
    story.Run(runner);

    Console.WriteLine("Story loaded successfully.");
  }
}