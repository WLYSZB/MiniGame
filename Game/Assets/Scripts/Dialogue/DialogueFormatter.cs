public static class DialogueFormatter
{
    private const string FallbackPlayerName = "Trash Bag";

    public static string Format(string template, string playerName)
    {
        var safeTemplate = template ?? string.Empty;
        var safeName = string.IsNullOrWhiteSpace(playerName) ? FallbackPlayerName : playerName.Trim();
        return safeTemplate.Replace("{playerName}", safeName);
    }
}
