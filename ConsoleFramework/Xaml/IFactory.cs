namespace ConsoleFramework.Xaml;

/// <summary>
/// Primitive types, such as strings, integers, can be specified in XAML
/// with a separate tag. But since primitives have no properties and Content-properties either,
/// for the convenience of writing a parser, setting such primitives is done through wrappers.
/// The wrapper class has a property of the corresponding primitive type, the user sets it
/// in XAML, and during processing the parser sees that this is exactly an IFactory object, and instead of the object itself
/// substitutes the result of calling the GetObject() method. As a result, the parent object
/// receives the value of the primitive, which can be set in various ways (including
/// using markup extensions).
/// </summary>
public interface IFactory
{
    object GetObject();
}

/// <summary>
/// Available in XAML markup as "string", "int", "double",
/// "float", "char", "bool" elements.
/// </summary>
class Primitive<T> : IFactory
{
    public T Content { get; set; }

    public object GetObject()
    {
        return Content;
    }
}
