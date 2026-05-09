namespace QuestTracker.Services;

public interface IDataService : IHostedService
{
  QuestData QuestData { get; }
  bool IsQuestComplete(Types.Quest quest);
  void UpdateQuestData();
}

public class DataService(ILogger _logger, Configuration _configuration, IDataManager _dataManager, IClientState _clientState) : IDataService
{
  public QuestData RawQuestData { get; private set; } = new();
  public QuestData QuestData { get; private set; } = new();
  private string _startArea = "";
  private string _grandCompany = "";
  private List<uint> _startClass = [];

  private readonly List<uint> _gridaniaStartQuests = [
    65621, 65659, 65660, 65564, 65737, 65981, 65664, 69390, 65711, 65661, 69391,
    65665, 65712, 65912, 65913, 65915, 65916, 65917, 65920, 65923, 65697, 65982,
    65983, 65984, 65985, 66043, 66210, 65575, 65537, 65568, 65570, 65573, 65756,
    65708, 65663, 65666, 65596, 65914
  ];

  private readonly List<uint> _gridaniaStartLeves = [
    546
  ];

  private readonly List<uint> _limsaStartQuests = [
    65644, 65645, 65998, 65999, 66079, 66001, 66002, 66003, 66004, 66005, 65933,
    65938, 65939, 65942, 65948, 65951, 65949, 65950, 66225, 66080, 66226, 66081,
    66082, 65643, 65647, 65648, 65658, 66199, 66229, 66008, 66006, 66009, 66010,
    65936, 65937, 66011, 66012, 66013, 66022, 66014, 66015, 65595, 65941
  ];

  private readonly List<uint> _limsaStartLeves = [
    556
  ];

  private readonly List<uint> _uldahStartQuests = [
    66104, 66105, 66106, 66131, 66207, 66086, 65839, 65842, 69388, 65843, 65856,
    66159, 65864, 66039, 65865, 65866, 65867, 69389, 65868, 65869, 65870, 65872,
    66164, 66087, 66177, 66088, 66064, 66209, 66130, 65925, 65926, 66223, 65594,
    65877, 66040, 66042, 65878, 66041, 65857, 65840, 65858, 65924, 65844, 65862,
    66067, 66066, 66109
  ];

  private readonly List<uint> _uldahStartLeves = [
    566
  ];

  private readonly List<uint> _twinAdderQuests = [66216, 66219, 66236, 66641, 67063, 67099, 67925];
  private readonly List<uint> _maelstromQuests = [66217, 66220, 66237, 66640, 67064, 67100, 67926];
  private readonly List<uint> _immortalFlamesQuests = [66218, 66221, 66238, 66642, 67065, 67101, 67927];

  private readonly List<uint> _arrShenanigansAdded = [];
  private readonly List<List<uint>> _arrShenanigans = [
    [65621, 65659, 65660], [65644, 65645], [66104, 66105, 66106], [65664, 69390], [65842, 69388],
    [65661, 69391], [65867, 69389], [65781, 66211], [66244, 69392], [66253, 69393], [66254, 69394],
    [66262, 69395], [66269, 69396], [66270, 69397], [66276, 69398], [66320, 69399], [66321, 69400],
    [66355, 69401], [66375, 69402], [66408, 69403], [66453, 69404], [66473, 69405], [66504, 69406],
    [66539, 69407], [66572, 70057], [66579, 69408], [66672, 69409], [66060, 70058], [66712, 69410],
    [66714, 69411], [66716, 69412], [66724, 69413], [66729, 69414], [66881, 69415], [66884, 69416],
    [66886, 69417], [66889, 69418], [66980, 69419], [66981, 69420], [66988, 69421], [65615, 69422],
    [65617, 69423], [65903, 69424], [65955, 70127], [66735, 67245], [67823, 66097], [66552, 67089],
    [65821, 65789], [65797, 65824], [66069, 66068], [66091, 66234], [65847, 65846], [65850, 65851],
    [65668, 65559], [65571, 65679], [65667, 65557], [65669, 65558], [65683, 65627], [65881, 65880],
    [65884, 65885], [65988, 65989], [65993, 65992], [68753, 69569]
  ];

  private readonly List<List<uint>> _furtherArrShenanigans = [
    [66700, 66704, 66708], [66701, 66705, 66709],
    [66702, 66706, 66710], [66699, 66703, 66707]
  ];

  private readonly List<uint> _retiredQuests = [
    65603, 66023, 66033, 66034, 66957, 66958, 66964, 66965, 67819, 68629, 68727,
    71000, 71001, 71003, 71004, 69377, 69296, 67635, 67752, 67870, 69508, 69578,
    65860, 66000, 65841, 65863, 65934, 65940, 65871, 67653
  ];

