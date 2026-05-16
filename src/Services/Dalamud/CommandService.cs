namespace QuestTracker.Services;

public interface ICommandService : IHostedService;

public class CommandService(ILogger _logger, IDataService _dataService, IWindowService _windowService, ICommandManager _commandManager) : ICommandService
{
  private const string QuestTrackerCommand = "/questtracker";
  private const string QuestTrackerCommandAlias = "/qt";

  public Task StartAsync(CancellationToken cancellationToken)
  {
    _commandManager.AddHandler(QuestTrackerCommand, new CommandInfo(OnCommand)
    {
      HelpMessage = $"See '{QuestTrackerCommand} help' for more."
    });
    _commandManager.AddHandler(QuestTrackerCommandAlias, new CommandInfo(OnCommand)
    {
      HelpMessage = $"Alias for {QuestTrackerCommand}."
    });

    return _logger.ServiceLifecycle();
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    _commandManager.RemoveHandler(QuestTrackerCommand);
    _commandManager.RemoveHandler(QuestTrackerCommandAlias);

    return _logger.ServiceLifecycle();
  }

  private async void OnCommand(string command, string arguments)
  {
    _logger.Debug($"command::'{command}' arguments::'{arguments}'");

    string[] args = arguments.Split(" ", StringSplitOptions.RemoveEmptyEntries);
    if (args.Length == 0)
    {
      _windowService.Toggle();
      return;
    }

    switch (args[0])
    {
      case "help":
      case "?":
        _logger.Chat("Available commands:");
        _logger.Chat($"  {command} reset - Resets the Quest Tracker");
        _logger.Chat($"  {command} - Opens the QuestTracker window");
        break;
      case "reset":
        _dataService.Reset();
        break;
      default:
        _logger.Chat("Invalid command:");
        _logger.Chat($"  {command} {arguments}");
        goto case "help";
    }
  }
}
