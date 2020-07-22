using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NunitDemo.Domain.Application
{
    public class ProductComparer
    {
        readonly LoanAmount _loanAmount;
        readonly List<LoanProduct> _productsToComparer;

        public ProductComparer(LoanAmount loanAmount, List<LoanProduct> productsToComparer)
        {
            _loanAmount = loanAmount;
            _productsToComparer = productsToComparer;
        }

        public List<MonthlyRepaymentComparsion> CompareMonthlyRepayments(LoanTerm loanTerm)
        {
            var calculator = new LoanRepaymentCalculator();
            var compared = new List<MonthlyRepaymentComparsion>();

            foreach (var product in _productsToComparer)
            {
                decimal repayment = calculator.CalculatorMonthlyRepayment(_loanAmount, product.GetInterestRate(), loanTerm);
                compared.Add(new MonthlyRepaymentComparsion(product.GetProductName(), product.GetInterestRate(), repayment));
            }

            return compared;
        }
    }
}