  public Task StartAsync(CancellationToken cancellationToken)
  {
    _clientState.Login += OnLogin;

    foreach (JournalCategory journalCategory in _dataManager.GetExcelSheet<JournalCategory>())
    {
      string mainCategory = Regex.Replace(journalCategory.JournalSection.ValueNullable?.Name.ToString() ?? "Other Quests", @"\s*\([^)]*\)", "").Trim();
      string subCategory = journalCategory.Name.ToString();

      foreach (JournalGenre journalGenre in _dataManager.GetExcelSheet<JournalGenre>().Where((r) => r.JournalCategory.RowId == journalCategory.RowId))
      {
        if (journalGenre.RowId == 0) subCategory = "Quasi-Quests";
        string section = journalGenre.Name.ToString();

        foreach (Lumina.Excel.Sheets.Quest quest in _dataManager.GetExcelSheet<Lumina.Excel.Sheets.Quest>().Where((r) => r.JournalGenre.RowId == journalGenre.RowId && !r.Name.IsEmpty))
        {
          bool isSidequestCategory = subCategory.Contains("Sidequests");
          if (isSidequestCategory) section = quest.PlaceName.Value.Name.ToString();

          string? start = null;
          if (_gridaniaStartQuests.Contains(quest.RowId)) start = "Gridania";
          if (_limsaStartQuests.Contains(quest.RowId)) start = "Limsa Lominsa";
          if (_uldahStartQuests.Contains(quest.RowId)) start = "Ul'dah";

          string? gc = null;
          if (_twinAdderQuests.Contains(quest.RowId)) gc = "Order of the Twin Adder";
          if (_maelstromQuests.Contains(quest.RowId)) gc = "Maelstrom";
          if (_immortalFlamesQuests.Contains(quest.RowId)) gc = "Immortal Flames";

          List<uint> ids = [quest.RowId];
          if (quest.Expansion.RowId == 0)
          {
            foreach (List<uint> _ids in _arrShenanigans)
            {
              if (_ids.Contains(quest.RowId))
              {
                if (_arrShenanigansAdded.Contains(quest.RowId) || quest.JournalGenre.RowId == 0) goto SkipQuest;
                ids = _ids;
                _ids.ForEach(_arrShenanigansAdded.Add);
              }
            }
            foreach (List<uint> _ids in _furtherArrShenanigans)
            {
              if (_ids.Contains(quest.RowId)) ids = _ids;
            }
          }

          if (_retiredQuests.Contains(quest.RowId)) continue;
          AddQuest(mainCategory, subCategory, section, new()
          {
            Title = quest.Name.ToString(),
            Ids = ids,
            Area = quest.PlaceName.Value.Name.ToString(),
            Level = quest.ClassJobLevel[0],
            SortKey = quest.SortKey,
            Gc = gc,
            Start = start,
          }, sortKey: isSidequestCategory ? quest.PlaceName.RowId : 0);

        SkipQuest:
          continue;
        }

        foreach (Leve leve in _dataManager.GetExcelSheet<Leve>().Where((r) => r.JournalGenre.RowId == journalGenre.RowId && !r.Name.IsEmpty))
        {
          if (!leve.Name.ToString().Any(c => c <= 0x7F)) continue;
          section = leve.PlaceNameStart.Value.Name.ToString();

          string? start = null;
          if (_gridaniaStartLeves.Contains(leve.RowId)) start = "Gridania";
          if (_limsaStartLeves.Contains(leve.RowId)) start = "Limsa Lominsa";
          if (_uldahStartLeves.Contains(leve.RowId)) start = "Ul'dah";

          AddQuest(mainCategory, subCategory, section, new()
          {
            Title = leve.Name.ToString(),
            Ids = [leve.RowId],
            Area = leve.PlaceNameStart.Value.Name.ToString(),
            Level = leve.ClassJobLevel,
            SortKey = leve.RowId,
            Start = start,
            IsLeve = true
          }, leve.PlaceNameStart.RowId);
        }
      }
    }

    List<string> mainCategoryOrder = ["Main Scenario", "Chronicles of a New Era", "Sidequests", "Allied Society Quests", "Class & Job Quests", "Other Quests", "Levequests"];
    RawQuestData.Categories.Sort((a, b) =>
    {
      int ia = mainCategoryOrder.IndexOf(a.Title);
      int ib = mainCategoryOrder.IndexOf(b.Title);
      if (ia < 0) ia = int.MaxValue;
      if (ib < 0) ib = int.MaxValue;
      if (ia != ib) return ia.CompareTo(ib);
      return string.Compare(a.Title, b.Title, StringComparison.OrdinalIgnoreCase);
    });

    foreach (QuestData questData in RawQuestData.Categories)
    {
      if (questData.Title == "Allied Society Quests")
      {
        List<string> tribeOrder = [.. _dataManager.GetExcelSheet<BeastTribe>().Select((r) => r.NameRelation.ToString()).ToList(), "Intersocietal"];
        questData.Categories.Sort((a, b) =>
        {
          string fa = a.Title.Split(' ')[0];
          string fb = b.Title.Split(' ')[0];
          int ia = tribeOrder.FindIndex(t => t.Contains(fa, StringComparison.OrdinalIgnoreCase));
          int ib = tribeOrder.FindIndex(t => t.Contains(fb, StringComparison.OrdinalIgnoreCase));
          if (ia < 0) ia = int.MaxValue;
          if (ib < 0) ib = int.MaxValue;
          if (ia != ib) return ia.CompareTo(ib);
          return string.Compare(a.Title, b.Title, StringComparison.OrdinalIgnoreCase);
        });
      }

      foreach (QuestData c1 in questData.Categories)
      {
        c1.Categories.Sort((a, b) => a.SortKey.CompareTo(b.SortKey));
        foreach (QuestData c2 in c1.Categories)
        {
          c2.Quests.Sort((a, b) => a.SortKey.CompareTo(b.SortKey));
        }
      }
    }

    QuestData = RawQuestData;

    return _logger.ServiceLifecycle();
  }
  public Task StopAsync(CancellationToken cancellationToken)
  {
    _clientState.Login -= OnLogin;

    return _logger.ServiceLifecycle();
  }

