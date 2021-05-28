using System.Reflection;

namespace MoqProtectedLike
{
    public static class ReflectionExtensions
    {
        public static bool IsProtected(this MethodInfo method)
        {
            return method.IsFamily || method.IsFamilyOrAssembly;
        }
    }
}
