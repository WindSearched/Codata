using CdPlugin;
using MoonSharp.Interpreter;

namespace Codata.scripts.classes;

public class LuaFunc<T>(Closure closure) : ICdFunc<T>
{
    public Closure closure = closure;
    public T Invoke()
    {
        var r = Center.luascript.Call(closure);
        return r.ToObject<T>();
    }
}
public class LuaFunc<I, O>(Closure closure) : ICdFunc<I, O>
{
    public Closure closure = closure;
    public O Invoke(I input)
    {
        var r = Center.luascript.Call(closure,
            DynValue.FromObject(Center.luascript, input));
        return r.ToObject<O>();
    }
}

public class LuaAction(Closure closure) : ICdAction
{
    public Closure closure = closure;

    public void Invoke() => Center.luascript.Call(closure);

    public void Clear() => closure = null;

}