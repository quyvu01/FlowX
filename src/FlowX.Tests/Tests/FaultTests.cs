using FlowX.Responses;
using Xunit;

namespace FlowX.Tests.Tests;

public sealed class FaultTests
{
    [Fact]
    public void FromException_Should_Create_Fault_With_Exception_Info()
    {
        var exception = new InvalidOperationException("Test error");

        var fault = Fault.FromException(exception);

        Assert.NotNull(fault);
        Assert.NotNull(fault.FaultId);
        Assert.NotNull(fault.Exceptions);
        Assert.Single(fault.Exceptions);
        Assert.Equal(typeof(InvalidOperationException).FullName, fault.Exceptions[0].ExceptionType);
        Assert.Equal("Test error", fault.Exceptions[0].Message);
    }

    [Fact]
    public void FromException_Should_Include_FaultedMessageId()
    {
        var exception = new Exception("Error");
        var messageId = "msg-456";

        var fault = Fault.FromException(exception, messageId);

        Assert.Equal(messageId, fault.FaultedMessageId);
    }

    [Fact]
    public void FromException_Should_Set_Timestamp()
    {
        var beforeCreate = DateTime.UtcNow;

        var fault = Fault.FromException(new Exception("Error"));

        Assert.True(fault.Timestamp >= beforeCreate);
        Assert.True(fault.Timestamp <= DateTime.UtcNow);
    }

    [Fact]
    public void FromException_Should_Include_Host_Info()
    {
        var fault = Fault.FromException(new Exception("Error"));

        Assert.NotNull(fault.Host);
        Assert.Equal(Environment.MachineName, fault.Host.MachineName);
        Assert.True(fault.Host.ProcessId > 0);
    }

    [Fact]
    public void FromException_Should_Capture_Inner_Exception_Chain()
    {
        var innermost = new ArgumentNullException("param", "Innermost");
        var middle = new InvalidOperationException("Middle", innermost);
        var outer = new Exception("Outer", middle);

        var fault = Fault.FromException(outer);

        Assert.Equal(3, fault.Exceptions.Length);
        Assert.Equal("Outer", fault.Exceptions[0].Message);
        Assert.Equal("Middle", fault.Exceptions[1].Message);
        Assert.Contains("Innermost", fault.Exceptions[2].Message);
    }

    [Fact]
    public void FromException_Should_Capture_StackTrace()
    {
        Exception exception;
        try
        {
            throw new Exception("Test with stack trace");
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        var fault = Fault.FromException(exception);

        Assert.NotNull(fault.Exceptions[0].StackTrace);
        Assert.Contains("FaultTests", fault.Exceptions[0].StackTrace);
    }

    [Fact]
    public void ToException_Should_Reconstruct_Exception()
    {
        var original = new InvalidOperationException("Original error");
        var fault = Fault.FromException(original);

        var reconstructed = fault.ToException();

        Assert.NotNull(reconstructed);
        Assert.IsType<FaultException>(reconstructed);
        Assert.Equal("Original error", reconstructed.Message);
    }

    [Fact]
    public void ToException_Should_Preserve_Original_Exception_Type()
    {
        var original = new ArgumentException("Arg error");
        var fault = Fault.FromException(original);

        var reconstructed = fault.ToException() as FaultException;

        Assert.Equal(typeof(ArgumentException).FullName, reconstructed!.OriginalExceptionType);
    }

    [Fact]
    public void ToException_Should_Reconstruct_Inner_Exception_Chain()
    {
        var inner = new ArgumentNullException("param");
        var outer = new InvalidOperationException("Outer", inner);
        var fault = Fault.FromException(outer);

        var reconstructed = fault.ToException();

        Assert.NotNull(reconstructed.InnerException);
        var innerReconstructed = reconstructed.InnerException as FaultException;
        Assert.NotNull(innerReconstructed);
        Assert.Equal(typeof(ArgumentNullException).FullName, innerReconstructed.OriginalExceptionType);
    }

    [Fact]
    public void ToException_With_Empty_Exceptions_Should_Return_Null()
    {
        var fault = new Fault { Exceptions = [] };

        var exception = fault.ToException();

        Assert.Null(exception);
    }

    [Fact]
    public void ToException_With_Null_Exceptions_Should_Return_Null()
    {
        var fault = new Fault { Exceptions = null };

        var exception = fault.ToException();

        Assert.Null(exception);
    }

    [Fact]
    public void FaultException_ToString_Should_Include_Original_Type()
    {
        var exceptionInfo = new ExceptionInfo
        {
            ExceptionType = "System.InvalidOperationException",
            Message = "Test message",
            StackTrace = "at Test.Method()"
        };
        var faultException = new FaultException(exceptionInfo);

        var str = faultException.ToString();

        Assert.Contains("System.InvalidOperationException", str);
        Assert.Contains("Test message", str);
        Assert.Contains("at Test.Method()", str);
    }

    [Fact]
    public void HostInfo_Current_Should_Return_Valid_Info()
    {
        var hostInfo = HostInfo.Current;

        Assert.NotNull(hostInfo);
        Assert.NotEmpty(hostInfo.MachineName);
        Assert.NotEmpty(hostInfo.ProcessName);
        Assert.True(hostInfo.ProcessId > 0);
        Assert.NotEmpty(hostInfo.Assembly);
    }
}
