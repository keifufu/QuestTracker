using Microsoft.Extensions.Logging;
using ILogger = QuestTracker.Services.ILogger;

namespace QuestTracker;

public sealed class Plugin : IAsyncDalamudPlugin
{
  private readonly IHost _host;

  public Plugin(
    IChatGui chatGui,
    IGameGui gameGui,
    IPluginLog pluginLog,
    IClientState clientState,
    IDataManager dataManager,
    ICommandManager commandManager,
    IDalamudPluginInterface pluginInterface
  )
  {
    _host = new HostBuilder()
      .UseContentRoot(pluginInterface.ConfigDirectory.FullName)
      .ConfigureLogging(lb =>
      {
        lb.ClearProviders();
        lb.SetMinimumLevel(LogLevel.Trace);
      })
      .ConfigureServices(collection =>
      {
        collection.AddSingleton(chatGui);
        collection.AddSingleton(gameGui);
        collection.AddSingleton(pluginLog);
        collection.AddSingleton(clientState);
        collection.AddSingleton(dataManager);
        collection.AddSingleton(commandManager);
        collection.AddSingleton(pluginInterface);

        collection.AddSingleton<MainWindow>();
        collection.AddSingleton<ILogger, Logger>();
        collection.AddSingleton<IDataService, DataService>();
        collection.AddSingleton<IWindowService, WindowService>();
        collection.AddSingleton<ICommandService, CommandService>();

        collection.AddSingleton(InitializeConfiguration);
        collection.AddSingleton(new WindowSystem(pluginInterface.InternalName));

        collection.AddHostedService(sp => sp.GetRequiredService<IDataService>());
        collection.AddHostedService(sp => sp.GetRequiredService<IWindowService>());
        collection.AddHostedService(sp => sp.GetRequiredService<ICommandService>());
      }).Build();
  }

  private Configuration InitializeConfiguration(IServiceProvider s)
  {
    IDalamudPluginInterface pluginInterface = s.GetRequiredService<IDalamudPluginInterface>();
    Configuration configuration = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
    configuration.Initialize(pluginInterface);
    return configuration;
  }

  public async Task LoadAsync(CancellationToken cancellationToken)
    => await _host.StartAsync(cancellationToken);

  public async ValueTask DisposeAsync()
  {
    await _host.StopAsync();
    _host.Dispose();
  }
}