  private void OnLogin()
  {
    _startArea = "";
    _grandCompany = "";
    _startClass = [];
    QuestData = RawQuestData;
  }

  private void AddQuest(string category, string subCategory, string section, Types.Quest quest, uint sortKey = 0)
  {
    QuestData FindOrCreateCategory(List<QuestData> list, string title)
    {
      QuestData? node = list.FirstOrDefault(c => string.Equals(c.Title, title, StringComparison.Ordinal));
      if (node == null)
      {
        node = new QuestData { Title = title };
        list.Add(node);
      }
      return node;
    }

    QuestData categoryNode = FindOrCreateCategory(RawQuestData.Categories, category);
    QuestData subCategoryNode = FindOrCreateCategory(categoryNode.Categories, subCategory);
    QuestData sectionNode = FindOrCreateCategory(subCategoryNode.Categories, section);
    sectionNode.SortKey = sortKey;
    sectionNode.Quests.Add(quest);
  }

  public unsafe bool IsQuestComplete(Types.Quest quest)
  {
    if (quest.IsLeve)
      return QuestManager.Instance()->IsLevequestComplete((ushort)quest.Ids[0]);

    foreach (uint id in quest.Ids)
      if (QuestManager.IsQuestComplete(id)) return true;

    return false;
  }

  public void UpdateQuestData()
  {
    UpdateQuestData(QuestData);
  }

