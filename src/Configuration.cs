namespace QuestTracker;

[Serializable]
public class Configuration : IPluginConfiguration
{
  public int Version { get; set; }
  public int DisplayOption { get; set; }
  public bool ShowCount { get; set; } = true;
  public bool ShowPercentage { get; set; } = true;
  public bool ExcludeOtherQuests { get; set; }
  public bool ExcludeLevequests { get; set; }

  [NonSerialized]
  private IDalamudPluginInterface _pluginInterface = null!;

  public void Initialize(IDalamudPluginInterface pluginInterface)
    => _pluginInterface = pluginInterface;

  public void Save()
    => _pluginInterface.SavePluginConfig(this);
}
