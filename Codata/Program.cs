using CdPlugin;
using Codata.scripts;
using Codata.scripts.classes;
using Codata.scripts.commandBranches;

namespace Codata;

class Program
{
	public static CommandBranch command;
	public static SpecialPointTube<string> commandTube = new(32);
	public static MainForm form;
	public static ICdAction afterConfirm;
	public static event Action OnProgramClose;
	public static PointCapturer capturer;
	public static Stack<string> argstack = new();
	public static LineCommand lineCommand;

	[STAThread]
	static void Main()
	{
		capturer = new();

		Data.Init();
		Center.info = Info.ReadJson(Data.infoPath);
		Langue.Init(out Center.langue);
		RegisterCommands();
		Commands.Init();
		Lua.Init();
		lineCommand = new LineCommand();
		PluginRegister.Registering(Data.PathCombine(Data.filePath, "mods"));
		Center.logAction = Log;

		Log(Center.langue.ToString());

		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);
		form = new MainForm();
		form.textBox.WhenTextChanged += (_,_) =>
		{
			var s = form.textBox.Text;
			var l = Tools.Strings.SplitAndRemoveRepeatedEmpties(s, " ");

			var v = command.GetSuggestions(l.ToList(), out var branch);


			if(Center.info.viewDescription)
				form.descriptionBox.Text = branch.description;
			form.ListBox.DataSource = v.list;
			form.textBox.SetSuggestion(v.tag);
		};

		BindingKeys();

		form.ListBox.DataSource = command.branches.Select(v => v.name).ToList();

		Application.Run(form);

		//when program close
		OnProgramClose?.Invoke();
		Info.WriteJson(Data.infoPath, Center.info);
	}

	static void RegisterCommands()
	{
		command = new CommandBranch("codata")
				.Execute(_ => new Result(Word.word("codata","welcome to codata! ฅ՞Ⱉ՞ฅ"),true))
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
					.SetDescription(Word.worddcr("print", "print the input, command for test"))
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
						Center.info.user = n;
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
								Center.info = new();
							});
							return new Result("input confirm command to execute this command",true);
						})
					)
					.AddArgument(new CommandBranch.Argument("name")
						.SetSuggestion(() => Tools.ReflectionHelper.GetFieldsString(Center.info))
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
						Tools.ReflectionHelper.SetFieldFromString(Center.info, name, value);

						Tools.DebugLog(value);

						return new(true);
					})
				)
				.AddBranch(new CommandBranch("confirm")
					.SetDescription(Word.word("confirm", "confirm an operation"))
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
						if (Center.info.confirmToExit)
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
						capturer.Start(3,
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
				.AddBranch(Test.test)
			;
		command.AddBranch(command);
		command.AddBranch(FileViewer.branch);

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

			if (c.EndsWith('\\'))
			{
				c += s + '\\';
			}
			else
			{
				c = index != -1 ? c.Substring(0, index+1) : "";
				c += s.EndsWith('\\') ? s : s + ' ';
			}

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
			else if (form.ctrl)//show command line
			{
				int l = Tools.KeyCtrl.GetLineOfInsertPosition();
				l = lineCommand.LineSearch(l, true, false);
				if (l != 0)
				{
					Tools.KeyCtrl.ShowCommandLine(l);
				}
				else
				{
					Tools.DebugLog("do not find line command");
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
			else if (form.ctrl)//show command line
			{
				int l = Tools.KeyCtrl.GetLineOfInsertPosition();
				l = lineCommand.LineSearchReverse(l, true, false);
				if (l != 0)
				{
					Tools.KeyCtrl.ShowCommandLine(l);
				}
				else
				{
					Tools.DebugLog("do not find line command");
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
		form.OnCtrlDown += () => form.ctrl = true;
		form.OnCtrlDown += () =>
		{
			if (Tools.KeyCtrl.SearchLineInForm(lineCommand, out int l))
			{
				if (!Tools.KeyCtrl.TryShowLine(l))
				{
					var c = lineCommand.Get(l);
					if (form.CompareCommandLine(c))
						form.Execute();
					else
						form.SetCommandLine(c);

				}
			}
		};
		form.OnCtrlUp += () => form.ctrl = false;
		form.OnEnterKeyDown += () =>
		{
			if (form.ctrl)
			{
				int l = Tools.KeyCtrl.GetLineOfInsertPosition();
				if (l != 0 && lineCommand.TryGet(l, out var s))
				{
					form.SetCommandLine(s);
				}
			}
		};
	}

	public static void Log(object message)
	{
		Console.WriteLine(message);
	}

	public static string PutUser() => Center.info.user + ">";
}