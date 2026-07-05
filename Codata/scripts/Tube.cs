namespace Codata.scripts;

public class Tube<T>
{
    public List<T> list;
    public int length;

    public Tube(int length)
    {
        list = new();
        this.length = length;
    }

    public void Add(T item)
    {
        list.Add(item);
        if (list.Count > length)
        {
            list.RemoveAt(list.Count - 1);
        }
    }

    public T Get(int index) => list[index];
}
public class PointTube<T>(int length) : Tube<T>(length)
{
    public int pointer;

    public virtual T PointAfter() => Get(++pointer);
    public virtual bool TryPointAfter(out T result)
    {
        if (pointer + 1 <= list.Count - 1)
        {
            result = Get(++pointer);
            return true;
        }

        result = default;
        return false;
    }
    public virtual T PointBefore() => Get(--pointer);

    public virtual bool TryPointBefore(out T result)
    {
        if (pointer - 1 >= 0)
        {
            result = Get(--pointer);
            return true;
        }
        result = default;
        return false;
    }

    protected int revertPonierIndex = 0;
    public void RevertPointer() => pointer = revertPonierIndex;
    public void PointerSet(int pointed) => pointer = pointed;
    public T Point(int pointed, bool setPointer = true)
    {
        if(setPointer)
        {
            Get(pointed);
        }
        return Get(pointed);;
    }
}

public class SpecialPointTube<T> : PointTube<T>
{
    public T specialGetter;

    public SpecialPointTube(int length) : base(length)
    {
        pointer = -1;
        revertPonierIndex = -1;
    }

    public override T PointBefore()
    {
        return --pointer == -1 ? specialGetter : Get(pointer);
    }

    public override bool TryPointBefore(out T result)
    {
        if (pointer -1 == -1)
        {
            pointer--;
            if (specialGetter != null)
            {
                result = specialGetter;
                return true;
            }
        }
        else if (pointer -1 > -1)
        {
            result = Get(pointer);
        }
        result = default;
        return false;
    }
}