namespace MetaTraderWorkerService.Helpers;

public static class PipCalculator
{
    private const decimal PointsToPipsRatio = 0.1m; // 100 points = 10 pips
    private const decimal PointValue = 0.01m; // 1.00 price difference = 100 points (MetaTrader standard)

    /// <summary>
    /// Converts points to pips.
    /// </summary>
    /// <param name="points">The number of points.</param>
    /// <returns>The equivalent number of pips.</returns>
    public static decimal PointsToPips(decimal points)
    {
        return points * PointsToPipsRatio;
    }

    /// <summary>
    /// Converts pips to points.
    /// </summary>
    /// <param name="pips">The number of pips.</param>
    /// <returns>The equivalent number of points.</returns>
    public static decimal PipsToPoints(decimal pips)
    {
        return pips / PointsToPipsRatio;
    }

    /// <summary>
    /// Calculates the pip difference between two prices.
    /// </summary>
    /// <param name="price1">The first price.</param>
    /// <param name="price2">The second price.</param>
    /// <returns>The difference in pips.</returns>
    public static decimal CalculatePipDifference(decimal price1, decimal price2)
    {
        var priceDifference = Math.Abs(price1 - price2); // Price difference
        var points = priceDifference / PointValue; // Convert to points
        return PointsToPips(points); // Convert points to pips
    }
}