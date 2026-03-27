using ConsoleFramework.Xaml;
using System.Collections.Generic;
using System.ComponentModel;
using Xunit;

namespace Tests.Xaml;

public class TestModel : INotifyPropertyChanged
{
    public string Title { get; set; }

    public TestModel SubModel { get; set; }

#pragma warning disable CS0067
    public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067
}

[ContentProperty("Content")]
[DataContextProperty("CustomDataContext")]
public class TestObject
{
    public object CustomDataContext { get; set; }

    public object Content { get; set; }
}

public class XamlDataContextTest
{
    [Fact]
    public void TestNestedDataContext()
    {
        TestModel rootContext = new()
        {
            Title = "Root",
            SubModel = new TestModel()
            {
                Title = "Nested"
            }
        };
        var obj = XamlParser.CreateFromXaml<TestObject>(
            @"
<test:TestObject xmlns:test=""clr-namespace:Tests.Xaml;assembly=Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null""
        xmlns:x=""http://consoleframework.org/xaml.xsd""
        CustomDataContext=""{Binding Path=SubModel, Mode=OneTime}"">
    <test:TestObject Content=""{Binding Path=Title, Mode=OneTime}""/>
</test:TestObject>
",
            rootContext,
            [
                    "clr-namespace:ConsoleFramework.Xaml;assembly=ConsoleFramework",
                    "clr-namespace:ConsoleFramework.XamlIntegration;assembly=ConsoleFramework"
            ]);
        // Value is bound from parent data context instead of root context
        Assert.Equal("Nested", ((TestObject)obj.Content).Content);
    }
}
