using System.Runtime.InteropServices;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Kohi;

public class ImGuiRenderer
{
    public GraphicsDevice GraphicsDevice { get; }

    private BasicEffect? effect;
    private readonly RasterizerState rasterizerState;

    private byte[]? vertexData;
    private int vertexBufferSize;
    private VertexBuffer? vertexBuffer;

    private byte[]? indexData;
    private int indexBufferSize;
    private IndexBuffer? indexBuffer;

    private readonly Dictionary<IntPtr, Texture2D> loadedTextures;
    private int textureId;
    private IntPtr? fontTextureId;

    private int scrollWheelValue;
    private readonly List<int> keys = new();

    public ImGuiRenderer(GraphicsDevice graphicsDevice)
    {
        var context = ImGui.CreateContext();
        ImGui.SetCurrentContext(context);
        GraphicsDevice = graphicsDevice;

        loadedTextures = new Dictionary<IntPtr, Texture2D>();

        rasterizerState = new RasterizerState()
        {
            CullMode = CullMode.None,
            DepthBias = 0,
            FillMode = FillMode.Solid,
            MultiSampleAntiAlias = false,
            ScissorTestEnable = true,
            SlopeScaleDepthBias = 0
        };

        SetupInput();
    }

    public unsafe void RebuildFontAtlas()
    {
        var io = ImGui.GetIO();
        io.Fonts.GetTexDataAsRGBA32(out byte* pixelData, out var width, out var height, out var bytesPerPixel);

        var pixels = new byte[width * height * bytesPerPixel];
        Marshal.Copy(new IntPtr(pixelData), pixels, 0, pixels.Length);

        var texture = new Texture2D(GraphicsDevice, width, height, false, SurfaceFormat.Color);
        texture.SetData(pixels);

        if (fontTextureId.HasValue)
            UnbindTexture(fontTextureId.Value);

        fontTextureId = BindTexture(texture);

        io.Fonts.SetTexID(fontTextureId.Value);
        io.Fonts.ClearTexData();
    }

    public IntPtr BindTexture(Texture2D texture)
    {
        var id = new IntPtr(textureId++);
        loadedTextures.Add(id, texture);
        return id;
    }

    public void UnbindTexture(IntPtr id)
    {
        loadedTextures.Remove(id);
    }

    public void BeforeLayout(GameTime gameTime)
    {
        ImGui.GetIO().DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        UpdateInput();
        ImGui.NewFrame();
    }

    public void AfterLayout()
    {
        ImGui.Render();
        RenderDrawData(ImGui.GetDrawData());
    }

    protected void SetupInput()
    {
        var io = ImGui.GetIO();

        keys.Add(io.KeyMap[(int)ImGuiKey.Tab] = (int)Keys.Tab);
        keys.Add(io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Keys.Left);
        keys.Add(io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Keys.Right);
        keys.Add(io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Keys.Up);
        keys.Add(io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Keys.Down);
        keys.Add(io.KeyMap[(int)ImGuiKey.PageUp] = (int)Keys.PageUp);
        keys.Add(io.KeyMap[(int)ImGuiKey.PageDown] = (int)Keys.PageDown);
        keys.Add(io.KeyMap[(int)ImGuiKey.Home] = (int)Keys.Home);
        keys.Add(io.KeyMap[(int)ImGuiKey.End] = (int)Keys.End);
        keys.Add(io.KeyMap[(int)ImGuiKey.Delete] = (int)Keys.Delete);
        keys.Add(io.KeyMap[(int)ImGuiKey.Backspace] = (int)Keys.Back);
        keys.Add(io.KeyMap[(int)ImGuiKey.Enter] = (int)Keys.Enter);
        keys.Add(io.KeyMap[(int)ImGuiKey.Escape] = (int)Keys.Escape);
        keys.Add(io.KeyMap[(int)ImGuiKey.Space] = (int)Keys.Space);
        keys.Add(io.KeyMap[(int)ImGuiKey.A] = (int)Keys.A);
        keys.Add(io.KeyMap[(int)ImGuiKey.C] = (int)Keys.C);
        keys.Add(io.KeyMap[(int)ImGuiKey.V] = (int)Keys.V);
        keys.Add(io.KeyMap[(int)ImGuiKey.X] = (int)Keys.X);
        keys.Add(io.KeyMap[(int)ImGuiKey.Y] = (int)Keys.Y);
        keys.Add(io.KeyMap[(int)ImGuiKey.Z] = (int)Keys.Z);

        TextInputEXT.TextInput += c =>
        {
            if (c == '\t') return;
            ImGui.GetIO().AddInputCharacter(c);
        };

        ImGui.GetIO().Fonts.AddFontDefault();
    }

    protected virtual Effect UpdateEffect(Texture2D texture)
    {
        effect ??= new BasicEffect(GraphicsDevice);

        var io = ImGui.GetIO();

        effect.World = Matrix.Identity;
        effect.View = Matrix.Identity;
        effect.Projection = Matrix.CreateOrthographicOffCenter(0f, io.DisplaySize.X, io.DisplaySize.Y, 0f, -1f, 1f);
        effect.TextureEnabled = true;
        effect.Texture = texture;
        effect.VertexColorEnabled = true;

        return effect;
    }

