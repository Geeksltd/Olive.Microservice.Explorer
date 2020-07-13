using MSharp;

class BackgroundTask : EntityType
{
    public BackgroundTask()
    {
        Implements("Olive.PassiveBackgroundTasks.IBackgourndTask");
        String("Name").Mandatory().Unique();
        Guid("Executing instance");
        DateTime("Heartbeat");
        DateTime("Last executed");
        Int("Interval in minutes").Mandatory();
        Int("Timeout in minutes").Mandatory();
    }
}