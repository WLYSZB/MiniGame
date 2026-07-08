using NUnit.Framework;

public class DialogueFormatterTests
{
    [Test]
    public void Format_Replaces_PlayerName_Token()
    {
        var result = DialogueFormatter.Format("Hello, {playerName}", "Baggy");

        Assert.That(result, Is.EqualTo("Hello, Baggy"));
    }

    [Test]
    public void Format_FallsBack_WhenNameIsBlank()
    {
        var result = DialogueFormatter.Format("Hello, {playerName}", " ");

        Assert.That(result, Is.EqualTo("Hello, Trash Bag"));
    }
}
