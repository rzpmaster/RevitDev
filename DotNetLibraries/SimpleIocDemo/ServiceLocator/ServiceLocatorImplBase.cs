using System;
using System.Collections.Generic;
using SimpleIocDemo.Exceptions;

namespace SimpleIocDemo.ServiceLocator
{
    public abstract class ServiceLocatorImplBase : IServiceLocator, IServiceProvider
    {
        #region System.IServiceProvider

        public virtual object GetService(Type serviceType)
        {
            return GetInstance(serviceType, null);
        }

        #endregion

        public virtual object GetInstance(Type serviceType)
        {
            return GetInstance(serviceType, null);
        }

        public virtual object GetInstance(Type serviceType, string key)
        {
            try
            {
                return DoGetInstance(serviceType, key);
            }
            catch (Exception ex)
            {
                throw new ActivationException(
                    FormatActivationExceptionMessage(ex, serviceType, key),
                    ex);
            }
        }

        public virtual IEnumerable<object> GetAllInstances(Type serviceType)
        {
            try
            {
                return DoGetAllInstances(serviceType);
            }
            catch (Exception ex)
            {
                throw new ActivationException(
                    FormatActivateAllExceptionMessage(ex, serviceType),
                    ex);
            }
        }

        public virtual TService GetInstance<TService>()
        {
            return (TService)GetInstance(typeof(TService), null);
        }

        public virtual TService GetInstance<TService>(string key)
        {
            return (TService)GetInstance(typeof(TService), key);
        }

        public virtual IEnumerable<TService> GetAllInstances<TService>()
        {
            foreach (object item in GetAllInstances(typeof(TService)))
            {
                yield return (TService)item;
            }
        }

        #region protected Methods

        protected abstract object DoGetInstance(Type serviceType, string key);

        protected abstract IEnumerable<object> DoGetAllInstances(Type serviceType);

        protected virtual string FormatActivationExceptionMessage(Exception actualException, Type serviceType, string key)
        {
            return $"Activation error occurred while trying to get instance of type {serviceType.Name}, key \"{key}\"";
        }

        protected virtual string FormatActivateAllExceptionMessage(Exception actualException, Type serviceType)
        {
            return $"Activation error occurred while trying to get all instances of type {serviceType.Name}";
        }

        #endregion
    }
}
