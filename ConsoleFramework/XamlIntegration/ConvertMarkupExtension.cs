using ConsoleFramework.Binding.Converters;
using ConsoleFramework.Xaml;
using System;

namespace ConsoleFramework.XamlIntegration;

/// <summary>
/// Converts Value to property type using the specified converter.
/// </summary>
[MarkupExtension("Convert")]
public class ConvertMarkupExtension : IMarkupExtension
{
    /// <summary>Converter to be used.</summary>
    public IBindingConverter? Converter { get; set; }

    /// <summary>Value to convert. string or any object (if created using a nested markup extension).</summary>
    public object? Value { get; set; }

    public object? ProvideValue(IMarkupExtensionContext context)
    {
        if (Converter is null) throw new InvalidOperationException("Converter is null");
        if (Value is null) return null;

        var propertyInfo = context.Object.GetType().GetProperty(context.PropertyName)
            ?? throw new InvalidOperationException($"Property '{context.PropertyName}' not found.");
        var propertyType = propertyInfo.PropertyType;
        var valueType = Value.GetType();
        var firstType = Converter.FirstType;
        var secondType = Converter.SecondType;

        if (firstType.IsAssignableFrom(propertyType) && secondType.IsAssignableFrom(valueType))
        {
            var result = Converter.ConvertBack(Value);
            if (!result.Success)
                throw new InvalidOperationException($"Cannot convert value: {result.FailReason}");
            return result.Value;
        }

        if (firstType.IsAssignableFrom(valueType) && secondType.IsAssignableFrom(propertyType))
        {
            var result = Converter.Convert(Value);
            if (!result.Success)
                throw new InvalidOperationException($"Cannot convert value: {result.FailReason}");
            return result.Value;
        }

        throw new InvalidOperationException(
            $"Cannot use specified converter to convert {valueType.Name} to {propertyType.Name}");
    }
}
