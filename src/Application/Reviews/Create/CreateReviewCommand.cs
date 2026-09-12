using Nesto.Application.Abstractions.Messaging;

namespace Nesto.Application.Reviews.Create;

public sealed record CreateReviewCommand(Guid BookingId, int Rating, string Comment) : ICommand<Guid>;
