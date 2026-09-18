namespace Codata.scripts.classes;

public struct CommandLine
{
    public string cmd;
    public int line;

    public CommandLine(int  line, string cmd)
    {
        this.line = line;
        this.cmd = cmd;
    }
}