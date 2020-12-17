using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;
using Unity;

namespace MvvmTools.Utilities
{
    public class UnityServiceLocator : ServiceLocatorImplBase
    {
        private readonly IUnityContainer _container;

        public UnityServiceLocator(IUnityContainer container)
        {
            _container = container;
        }

        protected override object DoGetInstance(Type serviceType, string key)
        {
            // key == null must be specifically handled as not asking for a specific keyed instance
            // http://commonservicelocator.codeplex.com/wikipage?title=API%20Reference&referringTitle=Home
            //     The implementation should be designed to expect a null for the string key parameter, 
            //     and MUST interpret this as a request to get the "default" instance for the requested 
            //     type. This meaning of default varies from locator to locator.
            if (key == null)
                return _container.Resolve(serviceType);
            return _container.Resolve(serviceType, key);
        }

        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            return _container.ResolveAll(serviceType);
        }
    }
}