  private void UpdateQuestData(QuestData questData)
  {
    questData.NumComplete = questData.Total = 0;
    if (_startArea == "") DetermineStartArea();
    if (_grandCompany == "") DetermineGrandCompany();
    if (_startClass.Count == 0) DetermineStartClass();

    if (questData.Categories.Count > 0)
    {
      questData.Hide = true;
      foreach (QuestData category in questData.Categories)
      {
        UpdateQuestData(category);
        questData.NumComplete += category.NumComplete;
        questData.Total += category.Total;
        if (!category.Hide) questData.Hide = false;
      }
    }
    else
    {
      questData.Hide = true;
      foreach (Types.Quest? quest in questData.Quests.ToList())
      {
        if (!_startArea.IsNullOrEmpty() && !quest.Start.IsNullOrEmpty() && _startArea != quest.Start)
        {
          if (IsQuestComplete(quest))
          {
            _logger.Error($"Quest {quest.Title} {string.Join(" ", quest.Ids)} is restricted but completed");
          }

          questData.Quests.Remove(quest);
          continue;
        }

        if (!_grandCompany.IsNullOrEmpty() && !quest.Gc.IsNullOrEmpty() && _grandCompany != quest.Gc)
        {
          if (IsQuestComplete(quest))
          {
            _logger.Error($"Quest {quest.Title} {string.Join(" ", quest.Ids)} is restricted but completed");
          }

          questData.Quests.Remove(quest);
          continue;
        }

        foreach (uint startClass in _startClass)
        {
          if (quest.Ids.Contains(startClass))
          {
            questData.Quests.Remove(quest);
            continue;
          }
        }

        // ARR "Call of the Wild" Tribal Alliance Quests
        if ((QuestManager.IsQuestComplete(67001) && (quest.Ids.Contains(67002) || quest.Ids.Contains(67003))) ||
            (QuestManager.IsQuestComplete(67002) && (quest.Ids.Contains(67001) || quest.Ids.Contains(67003))) ||
            (QuestManager.IsQuestComplete(67003) && (quest.Ids.Contains(67001) || quest.Ids.Contains(67002))) ||
            // YorHa "Heads or Tails"
            (QuestManager.IsQuestComplete(69256) && quest.Ids.Contains(69257)) ||
            (QuestManager.IsQuestComplete(69257) && quest.Ids.Contains(69256)) ||
            // Qitari "The First Stela"
            (QuestManager.IsQuestComplete(69336) && quest.Ids.Contains(69337)) ||
            (QuestManager.IsQuestComplete(69337) && quest.Ids.Contains(69336)) ||
            // Qitari "The Second Stela"
            (QuestManager.IsQuestComplete(69338) && quest.Ids.Contains(69339)) ||
            (QuestManager.IsQuestComplete(69339) && quest.Ids.Contains(69338)) ||
            // Qitari "The Third Stela"
            (QuestManager.IsQuestComplete(69340) && quest.Ids.Contains(69341)) ||
            (QuestManager.IsQuestComplete(69341) && quest.Ids.Contains(69340)) ||
            // An Ill-conceived Venture
            (QuestManager.IsQuestComplete(66968) && (quest.Ids.Contains(66969) || quest.Ids.Contains(66970))) ||
            (QuestManager.IsQuestComplete(66969) && (quest.Ids.Contains(66968) || quest.Ids.Contains(66970))) ||
            (QuestManager.IsQuestComplete(66970) && (quest.Ids.Contains(66968) || quest.Ids.Contains(66969))))
        {
          questData.Quests.Remove(quest);
        }

        if (IsQuestComplete(quest)) questData.NumComplete++;

        quest.Hide = (_configuration.DisplayOption == 1 && !IsQuestComplete(quest)) ||
                     (_configuration.DisplayOption == 2 && IsQuestComplete(quest));
        if (!quest.Hide) questData.Hide = false;
      }

      questData.Total += questData.Quests.Count;
    }
  }

  private void DetermineStartArea()
  {
    _startArea = QuestManager.IsQuestComplete(65575) ? "Gridania" :
                               QuestManager.IsQuestComplete(65643) ? "Limsa Lominsa" :
                               QuestManager.IsQuestComplete(66130) ? "Ul'dah" : "";
    _logger.Debug($"Start Area {_startArea}");
  }

  private void DetermineGrandCompany()
  {
    _grandCompany = QuestManager.IsQuestComplete(66216) ? "Order of the Twin Adder" :
                                  QuestManager.IsQuestComplete(66217) ? "Maelstrom" :
                                  QuestManager.IsQuestComplete(66218) ? "Immortal Flames" : "";
    _logger.Debug($"Grand Company {_grandCompany}");
  }

  private void DetermineStartClass()
  {
    _startClass = (
      // Gladiator
      QuestManager.IsQuestComplete(65792) && !QuestManager.IsQuestComplete(65822) ? [65822, 65713] :
      // Pugilist
      QuestManager.IsQuestComplete(66090) && !QuestManager.IsQuestComplete(66089) ? [66089, 65714] :
      // Marauder
      QuestManager.IsQuestComplete(65849) && !QuestManager.IsQuestComplete(65848) ? [65848, 65715] :
      // Lancer
      QuestManager.IsQuestComplete(65583) && !QuestManager.IsQuestComplete(65754) ? [65754, 65716] :
      // Archer
      QuestManager.IsQuestComplete(65582) && !QuestManager.IsQuestComplete(65755) ? [65755, 65717] :
      // Rogue
      QuestManager.IsQuestComplete(65640) && !QuestManager.IsQuestComplete(65638) ? [65638, 65637] :
      // Conjurer
      QuestManager.IsQuestComplete(65584) && !QuestManager.IsQuestComplete(65747) ? [65747, 65718] :
      // Thaumaturge
      QuestManager.IsQuestComplete(65883) && !QuestManager.IsQuestComplete(65882) ? [65882, 65719] :
      // Arcanist
      QuestManager.IsQuestComplete(65991) && !QuestManager.IsQuestComplete(65990) ? [65990, 65987] : []);
    _logger.Debug($"Start Class {_startClass}");
  }
}
