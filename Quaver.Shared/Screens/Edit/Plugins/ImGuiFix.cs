using System.Runtime.CompilerServices;
using Hexa.NET.ImGui;

namespace Quaver.Shared.Screens.Edit.Plugins;

/// <summary>
///     Compatibility helpers for ImGui calls whose nullable-pointer behavior is not expressible with a normal C# ref.
/// </summary>
public static class ImGuiFix
{
    public static bool InputFloat(
        string label,
        ref float value,
        float step,
        float stepFast,
        string format,
        ImGuiInputTextFlags flags
    )
    {
        var changed = ImGui.InputFloat(label, ref value, step, stepFast, format,
            flags & ~ImGuiInputTextFlags.EnterReturnsTrue);
        return ScalarInputResult(changed, flags);
    }

    public static bool InputInt(
        string label,
        ref int value,
        int step,
        int stepFast,
        ImGuiInputTextFlags flags
    )
    {
        var changed = ImGui.InputInt(label, ref value, step, stepFast,
            flags & ~ImGuiInputTextFlags.EnterReturnsTrue);
        return ScalarInputResult(changed, flags);
    }

    private static bool ScalarInputResult(bool changed, ImGuiInputTextFlags flags) =>
        (flags & ImGuiInputTextFlags.EnterReturnsTrue) != 0 ? ImGui.IsItemDeactivatedAfterEdit() : changed;

    public static unsafe bool BeginTabItem(string label, ref bool p_open, ImGuiTabItemFlags flags)
    {
        if (Unsafe.IsNullRef(ref p_open))
            return ImGui.BeginTabItem(label, flags);

        return ImGui.BeginTabItem(label, ref p_open, flags);
    }
}
