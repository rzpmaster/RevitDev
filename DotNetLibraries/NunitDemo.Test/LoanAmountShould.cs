using NUnit.Framework;
using NunitDemo.Domain;
using NunitDemo.Domain.Application;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NunitDemo.Test
{
    /// <summary>
    /// ValueObject对象测试
    /// </summary>
    [TestFixture]
    public class LoanAmountShould
    {
        [Test]
        public void RespectValueEquality()
        {
            // 继承自ValueObject，只要他们的原子数据都指向同一个引用，那么他们相等
            var a = new LoanAmount("USD", 100m);
            var b = new LoanAmount("USD", 100m);

            // EqualTo 调用的是 Equal()方法
            Assert.That(a, Is.EqualTo(b));
        }

        [Test]
        public void RespectValueEquality2()
        {
            var a = new LoanAmount("USD", 100m);
            var b = new LoanAmount("USD", 100m);

            // == 操作符还是 object 的
            Assert.IsFalse(a == b);
        }

        [Test]
        public void RespectValueEquality3()
        {
            var a = new LoanAmount("USD", 100m);
            var b = new LoanAmount("USD", 100m);

            Assert.IsTrue(ValueObject.EqualOperator(a, b));
        }
    }
}
