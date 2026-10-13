namespace QuestTracker;

[Serializable]
public class Configuration : IPluginConfiguration
{
  public int Version { get; set; }
  public int DisplayOption = 0;
  public bool ShowCount = true;
  public bool ShowPercentage = true;
  public bool ExcludeOtherQuests = false;
  public bool ExcludeLevequests = false;

  [NonSerialized]
  private IDalamudPluginInterface _pluginInterface = null!;

  public void Initialize(IDalamudPluginInterface pluginInterface)
    => _pluginInterface = pluginInterface;

  public void Save()
    => _pluginInterface.SavePluginConfig(this);
}
