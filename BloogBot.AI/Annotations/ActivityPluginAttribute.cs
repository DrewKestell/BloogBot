using BloogBot.AI.States;

namespace BloogBot.AI.Annotations;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ActivityPluginAttribute(BotActivity activity) : Attribute
{
    public BotActivity Activity { get; } = activity;
}
