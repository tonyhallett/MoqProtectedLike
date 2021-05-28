using System.Linq.Expressions;

namespace MoqProtectedLike
{
	static partial class ExpressionExtensions
	{
		public static Expression Apply(this Expression expression, ExpressionVisitor visitor)
		{
			return visitor.Visit(expression);
		}
	}
}
