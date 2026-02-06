namespace MarketingPlatform.Core.Enums
{
    public enum TriggerType
    {
        Event,
        Schedule,
        Keyword,
        Manual
    }

    public enum WorkflowActionType
    {
        SendSMS,
        SendMMS,
        SendEmail,
        Wait,
        AddToGroup,
        RemoveFromGroup,
        AddTag
    }

    public enum WorkflowExecutionStatus
    {
        Running,
        Completed,
        Failed,
        Paused
    }
}
