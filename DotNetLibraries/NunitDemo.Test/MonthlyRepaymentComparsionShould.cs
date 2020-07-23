using NUnit.Framework;
using NunitDemo.Domain.Application;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NunitDemo.Test
{
    [TestFixture]
    [Category("Product Comparison")]
    public class MonthlyRepaymentComparsionShould
    {
        [Test]
        [Category("Product Comparison")]
        [Category("xyz")]
        public void RespectValueEquality()
        {
            var a = new MonthlyRepaymentComparsion("a", 42.42m, 22.22m);
            var b = new MonthlyRepaymentComparsion("a", 42.42m, 22.22m);

            Assert.That(a, Is.EqualTo(b));
        }

        [Test]
        [Category("xyz")]
        public void RespectValueInequality()
        {
            var a = new MonthlyRepaymentComparsion("a", 42.42m, 22.22m);
            var b = new MonthlyRepaymentComparsion("a", 42.43m, 22.22m);

            Assert.That(a, Is.Not.EqualTo(b));
        }
    }
}
