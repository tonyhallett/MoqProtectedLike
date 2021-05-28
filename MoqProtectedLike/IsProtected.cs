using Moq;

namespace MoqProtectedLike
{
    public class IsProtected<T> where T : class
    {
        private readonly Mock<T> mock;
        public IsProtected(Mock<T> mock)
        {
            this.mock = mock;
        }
        public IProtectedLike<T, TLike> Like<TLike>() where TLike : class
        {
            return new Like<T, TLike>(mock);
        }
    }
}
