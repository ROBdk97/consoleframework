using System;

namespace ConsoleFramework.Xaml;

/// <summary>
/// Returns a Type by name. The name may include type arguments,
/// e.g. ConsoleFramework.Xaml.TestClass`1[System.String].
/// </summary>
[MarkupExtension("Type")]
class TypeMarkupExtension : IMarkupExtension
{
    public TypeMarkupExtension() { }
    public TypeMarkupExtension(string name) => Name = name;

    public string? Name { get; set; }

    public object? ProvideValue(IMarkupExtensionContext context) => Type.GetType(Name!);
}
