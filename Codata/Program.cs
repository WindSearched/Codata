using Codata;
using Codata.scripts;
using Codata.scripts.classes;
using Codata.scripts.commandBranches;

class Program
{
	public static CommandBranch command;
	public static SpecialPointTube<string> commandTube = new(32);
	public static MainForm form;
	public static Info info;
	public static CdAction afterConfirm;
	public static event Action OnProgramClose;
	public static PointCapturer capturer;
	public static Stack<string> argstack = new();
	public static Langue langue;

	[STAThread]
	static void Main()
	{
		capturer = new();

		Data.Init();
		info = Info.ReadJson(Data.infoPath);
		Langue.Init(out langue);
		RegisterCommands();
		Commands.Init();
		Lua.Init();
		afterConfirm = new(Lua.script);

		Log(langue.ToString());

		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);
		form = new MainForm();
		form.textBox.WhenTextChanged += (_,_) =>
		{
			var s = form.textBox.Text;
			var l = Tools.Strings.SplitAndRemoveRepeatedEmpties(s, " ");

			var v = command.GetSuggestions(l.ToList(), out var branch);
			if(info.viewDescription)
				form.descriptionBox.Text = branch.description;
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
				.Execute(_ => new Result("welcome to codata! ฅ՞Ⱉ՞ฅ",true))
				.AddBranch(new CommandBranch("author")
					.SetDescription("some information of Codata's author")
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
				.AddBranch(new CommandBranch("print")
					.SetDescription("print the input, command for test")
					.AddArgument(new CommandBranch.Argument("name")
						.SetSuggestion(() => ["wind", "searched", "helloworld", "ciao", "hi", "114514"]))
					.Execute(arg => new(arg.Get("name"), true)))
				.AddBranch(new CommandBranch("open")
					.SetDescription("open a file, input a path")
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
					.SetDescription("change the user")
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
					.SetDescription("fix the setting information")
					.AddBranch(new CommandBranch("recover")
						.SetDescription("recover the setting, need confirm")
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
					.SetDescription("confirm an operation")
					.Execute(arg =>
					{
						afterConfirm?.Invoke();
						afterConfirm.Clear();
						return new Result("confirm",true);
					})
				)
				.AddBranch(new CommandBranch("exit")
					.SetDescription("exit to program")
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
				.AddBranch(Geometry.branch)
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
		form.OnLeftDown += () =>
		{
			if(!form.shift) return;
			var ss = Tools.Strings.SplitInLastIndexOf(form.textBox.Text.TrimEnd(' '), " ");
			argstack.Push(ss.s2);
			form.textBox.Text = ss.s1;
		};
		form.OnRightDown += () =>
		{
			if(!form.shift || argstack.Count == 0) return;
			form.textBox.Text += form.textBox.Text.EndsWith(' ') || form.textBox.Text == "" ? argstack.Pop() + " " : " " + argstack.Pop() + " ";
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
