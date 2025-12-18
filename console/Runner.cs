using Codex;

public class ConsoleRunner : IRunner
{
  Story story;
  public Section? currentSection { get; set; }

  public ConsoleRunner(Story story)
  {
    this.story = story;
  }

  public void GotoSection(Section section)
  {
    // Run any section Lua code
    section.Run().Wait();
    Console.WriteLine("\n" + section.Text);

    currentSection = section;

    int index = 1;
    var optionsList = section.GetOptions().Values.ToList();
    foreach (var option in optionsList)
    {
      Console.WriteLine($"  {index}. {option.Text}");
      index++;
    }

    var choice = GetUserChoice(optionsList.Count);
    var selectedOption = optionsList[choice - 1];

    selectedOption.Execute(story, currentSection);
  }

  // get user input
  public int GetUserChoice(int maxChoice)
  {
    while (true)
    {
      Console.Write("\nChoose an option: ");

      var input = Console.ReadLine();
      if (int.TryParse(input, out var choice))
      {
        if (choice >= 1 && choice <= maxChoice)
        {
          return choice;
        }
      }

      Console.WriteLine("Invalid choice. Please try again.");
    }
  }
}
