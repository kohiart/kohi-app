using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using ImGuiNET;

namespace Kohi;

public class Kohi : Game
{
    private readonly GraphicsDeviceManager graphics;
    public SpriteBatch sb = null!;

    private ImGuiRenderer imGui = null!;
    private readonly LogListener logListener;

    private bool lastActive;
    private bool devMenuEnabled = true;
    private bool showLogWindow;

    private LoadingScreen loadingScreen = null!;

    private bool oneTimeBackgroundLoad;
    private Task backgroundLoadTask = null!;

    public Kohi()
    {
        logListener = new LogListener();
        TargetElapsedTime = Constants.LoadingScreenFrameTime;

        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";

        Window.Title = "Kohi";
        Window.AllowUserResizing = false;
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        GraphicsDevice.PresentationParameters.MultiSampleCount = 8;
        GraphicsDevice.PresentationParameters.BackBufferFormat = SurfaceFormat.Color;

        GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
        GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        GraphicsDevice.RasterizerState.MultiSampleAntiAlias = true;
        GraphicsDevice.RasterizerState.FillMode = FillMode.Solid;
        GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicClamp;
        GraphicsDevice.BlendState = BlendState.Opaque;

        loadingScreen = new LoadingScreen(this);

        imGui = new ImGuiRenderer(GraphicsDevice);
        imGui.RebuildFontAtlas();

        base.Initialize();
    }
    
    protected override void LoadContent()
    {
        sb = new SpriteBatch(GraphicsDevice);

        graphics.GraphicsProfile = GraphicsProfile.HiDef;
        graphics.PreferMultiSampling = true;
        graphics.PreferredBackBufferWidth = 1024;
        graphics.PreferredBackBufferHeight = 768;
        graphics.ApplyChanges();

        if (!oneTimeBackgroundLoad)
        {
            StartBackgroundLoading();
            oneTimeBackgroundLoad = true;
        }
    }

    #region Update 

    protected override void Update(GameTime gameTime)
    {
        // calls FrameworkDispatcher
        base.Update(gameTime);

        Input.Update(IsActive);

        if (DidFinishLoading)
        {
            if (devMenuEnabled)
                UpdateEditor();
            else if (Input.KeyWentDown(Keys.F1))
                devMenuEnabled = !devMenuEnabled;
        }
        else
        {
            if (!backgroundLoadTask.IsCompleted)
                return;

            if (!DidFinishLoading)
                backgroundLoadTask.Wait();

            OnFinishedLoading();
        }
    }

    private void UpdateEditor()
    {
        if (lastActive != IsActive)
        {
            if (IsActive)
                DidGainFocus();
            else
                DidLoseFocus();
        }

        lastActive = IsActive;

        if (Input.KeyWentDown(Keys.F1))
            devMenuEnabled = !devMenuEnabled;

        if (ShortcutWentDown(logListener.Shortcut))
            showLogWindow = !showLogWindow;
    }

