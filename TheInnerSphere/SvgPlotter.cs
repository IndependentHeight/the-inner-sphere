using System.ComponentModel;
using System.Data;

internal class SvgPlotter
{
    public SvgPlotter(PlotterSettings settings)
    {
        _circles = new List<string>();
        _jumpLines = new List<string>();
        _hpgLines = new List<string>();
        _text = new List<string>();
        _systems = new List<PlanetInfo>();
        _overlays = new List<string>();

        _scale = settings.Scale;

        _width = (int)Math.Ceiling(settings.Width * _scale);
        _height = (int)Math.Ceiling(settings.Height * _scale);

        _centerX = _width / 2.0;
        _centerY = _height / 2.0;

        _transformX = settings.Center.X;
        _transformY = settings.Center.Y;

        _systemRadius = settings.SystemRadius;
        _systemPalette = settings.SystemPalette;
        _titleMapping = settings.SystemTitleMapping;
        _subtitleMapping = settings.SystemSubtitleMapping;
        _linkPalette = settings.LinkPalette;
        _linkStrokeWidth = settings.LinkStrokeWidth;
        _linkOpacity = settings.LinkOpacity;

        _importantMapping = settings.ImportantWorldMapping;

        _includeJumpLines = settings.IncludeJumpLines;
        _includeHPGLines = settings.IncludeHPGLines;
        _includeSystemNames = settings.IncludeSystemNames;

        _primaryFontSize = settings.PrimaryFontSize;
        _secondaryFontSize = settings.SecondaryFontSize;
    }

    public void Add(Rectangle rectangle)
    {
        (double transformedX1, double transformedY1) = TransformCoordinates(rectangle.PointA);
        (double transformedX2, double transformedY2) = TransformCoordinates(rectangle.PointB);

        double x = Math.Min(transformedX1, transformedX2);
        double y = Math.Min(transformedY1, transformedY2);

        double width = Math.Abs(transformedX2 - transformedX1);
        double height = Math.Abs(transformedY2 - transformedY1);

        string svg = $"<rect x=\"{x}\" y=\"{y}\" width=\"{width}\" height=\"{height}\" fill-opacity=\"0\" stroke=\"#cccccc\" />";

        _overlays.Add(svg);
    }

    public void Add(Circle circle)
    {
        (double transformedX, double transformedY) = TransformCoordinates(circle.Center);
        double transformedRadius = circle.Radius * _scale;

        string svg = $"<circle cx=\"{transformedX}\" cy=\"{transformedY}\" r=\"{transformedRadius}\" fill-opacity=\"0\" stroke=\"#cccccc\" />";

        _overlays.Add(svg);
    }

    public void AddHexGrid()
    {
        var rows = (int)Math.Ceiling(_height / (_scale * 30));
        var columns = (int)Math.Ceiling(_width / (_scale * 30 / Math.Sqrt(3)));

        if (rows % 2 == 0) { rows = rows + 1; }

        var grid = new HexGrid();
        var hexes = grid.GenerateGrid(new SystemCoordinates(0,0), rows, columns, 30);

        foreach (var hex in hexes)
        {
            var points = new List<SystemCoordinates>();
            foreach (var point in hex.Points)
            {
                (double transformedX, double transformedY) = TransformCoordinates(point);
                points.Add(new SystemCoordinates(transformedX, transformedY));
            }

            var svg = $"<polygon points=\"{points[0].X},{points[0].Y} {points[1].X},{points[1].Y} {points[2].X},{points[2].Y} {points[3].X},{points[3].Y} {points[4].X},{points[4].Y} {points[5].X},{points[5].Y} {points[0].X},{points[0].Y}\" fill=\"none\" stroke=\"#aaaaaa\" stroke-width=\"1.5\" />";
            _overlays.Add(svg);
        }
    }

    public void Add(PlanetInfo system)
    {
        (double transformedX, double transformedY) = TransformCoordinates(system.Coordinates);

        var color = "#ffffff";
        if (_systemPalette != null)
        {
            color = _systemPalette(system);
        }

        double overshoot = 5 * _scale;
        if (_includeJumpLines)
        {
            overshoot = 30 * _scale;
        }
        if (_includeHPGLines)
        {
            overshoot = 50 * _scale;
        }
        if (transformedX > 0 && transformedX < _width && transformedY > 0 && transformedY < _height)
        {
            bool systemIsImportant = false;
            if (_importantMapping != null)
            {
                systemIsImportant = _importantMapping(system);
            }

            var radius = systemIsImportant ? _systemRadius * 2 : _systemRadius;
            string svg = $"<circle cx=\"{transformedX}\" cy=\"{transformedY}\" r=\"{radius}\" stroke=\"#000000\" stroke-width=\"1\" fill=\"{color}\" />";
            _circles.Add(svg);

            if (_includeSystemNames)
            {
                int mainFontSize = _primaryFontSize;
                int subFontSize = _secondaryFontSize;
                string title = system.Name;
                if (_titleMapping != null)
                {
                    title = _titleMapping(system);
                }
                string label = $"<text x=\"{transformedX}\" y=\"{transformedY - (radius + subFontSize + 4)}\" fill=\"#eeeeee\" text-anchor=\"middle\" font-family=\"sans-serif\" font-size=\"{mainFontSize}\" stroke=\"black\" stroke-width=\"0.25\">{title}</text>";
                _text.Add(label);

                if (_subtitleMapping != null)
                {
                    string subtitle = _subtitleMapping(system);
                    if (!String.IsNullOrWhiteSpace(subtitle))
                    {
                        string subtitleElement = $"<text x=\"{transformedX}\" y=\"{transformedY - (radius + 2)}\" fill=\"#eeeeee\" text-anchor=\"middle\" font-family=\"sans-serif\" font-size=\"{subFontSize}\" stroke=\"black\" stroke-width=\"0.25\">{subtitle}</text>";
                        _text.Add(subtitleElement);
                    }
                }
                
            }

            _systems.Add(system);
        }
        else if (transformedX > (0 - overshoot) && transformedX < (_width + overshoot) && transformedY > (0 - overshoot) && transformedY < (_height + overshoot))
        {
            _systems.Add(system);
        }
    }

