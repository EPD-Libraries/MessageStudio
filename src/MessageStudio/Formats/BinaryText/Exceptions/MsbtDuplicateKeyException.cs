namespace MessageStudio.Formats.BinaryText.Exceptions;

public class MsbtDuplicateKeyException : Exception
{
    private const string MESSAGE = "An item with the same key has already been added. Key: {0}";

    public MsbtDuplicateKeyException(string key) : base(string.Format(MESSAGE, key))
    {
    }

    public MsbtDuplicateKeyException(string key, Exception? innerException) : base(string.Format(MESSAGE, key), innerException)
    {
    }
}
