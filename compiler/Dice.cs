namespace Codex;

using System.Text.RegularExpressions;

/// <summary>
/// Provides static methods for parsing and evaluating dice roll expressions.
/// Supports expressions like "2D6", "3D8+4", "1D20 - 2", etc.
/// </summary>
public static partial class Dice {
  private static readonly Random _random = new();

  // Regex pattern: optional count, 'D' or 'd', sides, optional modifier
  [GeneratedRegex(@"^\s*(\d+)?\s*[Dd]\s*(\d+)\s*([+-]\s*\d+)?\s*$")]
  private static partial Regex DicePattern();

  /// <summary>
  /// Evaluates a dice roll expression and returns the total result.
  /// </summary>
  /// <param name="expression">Dice expression like "2D6", "3D8+4", "1D20 - 2"</param>
  /// <returns>The total of all dice rolls plus any modifier</returns>
  /// <exception cref="ArgumentException">Thrown when the expression is invalid</exception>
  public static int Evaluate(string expression) {
    var (count, sides, modifier) = Parse(expression);
    return Roll(count, sides) + modifier;
  }

  /// <summary>
  /// Evaluates a dice roll expression and returns individual roll results.
  /// </summary>
  /// <param name="expression">Dice expression like "2D6", "3D8+4", "1D20 - 2"</param>
  /// <returns>A tuple containing the individual rolls, the modifier, and the total</returns>
  /// <exception cref="ArgumentException">Thrown when the expression is invalid</exception>
  public static (int[] rolls, int modifier, int total) EvaluateDetailed(string expression) {
    var (count, sides, modifier) = Parse(expression);
    var rolls = RollIndividual(count, sides);
    var total = rolls.Sum() + modifier;
    return (rolls, modifier, total);
  }

  /// <summary>
  /// Parses a dice expression into its components.
  /// </summary>
  /// <param name="expression">Dice expression like "2D6", "3D8+4", "1D20 - 2"</param>
  /// <returns>A tuple of (count, sides, modifier)</returns>
  /// <exception cref="ArgumentException">Thrown when the expression is invalid</exception>
  public static (int count, int sides, int modifier) Parse(string expression) {
    ArgumentException.ThrowIfNullOrWhiteSpace(expression);

    var match = DicePattern().Match(expression);
    if (!match.Success) {
      throw new ArgumentException($"Invalid dice expression: '{expression}'", nameof(expression));
    }

    // Count defaults to 1 if not specified (e.g., "D6" means "1D6")
    int count = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : 1;
    int sides = int.Parse(match.Groups[2].Value);

    // Parse modifier, removing spaces
    int modifier = 0;
    if (match.Groups[3].Success) {
      string modStr = match.Groups[3].Value.Replace(" ", "");
      modifier = int.Parse(modStr);
    }

    if (count < 1) {
      throw new ArgumentException("Dice count must be at least 1", nameof(expression));
    }

    if (sides < 1) {
      throw new ArgumentException("Dice sides must be at least 1", nameof(expression));
    }

    return (count, sides, modifier);
  }

  /// <summary>
  /// Checks if a string is a valid dice expression.
  /// </summary>
  /// <param name="expression">The string to check</param>
  /// <returns>True if the expression is a valid dice notation</returns>
  public static bool IsValid(string expression) {
    if (string.IsNullOrWhiteSpace(expression)) {
      return false;
    }

    return DicePattern().IsMatch(expression);
  }

  /// <summary>
  /// Rolls dice and returns the total.
  /// </summary>
  /// <param name="count">Number of dice to roll</param>
  /// <param name="sides">Number of sides on each die</param>
  /// <returns>The sum of all dice rolls</returns>
  public static int Roll(int count, int sides) {
    int total = 0;
    for (int i = 0; i < count; i++) {
      total += _random.Next(1, sides + 1);
    }
    return total;
  }

  /// <summary>
  /// Rolls dice and returns individual results.
  /// </summary>
  /// <param name="count">Number of dice to roll</param>
  /// <param name="sides">Number of sides on each die</param>
  /// <returns>An array containing each individual roll result</returns>
  public static int[] RollIndividual(int count, int sides) {
    var rolls = new int[count];
    for (int i = 0; i < count; i++) {
      rolls[i] = _random.Next(1, sides + 1);
    }
    return rolls;
  }
}
