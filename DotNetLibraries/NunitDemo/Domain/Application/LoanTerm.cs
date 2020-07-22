using System;
using System.Collections.Generic;

namespace NunitDemo.Domain.Application
{
    /// <summary>
    /// 贷款期限
    /// </summary>
    public class LoanTerm : ValueObject
    {
        public int Years { get; }

        private LoanTerm() { }

        public LoanTerm(int years)
        {
            if (years < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(years), "Please speify a value greater than 1");
            }

            Years = years;
        }

        public int ToMonths() => Years * 12;

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Years;
        }
    }
}
