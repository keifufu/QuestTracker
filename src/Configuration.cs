namespace QuestTracker;

[Serializable]
public class Configuration : IPluginConfiguration
{
  public int Version { get; set; } = 0;
  public bool ShowCount { get; set; } = true;
  public bool ShowPercentage { get; set; }
  public bool ExcludeOtherQuests { get; set; }
  public int DisplayOption { get; set; }

  [NonSerialized]
  private IDalamudPluginInterface PluginInterface = null!;

  public void Initialize(IDalamudPluginInterface pluginInterface)
  {
    PluginInterface = pluginInterface;
  }

  public void Save()
  {
    PluginInterface.SavePluginConfig(this);
  }
}
