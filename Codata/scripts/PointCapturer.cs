using System.Reflection;

namespace Codata.scripts;

public class PointCapturer
{
    public PointCapturerForm capturerForm;
    private int captureTimes;
    public event Action OnLeftMouseClick;
    public void Start(int captureTimes_, Func<List<Point>, string> resultCreator)
    {
        captureTimes = captureTimes_;

        capturerForm = new PointCapturerForm();
        capturerForm.Show();

        Program.form.WindowState = FormWindowState.Minimized;//hide the main form
        capturerForm.WindowState = FormWindowState.Maximized;
        capturerForm.FormClosing += (_, _) =>
            Program.form.WindowState = FormWindowState.Normal;

        capturerForm.BackColor = Color.Gray;
        capturerForm.Opacity = 0.3;

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
            float s = 0.5f;

            PictureBox pic = new();
            pic.SizeMode = PictureBoxSizeMode.Zoom;
            pic.Size = new((int)(image.Width * s), (int)(image.Height * s));
            pic.Image = image;

            Point p = GetCursorPosition();
            pic.Location = new(
                p.X - pic.Width / 2,
                p.Y - pic.Height
            );
            capturerForm.points.Add(p);

            capturerForm.Controls.Add(pic);

            capturerForm.DrawLine(capturerForm.points.Count -1, Color.Red);

            if (--captureTimes == 0)
            {
                capturerForm.Close();
            }
        };

        capturerForm.Closed += (_, _) =>
        {
            Program.form.Log(resultCreator(capturerForm.points), capturerForm.Text);
        };
    }

    public Point GetCursorPosition() => capturerForm.PointToClient(Cursor.Position);
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
    public List<Point> points;
    public PointCapturerForm()
    {
        Text = "capturer";
        points = new();

    }

    /// <summary>
    /// dra line with point on index and that on back index
    /// </summary>
    public void DrawLine(int index, Color color) => DrawLine(index, index - 1, color);
    public void DrawLine(int index1, int index2, Color color)
    {
        if(index1 < 0 || index2 < 0 ||points.Count <= index1 || points.Count <= index2)
            return;
        DrawLine(points[index1], points[index2],  color);
    }
    public void DrawLine(Point p1, Point p2, Color color)
    {
        Paint += (sender, args) =>
        {
            using (Pen pen = new Pen(color, 2))
            {
                args.Graphics.DrawLine(pen, p1.X, p1.Y, p2.X, p2.Y);
            }
        };
        Refresh();
    }
}