using MessageStudio.Formats.BinaryText.Extensions;

namespace MessageStudio.Formats.BinaryText.Components;

public class TagResolver
{
    public static void Load<T>() where T : ITagResolver, new()
    {
        Load(new T());
    }

    public static void Load(ITagResolver resolver)
    {
        TagResolverExtension.Register(resolver);
    }
}
