namespace Nesto.SharedKernel;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
