using Moq;
using Moq.Language.Flow;
using Moq.Protected;
using System;
using System.Linq.Expressions;

namespace MoqProtectedLike
{
    public interface IProtectedLike<T,TLike>:IProtectedAsMock<T,TLike> where T : class where TLike : class
    {
        T Object { get; }
        ISetup<T> SetupSet(Action<TLike> setup);
        ISetupSetter<T, TProperty> SetupSet<TProperty>(Action<TLike> setup);
        void VerifySet(Action<TLike> setup, Times? times = null, string failMessage = null);
    }
}
