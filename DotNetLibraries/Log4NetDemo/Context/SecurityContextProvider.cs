namespace Log4NetDemo.Context
{
    public class SecurityContextProvider
    {
        private static SecurityContextProvider s_defaultProvider = new SecurityContextProvider();

        public static SecurityContextProvider DefaultProvider
        {
            get { return s_defaultProvider; }
            set { s_defaultProvider = value; }
        }

        protected SecurityContextProvider()
        {
        }

        public virtual SecurityContext CreateSecurityContext(object consumer)
        {
            return NullSecurityContext.Instance;
        }
    }
}
