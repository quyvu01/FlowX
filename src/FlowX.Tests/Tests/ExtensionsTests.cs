using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;
using FlowX.Extensions;
using Xunit;

namespace FlowX.Tests.Tests;

public sealed class ExtensionsTests
{
    #region TryParseOrderBy Tests

    [Theory]
    [InlineData("Name", "Name", true)]
    [InlineData("name", "name", true)]
    [InlineData("Name asc", "Name", true)]
    [InlineData("Name ASC", "Name", true)]
    [InlineData("Name desc", "Name", false)]
    [InlineData("Name DESC", "Name", false)]
    [InlineData("CreatedAt", "CreatedAt", true)]
    [InlineData("Created_At desc", "Created_At", false)]
    [InlineData("_private asc", "_private", true)]
    public void TryParseOrderBy_With_Valid_Input_Should_Return_True(
        string input, string expectedProperty, bool expectedIsAsc)
    {
        var result = FlowX.Extensions.Extensions.TryParseOrderBy(input, out var property, out var isAsc);

        Assert.True(result);
        Assert.Equal(expectedProperty, property);
        Assert.Equal(expectedIsAsc, isAsc);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void TryParseOrderBy_With_Empty_Input_Should_Return_False(string input)
    {
        var result = FlowX.Extensions.Extensions.TryParseOrderBy(input, out var property, out var isAsc);

        Assert.False(result);
        Assert.Equal(string.Empty, property);
        Assert.True(isAsc); // default
    }

    [Theory]
    [InlineData("123Name")]
    [InlineData("Name invalid")]
    [InlineData("Name asc desc")]
    [InlineData("Name,Email")]
    public void TryParseOrderBy_With_Invalid_Input_Should_Return_False(string input)
    {
        var result = FlowX.Extensions.Extensions.TryParseOrderBy(input, out _, out _);

        Assert.False(result);
    }

    #endregion

    #region OrderDynamicOrDefault Tests

    [Fact]
    public void OrderDynamicOrDefault_With_Null_SortedFields_Should_Use_Default_Single_Asc()
    {
        var data = GetTestData().AsQueryable();
        var defaultOrders = new List<ExpressionOrderDetail<TestEntity>>
        {
            new(x => x.Name, true)
        };

        var ordered = data.OrderDynamicOrDefault(null, defaultOrders).ToList();

        Assert.Equal("Alice", ordered[0].Name);
        Assert.Equal("Bob", ordered[1].Name);
        Assert.Equal("Charlie", ordered[2].Name);
    }

    [Fact]
    public void OrderDynamicOrDefault_With_Empty_SortedFields_Should_Use_Default_Single_Desc()
    {
        var data = GetTestData().AsQueryable();
        var defaultOrders = new List<ExpressionOrderDetail<TestEntity>>
        {
            new(x => x.Name, false)
        };

        var ordered = data.OrderDynamicOrDefault("", defaultOrders).ToList();

        Assert.Equal("Charlie", ordered[0].Name);
        Assert.Equal("Bob", ordered[1].Name);
        Assert.Equal("Alice", ordered[2].Name);
    }

    [Fact]
    public void OrderDynamicOrDefault_With_Multiple_Default_Orders_Should_Apply_All()
    {
        var data = new List<TestEntity>
        {
            new() { Id = 1, Name = "Alice", Age = 30 },
            new() { Id = 2, Name = "Alice", Age = 25 },
            new() { Id = 3, Name = "Bob", Age = 35 }
        }.AsQueryable();

        var defaultOrders = new List<ExpressionOrderDetail<TestEntity>>
        {
            new(x => x.Name, true),
            new(x => x.Age, false)
        };

        var ordered = data.OrderDynamicOrDefault(null, defaultOrders).ToList();

        Assert.Equal("Alice", ordered[0].Name);
        Assert.Equal(30, ordered[0].Age); // Age desc within Alice
        Assert.Equal("Alice", ordered[1].Name);
        Assert.Equal(25, ordered[1].Age);
        Assert.Equal("Bob", ordered[2].Name);
    }

    [Fact]
    public void OrderDynamicOrDefault_With_Dynamic_SortedFields_Should_Override_Default()
    {
        var data = GetTestData().AsQueryable();
        var defaultOrders = new List<ExpressionOrderDetail<TestEntity>>
        {
            new(x => x.Name, true)
        };

        var ordered = data.OrderDynamicOrDefault("Age desc", defaultOrders).ToList();

        Assert.Equal(35, ordered[0].Age);
        Assert.Equal(30, ordered[1].Age);
        Assert.Equal(25, ordered[2].Age);
    }

    [Fact]
    public void OrderDynamicOrDefault_With_Multiple_Dynamic_Fields_Should_Work()
    {
        var data = new List<TestEntity>
        {
            new() { Id = 1, Name = "Alice", Age = 30 },
            new() { Id = 2, Name = "Alice", Age = 25 },
            new() { Id = 3, Name = "Bob", Age = 30 }
        }.AsQueryable();

        var defaultOrders = new List<ExpressionOrderDetail<TestEntity>>
        {
            new(x => x.Id, true)
        };

        var ordered = data.OrderDynamicOrDefault("Age desc, Name asc", defaultOrders).ToList();

        Assert.Equal(30, ordered[0].Age);
        Assert.Equal("Alice", ordered[0].Name); // Alice before Bob at Age 30
        Assert.Equal(30, ordered[1].Age);
        Assert.Equal("Bob", ordered[1].Name);
        Assert.Equal(25, ordered[2].Age);
    }

    [Fact]
    public void OrderDynamicOrDefault_With_Empty_DefaultOrders_Should_Throw()
    {
        var data = GetTestData().AsQueryable();
        var defaultOrders = new List<ExpressionOrderDetail<TestEntity>>();

        Assert.Throws<ArgumentException>(() =>
            data.OrderDynamicOrDefault(null, defaultOrders).ToList());
    }

    [Fact]
    public void OrderDynamicOrDefault_With_Invalid_SortedFields_Should_Throw()
    {
        var data = GetTestData().AsQueryable();
        var defaultOrders = new List<ExpressionOrderDetail<TestEntity>>
        {
            new(x => x.Name, true)
        };

        Assert.Throws<ArgumentException>(() =>
            data.OrderDynamicOrDefault("123invalid", defaultOrders).ToList());
    }

    #endregion

    #region Offset and Limit Tests

    [Fact]
    public void Offset_With_Null_Should_Return_Original()
    {
        var data = GetTestData().AsQueryable();

        var result = data.Offset(null).ToList();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void Offset_With_Value_Should_Skip_Items()
    {
        var data = GetTestData().AsQueryable();

        var result = data.Offset(1).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Limit_With_Null_Should_Return_Original()
    {
        var data = GetTestData().AsQueryable();

        var result = data.Limit(null).ToList();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void Limit_With_Value_Should_Take_Items()
    {
        var data = GetTestData().AsQueryable();

        var result = data.Limit(2).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Offset_And_Limit_Combined_Should_Work()
    {
        var data = Enumerable.Range(1, 10).AsQueryable();

        var result = data.Offset(2).Limit(3).ToList();

        Assert.Equal(3, result.Count);
        Assert.Equal(3, result[0]);
        Assert.Equal(5, result[2]);
    }

    #endregion

    #region ForEach Tests

    [Fact]
    public void ForEach_Should_Execute_Action_For_Each_Item()
    {
        var items = new List<int> { 1, 2, 3 };
        var sum = 0;

        items.ForEach(x => sum += x);

        Assert.Equal(6, sum);
    }

    [Fact]
    public void ForEach_With_Null_Collection_Should_Not_Throw()
    {
        IEnumerable<int> items = null;

        var exception = Record.Exception(() => items.ForEach(_ => { }));

        Assert.Null(exception);
    }

    #endregion

    #region Type Extension Tests

    [Fact]
    public void IsConcrete_With_Concrete_Class_Should_Return_True()
    {
        Assert.True(typeof(TestEntity).IsConcrete());
    }

    [Fact]
    public void IsConcrete_With_Abstract_Class_Should_Return_False()
    {
        Assert.False(typeof(AbstractClass).IsConcrete());
    }

    [Fact]
    public void IsConcrete_With_Interface_Should_Return_False()
    {
        Assert.False(typeof(ITestInterface).IsConcrete());
    }

    [Fact]
    public void IsOpenGeneric_With_Open_Generic_Should_Return_True()
    {
        Assert.True(typeof(List<>).IsOpenGeneric());
    }

    [Fact]
    public void IsOpenGeneric_With_Closed_Generic_Should_Return_False()
    {
        Assert.False(typeof(List<string>).IsOpenGeneric());
    }

    [Fact]
    public void CanBeCastTo_With_Same_Type_Should_Return_True()
    {
        Assert.True(typeof(TestEntity).CanBeCastTo(typeof(TestEntity)));
    }

    [Fact]
    public void CanBeCastTo_With_Base_Type_Should_Return_True()
    {
        Assert.True(typeof(DerivedClass).CanBeCastTo(typeof(BaseClass)));
    }

    [Fact]
    public void CanBeCastTo_With_Unrelated_Type_Should_Return_False()
    {
        Assert.False(typeof(TestEntity).CanBeCastTo(typeof(string)));
    }

    #endregion

    #region Fill Tests

    [Fact]
    public void Fill_Should_Add_New_Item()
    {
        var list = new List<int> { 1, 2 };

        list.Fill(3);

        Assert.Contains(3, list);
    }

    [Fact]
    public void Fill_Should_Not_Add_Duplicate()
    {
        var list = new List<int> { 1, 2 };

        list.Fill(2);

        Assert.Equal(2, list.Count);
    }

    #endregion

    private static List<TestEntity> GetTestData() =>
    [
        new TestEntity { Id = 1, Name = "Charlie", Age = 25 },
        new TestEntity { Id = 2, Name = "Alice", Age = 30 },
        new TestEntity { Id = 3, Name = "Bob", Age = 35 }
    ];

    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
    }

    private abstract class AbstractClass { }
    private interface ITestInterface { }
    private class BaseClass { }
    private class DerivedClass : BaseClass { }
}
