﻿using System.Diagnostics;
using System.Reactive.Disposables;
using Prism.Ioc;
using WDE.Common.Managers;
using WDE.Common.Services.MessageBox;
using WDE.Common.Tasks;
using WDE.MapRenderer;
using WDE.MapRenderer.Managers;
using WDE.MapSpawns;
using WDE.MapSpawns.Rendering;
using WDE.MpqReader.DBC;
using WDE.MpqReader.Structures;

namespace RenderingTester;

public class DummyStatusBar : IStatusBar
{
    public void PublishNotification(INotification notification)
    {
    }
}

public class DummyGameView : IGameView
{
    public IEnumerable<Func<IContainerProvider, IGameModule>> Modules { get; } =
        new List<Func<IContainerProvider, IGameModule>>()
        {
            provider => provider.Resolve<StandaloneCustomGameModule>(),
            provider => provider.Resolve<DebugWindow>(),
            provider => provider.Resolve<SpawnViewer>()
        };
    public event Action<Func<IContainerProvider, IGameModule>>? ModuleRegistered;
    public event Action<Func<IContainerProvider, IGameModule>>? ModuleRemoved;
    public IDisposable RegisterGameModule(Func<IContainerProvider, IGameModule> gameModule) => Disposable.Empty;

    public Task<Game> Open()
    {
        return Task.FromResult<Game>(null!);
    }
}

public class DummyGameProperties : IGameProperties
{
    public bool OverrideLighting { get; set; } = false;
    public bool DisableTimeFlow { get; set; } = false;
    public int TimeSpeedMultiplier { get; set; } = 2;
    public bool ShowGrid { get; set; } = false;
    public Time CurrentTime { get; set; } = Time.FromMinutes(720);
    public float ViewDistanceModifier { get; set; } = 16;
    public bool ShowAreaTriggers { get; set; } = false;
    public int TextureQuality { get; set; } = 0;
    public float DynamicResolution { get; set; } = 1;
    public bool RenderGui { get; set; } = true;
}

public class DummyMessageBox : IMessageBoxService
{
    public Task<T?> ShowDialog<T>(IMessageBox<T> messageBox)
    {
        Console.WriteLine(messageBox.MainInstruction);
        Console.WriteLine(messageBox.Content);
        return Task.FromResult<T?>(default);
    }
}

public class MainThread : IMainThread
{
    private List<(TimeSpan delay, Action action)> delayed = new();

    public void Delay(Action action, TimeSpan delay)
    {
        delayed.Add((delay, action));
    }

    public void Dispatch(Action action)
    {
        action();
    }

    public Task Dispatch(Func<Task> action)
    {
        return action();
    }

    public IDisposable StartTimer(Func<bool> action, TimeSpan interval)
    {
        throw new Exception("Operation not supported, but could be implemented");
    }

    public void Tick(TimeSpan time)
    {
        for (int i = 0; i < delayed.Count; ++i)
        {
            if (delayed[i].delay <= time)
            {
                var action = delayed[i].action;
                if (i != delayed.Count - 1)
                {
                    delayed[i] = delayed[^1];
                    --i;
                }
                delayed.RemoveAt(delayed.Count - 1);
                
                action();
            }
            else
            {
                var newDelay = delayed[i].delay - time;
                delayed[i] = (newDelay, delayed[i].action);
            }
        }
    }
}