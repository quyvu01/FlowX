using System.Diagnostics;

namespace FlowX.Structs;

public readonly struct OneOf<T0, T1>
{
    private readonly T0 _t0;
    private readonly T1 _t1;
    public T0 AsT0 => IsT0 ? _t0 : throw new Exception("The value is not T0!");
    public T1 AsT1 => IsT1 ? _t1 : throw new Exception("The value is not T1!");
    public bool IsT0 { get; }
    public bool IsT1 { get; }

    public object Value => IsT0 ? AsT0 : IsT1 ? AsT1 : throw new UnreachableException();

    public void Switch(Action<T0> t0Action, Action<T1> t1Action)
    {
        if (IsT0)
        {
            t0Action.Invoke(_t0);
            return;
        }

        t1Action.Invoke(_t1);
    }

    public Task SwitchAsync(Func<T0, Task> t0Func, Func<T1, Task> t1Func)
    {
        if (IsT0) return t0Func.Invoke(_t0);

        t1Func.Invoke(_t1);
        return Task.CompletedTask;
    }

    public TResult Match<TResult>(Func<T0, TResult> t0Func, Func<T1, TResult> t1Resul) =>
        IsT0 ? t0Func.Invoke(_t0) : t1Resul.Invoke(_t1);

    public Task<TResult> MatchAsync<TResult>(Func<T0, Task<TResult>> t0FuncTask, Func<T1, Task<TResult>> t1FuncTask) =>
        IsT0 ? t0FuncTask.Invoke(_t0) : t1FuncTask.Invoke(_t1);

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

    public static implicit operator OneOf<T0, T1>(T0 t0) => new(t0);
    public static implicit operator OneOf<T0, T1>(T1 t1) => new(t1);
}