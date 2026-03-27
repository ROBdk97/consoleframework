using System;

namespace ConsoleFramework.Xaml;

/// <summary>
/// Attribute to specify name of content property of any type to be created
/// and configured using XAML if that type supports content abstraction.
/// If no attribute found at type, "Content" property will be used.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class ContentPropertyAttribute(string name) : Attribute
{
    public string Name { get; set; } = name;
}
