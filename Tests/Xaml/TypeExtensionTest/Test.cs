using ConsoleFramework.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit;

namespace Tests.Xaml.TypeExtensionTest;
    public class ObjectToCreate
    {
        public Type Type
        {
            get;
            set;
        }
    }

    public class Test
    {
        private static string loadResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
        using Stream stream = assembly.GetManifestResourceStream(resourceName);
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

        [Fact]
        public void test()
        {
            string xaml = loadResource("Tests.Xaml.TypeExtensionTest.object.xml");
            ObjectToCreate createdObject = XamlParser.CreateFromXaml<ObjectToCreate>(xaml, null, [
                "clr-namespace:Tests.Xaml.TypeExtensionTest;assembly=Tests"
            ]);
            Assert.True(createdObject.Type == typeof(ObjectToCreate));
        }
    }
