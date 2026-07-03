using System;
using System.Windows.Forms;
using Codata.scripts;

class MainForm : Form
{
	public event Action OnEnterKeyDown;
	public event Action<string> OnKeyDown;
	public event Action OnTabDown;
	public event Action OnDownDown;
	public event Action OnUpDown;

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
		textBox.PreviewKeyDown += (s, e) =>
		{
			if (e.KeyCode == Keys.Tab)
			{
				e.IsInputKey = true; //告诉系统这是输入键
			}
		};
		textBox.KeyDown += (s, e) =>
		{
			if (e.KeyCode == Keys.Tab)
			{
				e.SuppressKeyPress = true; // 阻止跳转
				Console.WriteLine("tab");
				OnTabDown?.Invoke();
			}
		};

		Button button = new Button();
		button.Text = "enter";
		button.Location = new System.Drawing.Point(50, 100);
		void click()
		{
			Program.command.Command(textBox.Text.Trim(' '));
			MessageBox.Show("输出：" + Program.output);
			Program.output = "";
			textBox.Text = "";
		}
		button.Click += (s, e) => click();
		OnEnterKeyDown += click;
		KeyDown += Keydown;

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

		switch (e.KeyCode)
		{
			case Keys.Enter:
				OnEnterKeyDown?.Invoke();
				break;
			case Keys.Down:
				OnDownDown?.Invoke();
				break;
			case Keys.Up:
				OnUpDown?.Invoke();
				break;
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
		Data.Init();
		Commands.Init();
		Lua.Init();

		//test
		Commands.SetPointerCommand("optest", "open \"C:\\Users\\Public\\Desktop\\Unity Hub.lnk\"");

		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);
		form = new MainForm();
		form.textBox.TextChanged += (_,_) =>
		{
			var s = form.textBox.Text;
			var l = s.Split(' ');
			form.ListBox.DataSource = command.GetSuggestions(l.ToList());;
		};

		BindingKeys();

		Application.Run(form);
	}

	static void RegisterCommands()
	{
		command = new CommandBranch("codata")
				.SetSuggestion(b =>  b.branches.Select(v => v.name).ToList())
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
						}))
					.Execute(arg =>
					{
						output = arg.Get("name");
						return true;
					}))
				.AddBranch(new CommandBranch("open")
					.AddArgument(new CommandBranch.Argument("path"))
					.Execute(arg =>
					{
						string path = arg.Get("path");
						if (Data.FileExists(path))
						{
							Data.OpenForm(path);
							return true;
						}
						else
							return false;
					})
				)

			;
	}

	static void BindingKeys()
	{
		form.OnTabDown += () =>
		{
			Console.WriteLine("OnTabDown");
			var box = form.ListBox;
			if(box.SelectedItem == null) return;
			var s = box.SelectedItem.ToString();

			var c = form.textBox.Text;
			int index = c.LastIndexOf(' ');

			c = index != -1 ? c.Substring(0, index+1) : "";
			c += s + ' ';
			form.textBox.Text = c;
			form.textBox.SelectionStart = form.textBox.Text.Length;
			form.textBox.SelectionLength = 0;
		};
		form.OnDownDown += () =>
		{
			int i = form.ListBox.SelectedIndex;
			int max = form.ListBox.Items.Count -1;
			i = i == max ? 0 : i + 1;

			form.ListBox.SelectedIndex = i;
		};
		form.OnUpDown += () =>
		{
			int i = form.ListBox.SelectedIndex;
			int max = form.ListBox.Items.Count -1;
			i = i == 0 ? max : i - 1;

			form.ListBox.SelectedIndex = i;
		};
	}

	public static void Log(object message)
	{
		Console.WriteLine(message);
	}
}
