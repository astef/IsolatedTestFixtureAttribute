using ClassLibrary2;
using NUnit.Framework;
using Shouldly;

namespace AlcAttr
{
    [TestFixture]
    public sealed class IsolatedTests1
    {
        [Test]
        public void TestSharedVariableIncrement()
        {
            Class1.Increment(5);
            Class1.SharedVariable.ShouldBe(5);
        }

    }
}
