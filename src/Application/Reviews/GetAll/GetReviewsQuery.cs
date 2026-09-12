using Nesto.Application.Abstractions.Messaging;

namespace Nesto.Application.Reviews.GetAll;

public sealed record GetReviewsQuery(Guid? ApartmentId) : IQuery<IReadOnlyList<ReviewResponse>>;
