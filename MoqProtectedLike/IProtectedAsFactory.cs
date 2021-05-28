using Moq;
using Moq.Protected;

namespace MoqProtectedLike
{
	public interface IProtectedAsFactory<T, TLike> where T : class where TLike : class
	{
		IProtectedAsMock<T, TLike> Create(Mock<T> mock);
	}

}
