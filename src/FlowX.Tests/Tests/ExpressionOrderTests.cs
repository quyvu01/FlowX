using FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryManyFlow;
using Xunit;

namespace FlowX.Tests.Tests;

public sealed class ExpressionOrderTests
{
    [Fact]
    public void Of_With_Ascending_Should_Create_Single_Ascending_Order()
    {
        var order = ExpressionOrder<TestEntity>.Of(x => x.Name);

        Assert.Single(order.ExpressionDetails);
        Assert.True(order.ExpressionDetails[0].IsAsc);
    }

    [Fact]
    public void Of_With_Descending_Should_Create_Single_Descending_Order()
    {
        var order = ExpressionOrder<TestEntity>.Of(x => x.Name, false);

        Assert.Single(order.ExpressionDetails);
        Assert.False(order.ExpressionDetails[0].IsAsc);
    }

    [Fact]
    public void ThenBy_Should_Add_Ascending_Order()
    {
        var order = ExpressionOrder<TestEntity>.Of(x => x.Name)
            .ThenBy(x => x.Id);

        Assert.Equal(2, order.ExpressionDetails.Count);
        Assert.True(order.ExpressionDetails[0].IsAsc);
        Assert.True(order.ExpressionDetails[1].IsAsc);
    }

    [Fact]
    public void ThenDescBy_Should_Add_Descending_Order()
    {
        var order = ExpressionOrder<TestEntity>.Of(x => x.Name)
            .ThenDescBy(x => x.Id);

        Assert.Equal(2, order.ExpressionDetails.Count);
        Assert.True(order.ExpressionDetails[0].IsAsc);
        Assert.False(order.ExpressionDetails[1].IsAsc);
    }

    [Fact]
    public void Chaining_Multiple_ThenBy_Should_Work()
    {
        var order = ExpressionOrder<TestEntity>.Of(x => x.Name)
            .ThenDescBy(x => x.Id)
            .ThenBy(x => x.CreatedAt)
            .ThenDescBy(x => x.Email);

        Assert.Equal(4, order.ExpressionDetails.Count);
        Assert.True(order.ExpressionDetails[0].IsAsc);   // Name asc
        Assert.False(order.ExpressionDetails[1].IsAsc);  // Id desc
        Assert.True(order.ExpressionDetails[2].IsAsc);   // CreatedAt asc
        Assert.False(order.ExpressionDetails[3].IsAsc);  // Email desc
    }

    [Fact]
    public void ExpressionDetails_Should_Be_ReadOnly()
    {
        var order = ExpressionOrder<TestEntity>.Of(x => x.Name);

        Assert.IsAssignableFrom<IReadOnlyList<ExpressionOrderDetail<TestEntity>>>(order.ExpressionDetails);
    }

    [Fact]
    public void Expression_Should_Be_Preserved()
    {
        var order = ExpressionOrder<TestEntity>.Of(x => x.Name);

        var expression = order.ExpressionDetails[0].Expression;
        var compiled = expression.Compile();
        var entity = new TestEntity { Name = "Test" };

        Assert.Equal("Test", compiled(entity));
    }

    [Fact]
    public void Multiple_Expressions_Should_Preserve_Order()
    {
        var order = ExpressionOrder<TestEntity>.Of(x => x.Name)
            .ThenBy(x => x.Email);

        var entity = new TestEntity { Name = "John", Email = "john@test.com" };

        var nameValue = order.ExpressionDetails[0].Expression.Compile()(entity);
        var emailValue = order.ExpressionDetails[1].Expression.Compile()(entity);

        Assert.Equal("John", nameValue);
        Assert.Equal("john@test.com", emailValue);
    }

    [Fact]
    public void ImplicitConversion_From_Expression_Should_Create_Ascending_Order()
    {
        System.Linq.Expressions.Expression<Func<TestEntity, object>> expr = x => x.Name;
        ExpressionOrder<TestEntity> order = expr;

        Assert.Single(order.ExpressionDetails);
        Assert.True(order.ExpressionDetails[0].IsAsc);
    }

    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
