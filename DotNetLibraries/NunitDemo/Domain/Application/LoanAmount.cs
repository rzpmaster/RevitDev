using System.Collections.Generic;

namespace NunitDemo.Domain.Application
{
    /// <summary>
    /// 贷款规模
    /// </summary>
    public class LoanAmount : ValueObject
    {
        /// <summary>
        /// 货币类型
        /// </summary>
        public string CurrencyCode { get; }
        /// <summary>
        /// 本金
        /// </summary>
        public decimal Principal { get; }

        private LoanAmount() { }

        public LoanAmount(string currencyCode,decimal principal)
        {
            CurrencyCode = currencyCode;
            Principal = principal;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return CurrencyCode;
            yield return Principal;
        }
    }
}
