namespace NunitDemo.Domain.Application
{
    public class LoanRepaymentCalculator
    {
        public decimal CalculatorMonthlyRepayment(LoanAmount loanAmount, decimal annualInterestRate, LoanTerm loanTermm)
        {
            // 月收益=月利率*时间*本金
            var monthly = (double)annualInterestRate / 100 / 12 * (double)loanAmount.Principal * loanTermm.ToMonths();
            return decimal.Parse(monthly.ToString());
        }
    }
}
