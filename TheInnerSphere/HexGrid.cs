using System.Drawing;

internal class HexGrid
{
    public HexGrid()
    {
    }

    public List<Hexagon> GenerateGrid(SystemCoordinates center, int rows, int columns, double hexHeight)
    {
        double hexRadius = hexHeight / Math.Sqrt(3);

        List<Hexagon> hexes = new List<Hexagon>();

        SystemCoordinates anchor = center;

        hexes.Add(new Hexagon(anchor, hexRadius, "0000"));

        anchor = new SystemCoordinates(center.X, center.Y - hexHeight * (rows / 2));

        for (int c = 0; c < columns / 2 + 1; c++)
        {
            if (c != 0 && c % 2 == 0)
            {
                anchor = new SystemCoordinates(anchor.X + hexRadius * 1.5, anchor.Y + hexHeight / 2);
            }
            else if (c % 2 == 1)
            {
                anchor = new SystemCoordinates(anchor.X + hexRadius * 1.5, anchor.Y - hexHeight / 2);
            }

            for (int r = 0; r < rows; r++)
            {
                var hexCenter = new SystemCoordinates(anchor.X, anchor.Y + r * hexHeight);
                hexes.Add(new Hexagon(hexCenter, hexRadius, $"{c:00}{r:00}"));
            }
        }

        anchor = new SystemCoordinates(center.X, center.Y - hexHeight * (rows / 2));

        for (int c = 1; c < columns / 2 + 1; c++)
        {
            if (c != 0 && c % 2 == 0)
            {
                anchor = new SystemCoordinates(anchor.X - hexRadius * 1.5, anchor.Y + hexHeight / 2);
            }
            else if (c % 2 == 1)
            {
                anchor = new SystemCoordinates(anchor.X - hexRadius * 1.5, anchor.Y - hexHeight / 2);
            }

            for (int r = 0; r < rows; r++)
            {
                var hexCenter = new SystemCoordinates(anchor.X, anchor.Y + r * hexHeight);
                hexes.Add(new Hexagon(hexCenter, hexRadius, $"{c:00}{r:00}"));
            }
        }
        

        return hexes;
    }
}