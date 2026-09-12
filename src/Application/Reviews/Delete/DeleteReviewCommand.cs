using Nesto.Application.Abstractions.Messaging;

namespace Nesto.Application.Reviews.Delete;

public sealed record DeleteReviewCommand(Guid ReviewId) : ICommand;
