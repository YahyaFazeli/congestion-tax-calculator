using System.Data.Common;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Interceptors;

public class SlowQueryInterceptor(ILogger<SlowQueryInterceptor> logger) : DbCommandInterceptor
{
    private const int SlowQueryThresholdMs = 1000;

    public override async ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default
    )
    {
        if (eventData.Duration.TotalMilliseconds > SlowQueryThresholdMs)
        {
            logger.LogWarning(
                "Slow SQL query detected: {ElapsedMs}ms - {Sql}",
                eventData.Duration.TotalMilliseconds,
                command.CommandText
            );
        }

        return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result
    )
    {
        if (eventData.Duration.TotalMilliseconds > SlowQueryThresholdMs)
        {
            logger.LogWarning(
                "Slow SQL query detected: {ElapsedMs}ms - {Sql}",
                eventData.Duration.TotalMilliseconds,
                command.CommandText
            );
        }

        return base.ReaderExecuted(command, eventData, result);
    }

    public override async ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default
    )
    {
        if (eventData.Duration.TotalMilliseconds > SlowQueryThresholdMs)
        {
            logger.LogWarning(
                "Slow SQL command detected: {ElapsedMs}ms - {Sql}",
                eventData.Duration.TotalMilliseconds,
                command.CommandText
            );
        }

        return await base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override int NonQueryExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result
    )
    {
        if (eventData.Duration.TotalMilliseconds > SlowQueryThresholdMs)
        {
            logger.LogWarning(
                "Slow SQL command detected: {ElapsedMs}ms - {Sql}",
                eventData.Duration.TotalMilliseconds,
                command.CommandText
            );
        }

        return base.NonQueryExecuted(command, eventData, result);
    }
}
