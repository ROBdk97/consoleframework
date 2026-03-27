using System;

namespace ConsoleFramework.Binding;

/// <summary>
/// Represents result of one synchronization operation from Target to Source.
/// If hasConversionError is true, message will represent conversion error message.
/// If hasValidationError is true, message will represent validation error message.
/// Both hasConversionError and hasValidationError cannot be set to true.
/// </summary>
public class BindingResult
{
    public bool hasError;
    public bool hasConversionError;
    public bool hasValidationError;
    public string message;

    public BindingResult(bool hasError)
    {
        this.hasError = hasError;
    }

    public BindingResult(bool hasConversionError, bool hasValidationError, string message)
    {
        this.hasConversionError = hasConversionError;
        this.hasValidationError = hasValidationError;
        hasError = hasConversionError || hasValidationError;
        this.message = message;
    }
}

