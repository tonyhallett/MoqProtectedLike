using System;
using System.Linq.Expressions;
using Moq;
using Moq.Language;
using Moq.Language.Flow;
using Moq.Protected;
using MoqProtectedLike;
using NUnit.Framework;

namespace MoqProtectedLikeTests
{
    #region types
    public abstract class VirtualProtectedGetter
	{
		protected string virtualProperty;
		public virtual string Virtual
		{
			protected get
			{
				return virtualProperty;
			}
			set
			{
				virtualProperty = value;
			}

		}
		public string UsesGetter()
		{
			return this.Virtual;
		}
	}

#pragma warning disable IDE1006 // Naming Styles
	public interface VirtualProtectedGetterIsh
#pragma warning restore IDE1006 // Naming Styles
	{
		string Virtual { get; set; }
	}

	public interface INested
	{
		IDeepNested Deep { get; }
	}
	public interface IDeepNested
	{
		int Value { get; set; }
	}
	public abstract class Foo
	{
		protected Foo()
		{
		}

		public int ReadOnlyProperty => this.ReadOnlyPropertyImpl;

		public int ReadWriteProperty
		{
			get => this.ReadWritePropertyImpl;
			set => this.ReadWritePropertyImpl = value;
		}

		protected abstract int ReadOnlyPropertyImpl { get; }
		protected abstract INested Nested { get; set; }

		protected abstract int ReadWritePropertyImpl { get; set; }

		[System.Runtime.CompilerServices.IndexerName("Indexer")]
		protected abstract int this[int i] { get; set; }


		public virtual string VirtualSetter
		{
			get
			{
				return "got";
			}
			protected set
			{

			}
		}

		public void SetVirtualSetter(string value)
		{
			this.VirtualSetter = value;
		}

		public void SetIndexer(int index, int value)
		{
			this[index] = value;
		}

		public int GetIndexer(int index)
		{
			return this[index];
		}

		public INested GetNested()
		{
			return Nested;
		}

	}

#pragma warning disable IDE1006 // Naming Styles
	public interface Fooish
#pragma warning restore IDE1006 // Naming Styles
	{
		int ReadOnlyPropertyImpl { get; }
		int ReadWritePropertyImpl { get; set; }
		void DoSomethingImpl();
		int GetSomethingImpl();
		string VirtualSetter { get; set; }

		[System.Runtime.CompilerServices.IndexerName("Indexer")]
		int this[int i] { get; set; }

		INested Nested { get; set; }
	}

	public class Index { }

	public abstract class OverloadedIndexers
	{
		protected OverloadedIndexers()
		{
		}

		protected abstract int this[int i] { get; set; }

		protected abstract int this[int i, string s] { get; set; }

		protected abstract int this[string i] { get; set; }

		protected abstract int this[Index i] { get; set; }


		public void SetIntIndexer(int index, int value)
		{
			this[index] = value;
		}

		public void SetMultipleIndexer(int index, string sIndex, int value)
		{
			this[index, sIndex] = value;
		}

		public void SetClassIndexer(Index index, int value)
		{
			this[index] = value;
		}
		public int GetIntIndexer(int index)
		{
			return this[index];
		}
		public void SetStringIndexer(string index, int value)
		{
			this[index] = value;
		}
		public int GetStringIndexer(string index)
		{
			return this[index];
		}
	}

#pragma warning disable IDE1006 // Naming Styles
	public interface OverloadedIndexerIsh
#pragma warning restore IDE1006 // Naming Styles
	{
		int this[int i] { get; set; }
		int this[string i] { get; set; }

		int this[int i, string s] { get; set; }

		int this[Index i] { get; set; }
	}
	#endregion

	public static class Expected
    {
		private static readonly ExpectedException expected = new ExpectedException();
		public static ExpectedException Exception => expected;
    }

	public class ExpectedException : Exception {}

	public class MoqLike_PassThroughs
	{
		private static Expression<Action<TLike>> ActionToExpression<TLike>(Action<TLike> f)
		{
			return x => f(x);
		}

		private static Expression<Func<T, U>> FuncToExpression<T, U>(Func<T, U> f)
		{
			return x => f(x);
		}

