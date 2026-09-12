using Nesto.Application.Abstractions.Messaging;

namespace Nesto.Application.Reviews.Update;

public sealed record UpdateReviewCommand(Guid ReviewId, int Rating, string Comment) : ICommand;
