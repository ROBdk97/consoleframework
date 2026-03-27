namespace ConsoleFramework.Binding.Validators;

/// <summary>
/// Validator checks the value is not null or empty (if value string).
/// </summary>
public class RequiredValidator : IBindingValidator
{
    public ValidationResult Validate(object value)
    {
        if (value is null || (value is string s && s.Length == 0))
            return new ValidationResult(false, "Value is required");
        return new ValidationResult(true);
    }
}
