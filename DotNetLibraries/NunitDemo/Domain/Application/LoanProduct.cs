namespace NunitDemo.Domain.Application
{
    /// <summary>
    /// 理财产品
    /// </summary>
    public class LoanProduct : Entity
    {
        /// <summary>
        /// 产品名称
        /// </summary>
        string _productName;
        /// <summary>
        /// 利率
        /// </summary>
        decimal _interestRate;

        protected LoanProduct() { }

        public LoanProduct(int id,string productName,decimal interestRate)
        {
            Id = id;
            _productName = productName;
            _interestRate = interestRate;
        }

        public string GetProductName()
        {
            return _productName;
        }

        public decimal GetInterestRate()
        {
            return _interestRate;
        }
    }
}
