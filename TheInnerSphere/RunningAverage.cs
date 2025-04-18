using System.Runtime.CompilerServices;

internal class RunningAverage
{
    public RunningAverage()
    {
        Average = 0;
        Count = 0;
    }

    public void AddValue(double value)
    {
        Average = (Count * Average + value) / (Count + 1);
        Count += 1;
    }

    public double Average { get; private set; }
    private uint Count { get; set; }
}

internal class RunningAverageCoordinates
{
    public RunningAverageCoordinates()
    {
        X = new RunningAverage();
        Y = new RunningAverage();
    }

    public SystemCoordinates Average 
    {
        get {
            return new SystemCoordinates(X.Average, Y.Average);
        }
    }

    public void AddValue(SystemCoordinates value)
    {
        X.AddValue(value.X);
        Y.AddValue(value.Y);
    }

    private RunningAverage X;
    private RunningAverage Y;
}