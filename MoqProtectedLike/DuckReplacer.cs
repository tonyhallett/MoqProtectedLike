using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MoqProtectedLike
{
	internal class DuckReplacer : ExpressionVisitor
	{
		private readonly Type duckType;
		private readonly Type targetType;

		public DuckReplacer(Type duckType, Type targetType)
		{
			this.duckType = duckType;
			this.targetType = targetType;
		}

		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			if (node.Object is ParameterExpression left && left.Type == this.duckType)
			{
				var targetParameter = Expression.Parameter(this.targetType, left.Name);
				return Expression.Call(targetParameter, FindCorrespondingMethod(node.Method), node.Arguments);
			}
			else
			{
				return node;
			}
		}

		protected override Expression VisitMember(MemberExpression node)
		{
			if (node.Expression is ParameterExpression left && left.Type == this.duckType)
			{
				var targetParameter = Expression.Parameter(this.targetType, left.Name);
				return Expression.MakeMemberAccess(targetParameter, FindCorrespondingMember(node.Member));
			}
			else
			{
				/*
					Correction to https://github.com/moq/moq4/blob/a6fde8b6d79a7437bf642d115785b97f40779b6a/src/Moq/Protected/ProtectedAsMock.cs#L233
					Without this correction you cannot have recursive mocks !
				*/
				return base.VisitMember(node);
			}
		}

		private MemberInfo FindCorrespondingMember(MemberInfo duckMember)
		{
			if (duckMember is MethodInfo duckMethod)
			{
				return FindCorrespondingMethod(duckMethod);
			}
			else if (duckMember is PropertyInfo duckProperty)
			{
				return FindCorrespondingProperty(duckProperty);
			}
			else
			{
				throw new NotSupportedException();
			}
		}

		private MethodInfo FindCorrespondingMethod(MethodInfo duckMethod)
		{
			var candidateTargetMethods =
				this.targetType
				.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(ctm => IsCorrespondingMethod(duckMethod, ctm))
				.ToArray();

			if (candidateTargetMethods.Length == 0)
			{
				throw new ArgumentException($"Type { this.targetType } does not have matching protected member: {duckMethod}");
			}

			var targetMethod = candidateTargetMethods[0];

			if (targetMethod.IsGenericMethodDefinition)
			{
				var duckGenericArgs = duckMethod.GetGenericArguments();
				targetMethod = targetMethod.MakeGenericMethod(duckGenericArgs);
			}

			return targetMethod;
		}

		private PropertyInfo FindCorrespondingProperty(PropertyInfo duckProperty)
		{
			/*
				Correction to https://github.com/moq/moq4/blob/a6fde8b6d79a7437bf642d115785b97f40779b6a/src/Moq/Protected/ProtectedAsMock.cs#L283
				Allows for public virtual property with protected
			*/
			var candidateTargetProperties =
				this.targetType
				.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
				.Where(ctp => IsCorrespondingProperty(duckProperty, ctp))
				.ToArray();

			if (candidateTargetProperties.Length == 0)
			{
				throw new ArgumentException($"Type { this.targetType } does not have matching protected member: {duckProperty}");
			}

			return candidateTargetProperties[0];
		}

		private static bool IsCorrespondingMethod(MethodInfo duckMethod, MethodInfo candidateTargetMethod)
		{
			if (candidateTargetMethod.Name != duckMethod.Name)
			{
				return false;
			}

			if (candidateTargetMethod.IsGenericMethod != duckMethod.IsGenericMethod)
			{
				return false;
			}

			if (candidateTargetMethod.IsGenericMethodDefinition)
			{
				// when both methods are generic, then the candidate method should be an open generic method
				// while the duck-type method should be a closed one. in this case, we close the former
				// over the same generic type arguments that the latter uses.

				//Debug.Assert(!duckMethod.IsGenericMethodDefinition);

				var candidateGenericArgs = candidateTargetMethod.GetGenericArguments();
				var duckGenericArgs = duckMethod.GetGenericArguments();

				if (candidateGenericArgs.Length != duckGenericArgs.Length)
				{
					return false;
				}

				// this could perhaps go wrong due to generic type parameter constraints; if it does
				// go wrong, then obviously the duck-type method doesn't correspond to the candidate.
				try
				{
					candidateTargetMethod = candidateTargetMethod.MakeGenericMethod(duckGenericArgs);
				}
				catch
				{
					return false;
				}
			}

			var duckParameters = duckMethod.GetParameters();
			var candidateParameters = candidateTargetMethod.GetParameters();

			if (candidateParameters.Length != duckParameters.Length)
			{
				return false;
			}

			for (int i = 0, n = candidateParameters.Length; i < n; ++i)
			{
				if (candidateParameters[i].ParameterType != duckParameters[i].ParameterType)
				{
					return false;
				}
			}

			return true;
		}

		private static bool IsCorrespondingProperty(PropertyInfo duckProperty, PropertyInfo candidateTargetProperty)
		{
			return candidateTargetProperty.Name == duckProperty.Name
				&& candidateTargetProperty.PropertyType == duckProperty.PropertyType
				&& candidateTargetProperty.CanRead(out _) == duckProperty.CanRead(out _)
				&& candidateTargetProperty.CanWrite(out _) == duckProperty.CanWrite(out _);
		}
	}


}
