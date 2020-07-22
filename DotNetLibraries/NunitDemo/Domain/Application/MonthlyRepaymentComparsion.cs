using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NunitDemo.Domain.Application
{
    public class MonthlyRepaymentComparsion
    {
        public string ProductName { get; private set; }
        public decimal InterestRate { get; private set; }
        public decimal Repayment { get; private set; }

        public MonthlyRepaymentComparsion(string productName, decimal interestRate, decimal repayment)
        {
            this.ProductName = productName;
            this.InterestRate = interestRate;
            this.Repayment = repayment;
        }
    }
}
