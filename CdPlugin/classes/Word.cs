using CdPlugin;

namespace Codata.scripts.classes;

public class Word
{
    public Langue langue;
    public string key;
    public string default_ = "";
    public Word(string key, Langue langue)
    {
        this.langue = langue;
        this.key = key;
    }

    public Word(string @default)
    {
        default_ = @default;
    }

    public Word(string @default, string key,  Langue langue)
    {
        this.langue = langue;
        this.default_ = @default;
        this.key = key;
    }

    public Word()
    {

    }

    public string Get()
    {
        if (langue == null)
        {
            return default_;
        }
        string r = langue.Get(key);
        return string.IsNullOrEmpty(r) ? default_ : r;
    }

    public static implicit operator string(Word word ) => word.Get();
    public static implicit operator Word(string str) => new(str);
    public static Word word(string key, string @default) => new Word(@default, key, Center.langue);
    /// <summary>
    /// add "_dcr" suffix to key
    /// </summary>
    public static Word worddcr(string key, string @default) => new Word(@default, key+ "_dcr", Center.langue);
}