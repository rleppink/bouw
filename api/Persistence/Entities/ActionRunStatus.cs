namespace Bouw.API.Persistence.Entities;

public enum ActionRunStatus
{
    Pending,
    Running,
    WaitingForUser,
    Complete,
    Failed,
}
