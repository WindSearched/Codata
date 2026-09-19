using CdPlugin;
using MoonSharp.Interpreter;

namespace Codata.scripts.classes;

public class LuaFunc<T>(Closure closure) : ICdFunc<T>
{
    public Closure closure = closure;
    public T Invoke()
    {
        var r = Lua.script.Call(closure);
        return r.ToObject<T>();
    }
}
public class LuaFunc<I, O>(Closure closure) : ICdFunc<I, O>
{
    public Closure closure = closure;
    public O Invoke(I input)
    {
        var r = Lua.script.Call(closure,
            DynValue.FromObject(Lua.script, input));
        return r.ToObject<O>();
    }
}

public class LuaAction(Closure closure) : ICdAction
{
    public Closure closure = closure;

    public void Invoke() => Lua.script.Call(closure);

    public void Clear() => closure = null;

}