    private static bool ShortcutWentDown(string shortcut)
    {
        var tokens = shortcut.Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
        for (var j = 0; j < tokens.Length; j++)
            tokens[j] = tokens[j].Trim();

        bool isControl = false, isAlt = false, isShift = false;

        for (var t = 0; t < tokens.Length; t++)
        {
            if (tokens[t].Equals("Ctrl", StringComparison.OrdinalIgnoreCase))
            {
                isControl = true;
                continue;
            }

            if (tokens[t].Equals("Alt", StringComparison.OrdinalIgnoreCase))
            {
                isAlt = true;
                continue;
            }

            if (tokens[t].Equals("Shift", StringComparison.OrdinalIgnoreCase))
            {
                isShift = true;
                continue;
            }

            if (tokens[t].Equals("0", StringComparison.OrdinalIgnoreCase))
            {
                tokens[t] = "D0";
            }

            if (tokens[t].Equals("1", StringComparison.OrdinalIgnoreCase))
            {
                tokens[t] = "D1";
            }

            if (tokens[t].Equals("2", StringComparison.OrdinalIgnoreCase))
            {
                tokens[t] = "D2";
            }

            if (tokens[t].Equals("3", StringComparison.OrdinalIgnoreCase))
            {
                tokens[t] = "D3";
            }

            if (tokens[t].Equals("4", StringComparison.OrdinalIgnoreCase))
            {
                tokens[t] = "D4";
            }

            if (tokens[t].Equals("5", StringComparison.OrdinalIgnoreCase))
            {
                tokens[t] = "D5";
            }

            if (tokens[t].Equals("6", StringComparison.OrdinalIgnoreCase))
            {
                tokens[t] = "D6";
            }

            if (tokens[t].Equals("7", StringComparison.OrdinalIgnoreCase))
            {
                tokens[t] = "D7";
            }

            if (tokens[t].Equals("8", StringComparison.OrdinalIgnoreCase))
            {
                tokens[t] = "D8";
            }

            if (tokens[t].Equals("9", StringComparison.OrdinalIgnoreCase))
            {
                tokens[t] = "D9";
            }

            if (!Enum.TryParse(tokens[t], true, out Keys keys))
                continue;

            var control = isControl;
            var alt = isAlt;
            var shift = isShift;

            if (control && !Input.Control)
                return false;
            if (alt && !Input.Alt)
                return false;
            if (shift && !Input.Shift)
                return false;

            return Input.KeyWentDown(keys);
        }

        return false;
    }

    private static void DidLoseFocus() { }

    private static void DidGainFocus() { }

    #endregion

    #region Draw

    protected override void Draw(GameTime gameTime)
    {
        if (DidFinishLoading)
        {
            DrawScreen(gameTime);
        }
        else
        {
            loadingScreen.Draw();
        }

        // calls component draw
        base.Draw(gameTime);
    }

    private void DrawScreen(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        if (devMenuEnabled)
        {
            imGui.BeforeLayout(gameTime);
            DrawEditor(gameTime);
            imGui.AfterLayout();
        }
    }

    private void DrawEditor(GameTime gameTime)
    {
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("Editor"))
            {
                if (ImGui.MenuItem("Toggle UI", "F1", devMenuEnabled, devMenuEnabled))
                    devMenuEnabled = !devMenuEnabled;

                ImGui.Separator();

                if (ImGui.MenuItem("Quit"))
                    Exit();

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Window"))
            {
                if (ImGui.MenuItem("Log", logListener.Shortcut, showLogWindow, true))
                    showLogWindow = !showLogWindow;

                ImGui.EndMenu();
            }

            var fps = $"{ImGui.GetIO().Framerate:F2} FPS ({1000f / ImGui.GetIO().Framerate:F2} ms)";

            ImGui.SameLine(Window.ClientBounds.Width - ImGui.CalcTextSize(fps).X - 10f);
            if (gameTime.IsRunningSlowly)
                ImGui.TextColored(Color.Red.ToImGuiVector4(), fps);
            else
                ImGui.Text(fps);

            ImGui.EndMainMenuBar();
        }

        if (showLogWindow)
        {
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(500, 100), ImGuiCond.FirstUseEver);
            if (ImGui.Begin("Logs", ref showLogWindow, ImGuiWindowFlags.None))
                logListener.Draw();
            ImGui.End();
        }
    }

    #endregion

    #region Background Loading

    public bool DidFinishLoading { get; private set; }

    private void StartBackgroundLoading()
    {
        var backgroundTasks = new[]
        {
            Task.Delay(5000)
        };

        if (backgroundTasks.Length == 0)
        {
            DidFinishLoading = true;
            OnFinishedLoading();
            return;
        }

        var sw = Stopwatch.StartNew();

        backgroundLoadTask = Task.Factory.ContinueWhenAll(backgroundTasks, tasks =>
        {
            Task.WaitAll(tasks);
            Trace.TraceInformation($"Background load completed in {sw.Elapsed}");
            OnFinishedLoading();
        });
    }

    public void OnFinishedLoading()
    {
        DidFinishLoading = true;
        TargetElapsedTime = Constants.DefaultFrameTime;
    }

    #endregion
}