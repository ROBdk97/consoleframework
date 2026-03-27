using ConsoleFramework.Xaml;
using System;
using Xunit;

namespace Tests.Xaml;

public class MarkupExtensionsTest
{
    public class TestExtension : IMarkupExtension
    {
        public TestExtension()
        {
            Property1 = string.Empty;
            Property2 = string.Empty;
            Property3 = string.Empty;
        }

        public TestExtension(string param1)
        {
            Property1 = param1;
            Property2 = string.Empty;
            Property3 = string.Empty;
        }

        public TestExtension(string param1, string param2)
        {
            Property1 = param1;
            Property2 = param2;
            Property3 = string.Empty;
        }

        public string Property1 { get; set; }

        public string Property2 { get; set; }

        public string Property3 { get; set; }

        public object ProvideValue(IMarkupExtensionContext context)
        {
            return Property1 + "_" + Property2 + "_" + Property3;
        }
    }

    public class TestResolver : IMarkupExtensionsResolver
    {
        public Type Resolve(string name)
        {
            return typeof(TestExtension);
        }
    }

    [Fact]
    public void TestEscaping()
    {
        MarkupExtensionsParser parser = new(new TestResolver(),
            "{}Just a string{{}}");
        // should be thrown syntax error: markup extension name is empty
        Assert.Throws<InvalidOperationException>(() =>
        {
            parser.ProcessMarkupExtension(null);
        });
    }

    [Fact]
    public void TestEscaping2()
    {
        MarkupExtensionsParser parser = new(new TestResolver(),
            @"{xm:TestExtension Arg1, Arg2, Property3=\=\{\}\\sdf}");
        string result = (string)parser.ProcessMarkupExtension(null);
        Assert.Equal(@"Arg1_Arg2_={}\sdf", result);
    }

    [Fact]
    public void TestInner()
    {
        MarkupExtensionsParser parser = new(new TestResolver(),
            @"{xm:TestExtension Arg1, {TestExtension Property1=1}, Property3=\=\{\}\\sdf}");
        string result = (string)parser.ProcessMarkupExtension(null);
        Assert.Equal(@"Arg1_1___={}\sdf", result);
    }

    [Fact]
    public void TestInner2()
    {
        MarkupExtensionsParser parser = new(new TestResolver(),
            @"{xm:TestExtension Arg1, Property3=\=\{\}\\sdf, Property2={TestExtension Property1=1}}");
        string result = (string)parser.ProcessMarkupExtension(null);
        Assert.Equal(@"Arg1_1___={}\sdf", result);
    }

    [Fact]
    public void TestSyntaxError1()
    {
        MarkupExtensionsParser parser = new(new TestResolver(), "{ }");
        // should be thrown syntax error: markup extension name is empty
        Assert.Throws<InvalidOperationException>(() =>
        {
            parser.ProcessMarkupExtension(null);
        });
    }

    [Fact]
    public void TestSyntaxError2()
    {
        MarkupExtensionsParser parser = new(new TestResolver(),
            "{TestExtension* }");
        // should be thrown syntax error: whitespace expected after name
        Assert.Throws<InvalidOperationException>(() =>
        {
            parser.ProcessMarkupExtension(null);
        });
    }

    [Fact]
    public void TestSyntaxError3()
    {
        MarkupExtensionsParser parser = new(new TestResolver(),
            "{TestExtension Property1=1, CtorArg }");
        // should be thrown syntax error: constructor argument cannot be after property assignment
        Assert.Throws<InvalidOperationException>(() =>
        {
            parser.ProcessMarkupExtension(null);
        });
    }

    [Fact]
    public void TestSyntaxError4()
    {
        MarkupExtensionsParser parser = new(new TestResolver(),
            "{TestExtension CtorArg, Property1=1 } ");
        // should be thrown syntax error: unexpected characters
        Assert.Throws<InvalidOperationException>(() =>
        {
            parser.ProcessMarkupExtension(null);
        });
    }

    [Fact]
    public void TestSyntaxError5()
    {
        MarkupExtensionsParser parser = new(new TestResolver(),
            "{TestExtension CtorArg, Property1=1,}");
        // should be thrown syntax error: member name or string expected
        Assert.Throws<InvalidOperationException>(() =>
        {
            parser.ProcessMarkupExtension(null);
        });
    }
}