		[Test]
		public void Should_Use_ProtectedAsMock_For_Setup()
		{
			var mock = new Mock<Foo>();
			var mockedSetup = new Mock<ISetup<Foo>>().Object;

			var mockProtectedAsFactory = new Mock<IProtectedAsFactory<Foo, Fooish>>();
			Expression<Action<Fooish>> expression = ActionToExpression<Fooish>(f => f.DoSomethingImpl());
			var mockProtectedAsMock = new Mock<IProtectedAsMock<Foo, Fooish>>();
			mockProtectedAsMock.Setup(pm => pm.Setup(expression)).Returns(mockedSetup);
			mockProtectedAsFactory.Setup(f => f.Create(mock)).Returns(mockProtectedAsMock.Object);

			var like = new Like<Foo, Fooish>(mock)
			{
				protectedAsFactory = mockProtectedAsFactory.Object
			};

			var setUp = like.Setup(expression);
			Assert.AreEqual(setUp, mockedSetup);

		}

		[Test]
		public void Should_Use_ProtectedAsMock_For_SetupProperty()
		{
			var mockSetUpPropertyMock = new Mock<Foo>();
			var mock = new Mock<Foo>();
			var mockedSetupSequentialAction = new Mock<ISetupSequentialResult<int>>().Object;

			var mockProtectedAsFactory = new Mock<IProtectedAsFactory<Foo, Fooish>>();
			Expression<Func<Fooish, int>> expression = FuncToExpression<Fooish, int>(f => f.ReadWritePropertyImpl);


			var mockProtectedAsMock = new Mock<IProtectedAsMock<Foo, Fooish>>();
			mockProtectedAsMock.Setup(pm => pm.SetupProperty(expression, 42)).Returns(mockSetUpPropertyMock);
			mockProtectedAsFactory.Setup(f => f.Create(mock)).Returns(mockProtectedAsMock.Object);

			var like = new Like<Foo, Fooish>(mock)
			{
				protectedAsFactory = mockProtectedAsFactory.Object
			};

			var setUpPropertyMock = like.SetupProperty(expression, 42);
			Assert.AreEqual(mockSetUpPropertyMock, setUpPropertyMock);
		}

		[Test]
		public void Should_Use_ProtectedAsMock_For_SetupProperty_Default()
		{
			var mockSetUpPropertyMock = new Mock<Foo>();
			var mock = new Mock<Foo>();
			var mockedSetupSequentialAction = new Mock<ISetupSequentialResult<int>>().Object;

			var mockProtectedAsFactory = new Mock<IProtectedAsFactory<Foo, Fooish>>();
			Expression<Func<Fooish, int>> expression = FuncToExpression<Fooish, int>(f => f.ReadWritePropertyImpl);


			var mockProtectedAsMock = new Mock<IProtectedAsMock<Foo, Fooish>>();
			mockProtectedAsMock.Setup(pm => pm.SetupProperty(expression, 0)).Returns(mockSetUpPropertyMock);
			mockProtectedAsFactory.Setup(f => f.Create(mock)).Returns(mockProtectedAsMock.Object);

			var like = new Like<Foo, Fooish>(mock)
			{
				protectedAsFactory = mockProtectedAsFactory.Object
			};

			var setUpPropertyMock = like.SetupProperty(expression);
			Assert.AreEqual(mockSetUpPropertyMock, setUpPropertyMock);
		}

		[Test]
		public void Should_Use_ProtectedAsMock_For_SetupSequence_Func()
		{
			var mock = new Mock<Foo>();
			var mockedSetupSequentialAction = new Mock<ISetupSequentialResult<int>>().Object;

			var mockProtectedAsFactory = new Mock<IProtectedAsFactory<Foo, Fooish>>();
			Expression<Func<Fooish, int>> expression = FuncToExpression<Fooish, int>(f => f.GetSomethingImpl());


			var mockProtectedAsMock = new Mock<IProtectedAsMock<Foo, Fooish>>();
			mockProtectedAsMock.Setup(pm => pm.SetupSequence(expression)).Returns(mockedSetupSequentialAction);
			mockProtectedAsFactory.Setup(f => f.Create(mock)).Returns(mockProtectedAsMock.Object);

			var like = new Like<Foo, Fooish>(mock)
			{
				protectedAsFactory = mockProtectedAsFactory.Object
			};

			var setUpSequence = like.SetupSequence(expression);
			Assert.AreEqual(setUpSequence, mockedSetupSequentialAction);
		}

