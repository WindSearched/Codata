using System;
using System.Reflection;
using System.Windows.Forms;
using Codata.scripts;
using Codata.scripts.classes;
using Codata.scripts.commandBranches;

class MainForm : Form
{
	public event Action OnEnterKeyDown;
	public event Action<string> OnKeyDown;
	public event Action OnTabDown;
	public event Action OnDownDown;
	public event Action OnUpDown;
	public event Action OnShiftDown;
	public event Action OnShiftUp;
	public bool shift;

	public ListBox  ListBox;
	public Cmdtext textBox;
	public RichTextBox rtb;
	public MainForm()
	{
		this.Text = "Codata";

		this.ClientSize = new Size(800, 400);

		Cmdtext textBox = this.textBox = new ();
		//textBox.BorderStyle = BorderStyle.None;
		//textBox.Multiline = false;
		textBox.Font = new("Calibri", 11);
		textBox.Size = new Size(645, 23);
		//textBox.Width = 645;
		textBox.PreviewKeyDown += (s, e) =>
		{
			if (e.KeyCode == Keys.Tab ||  e.KeyCode == Keys.Shift)
			{
				e.IsInputKey = true; //告诉系统这是输入键
			}
		};
		textBox.KeyDown += (s, e) =>
		{
			if (e.KeyCode == Keys.Tab)
			{
				e.SuppressKeyPress = true; // 阻止跳转
				OnTabDown?.Invoke();
			}
		};
		//Panel box = new Panel();

		//box.BorderStyle = BorderStyle.FixedSingle;
		//box.Size = textBox.Size;
		textBox.Location = new System.Drawing.Point(40, 347);

		//textBox.Dock = DockStyle.Fill;

		Controls.Add(textBox.hint);
		//box.Controls.Add(textBox);
		Controls.Add(textBox);

		Button button = new Button();
		button.Text = "enter";
		Program.Log(button.Font.Name);
		button.Location = new System.Drawing.Point(685, 347);
		void click()
		{
			string cmd = textBox.Text.Trim(' ');

			var result = Program.command.Command(cmd);

			Log(cmd + "\n" + result.put);
			textBox.Text = "";

			Program.commandTube.Add(cmd);
			Program.commandTube.RevertPointer();
		}
		Program.Log(button.Width);
		button.Click += (s, e) => click();
		OnEnterKeyDown += click;
		textBox.KeyDown += Keydown;
		textBox.KeyUp += Keyup;

		Controls.Add(button);
		this.KeyPreview = true;

		//list
		ListBox listBox = ListBox = new ListBox();
		listBox.Location = new System.Drawing.Point(590, 40);
		listBox.Width = 170;
		listBox.Height = 267;  // 超过高度就会自动出现滚动条
		this.Controls.Add(listBox);

		RichTextBox rtb = this.rtb = new RichTextBox();
		rtb.ReadOnly = true;
		rtb.ScrollBars = RichTextBoxScrollBars.Vertical;
		rtb.Location = new Point(40, 40);
		rtb.Width = 510;
		rtb.Height = 267;
		Controls.Add(rtb);
		rtb.Text += Program.PutUser();
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
			case Keys.ShiftKey:
				OnShiftDown?.Invoke();
				break;
		}
	}

	private void Keyup(object sender, KeyEventArgs e)
	{
		OnKeyDown?.Invoke(e.KeyCode.ToString());

		switch (e.KeyCode)
		{
			case Keys.ShiftKey:
				OnShiftUp?.Invoke();
				break;
		}
	}

	public void Log(string message, string user = "")
	{
		if (user == "")
			user = GetUser;
		RemovePreviewUser();
		rtb.Text += user + ">" + message;
		SetPreviewUser();
	}

	public string GetUser => Program.info.user;

	public void SetPreviewUser()
	{
		rtb.Text += (rtb.Text.EndsWith('\n') ? "" : "\n") + GetUser + ">";
	}
	public void RemovePreviewUser()
	{
		string t = rtb.Text;
		string u = GetUser + ">";
		rtb.Text = t.Remove(t.Length - u.Length, u.Length);
	}
}


class Program
{
	public static CommandBranch command;
	public static SpecialPointTube<string> commandTube = new(32);
	public static MainForm form;
	public static Info info;
	public static CdAction afterConfirm;
	public static event Action OnProgramClose;
	public static PointCapturer capturer;

	[STAThread]
	static void Main()
	{
		capturer = new();

		RegisterCommands();
		Data.Init();
		Commands.Init();
		Lua.Init();
		info = Info.ReadJson(Data.infoPath);
		afterConfirm = new(Lua.script);

		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);
		form = new MainForm();
		form.textBox.WhenTextChanged += (_,_) =>
		{
			var s = form.textBox.Text;
			var l = s.Split(' ');

			var v = command.GetSuggestions(l.ToList());
			form.ListBox.DataSource = v.list;
			form.textBox.SetSuggestion(v.tag);
		};

		BindingKeys();

		form.ListBox.DataSource = command.branches.Select(v => v.name).ToList();

		Application.Run(form);

