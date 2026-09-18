using CdPlugin;
using Codata.scripts;
using Codata.scripts.classes;

namespace Codata;

class MainForm : Form
{
	public event Action OnEnterKeyDown;
	public event Action<string> OnKeyDown;
	public event Action OnTabDown;
	public event Action OnCtrlDown;
	public event Action OnCtrlUp;
	public event Action OnDownDown;
	public event Action OnRightDown;
	public event Action OnLeftDown;
	public event Action OnUpDown;
	public event Action OnShiftDown;
	public event Action OnShiftUp;
	public bool shift;
	public bool ctrl;

	public ListBox  ListBox;
	public Cmdtext textBox;
	public RichTextBox rtb;
	public Label descriptionBox;
	public MainForm()
	{
		this.Text = "Codata";

		this.ClientSize = new Size(800, 400);

		Cmdtext textBox = this.textBox = new ();
		textBox.Font = new("Calibri", 11);
		textBox.Size = new Size(645, 23);
		textBox.PreviewKeyDown += (s, e) =>
		{
			if (e.KeyCode is Keys.Tab
			    or Keys.Control
			    or Keys.Shift
			    or Keys.Up
			    or Keys.Down
			    or Keys.Left
			    or Keys.Right)
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
		textBox.Location = new System.Drawing.Point(40, 347);

		Controls.Add(textBox.hint);
		Controls.Add(textBox);

		Label descript = descriptionBox = new Label();
		descript.Location = new Point(40, 329);
		descript.AutoEllipsis = true;
		descript.AutoSize = false;
		descript.Size = new Size(720, 23);
		Program.Log(descript.Size);
		Controls.Add(descript);

		Button button = new Button();
		button.Text = "enter";
		Program.Log(button.Font.Name);
		button.Location = new System.Drawing.Point(685, 347);
		Program.Log(button.Width);
		button.Click += (s, e) => Execute();
		OnEnterKeyDown += Execute;
		KeyDown += Keydown;
		KeyUp += Keyup;

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

	public void Execute()
	{
		string cmd = textBox.Text.Trim(' ');

		var result = Program.command.Command(cmd);

		Tools.KeyCtrl.Register(Program.lineCommand, result.linecmds);

		Log(cmd + "\n" + result.put);
		textBox.Text = "";

		Program.commandTube.Add(cmd);
		Program.commandTube.RevertPointer();
		Program.argstack.Clear();
	}
	private void Keydown(object sender, KeyEventArgs e)
	{
		OnKeyDown?.Invoke(e.KeyCode.ToString());

		void notCHangeText()
		{
			e.Handled = true;
			e.SuppressKeyPress = true;
		}

		switch (e.KeyCode)
		{
			case Keys.Enter:
				OnEnterKeyDown?.Invoke();
				break;
			case Keys.Down:
				if(shift || ctrl)
					notCHangeText();
				OnDownDown?.Invoke();
				break;
			case Keys.Up:
				if(shift || ctrl)
					notCHangeText();
				OnUpDown?.Invoke();
				break;
			case Keys.Left:
				if(shift || ctrl)
					notCHangeText();
				OnLeftDown?.Invoke();
				break;
			case Keys.Right:
				if(shift || ctrl)
					notCHangeText();
				OnRightDown?.Invoke();
				break;
			case Keys.ShiftKey:
				OnShiftDown?.Invoke();
				break;
			case Keys.ControlKey:
				OnCtrlDown?.Invoke();
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
			case Keys.ControlKey:
				OnCtrlUp?.Invoke();
				break;
		}
	}

	/// <summary>
	/// log in output form
	/// </summary>
	/// <param name="message"></param>
	/// <param name="user"></param>
	public void Log(string message, string user = "")
	{
		if (user == "")
			user = GetUser;

		RemovePreviewUser();

		rtb.AppendText(user + ">" + message);

		SetPreviewUser();

		rtb.SelectionStart = rtb.TextLength;
		rtb.SelectionLength = 0;
		rtb.ScrollToCaret();
	}

	public string GetUser => Center.info.user;

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

	private Label label;

	public void SetLabel(string text, Point location)
	{
		label = new Label();
		label.Text = text;
		label.Location = location;
		label.AutoSize = true;
		label.MaximumSize = new Size(300,0);
		Controls.Add(label);
	}
	public void RemoveLabel()
	{
		Controls.Remove(label);
	}

	public Point cursorLocation => PointToClient(Cursor.Position);

	public void SetCommandLine(string command) => textBox.Text = command;
	public bool CompareCommandLine(string command) => textBox.Text.Equals(command);
}