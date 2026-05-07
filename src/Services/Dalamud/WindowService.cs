namespace QuestTracker.Services;

public interface IWindowService : IHostedService
{
  void Toggle();
}

public class WindowService(ILogger _logger, MainWindow _mainWindow, WindowSystem _windowSystem, IDalamudPluginInterface _pluginInterface) : IWindowService
{
  public Task StartAsync(CancellationToken cancellationToken)
  {
    _windowSystem.AddWindow(_mainWindow);

    _pluginInterface.UiBuilder.DisableCutsceneUiHide = true;
    _pluginInterface.UiBuilder.Draw += UiBuilderOnDraw;
    _pluginInterface.UiBuilder.OpenConfigUi += Toggle;
    _pluginInterface.UiBuilder.OpenMainUi += Toggle;

#if DEBUG
    _mainWindow.IsOpen = true; ;
#endif

    return _logger.ServiceLifecycle();
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    _pluginInterface.UiBuilder.OpenConfigUi -= Toggle;
    _pluginInterface.UiBuilder.OpenMainUi -= Toggle;
    _pluginInterface.UiBuilder.Draw -= UiBuilderOnDraw;

    _windowSystem.RemoveAllWindows();

    return _logger.ServiceLifecycle();
  }

  public void Toggle()
  {
    _mainWindow.Toggle();
  }

  private void UiBuilderOnDraw()
  {
    _windowSystem.Draw();
  }
}
