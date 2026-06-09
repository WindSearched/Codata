using System;
using System.Windows.Forms;
using Codata.scripts;

class MainForm : Form
{
    public event Action OnEnterKeyDown;
    public event Action<string> OnKeyDown;

    public ListBox  ListBox;
    public TextBox textBox;
    public MainForm()
    {
        this.Text = "Codata";
        this.Width = 800;
        this.Height = 600;

        TextBox textBox = this.textBox = new TextBox();
        textBox.Location = new System.Drawing.Point(50, 50);
        textBox.Width = 200;

        Button button = new Button();
        button.Text = "enter";
        button.Location = new System.Drawing.Point(50, 100);
        void click()
        {
            Program.command.Command(textBox.Text);
            MessageBox.Show("输出：" + Program.output);
            Program.output = "";
        }
        button.Click += (s, e) => click();
        OnEnterKeyDown += click;
        textBox.KeyDown += (s, e) =>
        {
            OnKeyDown?.Invoke(e.KeyCode.ToString());

            if (e.KeyCode == Keys.Enter)
            {
                Console.WriteLine("enter");
                OnEnterKeyDown?.Invoke();
            }
        };

        Controls.Add(textBox);
        Controls.Add(button);
        this.KeyPreview = true;

        //list
        ListBox listBox = ListBox = new ListBox();
        listBox.Location = new System.Drawing.Point(400, 50);
        listBox.Width = 200;
        listBox.Height = 150;  // 超过高度就会自动出现滚动条

        this.Controls.Add(listBox);
    }
    private void Keydown(object sender, KeyEventArgs e)
    {
        OnKeyDown?.Invoke(e.KeyCode.ToString());

        if (e.KeyCode == Keys.Enter)
        {
            Console.WriteLine("enter");
            OnEnterKeyDown?.Invoke();
        }
    }
}


class Program
{
    private static string author = "WindSearched";
    public static CommandBranch command;
    public static string output = "";
    public static MainForm form;

    public static string addOutput
    {
        set => output = value;
    }

    [STAThread]
    static void Main()
    {
        RegisterCommands();

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        form = new MainForm();
        form.OnKeyDown += downed =>
        {
            var s = form.textBox.Text + downed;
            var l = s.Split(' ');
            form.ListBox.Items.Clear();
            form.ListBox.DataSource = command.GetSuggestions(l.ToList());;
        };

        Application.Run(form);
    }

    static void RegisterCommands()
    {
        command = new CommandBranch("codata")
                .AddBranch(new CommandBranch("author")
                    .Execute(_ =>
                    {
                        addOutput = author;
                        return true;
                    }))
                .AddBranch(new CommandBranch("add")
                    .AddArguments(new("a"),new("b"))
                    .Execute(arg =>
                    {
                        if(float.TryParse(arg.Get("a"), out float a))
                            if (float.TryParse(arg.Get("b"), out float b))
                            {
                                addOutput = (a + b).ToString();
                            }
                        return false;
                    }))
                .AddBranch(new CommandBranch("print")
                    .AddArgument(new CommandBranch.Argument("name")
                        .SetSuggestion(() => new List<string>
                        {
                            "wind","searched", "helloworld", "ciao", "hi", "114514"
                        })))
            ;
    }
}
