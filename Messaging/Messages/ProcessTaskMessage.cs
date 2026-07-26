using TaskProcessor.Enums;

namespace TaskProcessor.Messaging.Messages;

public sealed record ProcessTaskMessage(
    string TaskId,
    TaskType? Type,
    string Data);