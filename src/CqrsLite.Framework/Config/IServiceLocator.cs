using System;

namespace CqrsLite.Framework.Config
{
    public interface IServiceLocator 
	{
        T GetService<T>();
        object GetService(Type type);
    }
}