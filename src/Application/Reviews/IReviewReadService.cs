namespace Nesto.Application.Reviews;

public interface IReviewReadService
{
    Task<IReadOnlyList<ReviewResponse>> GetAllAsync(Guid? apartmentId, CancellationToken cancellationToken);
}
