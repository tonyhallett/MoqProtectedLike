using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Moq;
using Moq.Language.Flow;

namespace MoqProtectedLike
{
	internal static class MoqTypes
	{
		private static Assembly assembly = typeof(Mock).Assembly;
		
		public static Type GetInternalType(string name)
        {
			return assembly.GetType(name);
        }
	}
	internal static class ExpressionReconstructorReflection {
		public static readonly object ExpressionReconstructor;
		public static readonly MethodInfo ReconstructExpressionMethodOpenGeneric;
		public static readonly PropertyInfo MockConstructorArgumentsProperty;
		static ExpressionReconstructorReflection()
		{
			MockConstructorArgumentsProperty = typeof(Mock).GetProperty("ConstructorArguments", BindingFlags.Instance | BindingFlags.NonPublic);
			var expressionReconstructorType = MoqTypes.GetInternalType("Moq.ExpressionReconstructor");
			ExpressionReconstructor = expressionReconstructorType.GetProperty("Instance").GetValue(null);
			ReconstructExpressionMethodOpenGeneric = expressionReconstructorType.GetMethod("ReconstructExpression");
		}
		
	}

	internal static class ExpressionReconstructorReflector<T>
	{
		private static readonly MethodInfo ReconstructExpressionMethod;
		static ExpressionReconstructorReflector()
		{
			ReconstructExpressionMethod = ExpressionReconstructorReflection.ReconstructExpressionMethodOpenGeneric.MakeGenericMethod(typeof(T));
		}

		public static Expression<Action<T>> ReconstructExpression(Action<T> action, Mock mock)
		{
			var ctorArgs = ExpressionReconstructorReflection.MockConstructorArgumentsProperty.GetValue(mock) as object[];
			return ReconstructExpressionMethod.Invoke(ExpressionReconstructorReflection.ExpressionReconstructor, new object[] { action, ctorArgs }) as Expression<Action<T>>;
		}
	}

	internal static class MockSetupReflectionExtensions
	{
		private static readonly MethodInfo MockStaticSetupSetMethod;
		private static readonly MethodInfo MockStaticVerifySetMethod;
		private static readonly Type SetterSetupPhraseTypeOpenGeneric; //SetterSetupPhrase<T, TProperty>
		private static readonly Type VoidSetupPhraseTypeOpenGeneric; //VoidSetupPhrase<T>

		private static readonly Dictionary<string, Type> SetterSetupPhraseTypeCache = new Dictionary<string, Type>();

		static MockSetupReflectionExtensions()
		{
			var conditionType = MoqTypes.GetInternalType("Moq.Condition");
			MockStaticSetupSetMethod = typeof(Mock).GetMethod("SetupSet", BindingFlags.Static | BindingFlags.NonPublic,null,new Type[] { typeof(Mock),typeof(LambdaExpression),conditionType},new ParameterModifier[] { });
			MockStaticVerifySetMethod = typeof(Mock).GetMethod("VerifySet", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof(Mock), typeof(LambdaExpression), typeof(Times), typeof(string) }, new ParameterModifier[] { });
			
			SetterSetupPhraseTypeOpenGeneric = MoqTypes.GetInternalType("Moq.Language.Flow.SetterSetupPhrase`2");
			VoidSetupPhraseTypeOpenGeneric = MoqTypes.GetInternalType("Moq.Language.Flow.VoidSetupPhrase`1");

		}

		private static object StaticSetupSet<T>(this Mock<T> mock,Expression<Action<T>> action) where T:class
		{
			return MockStaticSetupSetMethod.Invoke(null, new object[] { mock, action, null });
		}
		public static object StaticVerifySet<T>(this Mock<T> mock, Expression<Action<T>> action, Times times, string failMessage) where T : class
		{
			return MockStaticVerifySetMethod.Invoke(null, new object[] { mock, action, times, failMessage });
		}

		private static ISetupSetter<T, TProperty> CreateSetupSetter<T,TProperty>(object setup) where T:class
		{
			var cacheKey = typeof(T).FullName + typeof(TProperty).FullName;
			if(!SetterSetupPhraseTypeCache.TryGetValue(cacheKey,out Type setterSetupPhraseType)){
				setterSetupPhraseType = SetterSetupPhraseTypeOpenGeneric.MakeGenericType(new Type[] { typeof(T), typeof(TProperty) });
				SetterSetupPhraseTypeCache.Add(cacheKey, setterSetupPhraseType);
			}
			return Activator.CreateInstance(
				setterSetupPhraseType,
				new object[] { setup }
				) as ISetupSetter<T, TProperty>;
		}
		public static ISetupSetter<T, TProperty> MockSetupSetter<T, TProperty>(this Mock<T> mock, Expression<Action<T>> action) where T : class
		{
			return CreateSetupSetter<T, TProperty>(mock.StaticSetupSet(action));
		}

		public static ISetup<T> MockSetup<T>(this Mock<T> mock, Expression<Action<T>> action) where T : class
		{
			return MockSetupReflector<T>.CreateSetup(mock.StaticSetupSet(action));
		}

		private static class MockSetupReflector<T> where T : class
		{
			private static readonly Type VoidSetupPhraseType;
			static MockSetupReflector()
			{
				VoidSetupPhraseType = VoidSetupPhraseTypeOpenGeneric.MakeGenericType(new Type[] { typeof(T) });
			}
			public static ISetup<T> CreateSetup(object setup)
			{
				return Activator.CreateInstance(
					VoidSetupPhraseType,
					new object[] { setup }
					) as ISetup<T>;
			}

		}
	}

}
