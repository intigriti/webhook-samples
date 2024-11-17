using System.Text.Json.Serialization;

namespace Intigriti.Webhook.Events;

internal abstract record Enumeration
{
    public required int Id { get; init; }
    public required string Value { get; init; }
}

internal record Severity : Enumeration
{
    public required string? Vector { get; init; }
    public required decimal? Score { get; init; }
}

internal record Status : Enumeration;

internal record CloseReason : Enumeration;


[JsonDerivedType(typeof(TestEvent))]
[JsonDerivedType(typeof(SubmissionCreated))]
[JsonDerivedType(typeof(SubmissionSeverityChanged))]
[JsonDerivedType(typeof(SubmissionStatusChanged))]
[JsonDerivedType(typeof(SubmissionMessagePlaced))]
internal abstract record SubmissionEvent
{
    public required Guid ProgramId { get; init; }
    public required string ProgramName { get; init; }
    public required string SubmissionCode { get; init; }
    public required Guid UserId { get; init; }
    public required long CreatedAt { get; init; }
}

internal record TestEvent : SubmissionEvent;

internal record SubmissionCreated : SubmissionEvent
{
    public required Severity Severity { get; init; }
    public required string Type { get; init; }
    public required string Domain { get; init; }
}

internal record SubmissionSeverityChanged : SubmissionEvent
{
    public required string UserRole { get; init; }
    public required Severity FromSeverity { get; init; }
    public required Severity ToSeverity { get; init; }
}

internal record SubmissionStatusChanged : SubmissionEvent
{
    public required string UserRole { get; init; }
    public required Status FromStatus { get; init; }
    public required Status ToStatus { get; init; }
    public required CloseReason? CloseReason { get; init; }
}

internal record SubmissionMessagePlaced : SubmissionEvent
{
    public required string UserRole { get; init; }
    public required bool IsInternal { get; init; }
}