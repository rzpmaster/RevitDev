using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjection
{
    public class ServiceProviderOptions
    {
        internal static readonly ServiceProviderOptions Default = new ServiceProviderOptions();

        /// <summary>
        /// <c>true</c> to perform check verifying that scoped services never gets resolved from root provider; 
        /// otherwise <c>false</c>.
        /// </summary>
        public bool ValidateScopes { get; set; }

        internal ServiceProviderMode Mode { get; set; } = ServiceProviderMode.Dynamic;
    }
}