		//when program close
		OnProgramClose?.Invoke();
		Info.WriteJson(Data.infoPath, info);
	}

	static void RegisterCommands()
	{
		command = new CommandBranch("codata")
				.SetSuggestion(b =>  b.branches.Select(v => v.name).ToList())
				.AddBranch(new CommandBranch("author")
					.Execute(_ => new(Info.author,true))
					.AddBranches(
						new CommandBranch("bilibili")
							.Execute(_ => new(Info.bilibili,true))
							.AddBranch(new CommandBranch("open")
								.Execute(_ =>
								{
									Data.OpenForm(Info.bilibili);
									return new(true);
								})
							),
						new CommandBranch("gitHub")
							.Execute(_ => new(Info.gitHub,true))
							.AddBranch(new CommandBranch("open")
								.Execute(_ =>
								{
									Data.OpenForm(Info.gitHub);
									return new(true);
								})
							),
						new CommandBranch("codataRepotory")
							.Execute(_ => new(Info.codataGit, true))
							.AddBranch(new CommandBranch("open")
								.Execute(_ =>
								{
									Data.OpenForm(Info.codataGit);
									return new(true);
								})
							)
					)
				)
				.AddBranch(new CommandBranch("add")
					.AddArguments(new("a"),new("b"))
					.Execute(arg =>
					{
						Result r =  new Result(true);
						if (!float.TryParse(arg.Get("a"), out float a)) return r;
						if (float.TryParse(arg.Get("b"), out float b))
						{
							r.put = (a + b).ToString();
						}
						return r;
					}))
				.AddBranch(new CommandBranch("print")
					.AddArgument(new CommandBranch.Argument("name")
						.SetSuggestion(() => new List<string>
						{
							"wind","searched", "helloworld", "ciao", "hi", "114514"
						}))
					.Execute(arg => new(arg.Get("name"), true)))
				.AddBranch(new CommandBranch("open")
					.AddArgument(new CommandBranch.Argument("path"))
					.Execute(arg =>
					{
						string path = arg.Get("path");
						if (Data.FileExists(path))
						{
							Data.OpenForm(path);
							return new(true);
						}
						else
							return new(false);
					})
				)
				.AddBranch(new CommandBranch("user")
					.AddArgument(new CommandBranch.Argument("name"))
					.Execute(arg =>
					{
						string n = arg.Get("name");
						if (n == "")
							return new("changes name is null", false);
						info.user = n;
						return new("change succeed",true);

					})
				)
				.AddBranch(new CommandBranch("setting")
					.AddBranch(new CommandBranch("recover")
						.Execute(arg =>
						{
							Tools.SetAfterConfirm(() =>
							{
								Program.Log("test");
							 	info = new();
							});
							return new Result("input confirm command to execute this command",true);
						})
					)
					.AddArgument(new CommandBranch.Argument("name")
						.SetSuggestion(() => Tools.ReflectionHelper.GetFieldsString(info))
					)
					.AddArgument(new CommandBranch.Argument("value"))
					.Execute(arg =>
					{
						if (!arg.TryGet("name", out var name))
						{
							return new("changes name is null", false);
						}
						if (!arg.TryGet("value", out var value))
						{
							return new("changes value is null", false);
						}
						Tools.ReflectionHelper.SetFieldFromString(info, name, value);

						Tools.DebugLog(value);

						return new(true);
					})
				)
				.AddBranch(new CommandBranch("confirm")
					.Execute(arg =>
					{
						afterConfirm?.Invoke();
						afterConfirm.Clear();
						return new Result("confirm",true);
					})
				)
				.AddBranch(new CommandBranch("exit")
					.Execute(arg =>
					{
						if (info.confirmToExit)
						{
							Tools.SetAfterConfirm(() => form.Close());
							return Result.confirm;
						}
						else
						{
							form.Close();
							return new(true);
						}
					})
					.AddBranch(new CommandBranch("confirm")
						.Execute(arg =>
						{
							Tools.SetAfterConfirm(() => form.Close());
							return Result.confirm;
						})
					)
				)
				.AddBranch(SMath.math)
				.AddBranch(new CommandBranch("capture")
					.Execute(arg =>
					{
						Program.capturer.Start(3,
							ps =>
							{
								string s = "";
								foreach (var p in ps)
								{
									s += p + "\n";
								}

								s = s.TrimEnd('\n');
								return s;
							});
						return new Result(true);
					})
				)
			;
		command.AddBranch(command);

	}

	static void BindingKeys()
	{
		form.OnTabDown += () =>
		{
			var box = form.ListBox;
			if(box.SelectedItem == null) return;
			var s = box.SelectedItem.ToString();

			var c = form.textBox.Text;
			int index = c.LastIndexOf(' ');

			c = index != -1 ? c.Substring(0, index+1) : "";
			c += s + ' ';

			var t = form.textBox;
			t.canChange = false;
			t.Clear();
			t.canChange = true;
			t.Append(c);
		};
		form.OnDownDown += () =>
		{
			int i = form.ListBox.SelectedIndex;
			int max = form.ListBox.Items.Count -1;

			if (form.shift)
			{
				if (commandTube.pointer == -1)
				{
					commandTube.specialGetter = form.textBox.Text;
				}
				if (commandTube.TryPointBefore(out var result))
				{
					form.textBox.Text = result;
				}
			}
			else
			{
				if (max >= 0)
				{
					i = i == max ? 0 : i + 1;
					form.ListBox.SelectedIndex = i;
				}
			}

		};
		form.OnUpDown += () =>
		{
			int i = form.ListBox.SelectedIndex;
			int max = form.ListBox.Items.Count -1;

			if (form.shift)
			{
				if (commandTube.pointer == -1)
				{
					commandTube.specialGetter = form.textBox.Text;

				}
				if (commandTube.TryPointAfter(out var result))
				{
					form.textBox.Text = result;
				}
			}
			else
			{
				if (max >= 0)
				{
					i = i == 0 ? max : i - 1;
					form.ListBox.SelectedIndex = i;
				}
			}

		};
		form.OnShiftDown += () => form.shift = true;
		form.OnShiftUp += () => form.shift = false;
	}

	public static void Log(object message)
	{
		Console.WriteLine(message);
	}

	public static string PutUser() => info.user + ">";
}
