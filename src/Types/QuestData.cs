namespace QuestTracker.Types;

public class QuestData
{
  public string Title { get; set; } = "";
  public string EnglishTitle { get; set; } = "";
  public List<QuestData> Categories { get; set; } = [];
  public List<Quest> Quests { get; set; } = [];
  public float NumComplete { get; set; }
  public float Total { get; set; }
  public bool Hide { get; set; }
  public uint SortKey { get; set; }
}

public class Quest
{
  public required string Title { get; set; }
  public required List<uint> Ids { get; set; }
  public required string Area { get; set; }
  public string? Start { get; set; }
  public string? Gc { get; set; }
  public int Level { get; set; }
  public bool Hide { get; set; }
  public uint SortKey { get; set; }
  public bool IsLeve { get; set; }
}
