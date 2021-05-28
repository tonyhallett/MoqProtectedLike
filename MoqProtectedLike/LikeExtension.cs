using Moq;

namespace MoqProtectedLike
{
    public static class LikeExtension
    {
        public static IsProtected<T> IsProtected<T>(this Mock<T> mock) where T :class
        {
            return new IsProtected<T>(mock);
        }
    }
}
