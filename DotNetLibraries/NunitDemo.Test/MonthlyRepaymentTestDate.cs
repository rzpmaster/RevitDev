using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;

namespace NunitDemo.Test
{
    public class MonthlyRepaymentTestDate
    {
        public static IEnumerable TestCases
        {
            get
            {
                yield return new TestCaseData(200_000m, 6.5m, 30, 390000);
                yield return new TestCaseData(200_000m, 10m, 30, 390000);
                yield return new TestCaseData(500_000m, 10m, 30, 390000);
            }
        }
    }

    public class MonthlyRepaymentTestDateWithReture
    {
        public static IEnumerable TestCases
        {
            get
            {
                yield return new TestCaseData(200_000m, 6.5m, 30).Returns(390000);
                yield return new TestCaseData(200_000m, 10m, 30).Returns(390000);
                yield return new TestCaseData(500_000m, 10m, 30).Returns(390000);
            }
        }
    }

    public class MonthlyRepaymentTestDateCsv
    {
        public static IEnumerable GetTestCases(string csvFilePath)
        {
            var csvLines = File.ReadAllLines(csvFilePath);

            var testCases = new List<TestCaseData>();

            foreach (var line in csvLines)
            {
                string[] values = line.Replace(" ", "").Split(',');

                decimal principle = decimal.Parse(values[0]);
                decimal interestRate = decimal.Parse(values[1]);
                int termYears = int.Parse(values[2]);
                decimal expectedRepayment = decimal.Parse(values[3]);

                testCases.Add(new TestCaseData(principle, interestRate, termYears, expectedRepayment));
            }

            return testCases;
        }
    }
}
