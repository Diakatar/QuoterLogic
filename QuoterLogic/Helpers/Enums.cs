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
}