		[Test]
		public void Should_Use_ProtectedAsMock_For_SetupSequence_Action()
		{
			var mock = new Mock<Foo>();
			var mockedSetupSequentialAction = new Mock<ISetupSequentialAction>().Object;

			var mockProtectedAsFactory = new Mock<IProtectedAsFactory<Foo, Fooish>>();
			Expression<Action<Fooish>> expression = ActionToExpression<Fooish>(f => f.DoSomethingImpl());
			var mockProtectedAsMock = new Mock<IProtectedAsMock<Foo, Fooish>>();
			mockProtectedAsMock.Setup(pm => pm.SetupSequence(expression)).Returns(mockedSetupSequentialAction);
			mockProtectedAsFactory.Setup(f => f.Create(mock)).Returns(mockProtectedAsMock.Object);

			var like = new Like<Foo, Fooish>(mock)
			{
				protectedAsFactory = mockProtectedAsFactory.Object
			};

			var setUpSequence = like.SetupSequence(expression);
			Assert.AreEqual(setUpSequence, mockedSetupSequentialAction);
		}

		[Test]
		public void Should_Use_ProtectedAsMock_For_Verify_Func()
		{
			var mock = new Mock<Foo>();
			var times = Times.Exactly(10);
			var mockProtectedAsFactory = new Mock<IProtectedAsFactory<Foo, Fooish>>();
			Expression<Action<Fooish>> expression = ActionToExpression<Fooish>(f => f.DoSomethingImpl());
			var mockProtectedAsMock = new Mock<IProtectedAsMock<Foo, Fooish>>();
			mockProtectedAsMock.Setup(pm => pm.Verify(expression, times, "failure !"));
			mockProtectedAsFactory.Setup(f => f.Create(mock)).Returns(mockProtectedAsMock.Object);

			var like = new Like<Foo, Fooish>(mock)
			{
				protectedAsFactory = mockProtectedAsFactory.Object
			};

			like.Verify(expression, times, "failure !");

			mockProtectedAsMock.VerifyAll();
		}
	}

	public class MoqLike_Rewritten
	{
		[Test] // https://github.com/moq/moq4/blob/main/tests/Moq.Tests/ProtectedAsMockFixture.cs#L134
		public void SetupGet_can_setup_readonly_property()
		{
			var protectedLike = new Mock<Foo>().IsProtected().Like<Fooish>();

			protectedLike.SetupGet(m => m.ReadOnlyPropertyImpl).Returns(42);

			var actual = protectedLike.Object.ReadOnlyProperty;

			Assert.AreEqual(42, actual);
		}

		[Test] // https://github.com/moq/moq4/blob/main/tests/Moq.Tests/ProtectedAsMockFixture.cs#L144
		public void SetupGet_can_setup_readwrite_property()
		{
			var protectedLike = new Mock<Foo>().IsProtected().Like<Fooish>();

			protectedLike.SetupGet(m => m.ReadWritePropertyImpl).Returns(42);

			var actual = protectedLike.Object.ReadWriteProperty;

			Assert.AreEqual(42, actual);
		}

		[Test]
		public void SetupGet_Should_Work_With_Overloaded_Indexers()
		{
			var protectedLike = new Mock<OverloadedIndexers>().IsProtected().Like<OverloadedIndexerIsh>();

			protectedLike.SetupGet(f => f[1]).Returns(999);

			Assert.AreEqual(0, protectedLike.Object.GetIntIndexer(2));
			Assert.AreEqual(999, protectedLike.Object.GetIntIndexer(1));

			protectedLike.SetupGet(f => f["Match"]).Returns(123);
			Assert.AreEqual(0, protectedLike.Object.GetStringIndexer("No Match"));
			Assert.AreEqual(123, protectedLike.Object.GetStringIndexer("Match"));
		}

