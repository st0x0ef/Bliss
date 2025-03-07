using Bliss.CSharp;
using Bliss.CSharp.Logging;
using Bliss.CSharp.Rendering;
using Bliss.CSharp.Windowing;
using Veldrid;
using Veldrid.Sdl2;

namespace Bliss.Test;

public class Game : Disposable {
    
    public static Game Instance { get; private set; }
    public GameSettings Settings { get; private set; }

    public Window Window { get; private set; }
    public GraphicsDevice GraphicsDevice { get; private set; }
    
    public CommandList CommandList { get; private set; }
    public Graphics Graphics { get; private set; }

    private double _fixedFrameRate;
    
    private readonly double _fixedUpdateTimeStep;
    private double _fixedUpdateTimer;
    
    public Game(GameSettings settings) {
        Instance = this;
        this.Settings = settings;
        this._fixedUpdateTimeStep = settings.FixedTimeStep;
    }

    public void Run() {
        Logger.Info("Hello World! Bliss start...");
        Logger.Info($"\t> CPU: {SystemInfo.Cpu}");
        Logger.Info($"\t> MEMORY: {SystemInfo.MemorySize} GB");
        Logger.Info($"\t> THREADS: {SystemInfo.Threads}");
        Logger.Info($"\t> OS: {SystemInfo.Os}");
        
        Logger.Info("Initialize window and graphics device...");
        GraphicsDeviceOptions options = new GraphicsDeviceOptions() {
            PreferStandardClipSpaceYDirection = true,
            PreferDepthRangeZeroToOne = true
        };
        
        this.Window = new Window(this.Settings.Width, this.Settings.Height, this.Settings.Title, options, this.Settings.Backend, out GraphicsDevice graphicsDevice);
        this.GraphicsDevice = graphicsDevice;
        
        Logger.Info("Initialize time...");
        Time.Init();
        
        Logger.Info($"Set target FPS to: {this.Settings.TargetFps}");
        this.SetTargetFps(this.Settings.TargetFps);
        
        Logger.Info("Initialize command list...");
        this.CommandList = this.GraphicsDevice.ResourceFactory.CreateCommandList();
        
        Logger.Info("Initialize graphics...");
        this.Graphics = new Graphics(this.GraphicsDevice, this.CommandList);
        
        this.Init();
        
        Logger.Info("Start main loops...");
        while (Window.Exists) {
            if (this.GetTargetFps() != 0 && Time.Timer.Elapsed.TotalSeconds <= this._fixedFrameRate) {
                continue;
            }
            Time.Update();
            
            this.Window.PumpEvents();
            Sdl2Events.ProcessEvents();
            
            this.Update();
            this.AfterUpdate();

            this._fixedUpdateTimer += Time.Delta;
            while (this._fixedUpdateTimer >= this._fixedUpdateTimeStep) {
                this.FixedUpdate();
                this._fixedUpdateTimer -= this._fixedUpdateTimeStep;
            }
            
            this.Graphics.BeginDrawing();
            this.Graphics.ClearBackground(0, RgbaFloat.Grey);
            this.Draw(this.Graphics);
            this.Graphics.EndDrawing();
        }
        
        Logger.Warn("Application shuts down!");
        this.OnClose();
    }

    protected virtual void Init() { }

    protected virtual void Update() { }

    protected virtual void AfterUpdate() { }

    protected virtual void FixedUpdate() { }

    protected virtual void Draw(Graphics graphics) { }
    
    protected virtual void OnClose() { }

    public int GetTargetFps() {
        return (int) (1.0F / this._fixedFrameRate);
    }

    public void SetTargetFps(int fps) {
        this._fixedFrameRate = 1.0F / fps;
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            this.GraphicsDevice.Dispose();
            this.Window.Close();
        }
    }
}