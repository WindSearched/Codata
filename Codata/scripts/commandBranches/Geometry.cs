namespace Codata.scripts.commandBranches;

public static class Geometry
{
    public static double Distance(Point p1, Point p2)
    {
        int x = p1.X - p2.X;
        int y = p1.Y - p2.Y;
        var r = SMath.Sqrt(x * x + y * y);
        return r;
    }

    public static double Angle(Point p1,  Point center, Point p2)
    {
        Point A = p1, B = center, C = p2;

        double x1 = A.X - B.X;
        double y1 = A.Y - B.Y;

        double x2 = C.X - B.X;
        double y2 = C.Y - B.Y;

        double dot = x1 * x2 + y1 * y2;

        double len1 = Math.Sqrt(x1 * x1 + y1 * y1);
        double len2 = Math.Sqrt(x2 * x2 + y2 * y2);

        double angle = Math.Acos(dot / (len1 * len2));

        double angleDeg = angle * 180 / Math.PI;

        return angleDeg;
    }

    public static CommandBranch branch =
        new CommandBranch("geometry")
            .SetAbbreviation("geom")
            .AddBranches(
                new CommandBranch("length")
                    .SetAbbreviation("distance")
                    .Execute(_ =>
                    {
                        Program.capturer.Start(2,
                            ps => $"p1 = {ps[0]}\np2 = {ps[1]}\ndistance = {Distance(ps[0], ps[1])}");
                        return PointCapturer.result;
                    }),
                new CommandBranch("angle")
                    .Execute(_ =>
                    {
                        Program.capturer.Start(3,
                            ps => Angle(ps[0], ps[1], ps[2]).ToString());
                        return PointCapturer.result;
                    })
                )
        ;
}