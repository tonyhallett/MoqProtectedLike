# MoqProtectedLike - Why ?

## ProtectedMock does not work properly.
&nbsp;  
[SetUpSet](https://github.com/moq/moq4/blob/a6fde8b6d79a7437bf642d115785b97f40779b6a/src/Moq/Protected/ProtectedMock.cs#L128) ignores the object value and uses `ItExpr.IsAny<TProperty>()`
This is contrary to the [xml docs.](https://github.com/moq/moq4/blob/a6fde8b6d79a7437bf642d115785b97f40779b6a/src/Moq/Protected/IProtectedMock.cs#L108)
You are forced to supply TProperty and if you want to set up an indexer you have to cast.

[VerifySet](https://github.com/moq/moq4/blob/a6fde8b6d79a7437bf642d115785b97f40779b6a/src/Moq/Protected/ProtectedMock.cs#L294) also ignores the object value.
The behaviour of `ItExpr.IsAny<TProperty>()` can be seen with this ridiculous [test](https://github.com/moq/moq4/blob/a6fde8b6d79a7437bf642d115785b97f40779b6a/tests/Moq.Tests/ProtectedMockFixture.cs#L806).

```csharp
[Fact]
public void VerifySetAllowsProtectedInternalPropertySet()
{
	var mock = new Mock<FooBase>();
	mock.Object.ProtectedInternalValue = "foo";

	mock.Protected().VerifySet<string>("ProtectedInternalValue", Times.Once(), "bar");
}

```

Indexers are not properly supported.

You can workaround these issues by using Verify and Setup.  For instance

```csharp

[Fact]
public void SetUp_Workaround_For_SetupSet_Object_Ignored()
{
	var mock = new Mock<Foo>();
	var protectedMock = mock.Protected();
	protectedMock.Setup("set_ReadWritePropertyImpl", 999);
	//...
}


[Fact]
public void SetUp_Workaround_For_SetupGet_Overloaded_Indexers()
{
	var mock = new Mock<OverloadedIndexers>();
	var protectedMock = mock.Protected();
	//ambiguous match exception if provide Item
	protectedMock.Setup<int>("get_Item", new object[] { ItExpr.IsInRange(3, 4, Moq.Range.Inclusive) }).Returns(123);
	protectedMock.Setup<int>("get_Item", new object[] { ItExpr.Is<string>(index => index.StartsWith("Match")) }).Returns(456);
	//....
}
```

## ProtectedAsMock also has issues  
&nbsp;  
Recursive mocking does not work.

Virtual properties with protected accessors does not work.

There is no SetupSet or VerifySet.
 
&nbsp;

# MoqProtectedLike 

Corrects the issues with ProtectedAsMock and provides SetupSet and VerifySet.

## Usage


Use in a similar manner to ProtectedAsMock

```csharp
[Test]
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
	//note that you can get the proxy with Object
	Assert.AreEqual(123, protectedMock.Object.GetNested().Deep.Value);
}


```
