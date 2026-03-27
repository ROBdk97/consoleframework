using System;

namespace ConsoleFramework.Binding.Converters;

/// <summary>
/// Represents value conversion result.
/// </summary>
public class ConversionResult
{
    private readonly object value;
    private readonly bool success;
    private readonly string failReason;

    public object Value
    {
        get { return value; }
    }

    public bool Success
    {
        get { return success; }
    }

    public string FailReason
    {
        get { return failReason; }
    }

    public ConversionResult(object value)
    {
        this.value = value;
        success = true;
    }

    public ConversionResult(bool success, string failReason)
    {
        this.success = success;
        this.failReason = failReason;
    }
}

