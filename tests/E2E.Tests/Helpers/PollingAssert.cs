namespace E2E.Tests.Helpers;

/// <summary>
/// Provides polling-based assertion helpers for eventually-consistent systems.
///
/// RabbitMQ message delivery and EF Core persistence are asynchronous; a test that
/// asserts a consumer side-effect immediately after publishing will be flaky.
/// These helpers retry a condition at short intervals until it passes or a timeout
/// is reached, at which point they throw a descriptive <see cref="TimeoutException"/>.
/// </summary>
public static class PollingAssert
{
    /// <summary>
    /// Polls <paramref name="condition"/> every <paramref name="interval"/> until it returns
    /// <c>true</c> or <paramref name="timeout"/> elapses.
    /// </summary>
    /// <exception cref="TimeoutException">Thrown when the condition never becomes true.</exception>
    public static async Task WaitUntilAsync(
        Func<Task<bool>> condition,
        string description,
        TimeSpan? timeout = null,
        TimeSpan? interval = null,
        CancellationToken cancellationToken = default)
    {
        var deadline = DateTime.UtcNow.Add(timeout ?? TimeSpan.FromSeconds(10));
        var pollInterval = interval ?? TimeSpan.FromMilliseconds(250);

        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (await condition())
                return;

            await Task.Delay(pollInterval, cancellationToken);
        }

        throw new TimeoutException(
            $"Polling condition [{description}] did not become true within {timeout ?? TimeSpan.FromSeconds(10)}.");
    }

    /// <summary>
    /// Polls until <paramref name="getValue"/> returns a non-null value, then returns it.
    /// </summary>
    public static async Task<T> WaitForValueAsync<T>(
        Func<Task<T?>> getValue,
        string description,
        TimeSpan? timeout = null,
        TimeSpan? interval = null,
        CancellationToken cancellationToken = default) where T : class
    {
        T? result = null;

        await WaitUntilAsync(
            async () =>
            {
                result = await getValue();
                return result is not null;
            },
            description,
            timeout,
            interval,
            cancellationToken);

        return result!;
    }
}
