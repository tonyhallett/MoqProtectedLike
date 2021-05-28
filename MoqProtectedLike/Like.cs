using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Moq;
using Moq.Language;
using Moq.Language.Flow;
using Moq.Protected;

namespace MoqProtectedLike
{
    public class Like<T, TLike> : IProtectedLike<T, TLike> where T : class where TLike : class
	{
        #region Ducks
        #region DuckReplacer

        private static readonly DuckReplacer DuckReplacerInstance = new DuckReplacer(typeof(TLike), typeof(T));

		private static LambdaExpression ReplaceDuck(LambdaExpression expression)
		{
			Debug.Assert(expression.Parameters.Count == 1);

			var targetParameter = Expression.Parameter(typeof(T), expression.Parameters[0].Name);
			return Expression.Lambda(DuckReplacerInstance.Visit(expression.Body), targetParameter);
		}
		#endregion
		private static readonly DuckSetupSetVisitor<T, TLike> DuckSetUpSetVisitor = new DuckSetupSetVisitor<T, TLike>();
        #endregion

        private readonly Mock<T> mock;
		private IProtectedAsMock<T, TLike> protectedAsMock;
		private IProtectedAsMock<T, TLike> ProtectedAsMock
		{
			get
			{
				if (protectedAsMock == null)
				{
					protectedAsMock = protectedAsFactory.Create(mock);
				}
				return protectedAsMock;
			}
		}

		internal IProtectedAsFactory<T, TLike> protectedAsFactory = new ProtectedAsFactory<T, TLike>();

		public T Object => mock.Object;
		public Like(Mock<T> mock)
		{
			this.mock = mock;
		}

		#region missing setups

		private Expression<Action<T>> DuckConvertSetter(Action<TLike> setterExpression)
		{
			Guard.NotNull(setterExpression, nameof(setterExpression));
			var expression = ExpressionReconstructorReflector<TLike>.ReconstructExpression(setterExpression, mock);
			return DuckSetUpSetVisitor.Convert(expression);
		}


		public ISetupSetter<T, TProperty> SetupSet<TProperty>(Action<TLike> setup)
		{
			return mock.MockSetupSetter<T, TProperty>(DuckConvertSetter(setup));
		}
		public ISetup<T> SetupSet(Action<TLike> setup)
		{
			return mock.MockSetup(DuckConvertSetter(setup));
		}

		public void VerifySet(Action<TLike> setup, Times? times = null, string failMessage = null)
		{
			try
			{
				mock.StaticVerifySet(DuckConvertSetter(setup), times ?? Times.AtLeastOnce(), failMessage);
			}
			catch (TargetInvocationException exception)
			{
				throw exception.InnerException;
			}
		}
		#endregion

		#region Protected rewritten
		public ISetupGetter<T, TProperty> SetupGet<TProperty>(Expression<Func<TLike, TProperty>> expression)
		{
			Guard.NotNull(expression, nameof(expression));

			Expression<Func<T, TProperty>> rewrittenExpression;
			try
			{
				rewrittenExpression = (Expression<Func<T, TProperty>>)ReplaceDuck(expression);
			}
			catch (ArgumentException ex)
			{
				throw new ArgumentException(ex.Message, nameof(expression));
			}
			return mock.SetupGet(rewrittenExpression);
		}
		public void VerifyGet<TProperty>(Expression<Func<TLike, TProperty>> expression, Times? times = null, string failMessage = null)
		{
			Guard.NotNull(expression, nameof(expression));

			Expression<Func<T, TProperty>> rewrittenExpression;
			try
			{
				rewrittenExpression = (Expression<Func<T, TProperty>>)ReplaceDuck(expression);
			}
			catch (ArgumentException ex)
			{
				throw new ArgumentException(ex.Message, nameof(expression));
			}

			mock.VerifyGet(rewrittenExpression, times ?? Times.AtLeastOnce(), failMessage);
		}

		public ISetup<T, TResult> Setup<TResult>(Expression<Func<TLike, TResult>> expression)
		{
			Guard.NotNull(expression, nameof(expression));

			Expression<Func<T, TResult>> rewrittenExpression;
			try
			{
				rewrittenExpression = (Expression<Func<T, TResult>>)ReplaceDuck(expression);
			}
			catch (ArgumentException ex)
			{
				throw new ArgumentException(ex.Message, nameof(expression));
			}

			return mock.Setup(rewrittenExpression);
		}

		public void Verify<TResult>(Expression<Func<TLike, TResult>> expression, Times? times = null, string failMessage = null)
		{
			Guard.NotNull(expression, nameof(expression));

			Expression<Func<T, TResult>> rewrittenExpression;
			try
			{
				rewrittenExpression = (Expression<Func<T, TResult>>)ReplaceDuck(expression);
			}
			catch (ArgumentException ex)
			{
				throw new ArgumentException(ex.Message, nameof(expression));
			}

			mock.Verify(rewrittenExpression, (Times)times, failMessage);
		}

		#endregion

		#region passthroughs

		public ISetup<T> Setup(Expression<Action<TLike>> expression)
		{
			return ProtectedAsMock.Setup(expression);
		}


		public Mock<T> SetupProperty<TProperty>(Expression<Func<TLike, TProperty>> expression, TProperty initialValue = default)
		{
			return ProtectedAsMock.SetupProperty(expression, initialValue);
		}

		public ISetupSequentialResult<TResult> SetupSequence<TResult>(Expression<Func<TLike, TResult>> expression)
		{
			return ProtectedAsMock.SetupSequence(expression);
		}

		public ISetupSequentialAction SetupSequence(Expression<Action<TLike>> expression)
		{
			return ProtectedAsMock.SetupSequence(expression);
		}

		public void Verify(Expression<Action<TLike>> expression, Times? times = null, string failMessage = null)
		{
			ProtectedAsMock.Verify(expression, times, failMessage);
		}

		#endregion
	}

}
