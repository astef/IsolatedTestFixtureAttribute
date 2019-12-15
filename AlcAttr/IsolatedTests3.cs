using ClassLibrary2;
using NUnit.Framework;
using Shouldly;

namespace AlcAttr
{
    [IsolatedTestFixture]
    public sealed class IsolatedTests3
    {
        [TestCase(2)]
        [TestCase(4)]
        [TestCase(8)]
        public void TestSharedVariableIncrement(int n)
        {
            Class1.Increment(n);
            Class1.SharedVariable.ShouldBe(n);
        }

    }
}