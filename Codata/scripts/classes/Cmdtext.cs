namespace Codata.scripts.classes;

public class Cmdtext : TextBox
{
    /// <summary>
    /// real text end index
    /// </summary>
    public int textIndex => Text.Length;

    public event Action<object?, EventArgs> WhenTextChanged;
    public bool canChange = true;

    public Label hint;
    public Cmdtext()
    {
        TextChanged += (a, b) =>
        {
            if (canChange)
                WhenTextChanged?.Invoke(a,b);
        };

        hint = new Label();

        hint.ForeColor = Color.DimGray;
        hint.BackColor = Color.Transparent;
        hint.TextAlign = ContentAlignment.MiddleLeft;
        hint.Size = new(300, 21);
    }

    public void SetSuggestion(string suggestion)
    {
        canChange = false;

        int length = textIndex;

        Text = Text.Substring(0, length);

        hint.Text = suggestion;
        Size size = TextRenderer.MeasureText(
            Text.TrimEnd(' '),
            Font
        );

        hint.Location = new Point(
            Left + size.Width,
            Top + 2
        );
        hint.BackColor = Color.White;
        hint.Font = Font;

        canChange = true;

        ReplaceSelectior();
    }

    /// <summary>
    /// place selector to last real text character
    /// </summary>
    public void ReplaceSelectior() => Selector(textIndex, 0);
    /// <param name="length">can be negative</param>
    public void Selector(int start, int length)
    {
        SelectionStart = start;
        SelectionLength = length;
    }

    public void Append(string append) => Text = Text.Insert(textIndex, append);

    public void Clear(bool alsoTag = false)
    {
        if (alsoTag)
            Text = string.Empty;
        Text = Text.Remove(0, textIndex);
    }
    //public string getText => Text.Substring(0, textIndex);
}