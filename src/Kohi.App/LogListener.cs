using System.Diagnostics;
using System.Text;
using ImGuiNET;
using Microsoft.Xna.Framework;

namespace Kohi;

internal sealed class LogListener : TraceListener
{
    private readonly StringBuilder buffer = new();

    private readonly System.Numerics.Vector4 infoColor = Color.LightBlue.ToImGuiVector4();
    private readonly System.Numerics.Vector4 warningColor = Color.Yellow.ToImGuiVector4();
    private readonly System.Numerics.Vector4 errorColor = Color.Red.ToImGuiVector4();

    private bool tail;

    public string Shortcut => "Ctrl+L";

    public LogListener()
    {
        Trace.Listeners.Add(this);
    }

    public void Draw()
    {
        if (ImGui.Button("Clear"))
            Clear();
        ImGui.Separator();
        ImGui.BeginChild("scrolling");
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new System.Numerics.Vector2(0, 1));
        if (buffer.Length > 0)
        {
            using var sr = new StringReader(buffer.ToString());
            while (sr.ReadLine() is { } line)
            {
                line = line.Replace("Kohi.App Information: 0 : ", "info: ");
                line = line.Replace("Kohi.App Warning: 0 : ", "warning: ");
                line = line.Replace("Kohi.App Error: 0 : ", "error: ");

                if (line.StartsWith("info:"))
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, infoColor);
                    ImGui.Text(line);
                    ImGui.PopStyleColor();
                }
                else if (line.StartsWith("warning:"))
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, warningColor);
                    ImGui.Text(line);
                    ImGui.PopStyleColor();
                }
                else if (line.StartsWith("error:"))
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, errorColor);
                    ImGui.Text(line);
                    ImGui.PopStyleColor();
                }
                else
                {
                    ImGui.Text(line);
                }
            }
        }
        if (tail)
            ImGui.SetScrollHereY(1.0f);
        tail = false;
        ImGui.PopStyleVar();
        ImGui.EndChild();
    }

    public void Clear()
    {
        buffer.Clear();
    }

    private void AddLog(string? message, params object[] args)
    {
        if (message == null) return;
        buffer.AppendFormat(message, args);
        tail = true;
    }

    #region TraceListener

    public override void Write(string? message)
    {
        AddLog(message);
    }

    public override void WriteLine(string? message)
    {
        AddLog(message + Environment.NewLine);
    }

    #endregion
}