using NUnit.Framework;
using NunitDemo.Domain.Application;

namespace NunitDemo.Test
{
    /// <summary>
    /// Entity对象测试
    /// </summary>
    [TestFixture]
    public class LoanProductShould
    {
        [Test]
        public void RespectValueEquality()
        {
            // 继承自Entity （同类型）只要id相同，就判定相同
            var a = new LoanProduct(1, "a", 1);
            var b = new LoanProduct(1, "b", 1);

            // EqualTo 调用的是 Equal()方法
            Assert.That(a, Is.EqualTo(b));
        }
    }
}
