using System;

namespace Xaml;

/// <summary>
/// Returns an object referenced in the expression.
/// {Ref myObject} returns the object with x:Id="myObject". Forward-references are supported.
/// </summary>
[MarkupExtension("Ref")]
class RefMarkupExtension : IMarkupExtension
{
    public RefMarkupExtension() { }
    public RefMarkupExtension(string @ref) => Ref = @ref;

    /// <summary>String reference to ID of object to be used.</summary>
    public string? Ref { get; set; }

    public object ProvideValue(IMarkupExtensionContext context)
    {
        if (string.IsNullOrEmpty(Ref))
            throw new InvalidOperationException("Ref is null or empty string.");

        var obj = context.GetObjectById(Ref);
        if (obj is not null) return obj;

        if (context.IsFixupTokenAvailable)
            return context.GetFixupToken([Ref]);

        throw new InvalidOperationException($"Object with Id={Ref} not found.");
    }
}
