using ConsoleFramework.Xaml;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit;

namespace Tests.Xaml.EnumsTest;
    public enum MyEnumeration
    {
        Variant1,
        Variant2
    }

    public class ObjectToCreate
    {
        public MyEnumeration MyEnum
        {
            get;
            set;
        }
    }

    public class EnumsTest
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
            string xaml = loadResource("Tests.Xaml.EnumsTest.ObjectToCreate.xml");
            ObjectToCreate createdObject = XamlParser.CreateFromXaml<ObjectToCreate>(xaml, null, [
                "clr-namespace:Tests.Xaml.EnumsTest;assembly=Tests"
            ]);
            Assert.True(createdObject.MyEnum == MyEnumeration.Variant2);
        }
    }
