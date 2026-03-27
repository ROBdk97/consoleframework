using System;

namespace ConsoleFramework.Xaml
{
    public interface IMarkupExtensionsResolver
    {
        Type Resolve(string name);
    }
}