		[Test]
		public void VerifyGet_Should_Work_With_Overloaded_Indexers()
		{
			var protectedLike = new Mock<OverloadedIndexers>().IsProtected().Like<OverloadedIndexerIsh>();

			protectedLike.Object.GetIntIndexer(2);
			Assert.Throws<MockException>(() => protectedLike.VerifyGet(f => f[1]));
			protectedLike.VerifyGet(f => f[2]);

			protectedLike.Object.GetStringIndexer("Match");
			Assert.Throws<MockException>(() => protectedLike.VerifyGet(f => f["No Match"]));
			protectedLike.VerifyGet(f => f["Match"]);
		}
		
		[Test] // ************************
		public void This_SetupGet_Works_With_Virtual_Protected_Getters()
		{
			//does not work in Moq
			var mock = new Mock<VirtualProtectedGetter>();
			var protectedAs = mock.Protected().As<VirtualProtectedGetterIsh>();
			Assert.Throws<ArgumentException>(() =>
			{
				protectedAs.SetupGet(v => v.Virtual).Returns("Value");
			});

			// does here
			var protectedLike = new Mock<VirtualProtectedGetter>().IsProtected().Like<VirtualProtectedGetterIsh>();
			protectedLike.SetupGet(v => v.Virtual).Returns("Value");
			Assert.AreEqual("Value", protectedLike.Object.UsesGetter());
		}
	
		[Test] // ************************
		public void Should_Work_With_Recursive_Mocks()
        {
			//does not work in Moq
			var mock = new Mock<Foo>();
			var protectedAs = mock.Protected().As<Fooish>();
			protectedAs.SetupGet(f => f.Nested.Deep.Value).Returns(123);
			Assert.Throws<ArgumentException>(() => Assert.AreEqual(123, mock.Object.GetNested().Deep.Value));

			//does here
			var protectedMock = new Mock<Foo>().IsProtected().Like<Fooish>();
			protectedMock.SetupGet(f => f.Nested.Deep.Value).Returns(123);

			Assert.AreEqual(123, protectedMock.Object.GetNested().Deep.Value);
		}
	}

	public class MoqLike_New_Behaviour_Tests
	{
		[Test]
		public void Should_Work_With_SetupSetter()
		{
			var protectedLike = new Mock<Foo>().IsProtected().Like<Fooish>();
			protectedLike.SetupSet(f => f.ReadWritePropertyImpl = 999).Throws(Expected.Exception);

			protectedLike.Object.ReadWriteProperty = 123;
			Assert.Throws<ExpectedException>(() => protectedLike.Object.ReadWriteProperty = 999);

		}

		[Test]
		public void Should_Work_With_SetupSetter_Property_Typed()
		{
			var protectedLike = new Mock<Foo>().IsProtected().Like<Fooish>();
			int setValue = 0;
			protectedLike.SetupSet<int>(f => f.ReadWritePropertyImpl = It.IsInRange<int>(1, 10, Moq.Range.Inclusive)).Callback(v => setValue = v);

			protectedLike.Object.ReadWriteProperty = 123;
			Assert.AreEqual(0, setValue);
			protectedLike.Object.ReadWriteProperty = 9;
			Assert.AreEqual(9, setValue);

		}

		[Test]
		public void Should_Work_With_SetupSetter_Virtual_Protected()
		{
			var protectedLike = new Mock<Foo>().IsProtected().Like<Fooish>();
			protectedLike.SetupSet<int>(f => f.VirtualSetter = "Exception").Throws(Expected.Exception);

			protectedLike.Object.SetVirtualSetter("Ok");

			Assert.Throws<ExpectedException>(() => protectedLike.Object.SetVirtualSetter("Exception"));
		}

		[Test]
		public void Should_Work_With_Indexers()
		{
			var protectedLike = new Mock<Foo>().IsProtected().Like<Fooish>();

			int setValue = 0;
			int setIndex = 0;
			protectedLike.SetupSet(f => f[1] = 999).Callback<int, int>((index, v) =>
			{
				setIndex = index;
				setValue = v;
			});

			protectedLike.Object.SetIndexer(2, 999);
			Assert.AreEqual(0, setValue);
			Assert.AreEqual(0, setIndex);
			protectedLike.Object.SetIndexer(1, 999);
			Assert.AreEqual(999, setValue);
			Assert.AreEqual(1, setIndex);
		}

