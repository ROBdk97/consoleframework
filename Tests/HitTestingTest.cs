using ConsoleFramework.Controls;
using Xunit;

namespace Tests;
    public class HitTestingTest
    {
        class TestControl : Control
        {
            public new void AddChild(Control control)
            {
                base.AddChild(control);
            }
        }

        [Fact]
        public void TestControlsDoesntLinkedToCanvas()
        {
            TestControl a = new();
            TestControl b = new();
            Assert.Null(Control.FindCommonAncestor(a, b));
        }

        [Fact]
        public void TestRootCanvasIsCommonAncestor()
        {
            TestControl a = new();
            TestControl b = new();
            TestControl aa = new();
            a.AddChild(aa);
            TestControl bb = new();
            b.AddChild(bb);
            Control commonAncestor = Control.FindCommonAncestor(aa, bb);
            Control commonAncestor2 = Control.FindCommonAncestor(bb, aa);
            Assert.Null(commonAncestor);
            Assert.Null(commonAncestor2);
        }

        [Fact]
        public void TestSelfIsCommonAncestor()
        {
            Control a = new();
            Control commonAncestor = Control.FindCommonAncestor(a, a);
            Assert.Equal(commonAncestor, a);
        }

        [Fact]
        public void TestNormalSituation()
        {
            //
            TestControl x = new() { Name = "x" };
            TestControl ancestor = new() { Name = "ancestor" };
            x.AddChild(ancestor);
            TestControl a = new() { Name = "a" };
            ancestor.AddChild(a);
            TestControl aa = new() { Name = "aa" };
            a.AddChild(aa);
            TestControl aaa = new() { Name = "aaa" };
            aa.AddChild(aaa);
            TestControl b = new() { Name = "b" };
            ancestor.AddChild(b);
            Assert.Equal(Control.FindCommonAncestor(a, b), ancestor);
            Assert.Equal(Control.FindCommonAncestor(aa, b), ancestor);
            TestControl bb = new() { Name = "bb" };
            b.AddChild(bb);
            Assert.Equal(Control.FindCommonAncestor(aa, bb), ancestor);
            //
            Assert.Equal(Control.FindCommonAncestor(a, aa), a);
            Assert.Equal(Control.FindCommonAncestor(aa, ancestor), ancestor);
            Assert.Equal(Control.FindCommonAncestor(b, bb), b);
            Assert.Equal(Control.FindCommonAncestor(bb, ancestor), ancestor);
            //
            Assert.Equal(Control.FindCommonAncestor(aaa, ancestor), ancestor);
        }
    }
