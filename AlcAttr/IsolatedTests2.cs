using ClassLibrary2;
using NUnit.Framework;
using Shouldly;

namespace AlcAttr
{
    [IsolatedTestFixture]
    public sealed class IsolatedTests2
    {
        [Test]
        public void TestSharedVariableIncrement()
        {
            Class1.Increment(5);
            Class1.SharedVariable.ShouldBe(5);
        }

    }
}