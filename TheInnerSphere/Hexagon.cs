internal class Hexagon
{
    public Hexagon(SystemCoordinates center, double radius, string? text = null)
    {
        Center = center;
        Radius = radius;
        Text = text;
    }

    public SystemCoordinates Center { get; private set; }
    public double Radius { get; private set; }
    public string? Text { get; private set; }

    public SystemCoordinates[] Points
    {
        get
        {
            return new SystemCoordinates[] {
                new SystemCoordinates(Center.X + Radius, Center.Y),
                new SystemCoordinates(Center.X + Radius / 2, Center.Y - Math.Sqrt(3) * Radius / 2),
                new SystemCoordinates(Center.X - Radius / 2, Center.Y - Math.Sqrt(3) * Radius / 2),
                new SystemCoordinates(Center.X - Radius, Center.Y),
                new SystemCoordinates(Center.X - Radius / 2, Center.Y + Math.Sqrt(3) * Radius / 2),
                new SystemCoordinates(Center.X + Radius / 2, Center.Y + Math.Sqrt(3) * Radius / 2)
            };
        }
    }
}