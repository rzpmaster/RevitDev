using NUnit.Framework;
using NunitDemo.Domain.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NunitDemo.Test
{
    [TestFixture]
    [Category("Product Comparison")]
    public class ProductComparerShould
    {
        List<LoanProduct> products;
        ProductComparer sut;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // 方法开始只执行一次
            products = new List<LoanProduct>
            {
                new LoanProduct(1,"a",1),
                new LoanProduct(2,"b",2),
                new LoanProduct(3,"c",3),
                //new LoanProduct(3,"c",3),
            };
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            // 方法结束只执行一次
        }

        [SetUp]
        public void SetUp()
        {
            // 每次测试之前都会执行
            sut = new ProductComparer(new LoanAmount("USD", 200_000m), products);
        }

        [TearDown]
        public void TearDown()
        {
            // 每次测试之后都会执行
            // Run after each test executes
            // sut.Dispose();
        }

        [Test]
        [Category("Product Comparison")]
        public void ReturnCorrectNumberOfComparisons()
        {
            List<MonthlyRepaymentComparsion> comparsions = sut.CompareMonthlyRepayments(new LoanTerm(30));

            Assert.That(comparsions, Has.Exactly(3).Items);
        }

        [Test]
        public void NotReturnDuplicateComparisons()
        {
            List<MonthlyRepaymentComparsion> comparsions = sut.CompareMonthlyRepayments(new LoanTerm(30));

            Assert.That(comparsions, Is.Unique);
        }

        [Test]
        public void ReturnComparisonForFirstProduct()
        {
            List<MonthlyRepaymentComparsion> comparsions = sut.CompareMonthlyRepayments(new LoanTerm(30));

            var repayment = new LoanRepaymentCalculator().CalculatorMonthlyRepayment(new LoanAmount("USD", 200_000m), 1, new LoanTerm(30));
            var expectedProduct = new MonthlyRepaymentComparsion("a", 1, repayment);

            // Assert.That(comparsions, Does.Contain(comparsions[0]));
            Assert.That(comparsions, Does.Contain(expectedProduct));
        }

        [Test]
        public void ReturnComparisonForFirstProduct_WithPartialKnownExpectedValues()
        {
            List<MonthlyRepaymentComparsion> comparsions = sut.CompareMonthlyRepayments(new LoanTerm(30));

            Assert.That(comparsions, Has.Exactly(1)
                                        .Property("ProductName").EqualTo("a")
                                        .And
                                        .Property("InterestRate").EqualTo(1)
                                        .And
                                        .Property("Repayment").GreaterThan(50000m));
            Assert.That(comparsions, Has.Exactly(1)
                                        .Matches<MonthlyRepaymentComparsion>(
                                                item => item.ProductName == "a" &&
                                                        item.InterestRate == 1 &&
                                                        item.Repayment > 50000m));
        }

        [Test]
        public void NotAllowZeroYears()
        {
            Assert.That(() => new LoanTerm(0), Throws.TypeOf<ArgumentOutOfRangeException>());

            //Assert.That(() => new LoanTerm(0), Throws.TypeOf<ArgumentOutOfRangeException>()
            //                    .With
            //                    .Property("Message")
            //                    .EqualTo("Please specify a value greater than 0"));   //failed

            Assert.That(() => new LoanTerm(0), Throws.TypeOf<ArgumentOutOfRangeException>()
                                .With
                                .Property("Message")
                                .EqualTo("Please specify a value greater than 0\r\n参数名: years"));

            Assert.That(() => new LoanTerm(0), Throws.TypeOf<ArgumentOutOfRangeException>()
                                .With
                                .Message
                                .EqualTo("Please specify a value greater than 0\r\n参数名: years"));

            Assert.That(() => new LoanTerm(0), Throws.TypeOf<ArgumentOutOfRangeException>()
                                .With
                                .Property("ParamName")
                                .EqualTo("years"));

            Assert.That(() => new LoanTerm(0), Throws.TypeOf<ArgumentOutOfRangeException>()
                                .With
                                .Matches<ArgumentOutOfRangeException>(
                                    ex => ex.ParamName == "years"));
        }

        [Test]
        public void OtherAssertings()
        {
            /// string

            string name = "Sarah";

            //Assert.That(name, Is.Null);
            Assert.That(name, Is.Not.Null);

            //Assert.That(name, Is.Empty);
            Assert.That(name, Is.Not.Empty);

            Assert.That(name, Is.EqualTo("Sarah"));
            Assert.That(name, Is.EqualTo("SARAH").IgnoreCase);

            Assert.That(name, Does.StartWith("Sa"));
            Assert.That(name, Does.EndWith("ah"));
            Assert.That(name, Does.Contain("ara"));
            Assert.That(name, Does.Not.Contain("Amrit"));
            Assert.That(name, Does.StartWith("Sa")
                                    .And
                                    .EndWith("ah"));
            Assert.That(name, Does.StartWith("xyz")
                                    .Or
                                    .EndWith("rah"));

            /// bool

            bool isNew = true;

            Assert.That(isNew);
            Assert.That(isNew, Is.True);

            /// int

            int i = 42;

            Assert.That(i, Is.GreaterThan(40));
            Assert.That(i, Is.GreaterThanOrEqualTo(42));
            Assert.That(i, Is.LessThan(50));
            Assert.That(i, Is.LessThanOrEqualTo(42));
            Assert.That(i, Is.InRange(40, 50));

            /// DateTime

            DateTime d1 = new DateTime(2020, 7, 23);
            DateTime d2 = new DateTime(2020, 7, 24);

            Assert.That(d1, Is.EqualTo(d2).Within(4).Days);
        }
    }
}
