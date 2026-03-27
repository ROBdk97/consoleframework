using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ConsoleFramework.Xaml;

public class MarkupExtensionsParser(IMarkupExtensionsResolver resolver, string text)
{
    private readonly IMarkupExtensionsResolver resolver = resolver;
    private string text = text;
    private int index;

    private bool hasNextChar()
    {
        return index < text.Length;
    }

    private char consumeChar()
    {
        return text[index++];
    }

    private char peekNextChar()
    {
        return text[index];
    }

    public object ProcessMarkupExtension(IMarkupExtensionContext context)
    {
        // interpret as markup extension expression
        object result = processMarkupExtensionCore(context);
        if (result is IFixupToken) return result;

        if (hasNextChar())
        {
            throw new InvalidOperationException(
                string.Format("Syntax error: unexpected characters at {0}", index));
        }

        return result;
    }

    /// <summary>
    /// Consumes all whitespace characters. If necessary is true, at least one
    /// whitespace character should be consumed.
    /// </summary>
    private void processWhitespace(bool necessary = true)
    {
        if (necessary)
        {
            // at least one whitespace should be
            if (peekNextChar() != ' ')
                throw new InvalidOperationException(
                    string.Format("Syntax error: whitespace expected at {0}.", index));
        }
        while (peekNextChar() == ' ') consumeChar();
    }

    /// <summary>
    /// Recursive method. Consumes next characters as markup extension definition.
    /// Resolves type, ctor arguments and properties of markup extension,
    /// constructs and initializes it, and returns ProvideValue method result.
    /// </summary>
    /// <param name="context">Context object passed to ProvideValue method.</param>
    private object processMarkupExtensionCore(IMarkupExtensionContext context)
    {
        if (consumeChar() != '{')
            throw new InvalidOperationException("Syntax error: '{{' token expected at 0.");
        processWhitespace(false);
        string markupExtensionName = processQualifiedName();
        if (markupExtensionName.Length == 0)
            throw new InvalidOperationException("Syntax error: markup extension name is empty.");
        processWhitespace();

        Type type = resolver.Resolve(markupExtensionName);

        object obj = null;
        List<object> ctorArgs = [];

        for (; ; )
        {
            if (peekNextChar() == '{')
            {
                // inner markup extension processing

                // syntax error if ctor arg defined after any property
                if (obj != null)
                    throw new InvalidOperationException("Syntax error: constructor argument" +
                                                        " cannot be after property assignment.");

                object value = processMarkupExtensionCore(context);
                if (value is IFixupToken)
                    return value;
                ctorArgs.Add(value);
            }
            else
            {
                string membernameOrString = processString();

                if (membernameOrString.Length == 0)
                    throw new InvalidOperationException(
                        string.Format("Syntax error: member name or string expected at {0}",
                            index));

                if (peekNextChar() == '=')
                {
                    consumeChar();
                    object value = peekNextChar() == '{'
                        ? processMarkupExtensionCore(context)
                        : processString();

                    if (value is IFixupToken) return value;

                    // construct object if not constructed yet
                    obj ??= construct(type, ctorArgs);

                    // assign value to specified member
                    assignProperty(type, obj, membernameOrString, value);
                }
                else if (peekNextChar() == ',' || peekNextChar() == '}')
                {

                    // syntax error if ctor arg defined after any property
                    if (obj != null)
                        throw new InvalidOperationException("Syntax error: constructor argument" +
                                                            " cannot be after property assignment.");

                    // store membernameOrString as string argument of ctor
                    ctorArgs.Add(membernameOrString);

                }
                else
                {
                    // it is '{' token, throw syntax error
                    throw new InvalidOperationException(
                        string.Format("Syntax error : unexpected '{{' token at {0}.",
                        index));
                }
            }

            // after ctor arg or property assignment should be , or }
            if (peekNextChar() == ',')
            {
                consumeChar();
            }
            else if (peekNextChar() == '}')
            {
                consumeChar();

                // construct object
                obj ??= construct(type, ctorArgs);

                // markup extension is finished
                break;
            }
            else
            {
                // it is '{' token (without whitespace), throw syntax error
                throw new InvalidOperationException(
                    string.Format("Syntax error : unexpected '{{' token at {0}.",
                                   index));
            }

            processWhitespace(false);
        }

        return ((IMarkupExtension)obj).ProvideValue(context);
    }

    private static void assignProperty(Type type, object obj, string propertyName, object value)
    {
        PropertyInfo property = type.GetProperty(propertyName);
        property.SetValue(obj, value, null);
    }

    /// <summary>
    /// Constructs object of specified type using specified ctor arguments list.
    /// </summary>
    private static object construct(Type type, List<object> ctorArgs)
    {
        ConstructorInfo[] constructors = type.GetConstructors();
        List<ConstructorInfo> constructorInfos = constructors.Where(info => info.GetParameters().Length == ctorArgs.Count).ToList();
        if (constructorInfos.Count == 0)
        {
            throw new InvalidOperationException("No suitable constructor");
        }
        if (constructorInfos.Count > 1)
        {
            throw new InvalidOperationException("Ambiguous constructor call");
        }
        ConstructorInfo ctor = constructorInfos[0];
        ParameterInfo[] parameters = ctor.GetParameters();
        object[] convertedArgs = new object[ctorArgs.Count];
        for (int i = 0; i < parameters.Length; i++)
        {
            convertedArgs[i] = ctorArgs[i];
        }
        return ctor.Invoke(convertedArgs);
    }

    /// <summary>
    /// Returns a string that may contain any characters except {}, =.
    /// As soon as one of these characters is encountered without escaping with a backslash,
    /// parsing stops.
    /// </summary>
    private string processString()
    {
        StringBuilder sb = new();
        bool escaping = false;
        for (; ; )
        {
            if (!hasNextChar())
            {
                if (escaping) throw new InvalidOperationException("Invalid syntax.");
                break;
            }
            char c = peekNextChar();
            if (escaping)
            {
                sb.Append(c);
                consumeChar();
                escaping = false;
            }
            else
            {
                if (c == '\\')
                {
                    escaping = true;
                    consumeChar();
                }
                else
                {
                    if (c == '{' || c == '}' || c == ',' || c == '=')
                    {
                        // break without consuming it
                        break;
                    }
                    else
                    {
                        sb.Append(c);
                        consumeChar();
                    }
                }
            }
        }
        return sb.ToString();
    }

    private string processQualifiedName()
    {
        StringBuilder sb = new();
        for (; ; )
        {
            char c = peekNextChar();
            if (c != ':' && !char.IsLetterOrDigit(c))
            {
                break;
            }
            consumeChar();
            sb.Append(c);
        }
        return sb.ToString();
    }
}

