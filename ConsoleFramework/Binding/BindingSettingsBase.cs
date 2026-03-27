using ConsoleFramework.Binding.Adapters;
using ConsoleFramework.Binding.Converters;
using System;
using System.Collections.Generic;

namespace ConsoleFramework.Binding;

/// <summary>
/// Contains converters, validators and adapters.
/// </summary>
public class BindingSettingsBase
{
    public readonly static BindingSettingsBase DEFAULT_SETTINGS;

    static BindingSettingsBase()
    {
        DEFAULT_SETTINGS = new BindingSettingsBase();
        DEFAULT_SETTINGS.InitializeDefault();
    }

    private readonly Dictionary<Type, Dictionary<Type, IBindingConverter>> converters = [];
    private readonly Dictionary<Type, IBindingAdapter> adapters = [];

    public BindingSettingsBase()
    {
    }

    /// <summary>
    /// Adds default set of converters and ui adapters.
    /// </summary>
    public void InitializeDefault()
    {
        AddConverter(new StringToIntegerConverter());
    }

    public void AddAdapter(IBindingAdapter adapter)
    {
        Type targetClazz = adapter.TargetType;
        if (adapters.ContainsKey(targetClazz))
            throw new Exception($"Adapter for class {targetClazz.Name} is already registered.");
        adapters.Add(targetClazz, adapter);
    }

    public IBindingAdapter GetAdapterFor(Type clazz)
    {
        IBindingAdapter adapter = adapters[clazz];
        if (null == adapter) throw new Exception($"Adapter for class {clazz.Name} not found.");
        return adapter;
    }

    public void AddConverter(IBindingConverter converter)
    {
        RegisterConverter(converter);
        RegisterConverter(new ReversedConverter(converter));
    }

    private void RegisterConverter(IBindingConverter converter)
    {
        Type first = converter.FirstType;
        Type second = converter.SecondType;
        if (converters.TryGetValue(first, out Dictionary<Type, IBindingConverter> firstClassConverters))
        {
            if (firstClassConverters.ContainsKey(second))
            {
                throw new Exception(string.Format("Converter for {0} -> {1} classes is already registered.", first.Name, second.Name));
            }
            firstClassConverters.Add(second, converter);
        }
        else
        {
            firstClassConverters = new Dictionary<Type, IBindingConverter>
            {
                { second, converter }
            };
            converters.Add(first, firstClassConverters);
        }
    }

    public IBindingConverter GetConverterFor(Type first, Type second)
    {
        if (!converters.TryGetValue(first, out Dictionary<Type, IBindingConverter> firstClassConverters))
            return null;
        if (!firstClassConverters.TryGetValue(second, out IBindingConverter value))
            return null;
        return value;
    }
}
