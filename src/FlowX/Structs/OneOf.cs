using System.Diagnostics;
using System.Text.Json;
using FlowX.Abstractions;
using FlowX.Extensions;
using FlowX.Messages;

namespace FlowX.Structs;

public interface IOneOf
{
    object Value { get; }
}

public readonly struct OneOf<T0, T1> : IOneOf, IMessageSerialized
{
    private readonly T0 _t0;
    private readonly T1 _t1;
    public T0 AsT0 => IsT0 ? _t0 : throw new Exception("The value is not T0!");
    public T1 AsT1 => IsT1 ? _t1 : throw new Exception("The value is not T1!");
    public bool IsT0 { get; }
    public bool IsT1 { get; }

    public object Value => IsT0 ? AsT0 : IsT1 ? AsT1 : throw new UnreachableException();

    public MessageSerialized Serialize()
    {
        var type = IsT0 ? typeof(T0).GetAssemblyName() : typeof(T1).GetAssemblyName();
        return new MessageSerialized
            { Type = type, ObjectSerialized = Value is not null ? JsonSerializer.Serialize(Value) : null };
    }
    
    public static OneOf<T0, T1> DeSerialize(MessageSerialized message)
    {
        var type = Type.GetType(message.Type);
        if (type is null) throw new ArgumentNullException($"Type {message.Type} not found!");
        var value = JsonSerializer.Deserialize(message.ObjectSerialized, type);
        return new OneOf<T0, T1>(value);
    }

    public void Switch(Action<T0> t0Action, Action<T1> t1Action)
    {
        if (IsT0)
        {
            t0Action.Invoke(_t0);
            return;
        }

        t1Action.Invoke(_t1);
    }

    public Task SwitchAsync(Func<T0, Task> t0Func, Func<T1, Task> t1Func) =>
        IsT0 ? t0Func.Invoke(_t0) : t1Func.Invoke(_t1);

    public TResult Match<TResult>(Func<T0, TResult> t0Func, Func<T1, TResult> t1Resul) =>
        IsT0 ? t0Func.Invoke(_t0) : t1Resul.Invoke(_t1);

    public Task<TResult> MatchAsync<TResult>(Func<T0, Task<TResult>> t0FuncTask, Func<T1, Task<TResult>> t1FuncTask) =>
        IsT0 ? t0FuncTask.Invoke(_t0) : t1FuncTask.Invoke(_t1);

    public OneOf<T, T1> MapT0<T>(Func<T0, T> func)
    {
        if (IsT1) return AsT1;
        return func.Invoke(_t0);
    }
    
    public OneOf<T0, T> MapT1<T>(Func<T1, T> func)
    {
        if (IsT0) return AsT0;
        return func.Invoke(_t1);
    }

    private OneOf(T0 t0)
    {
        _t0 = t0;
        IsT0 = true;
    }

    private OneOf(T1 t1)
    {
        _t1 = t1;
        IsT1 = true;
    }

    // Do not remove it as we call it dynamically!
    public OneOf(object obj)
    {
        switch (obj)
        {
            case T0 t0:
                _t0 = t0;
                IsT0 = true;
                return;
            case T1 t1:
                _t1 = t1;
                IsT1 = true;
                return;
            default:
                throw new Exception($"Object is not {typeof(T0).Name} or {typeof(T1).Name}");
        }
    }

    public static OneOf<T0, T1> FromT0(T0 to) => new(to);
    public static OneOf<T0, T1> FromT1(T1 t1) => new(t1);

    public static implicit operator OneOf<T0, T1>(T0 t0) => new(t0);
    public static implicit operator OneOf<T0, T1>(T1 t1) => new(t1);

    public override string ToString() => JsonSerializer.Serialize(Value);
}