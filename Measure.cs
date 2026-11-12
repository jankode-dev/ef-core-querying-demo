using System.Diagnostics;

namespace EfCoreQueryingDemo;

public static class Measure
{
    public static async Task<Measurement<T>> RunAsync<T>(Func<Task<T>> action)
    {
        await action();

        GC.Collect();
        var allocatedBefore = GC.GetTotalAllocatedBytes(true);
        var stopwatch = Stopwatch.StartNew();

        var result = await action();

        stopwatch.Stop();
        return new Measurement<T>(result, stopwatch.Elapsed, GC.GetTotalAllocatedBytes(true) - allocatedBefore);
    }
}

public record Measurement<T>(T Result, TimeSpan Elapsed, long AllocatedBytes)
{
    public double Milliseconds => Elapsed.TotalMilliseconds;
    public double Megabytes => AllocatedBytes / 1024.0 / 1024.0;
}
