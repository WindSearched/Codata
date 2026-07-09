using MoonSharp.Interpreter;

namespace Codata.scripts.classes;

/// <summary>
/// class integrated Function and MoonSharp Closure
/// </summary>
public class CdFunc<T,Tout>
{
    public Script script;
    public Closure closure;
    public Func<T,Tout> func;

    public CdFunc(Script script, Closure closure = null, Func<T, Tout> func = null)
    {
        this.script = script;
        this.closure = closure;
        this.func = func;
    }

    public void Set(Func<T,Tout> func) =>  this.func = func;
    public void Set(Closure closure) => this.closure = closure;

    public Tout Invoke(T val)
    {
        if (closure != null)
        {
            var result = script.Call(
                closure,
                DynValue.FromObject(script, val)
            );

            return result.ToObject<Tout>();
        }

        // C# fallback
        return func == null ? default : func(val);
    }
}