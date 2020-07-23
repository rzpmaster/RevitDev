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
    public class LoanRepaymentCalculatorShould
    {
        [Test]
        public void CalculateCorrectMonthlyRepayment()
        {
            var sut = new LoanRepaymentCalculator();

            var monthlyPayment = sut.CalculatorMonthlyRepayment(
                                        new LoanAmount("USD", 200_000), 6.5m, new LoanTerm(30));

            Assert.That(monthlyPayment, Is.EqualTo(390000));
        }

        [Test]
        public void CalculateCorrectMonthlyRepayment_10Percent()
        {
            var sut = new LoanRepaymentCalculator();

            var monthlyPayment = sut.CalculatorMonthlyRepayment(
                                        new LoanAmount("USD", 200_000), 10m, new LoanTerm(30));

            Assert.That(monthlyPayment, Is.EqualTo(390000));
        }

        [Test]
        [TestCase(200_000, 6.5, 30, 390000)]
        [TestCase(200_000, 10, 30, 390000)]
        [TestCase(500_000, 10, 30, 390000)]
        public void CalculateCorrectMonthlyRepayment_Param(decimal principal,
                                                           decimal interestRate,
                                                           int termYears,
                                                           decimal expectedMontlyPayment)
        {
            var sut = new LoanRepaymentCalculator();

            var monthlyPayment = sut.CalculatorMonthlyRepayment(
                                        new LoanAmount("USD", principal), interestRate, new LoanTerm(termYears));

            Assert.That(monthlyPayment, Is.EqualTo(expectedMontlyPayment));
        }

        [Test]
        [TestCase(200_000, 6.5, 30, ExpectedResult = 390000)]
        [TestCase(200_000, 10, 30, ExpectedResult = 390000)]
        [TestCase(500_000, 10, 30, ExpectedResult = 390000)]
        public decimal CalculateCorrectMonthlyRepayment_Param_Reture(decimal principal,
                                                                     decimal interestRate,
                                                                     int termYears)
        {
            var sut = new LoanRepaymentCalculator();

            return sut.CalculatorMonthlyRepayment(
                                        new LoanAmount("USD", principal), interestRate, new LoanTerm(termYears));
        }

        [Test]
        [TestCaseSource(typeof(MonthlyRepaymentTestDate), "TestCases")]
        public void CalculateCorrectMonthlyRepayment_Param_Data(decimal principal,
                                                           decimal interestRate,
                                                           int termYears,
                                                           decimal expectedMontlyPayment)
        {
            var sut = new LoanRepaymentCalculator();

            var monthlyPayment = sut.CalculatorMonthlyRepayment(
                                        new LoanAmount("USD", principal), interestRate, new LoanTerm(termYears));

            Assert.That(monthlyPayment, Is.EqualTo(expectedMontlyPayment));
        }

        [Test]
        [TestCaseSource(typeof(MonthlyRepaymentTestDateWithReture), "TestCases")]
        public decimal CalculateCorrectMonthlyRepayment_Param_Reture_Data(decimal principal,
                                                                     decimal interestRate,
                                                                     int termYears)
        {
            var sut = new LoanRepaymentCalculator();

            return sut.CalculatorMonthlyRepayment(
                                        new LoanAmount("USD", principal), interestRate, new LoanTerm(termYears));
        }

        [Test]
        [TestCaseSource(typeof(MonthlyRepaymentTestDateCsv), "GetTestCases", new object[] { "Data.csv" })]
        public void CalculateCorrectMonthlyRepayment_Param_CsvData(decimal principal,
                                                           decimal interestRate,
                                                           int termYears,
                                                           decimal expectedMontlyPayment)
        {
            var sut = new LoanRepaymentCalculator();

            var monthlyPayment = sut.CalculatorMonthlyRepayment(
                                        new LoanAmount("USD", principal), interestRate, new LoanTerm(termYears));

            Assert.That(monthlyPayment, Is.EqualTo(expectedMontlyPayment));
        }

        [Test]
        public void CalculateCorrectMonthlyRepayment_Combinatorial(
            [Values(10000, 20000, 30000)] decimal principal,
            [Values(6.5, 10, 20)] decimal interestRate,
            [Values(10, 20, 30)] int termYears)
        {
            // 27种结果
            var sut = new LoanRepaymentCalculator();

            var monthlyPayment = sut.CalculatorMonthlyRepayment(
                                        new LoanAmount("USD", principal), interestRate, new LoanTerm(termYears));
        }

        [Test]
        [Sequential]
        public void CalculateCorrectMonthlyRepayment_Sequential(
            [Values(10000, 20000, 30000)] decimal principal,
            [Values(6.5, 10, 20)] decimal interestRate,
            [Values(10, 20, 30)] int termYears,
            [Values(390000,390000,390000)] decimal expectedMontlyPayment)
        {
            // 3中结果
            var sut = new LoanRepaymentCalculator();

            var monthlyPayment = sut.CalculatorMonthlyRepayment(
                                        new LoanAmount("USD", principal), interestRate, new LoanTerm(termYears));

            Assert.That(monthlyPayment, Is.EqualTo(expectedMontlyPayment));
        }

        [Test]
        public void CalculateCorrectMonthlyRepayment_Range(
            [Range(50000, 10000000, 50000)] decimal principal,
            [Range(0.5, 20, 0.5)] decimal interestRate,
            [Values(10, 20, 30)] int termYears)
        {
            // 2400种
            var sut = new LoanRepaymentCalculator();

            var monthlyPayment = sut.CalculatorMonthlyRepayment(
                                        new LoanAmount("USD", principal), interestRate, new LoanTerm(termYears));
        }
    }
}
