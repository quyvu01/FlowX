using FlowX.Responses;
using Xunit;

namespace FlowX.Tests.Tests;

public sealed class ResultTests
{
    [Fact]
    public void Success_Should_Create_Successful_Result()
    {
        var data = "test data";

        var result = Result<string>.Success(data);

        Assert.True(result.IsSuccess);
        Assert.Equal(data, result.Data);
        Assert.Null(result.Fault);
    }

    [Fact]
    public void Success_With_Null_Data_Should_Still_Be_Successful()
    {
        var result = Result<string>.Success(null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Null(result.Fault);
    }

    [Fact]
    public void Success_With_Complex_Object_Should_Preserve_Data()
    {
        var data = new TestData { Id = 1, Name = "Test" };

        var result = Result<TestData>.Success(data);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Data.Id);
        Assert.Equal("Test", result.Data.Name);
    }

    [Fact]
    public void Failed_With_Fault_Should_Create_Failed_Result()
    {
        var fault = Fault.FromException(new Exception("Test error"));

        var result = Result<string>.Failed(fault);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.NotNull(result.Fault);
        Assert.Equal(fault, result.Fault);
    }

    [Fact]
    public void Failed_With_Exception_Should_Create_Fault_Automatically()
    {
        var exception = new InvalidOperationException("Operation failed");

        var result = Result<string>.Failed(exception);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.NotNull(result.Fault);
        Assert.NotNull(result.Fault.Exceptions);
        Assert.Single(result.Fault.Exceptions);
        Assert.Equal("Operation failed", result.Fault.Exceptions[0].Message);
    }

    [Fact]
    public void Failed_With_Exception_And_MessageId_Should_Include_MessageId()
    {
        var exception = new Exception("Error");
        var messageId = "msg-123";

        var result = Result<string>.Failed(exception, messageId);

        Assert.False(result.IsSuccess);
        Assert.Equal(messageId, result.Fault.FaultedMessageId);
    }

    [Fact]
    public void Failed_Result_Data_Should_Be_Default()
    {
        var result = Result<int>.Failed(new Exception("Error"));

        Assert.False(result.IsSuccess);
        Assert.Equal(default, result.Data);
    }

    [Fact]
    public void Success_Result_With_ValueType_Should_Work()
    {
        var result = Result<int>.Success(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Data);
    }

    [Fact]
    public void Success_Result_With_Collection_Should_Work()
    {
        var list = new List<string> { "a", "b", "c" };

        var result = Result<List<string>>.Success(list);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Data.Count);
    }

    private class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
