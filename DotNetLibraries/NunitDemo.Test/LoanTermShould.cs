using NUnit.Framework;
using NunitDemo.Domain.Application;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace NunitDemo.Test
{
    [TestFixture]
    public class LoanTermShould
    {
        [Test]
        public void ReturnTermInMonths()
        {
            var sut = new LoanTerm(1);

            Assert.That(sut.ToMonths, Is.EqualTo(12),"Month should be 12 * numbers of years");
        }

        [Test]
        public void StoreYears()
        {
            var sut = new LoanTerm(1);

            Assert.That(sut.Years, Is.EqualTo(1));
        }

        [Test]
        public void RespectValueEquality()
        {
            //var a = 1;
            //var b = 1;

            var a = new LoanTerm(1);
            var b = new LoanTerm(1);

            // EqualTo 调用的是 Equal()方法
            Assert.That(a, Is.EqualTo(b));
        }

        [Test]
        public void RespectValueInequality()
        {
            var a = new LoanTerm(1);
            var b = new LoanTerm(2);

            Assert.That(a, Is.Not.EqualTo(b));
        }

        [Test]
        public void ReferenceEqualityExample()
        {
            var a = new LoanTerm(1);
            var b = a;
            var c = new LoanTerm(1);

            // SameAs 比较的是reference
            Assert.That(a, Is.SameAs(b));
            Assert.That(a, Is.Not.SameAs(c));

            var x = new List<string> { "a", "b" };
            var y = x;
            var z = new List<string> { "a", "b" };

            Assert.That(x, Is.SameAs(y));
            Assert.That(x, Is.Not.SameAs(z));
        }

        [Test]
        public void Double()
        {
            double a = 1.0 / 3.0;

            //Assert.That(a, Is.EqualTo(0.33)); //failed
            Assert.That(a, Is.EqualTo(0.33).Within(0.004));
            Assert.That(a, Is.EqualTo(0.33).Within(10).Percent);
        }
    }
}
