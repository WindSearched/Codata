namespace Codata.scripts.commandBranches;

public static class SMath
{
    public static int Add(params int[] vals)
    {
        if (vals == null || vals.Length == 0) return 0;
        int r = vals[0];
        for (int i = 1; i < vals.Length; i++)
        {
            r += vals[i];
        }
        return r;
    }
    public static float Add(params float[] vals)
    {
        if (vals == null || vals.Length == 0) return 0;
        float r = vals[0];
        for (int i = 1; i < vals.Length; i++)
        {
            r += vals[i];
        }
        return r;
    }
    public static int Subtract(params int[] vals)
    {
        if (vals == null || vals.Length == 0) return 0;
        int r = vals[0];
        for (int i = 1; i < vals.Length; i++)
        {
            r -= vals[i];
        }
        return r;
    }
    public static float Subtract(params float[] vals)
    {
        if (vals == null || vals.Length == 0) return 0;
        float r = vals[0];
        for (int i = 1; i < vals.Length; i++)
        {
            r -= vals[i];
        }
        return r;
    }

    /// <summary>
    /// get the greatest common divisor of two numbers
    /// </summary>
    public static int GCD(int a, int b)
    {
        while (b != 0)
        {
            (a, b) = (b, a % b);
        }
        return Math.Abs(a);
    }
    /// <summary>
    /// get the greatest common divisor
    /// </summary>
    public static int GCD(params int[] numbers)
    {
        if (numbers == null || numbers.Length == 0) return 0;

        int res = numbers[0];
        for (int i = 1; i < numbers.Length; i++)
        {
            res = GCD(res, numbers[i]);
            if(res == 1) return 1;
        }
        return res;
    }

    /// <summary>
    /// get the least common multiple of two numbers
    /// </summary>
    public static int LCM(int a, int b)
    {
        if (a == 0 || b == 0) return 0;
        return Math.Abs(a / GCD(a, b) * b);
    }

    public static int LCM(params int[] numbers)
    {
        if (numbers == null || numbers.Length == 0) return 0;

        int res = numbers[0];
        for (int i = 1; i < numbers.Length; i++)
        {
            res = LCM(res, numbers[i]);
            if(res == 1) return 1;
        }
        return res;
    }

    public static CommandBranch math = new CommandBranch("math")
        .AddBranches(
            new CommandBranch("add")
                .ActiveParam()
                .SetParamExecute(float.Parse, Add, i => new(i.ToString(), true)),
            new CommandBranch("subtract")
                .ActiveParam()
                .SetParamExecute(float.Parse, Subtract, f => new(f.ToString(), true)),
            new CommandBranch("gcd")
                .ActiveParam()
                .Execute(args =>
                {
                    int[] a = new int[args.args.Count];
                    for (int i = 0; i < args.args.Count; i++)
                    {
                        a[i] = args.GetInt(Commands.GetParamIndex(i));
                    }
                    int r =  GCD(a);
                    return new(r.ToString(), r != 0);
                }),
            new CommandBranch("lcm")
                .ActiveParam()
                .Execute(args =>
                {
                    int[] a = new int[args.args.Count];
                    for (int i = 0; i < args.args.Count; i++)
                    {
                        a[i] = args.GetInt(Commands.GetParamIndex(i));
                    }
                    int r =  LCM(a);
                    return new(r.ToString(), r != 0);
                })
            );
}