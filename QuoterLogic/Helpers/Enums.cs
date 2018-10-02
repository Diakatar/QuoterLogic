namespace QuoterLogic.Helpers
{
    public enum OrderState
    {
        Undefined,
        Placed,
        Moved,
        Canceled
    }

    public enum PlacerState
    {
        Unmodified,
        PendingCancelation,
        PendingMovement,
        PendingPlacing,
    }

    public enum ModificationType
    {
        Unmodified,
        Added,
        Deleted,
        Changed
    }
}
