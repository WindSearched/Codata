namespace Codata.scripts.classes;

public interface ICdFunc<T>
{
    public T Invoke();
}
public class CdFunc<T> : ICdFunc<T>
{
    public Func<T>? func;
    public T Invoke() => func.Invoke();
    public CdFunc(){}
    public CdFunc(Func<T> func) => this.func = func;
    public void Set(Func<T> func) => this.func = func;
}

public interface ICdFunc<I,O>
{
    public O Invoke(I input);
}
public class CdFunc<I, O> : ICdFunc<I, O>
{
    public Func<I, O> func;
    public O Invoke(I input) => func.Invoke(input);

    public CdFunc(){}
    public CdFunc(Func<I, O> func) => this.func = func;
    public void Set(Func<I,O> func) => this.func = func ?? throw new ArgumentNullException(nameof(func));
}

public interface ICdAction
{
    public void Invoke();
    public void Clear();
}

public class CdAction : ICdAction
{
    public Action action;
    public void Invoke() =>  action.Invoke();

    public CdAction(){}
    public CdAction(Action action) => this.action = action;
    public void Clear() => action = null;
}