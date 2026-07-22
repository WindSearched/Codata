using System.Reflection;

namespace Codata.scripts;

public class PointCapturer
{
    public PointCapturerForm capturerForm;
    public event Action OnLeftMouseClick;
    public void Start()
    {
        capturerForm = new PointCapturerForm();
        capturerForm.Show();

        Program.form.WindowState = FormWindowState.Minimized;//hide the main form
        capturerForm.WindowState = FormWindowState.Maximized;
        capturerForm.FormClosing += (_, _) =>
            Program.form.WindowState = FormWindowState.Normal;

        capturerForm.BackColor = Color.Gray;
        //capturerForm.Opacity = 0.3;

        Image image = GetPic();

        capturerForm.MouseClick += (sender, arg) =>
        {
            if(arg.Button == MouseButtons.Left)
            {
                OnLeftMouseClick?.Invoke();
            }
        };
        Program.Log(Tools.ImageTools.IsFullyTransparent(image));
        OnLeftMouseClick += () =>
        {
            PictureBox pic = new();
            pic.BackColor = Color.Red;
            pic.Image = image;
            pic.Location = GetCursorPosition();
            capturerForm.Controls.Add(pic);
            Program.Log("image");
        };

    }

    public Point GetCursorPosition() => Cursor.Position;
    public Image GetPic()
    {
        var assembly = Assembly.GetExecutingAssembly();

        using (Stream stream = assembly.GetManifestResourceStream(
                   "Codata.resources.thumbtack.png"))
        {
            if (stream == null)
                return null;

            using (Image temp = Image.FromStream(stream))
            {
                return new Bitmap(temp);
            }
        }
    }
}

public class PointCapturerForm : Form
{
    public PointCapturerForm()
    {
        Text = "capturer";

    }
}