    public void Write(string file)
    {
        if (_includeJumpLines || _includeHPGLines)
        {
            GenerateLines();
        }
        using (var writer = new StreamWriter(file))
        {
            writer.WriteLine("<!DOCTYPE svg PUBLIC \"-//W3C//DTD SVG 1.1//EN\" \"http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd\">");
            writer.WriteLine($"<svg version=\"1.1\" xmlns=\"http://www.w3.org/2000/svg\" height=\"{_height}\" width=\"{_width}\" >");
            writer.WriteLine($"  <rect height=\"{_height}\" width=\"{_width}\" fill=\"#000000\" />");
            foreach (var line in _hpgLines)
            {
                writer.WriteLine($"  {line}");
            }
            foreach (var line in _jumpLines)
            {
                writer.WriteLine($"  {line}");
            }
            foreach (var circle in _circles)
            {
                writer.WriteLine($"  {circle}");
            }
            foreach (var overlay in _overlays)
            {
                writer.WriteLine($"  {overlay}");
            }
            foreach (var text in _text)
            {
                writer.WriteLine($"  {text}");
            }
            writer.WriteLine("</svg>");
        }
    }

    private void GenerateLines()
    {
        _jumpLines = new List<string>();
        _hpgLines = new List<string>();
        for (int i = 0; i < _systems.Count; i++)
        {
            var system1 = _systems[i];
            (double x1, double y1) = TransformCoordinates(system1.Coordinates);

            for (int j = i + 1; j < _systems.Count; j++)
            {
                var system2 = _systems[j];
                if (_includeJumpLines && IsSingleJump(system1, system2))
                {
                    (double x2, double y2) = TransformCoordinates(system2.Coordinates);

                    string color = "#666666";
                    if (_linkPalette != null)
                    {
                        color = _linkPalette(system1, system2);
                    }
                    string svg = $"<line x1=\"{x1}\" y1=\"{y1}\" x2=\"{x2}\" y2=\"{y2}\" stroke=\"{color}\" stroke-width=\"{_linkStrokeWidth}\" opacity=\"{_linkOpacity}\" />";
                    _jumpLines.Add(svg);
                }
                else if (_includeHPGLines && IsWithinHPGDistance(system1, system2))
                {
                    (double x2, double y2) = TransformCoordinates(system2.Coordinates);

                    string color = "#666666";
                    if (_linkPalette != null)
                    {
                        color = _linkPalette(system1, system2);
                    }
                    string svg = $"<line x1=\"{x1}\" y1=\"{y1}\" x2=\"{x2}\" y2=\"{y2}\" stroke=\"{color}\" stroke-width=\"{_linkStrokeWidth / 2}\" stroke-dasharray=\"3 2\" opacity=\"{_linkOpacity}\" />";
                    _hpgLines.Add(svg);
                }
            }
        }
    }

    private bool IsSingleJump(PlanetInfo a, PlanetInfo b)
    {
        double distanceSquared = Math.Pow(a.Coordinates.X - b.Coordinates.X,2) + Math.Pow(a.Coordinates.Y - b.Coordinates.Y, 2);
        return (distanceSquared <= 900);
    }

    private bool IsWithinHPGDistance(PlanetInfo a, PlanetInfo b)
    {
        double distanceSquared = Math.Pow(a.Coordinates.X - b.Coordinates.X,2) + Math.Pow(a.Coordinates.Y - b.Coordinates.Y, 2);
        return (distanceSquared <= 2500);
    }

    private (double, double) TransformCoordinates(SystemCoordinates coordinates)
    {
        double transformedX = (_scale * (coordinates.X - _transformX)) + _centerX;
        double transformedY = (-1 * _scale * (coordinates.Y - _transformY)) + _centerY;
        return (transformedX, transformedY);
    }

    private List<PlanetInfo> _systems;
    private List<string> _circles;
    private List<string> _jumpLines;
    private List<string> _hpgLines;
    private List<string> _text;
    private List<string> _overlays;
    private int _width;
    private int _height;
    private double _centerX;
    private double _centerY;
    private double _transformX;
    private double _transformY;
    private double _scale;
    private SystemColorMapping? _systemPalette;
    private SystemTitleMapping? _titleMapping;
    private SystemSubtitleMapping? _subtitleMapping;
    private LinkColorMapping? _linkPalette;
    private ImportantWorldMapping? _importantMapping;
    private int _systemRadius;
    private bool _includeJumpLines;
    private bool _includeHPGLines;
    private bool _includeSystemNames;
    private int _primaryFontSize;
    private int _secondaryFontSize;
    private double _linkStrokeWidth;
    private double _linkOpacity;
}