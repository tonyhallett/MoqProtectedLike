using Moq;
using Moq.Protected;

namespace MoqProtectedLike
{
	internal class ProtectedAsFactory<T, TLike> : IProtectedAsFactory<T, TLike> where T : class where TLike : class
	{
		public IProtectedAsMock<T, TLike> Create(Mock<T> mock)
		{
			return mock.Protected().As<TLike>();
		}
	}

}