    protected virtual void UpdateInput()
    {
        var io = ImGui.GetIO();

        var mouse = Mouse.GetState();
        var keyboard = Keyboard.GetState();

        foreach (var key in keys)
            io.KeysDown[key] = keyboard.IsKeyDown((Keys)key);

        io.KeyShift = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);
        io.KeyCtrl = keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl);
        io.KeyAlt = keyboard.IsKeyDown(Keys.LeftAlt) || keyboard.IsKeyDown(Keys.RightAlt);
        io.KeySuper = keyboard.IsKeyDown(Keys.LeftWindows) || keyboard.IsKeyDown(Keys.RightWindows);

        io.DisplaySize = new System.Numerics.Vector2(GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
        io.DisplayFramebufferScale = new System.Numerics.Vector2(1f, 1f);

        io.MousePos = new System.Numerics.Vector2(mouse.X, mouse.Y);

        io.MouseDown[0] = mouse.LeftButton == ButtonState.Pressed;
        io.MouseDown[1] = mouse.RightButton == ButtonState.Pressed;
        io.MouseDown[2] = mouse.MiddleButton == ButtonState.Pressed;

        var scrollDelta = mouse.ScrollWheelValue - scrollWheelValue;
        io.MouseWheel = scrollDelta > 0 ? 1 : scrollDelta < 0 ? -1 : 0;
        scrollWheelValue = mouse.ScrollWheelValue;
    }

    private void RenderDrawData(ImDrawDataPtr drawData)
    {
        var lastViewport = GraphicsDevice.Viewport;
        var lastScissorBox = GraphicsDevice.ScissorRectangle;

        GraphicsDevice.BlendFactor = Color.White;
        GraphicsDevice.BlendState = BlendState.NonPremultiplied;
        GraphicsDevice.RasterizerState = rasterizerState;
        GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

        drawData.ScaleClipRects(ImGui.GetIO().DisplayFramebufferScale);

        GraphicsDevice.Viewport = new Viewport(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
        UpdateBuffers(drawData);
        RenderCommandLists(drawData);

        GraphicsDevice.Viewport = lastViewport;
        GraphicsDevice.ScissorRectangle = lastScissorBox;
    }

    private unsafe void UpdateBuffers(ImDrawDataPtr drawData)
    {
        if (drawData.TotalVtxCount == 0)
        {
            return;
        }

        if (drawData.TotalVtxCount > vertexBufferSize)
        {
            vertexBuffer?.Dispose();
            vertexBufferSize = (int)(drawData.TotalVtxCount * 1.5f);
            vertexBuffer = new VertexBuffer(GraphicsDevice, VertexPositionTextureColor.Declaration, vertexBufferSize, BufferUsage.None);
            vertexData = new byte[vertexBufferSize * VertexPositionTextureColor.Size];
        }

        if (drawData.TotalIdxCount > indexBufferSize)
        {
            indexBuffer?.Dispose();
            indexBufferSize = (int)(drawData.TotalIdxCount * 1.5f);
            indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, indexBufferSize, BufferUsage.None);
            indexData = new byte[indexBufferSize * sizeof(ushort)];
        }

        var vtxOffset = 0;
        var idxOffset = 0;
        for (var n = 0; n < drawData.CmdListsCount; n++)
        {
            var cmdList = drawData.CmdListsRange[n];
            fixed (void* vtxDstPtr = &vertexData![vtxOffset * VertexPositionTextureColor.Size])
            fixed (void* idxDstPtr = &indexData![idxOffset * sizeof(ushort)])
            {
                Buffer.MemoryCopy((void*)cmdList.VtxBuffer.Data, vtxDstPtr, vertexData.Length, cmdList.VtxBuffer.Size * VertexPositionTextureColor.Size);
                Buffer.MemoryCopy((void*)cmdList.IdxBuffer.Data, idxDstPtr, indexData.Length, cmdList.IdxBuffer.Size * sizeof(ushort));
            }
            vtxOffset += cmdList.VtxBuffer.Size;
            idxOffset += cmdList.IdxBuffer.Size;
        }

        // Copy the managed byte arrays to the gpu vertex- and index buffers
        vertexBuffer?.SetData(vertexData, 0, drawData.TotalVtxCount * VertexPositionTextureColor.Size);
        indexBuffer?.SetData(indexData, 0, drawData.TotalIdxCount * sizeof(ushort));
    }

    private void RenderCommandLists(ImDrawDataPtr drawData)
    {
        GraphicsDevice.SetVertexBuffer(vertexBuffer);
        GraphicsDevice.Indices = indexBuffer;

        var vtxOffset = 0;
        var idxOffset = 0;

        for (var n = 0; n < drawData.CmdListsCount; n++)
        {
            var cmdList = drawData.CmdListsRange[n];

            for (var cmdIndex = 0; cmdIndex < cmdList.CmdBuffer.Size; cmdIndex++)
            {
                var drawCmd = cmdList.CmdBuffer[cmdIndex];
                if (drawCmd.ElemCount == 0)
                    continue;

                if (!loadedTextures.ContainsKey(drawCmd.TextureId))
                    throw new InvalidOperationException($"Could not find a texture with id '{drawCmd.TextureId}', please check your bindings");

                GraphicsDevice.ScissorRectangle = new Rectangle(
                    (int)drawCmd.ClipRect.X,
                    (int)drawCmd.ClipRect.Y,
                    (int)(drawCmd.ClipRect.Z - drawCmd.ClipRect.X),
                    (int)(drawCmd.ClipRect.W - drawCmd.ClipRect.Y)
                );

                var fx = UpdateEffect(loadedTextures[drawCmd.TextureId]);
                foreach (var pass in fx.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    GraphicsDevice.DrawIndexedPrimitives(
                        primitiveType: PrimitiveType.TriangleList,
                        baseVertex: (int)drawCmd.VtxOffset + vtxOffset,
                        minVertexIndex: 0,
                        numVertices: cmdList.VtxBuffer.Size,
                        startIndex: (int)drawCmd.IdxOffset + idxOffset,
                        primitiveCount: (int)drawCmd.ElemCount / 3
                    );
                }
            }

            vtxOffset += cmdList.VtxBuffer.Size;
            idxOffset += cmdList.IdxBuffer.Size;
        }
    }
}