		[Test]
		public void Should_Work_With_Indexers_Any_Value()
		{
			var protectedLike = new Mock<Foo>().IsProtected().Like<Fooish>();

			int setValue = 0;
			int setIndex = 0;
			protectedLike.SetupSet(f => f[1] = It.IsAny<int>()).Callback<int, int>((index, v) =>
			{
				setIndex = index;
				setValue = v;
			});

			protectedLike.Object.SetIndexer(2, 999);
			Assert.AreEqual(0, setValue);
			Assert.AreEqual(0, setIndex);
			protectedLike.Object.SetIndexer(1, 6);
			Assert.AreEqual(6, setValue);
			Assert.AreEqual(1, setIndex);
		}

		[Test]
		public void Should_Work_With_Indexers_Any_Index()
		{
			var protectedLike = new Mock<Foo>().IsProtected().Like<Fooish>();

			int setValue = 0;
			int setIndex = 0;
			protectedLike.SetupSet(f => f[It.IsAny<int>()] = 999).Callback<int, int>((index, v) =>
			{
				setValue = v;
				setIndex = index;
			});

			protectedLike.Object.SetIndexer(2, 6);
			Assert.AreEqual(0, setValue);
			Assert.AreEqual(0, setIndex);
			protectedLike.Object.SetIndexer(1, 999);
			Assert.AreEqual(999, setValue);
			Assert.AreEqual(1, setIndex);
		}

		[Test]
		public void Should_Work_With_Overloaded_Indexers()
		{
			var protectedLike = new Mock<OverloadedIndexers>().IsProtected().Like<OverloadedIndexerIsh>();

			int setValue = 0;
			int setIndex = 0;
			string stringSetIndex = null;
			int stringSetValue = 0;
			protectedLike.SetupSet(f => f[1] = 999).Callback<int, int>((index, v) =>
			{
				setIndex = index;
				setValue = v;
			});

			protectedLike.SetupSet(f => f["Match"] = 123).Callback<string, int>((index, v) =>
			{
				stringSetIndex = index;
				stringSetValue = v;
			});

			protectedLike.Object.SetIntIndexer(2, 999);
			Assert.AreEqual(0, setValue);
			Assert.AreEqual(0, setIndex);
			protectedLike.Object.SetIntIndexer(1, 999);
			Assert.AreEqual(999, setValue);
			Assert.AreEqual(1, setIndex);

			protectedLike.Object.SetStringIndexer("Not a match", 123);
			Assert.AreEqual(0, stringSetValue);
			Assert.Null(stringSetIndex);
			protectedLike.Object.SetStringIndexer("Match", 123);
			Assert.AreEqual(123, stringSetValue);
			Assert.AreEqual("Match", stringSetIndex);
		}

		[Test]
		public void Should_Work_With_VerifySet()
		{
			var protectedLike = new Mock<Foo>().IsProtected().Like<Fooish>();

			protectedLike.Object.ReadWriteProperty = 123;

			Assert.Throws<MockException>(() =>
			{
				protectedLike.VerifySet(f => f.ReadWritePropertyImpl = 999);
			});

			protectedLike.Object.ReadWriteProperty = 999;
			protectedLike.VerifySet(f => f.ReadWritePropertyImpl = 999);

		}

		[Test]
		public void Should_Work_With_VerifySet_Indexers()
		{
			var protectedLike = new Mock<OverloadedIndexers>().IsProtected().Like<OverloadedIndexerIsh>();

			void VerifySetInt()
			{
				protectedLike.VerifySet(f => f[It.IsInRange(1, 5, Moq.Range.Inclusive)] = It.IsInRange(6, 10, Moq.Range.Inclusive));
			}

			void VerifySetString()
			{
				protectedLike.VerifySet(f => f[It.IsIn("Match", "Other")] = It.IsInRange(6, 10, Moq.Range.Inclusive));
			}

			protectedLike.Object.SetIntIndexer(2, 999);
			Assert.Throws<MockException>(VerifySetInt);
			protectedLike.Object.SetIntIndexer(1, 7);
			VerifySetInt();

			protectedLike.Object.SetStringIndexer("Not a match", 123);
			Assert.Throws<MockException>(VerifySetString);
			protectedLike.Object.SetStringIndexer("Match", 8);
			VerifySetString();
		}

	}


}