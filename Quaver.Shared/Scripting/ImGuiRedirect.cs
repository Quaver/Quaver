using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using ImGuiNET;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.CoreLib;
using MoonSharp.Interpreter.Interop;
using Wobble.Logging;

#pragma warning disable
// ReSharper disable ConvertToAutoPropertyWhenPossible InconsistentNaming MissingBlankLines UnusedMember.Global

namespace Quaver.Shared.Scripting
{
    [MoonSharpUserData]
    public static class ImGuiRedirect
    {
        public static void BeginPopupContextWindow() => ImGui.BeginPopupContextWindow();

        public static void BeginPopupContextWindow(string str_id) => ImGui.BeginPopupContextWindow(str_id);

        public static void BeginPopupContextWindow(string str_id, ImGuiPopupFlags popup_flags) =>
            ImGui.BeginPopupContextWindow(str_id, popup_flags);

        public static void BeginPopupContextWindow(string str_id, ImGuiPopupFlags popup_flags, bool also_over_items) =>
            ImGui.BeginPopupContextWindow(
                str_id,
                popup_flags | (also_over_items ? ImGuiPopupFlags.NoOpenOverItems : ImGuiPopupFlags.None)
            );

        public static void PlotHistogram(string label, float[] values) => PlotHistogram(label, values, values.Length);

        public static void PlotHistogram(string label, float[] values, int values_count) =>
            ImGui.PlotHistogram(label, ref Safe(values, values_count), values_count);

        public static void PlotHistogram(string label, float[] values, int values_count, int values_offset) =>
            ImGui.PlotHistogram(label, ref Safe(values, values_count - values_offset), values_count, values_offset);

        public static void PlotHistogram(
            string label,
            float[] values,
            int values_count,
            int values_offset,
            string overlay_text
        ) =>
            ImGui.PlotHistogram(
                label,
                ref Safe(values, values_count - values_offset),
                values_count,
                values_offset,
                overlay_text
            );

        public static void PlotHistogram(
            string label,
            float[] values,
            int values_count,
            int values_offset,
            string overlay_text,
            float scale_min
        ) =>
            ImGui.PlotHistogram(
                label,
                ref Safe(values, values_count - values_offset),
                values_count,
                values_offset,
                overlay_text,
                scale_min
            );

        public static void PlotHistogram(
            string label,
            float[] values,
            int values_count,
            int values_offset,
            string overlay_text,
            float scale_min,
            float scale_max
        ) =>
            ImGui.PlotHistogram(
                label,
                ref Safe(values, values_count - values_offset),
                values_count,
                values_offset,
                overlay_text,
                scale_min,
                scale_max
            );

        public static void PlotHistogram(
            string label,
            float[] values,
            int values_count,
            int values_offset,
            string overlay_text,
            float scale_min,
            float scale_max,
            Vector2 graph_size
        ) =>
            ImGui.PlotHistogram(
                label,
                ref Safe(values, values_count - values_offset),
                values_count,
                values_offset,
                overlay_text,
                scale_min,
                scale_max,
                graph_size
            );

        public static void PlotHistogram(
            string label,
            float[] values,
            int values_count,
            int values_offset,
            string overlay_text,
            float scale_min,
            float scale_max,
            Vector2 graph_size,
            int stride
        ) =>
            ImGui.PlotHistogram(
                label,
                ref Safe(values, values_count - values_offset),
                values_count,
                values_offset,
                overlay_text,
                scale_min,
                scale_max,
                graph_size,
                stride
            );

        public static void PlotLines(string label, float[] values) => PlotLines(label, values, values.Length);

        public static void PlotLines(string label, float[] values, int values_count) =>
            ImGui.PlotLines(label, ref Safe(values, values_count), values_count);

        public static void PlotLines(string label, float[] values, int values_count, int values_offset) =>
            ImGui.PlotLines(label, ref Safe(values, values_count - values_offset), values_count, values_offset);

        public static void PlotLines(
            string label,
            float[] values,
            int values_count,
            int values_offset,
            string overlay_text
        ) =>
            ImGui.PlotLines(
                label,
                ref Safe(values, values_count - values_offset),
                values_count,
                values_offset,
                overlay_text
            );

        public static void PlotLines(
            string label,
            float[] values,
            int values_count,
            int values_offset,
            string overlay_text,
            float scale_min
        ) =>
            ImGui.PlotLines(
                label,
                ref Safe(values, values_count - values_offset),
                values_count,
                values_offset,
                overlay_text,
                scale_min
            );

        public static void PlotLines(
            string label,
            float[] values,
            int values_count,
            int values_offset,
            string overlay_text,
            float scale_min,
            float scale_max
        ) =>
            ImGui.PlotLines(
                label,
                ref Safe(values, values_count - values_offset),
                values_count,
                values_offset,
                overlay_text,
                scale_min,
                scale_max
            );

        public static void PlotLines(
            string label,
            float[] values,
            int values_count,
            int values_offset,
            string overlay_text,
            float scale_min,
            float scale_max,
            Vector2 graph_size
        ) =>
            ImGui.PlotLines(
                label,
                ref Safe(values, values_count - values_offset),
                values_count,
                values_offset,
                overlay_text,
                scale_min,
                scale_max,
                graph_size
            );

        public static void PlotLines(
            string label,
            float[] values,
            int values_count,
            int values_offset,
            string overlay_text,
            float scale_min,
            float scale_max,
            Vector2 graph_size,
            int stride
        ) =>
            ImGui.PlotLines(
                label,
                ref Safe(values, values_count - values_offset),
                values_count,
                values_offset,
                overlay_text,
                scale_min,
                scale_max,
                graph_size,
                stride
            );

        public static void PushAllowKeyboardFocus() => PushTabStop();

        public static void PushButtonRepeat() => ImGui.PushItemFlag(ImGuiItemFlags.ButtonRepeat, true);

        public static void PushTabStop() => ImGui.PushItemFlag(ImGuiItemFlags.NoTabStop, true);

        public static void SaveIniSettingsToDisk(string ini_filename) =>
            throw new NotSupportedException("Please use the 'write' global instead.");

        public static void TreeAdvanceToLabelPos() =>
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetTreeNodeToLabelSpacing());

        public static void TreePush() => ImGui.TreePush(null);

        public static void TreePush(nint ptr_id) => ImGui.TreePush(ptr_id);

        public static void TreePush(string str_id) => ImGui.TreePush(str_id);

        public static bool DragInt2(string label, ref int[] v) => ImGui.DragInt2(label, ref Safe(v));

        public static bool DragInt2(string label, ref int[] v, float v_speed) =>
            ImGui.DragInt2(label, ref Safe(v), v_speed);

        public static bool DragInt2(string label, ref int[] v, float v_speed, int v_min) =>
            ImGui.DragInt2(label, ref Safe(v), v_speed, v_min);

        public static bool DragInt2(string label, ref int[] v, float v_speed, int v_min, int v_max) =>
            ImGui.DragInt2(label, ref Safe(v), v_speed, v_min, v_max);

        public static bool DragInt2(string label, ref int[] v, float v_speed, int v_min, int v_max, string format) =>
            ImGui.DragInt2(label, ref Safe(v), v_speed, v_min, v_max, format);

        public static bool DragInt2(
            string label,
            ref int[] v,
            float v_speed,
            int v_min,
            int v_max,
            string format,
            ImGuiSliderFlags flags
        ) =>
            ImGui.DragInt2(label, ref Safe(v), v_speed, v_min, v_max, format, flags);

        public static bool DragInt2(string label, ref Vector2 v, float v_speed)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y };
            var ret = ImGui.DragInt2(label, ref span[0], v_speed);
            v.X = span[0];
            v.Y = span[1];
            return ret;
        }

        public static bool DragInt2(string label, ref Vector2 v, float v_speed, int v_min)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y };
            var ret = ImGui.DragInt2(label, ref span[0], v_speed, v_min);
            v.X = span[0];
            v.Y = span[1];
            return ret;
        }

        public static bool DragInt2(string label, ref Vector2 v, float v_speed, int v_min, int v_max)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y };
            var ret = ImGui.DragInt2(label, ref span[0], v_speed, v_min, v_max);
            v.X = span[0];
            v.Y = span[1];
            return ret;
        }

        public static bool DragInt2(string label, ref Vector2 v, float v_speed, int v_min, int v_max, string format)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y };
            var ret = ImGui.DragInt2(label, ref span[0], v_speed, v_min, v_max, format);
            v.X = span[0];
            v.Y = span[1];
            return ret;
        }

        public static bool DragInt2(
            string label,
            ref Vector2 v,
            float v_speed,
            int v_min,
            int v_max,
            string format,
            ImGuiSliderFlags flags
        )
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y };
            var ret = ImGui.DragInt2(label, ref span[0], v_speed, v_min, v_max, format, flags);
            v.X = span[0];
            v.Y = span[1];
            return ret;
        }

        public static bool DragInt3(string label, ref int[] v) => ImGui.DragInt3(label, ref Safe(v));

        public static bool DragInt3(string label, ref int[] v, float v_speed) =>
            ImGui.DragInt3(label, ref Safe(v), v_speed);

        public static bool DragInt3(string label, ref int[] v, float v_speed, int v_min) =>
            ImGui.DragInt3(label, ref Safe(v), v_speed, v_min);

        public static bool DragInt3(string label, ref int[] v, float v_speed, int v_min, int v_max) =>
            ImGui.DragInt3(label, ref Safe(v), v_speed, v_min, v_max);

        public static bool DragInt3(string label, ref int[] v, float v_speed, int v_min, int v_max, string format) =>
            ImGui.DragInt3(label, ref Safe(v), v_speed, v_min, v_max, format);

        public static bool DragInt3(
            string label,
            ref int[] v,
            float v_speed,
            int v_min,
            int v_max,
            string format,
            ImGuiSliderFlags flags
        ) =>
            ImGui.DragInt3(label, ref Safe(v), v_speed, v_min, v_max, format, flags);

        public static bool DragInt3(string label, ref Vector3 v, float v_speed)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z };
            var ret = ImGui.DragInt3(label, ref span[0], v_speed);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            return ret;
        }

        public static bool DragInt3(string label, ref Vector3 v, float v_speed, int v_min)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z };
            var ret = ImGui.DragInt3(label, ref span[0], v_speed, v_min);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            return ret;
        }

        public static bool DragInt3(string label, ref Vector3 v, float v_speed, int v_min, int v_max)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z };
            var ret = ImGui.DragInt3(label, ref span[0], v_speed, v_min, v_max);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            return ret;
        }

        public static bool DragInt3(string label, ref Vector3 v, float v_speed, int v_min, int v_max, string format)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z };
            var ret = ImGui.DragInt3(label, ref span[0], v_speed, v_min, v_max, format);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            return ret;
        }

        public static bool DragInt3(
            string label,
            ref Vector3 v,
            float v_speed,
            int v_min,
            int v_max,
            string format,
            ImGuiSliderFlags flags
        )
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z };
            var ret = ImGui.DragInt3(label, ref span[0], v_speed, v_min, v_max, format, flags);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            return ret;
        }

        public static bool DragInt4(string label, ref int[] v) => ImGui.DragInt4(label, ref Safe(v));

        public static bool DragInt4(string label, ref int[] v, float v_speed) =>
            ImGui.DragInt4(label, ref Safe(v), v_speed);

        public static bool DragInt4(string label, ref int[] v, float v_speed, int v_min) =>
            ImGui.DragInt4(label, ref Safe(v), v_speed, v_min);

        public static bool DragInt4(string label, ref int[] v, float v_speed, int v_min, int v_max) =>
            ImGui.DragInt4(label, ref Safe(v), v_speed, v_min, v_max);

        public static bool DragInt4(string label, ref int[] v, float v_speed, int v_min, int v_max, string format) =>
            ImGui.DragInt4(label, ref Safe(v), v_speed, v_min, v_max, format);

        public static bool DragInt4(
            string label,
            ref int[] v,
            float v_speed,
            int v_min,
            int v_max,
            string format,
            ImGuiSliderFlags flags
        ) =>
            ImGui.DragInt4(label, ref Safe(v), v_speed, v_min, v_max, format, flags);

        public static bool DragInt4(string label, ref Vector4 v, float v_speed)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z, (int)v.W };
            var ret = ImGui.DragInt4(label, ref span[0], v_speed);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            v.W = span[3];
            return ret;
        }

        public static bool DragInt4(string label, ref Vector4 v, float v_speed, int v_min)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z, (int)v.W };
            var ret = ImGui.DragInt4(label, ref span[0], v_speed, v_min);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            v.W = span[3];
            return ret;
        }

        public static bool DragInt4(string label, ref Vector4 v, float v_speed, int v_min, int v_max)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z, (int)v.W };
            var ret = ImGui.DragInt4(label, ref span[0], v_speed, v_min, v_max);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            v.W = span[3];
            return ret;
        }

        public static bool DragInt4(string label, ref Vector4 v, float v_speed, int v_min, int v_max, string format)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z, (int)v.W };
            var ret = ImGui.DragInt4(label, ref span[0], v_speed, v_min, v_max, format);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            v.W = span[3];
            return ret;
        }

        public static bool DragInt4(
            string label,
            ref Vector4 v,
            float v_speed,
            int v_min,
            int v_max,
            string format,
            ImGuiSliderFlags flags
        )
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z, (int)v.W };
            var ret = ImGui.DragInt4(label, ref span[0], v_speed, v_min, v_max, format, flags);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            v.W = span[3];
            return ret;
        }

        public static bool InputInt2(string label, ref int[] v) => ImGui.InputInt2(label, ref Safe(v));

        public static bool InputInt2(string label, ref int[] v, ImGuiInputTextFlags flags) =>
            ImGui.InputInt2(label, ref Safe(v), flags);

        public static bool InputInt2(string label, ref Vector2 v)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y };
            var ret = ImGui.InputInt2(label, ref span[0]);
            v.X = span[0];
            v.Y = span[1];
            return ret;
        }

        public static bool InputInt2(string label, ref Vector2 v, ImGuiInputTextFlags flags)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y };
            var ret = ImGui.InputInt2(label, ref span[0], flags);
            v.X = span[0];
            v.Y = span[1];
            return ret;
        }

        public static bool InputInt3(string label, ref int[] v) => ImGui.InputInt2(label, ref Safe(v));

        public static bool InputInt3(string label, ref int[] v, ImGuiInputTextFlags flags) =>
            ImGui.InputInt2(label, ref Safe(v), flags);

        public static bool InputInt3(string label, ref Vector3 v)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z };
            var ret = ImGui.InputInt3(label, ref span[0]);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            return ret;
        }

        public static bool InputInt3(string label, ref Vector3 v, ImGuiInputTextFlags flags)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z };
            var ret = ImGui.InputInt3(label, ref span[0], flags);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            return ret;
        }

        public static bool InputInt4(string label, ref int[] v) => ImGui.InputInt2(label, ref Safe(v));

        public static bool InputInt4(string label, ref int[] v, ImGuiInputTextFlags flags) =>
            ImGui.InputInt2(label, ref Safe(v), flags);

        public static bool InputInt4(string label, ref Vector4 v)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z, (int)v.W };
            var ret = ImGui.InputInt4(label, ref span[0]);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            v.W = span[3];
            return ret;
        }

        public static bool InputInt4(string label, ref Vector4 v, ImGuiInputTextFlags flags)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z, (int)v.W };
            var ret = ImGui.InputInt4(label, ref span[0], flags);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            v.W = span[3];
            return ret;
        }

        public static bool SliderInt2(string label, ref int[] v, int v_min, int v_max) =>
            ImGui.SliderInt2(label, ref Safe(v), v_min, v_max);

        public static bool SliderInt2(string label, ref int[] v, int v_min, int v_max, string format) =>
            ImGui.SliderInt2(label, ref Safe(v), v_min, v_max, format);

        public static bool SliderInt2(
            string label,
            ref int[] v,
            int v_min,
            int v_max,
            string format,
            ImGuiSliderFlags flags
        ) =>
            ImGui.SliderInt2(label, ref Safe(v), v_min, v_max, format, flags);

        public static bool SliderInt2(string label, ref Vector2 v, int v_min, int v_max)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y };
            var ret = ImGui.SliderInt2(label, ref span[0], v_min, v_max);
            v.X = span[0];
            v.Y = span[1];
            return ret;
        }

        public static bool SliderInt2(string label, ref Vector2 v, int v_min, int v_max, string format)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y };
            var ret = ImGui.SliderInt2(label, ref span[0], v_min, v_max, format);
            v.X = span[0];
            v.Y = span[1];
            return ret;
        }

        public static bool SliderInt2(
            string label,
            ref Vector2 v,
            int v_min,
            int v_max,
            string format,
            ImGuiSliderFlags flags
        )
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y };
            var ret = ImGui.SliderInt2(label, ref span[0], v_min, v_max, format, flags);
            v.X = span[0];
            v.Y = span[1];
            return ret;
        }

        public static bool SliderInt3(string label, ref int[] v, int v_min, int v_max) =>
            ImGui.SliderInt3(label, ref Safe(v), v_min, v_max);

        public static bool SliderInt3(string label, ref int[] v, int v_min, int v_max, string format) =>
            ImGui.SliderInt3(label, ref Safe(v), v_min, v_max, format);

        public static bool SliderInt3(
            string label,
            ref int[] v,
            int v_min,
            int v_max,
            string format,
            ImGuiSliderFlags flags
        ) =>
            ImGui.SliderInt3(label, ref Safe(v), v_min, v_max, format, flags);

        public static bool SliderInt3(string label, ref Vector3 v, int v_min, int v_max)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z };
            var ret = ImGui.SliderInt3(label, ref span[0], v_min, v_max);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            return ret;
        }

        public static bool SliderInt3(string label, ref Vector3 v, int v_min, int v_max, string format)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z };
            var ret = ImGui.SliderInt3(label, ref span[0], v_min, v_max, format);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            return ret;
        }

        public static bool SliderInt3(
            string label,
            ref Vector3 v,
            int v_min,
            int v_max,
            string format,
            ImGuiSliderFlags flags
        )
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z };
            var ret = ImGui.SliderInt3(label, ref span[0], v_min, v_max, format, flags);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            return ret;
        }

        public static bool SliderInt4(string label, ref int[] v, int v_min, int v_max) =>
            ImGui.SliderInt4(label, ref Safe(v), v_min, v_max);

        public static bool SliderInt4(string label, ref int[] v, int v_min, int v_max, string format) =>
            ImGui.SliderInt4(label, ref Safe(v), v_min, v_max, format);

        public static bool SliderInt4(
            string label,
            ref int[] v,
            int v_min,
            int v_max,
            string format,
            ImGuiSliderFlags flags
        ) =>
            ImGui.SliderInt4(label, ref Safe(v), v_min, v_max, format, flags);

        public static bool SliderInt4(string label, ref Vector4 v, int v_min, int v_max)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z, (int)v.W };
            var ret = ImGui.SliderInt4(label, ref span[0], v_min, v_max);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            v.W = span[3];
            return ret;
        }

        public static bool SliderInt4(string label, ref Vector4 v, int v_min, int v_max, string format)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z, (int)v.W };
            var ret = ImGui.SliderInt4(label, ref span[0], v_min, v_max, format);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            v.W = span[3];
            return ret;
        }

        public static bool SliderInt4(
            string label,
            ref Vector4 v,
            int v_min,
            int v_max,
            string format,
            ImGuiSliderFlags flags
        )
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z, (int)v.W };
            var ret = ImGui.SliderInt4(label, ref span[0], v_min, v_max, format, flags);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            v.W = span[3];
            return ret;
        }

        public static bool IsMouseDragging() => ImGui.IsMouseDragging(0);

        public static bool IsMouseDragging(ImGuiMouseButton button) => ImGui.IsMouseDragging(button);

        public static bool IsMouseDragging(ImGuiMouseButton button, float lock_threshold) =>
            ImGui.IsMouseDragging(button, lock_threshold);

        public static int GetKeyIndex(ImGuiKey key) => (int)key;

        public static float GetContentRegionAvailWidth() => ImGui.GetContentRegionAvail().X;

        public static float GetWindowRegionAvailWidth() =>
            GetWindowContentRegionMax().X - GetWindowContentRegionMin().X;

        // These vector functions aren't part of ImGui but are needed to maintain compatibility
        // with plugins, even if its functionality is practically useless.
        public static DynValue CreateVector2(ScriptExecutionContext executionContext, CallbackArguments args) =>
            TableModule.pack(executionContext, args);

        public static DynValue CreateVector3(ScriptExecutionContext executionContext, CallbackArguments args) =>
            TableModule.pack(executionContext, args);

        public static DynValue CreateVector4(ScriptExecutionContext executionContext, CallbackArguments args) =>
            TableModule.pack(executionContext, args);

        public static Vector2 GetContentRegionMax() =>
            ImGui.GetContentRegionAvail() + ImGui.GetCursorScreenPos() - ImGui.GetWindowPos();

        public static Vector2 GetWindowContentRegionMax() => ImGui.GetContentRegionAvail() + ImGui.GetCursorPos();

        public static Vector2 GetWindowContentRegionMin() => ImGui.GetCursorPos();

        /// <summary>
        ///     Replaces the matched patterns with the result of the evaluator.
        /// </summary>
        /// <param name="pattern">The pattern to match.</param>
        /// <param name="regex">The regex to test.</param>
        /// <param name="evaluator">The function that converts matches.</param>
        /// <param name="count">If specified, the maximum number of times the replacement will occur.</param>
        /// <returns>
        /// The new <see cref="string"/> containing the result of the parameter <paramref name="evaluator"/> invoked
        /// with each match of the parameter <paramref name="pattern"/> onto the parameter <paramref name="regex"/>
        /// with the maximum number of times being the parameter <paramref name="count"/>, if specified.
        /// </returns>
        [MoonSharpHidden]
        public static string Replace(this string pattern, Regex regex, MatchEvaluator evaluator, int? count = null) =>
            count is { } c ? regex.Replace(pattern, evaluator, c) : regex.Replace(pattern, evaluator);

        [MoonSharpHidden]
        public static DynValue Debug(this DynValue t, [CallerArgumentExpression("t")] string expression = null)
        {
            Logger.Important($"{expression} = {t} - {t.UserData?.Object}", LogType.Runtime);
            return t;
        }

        [MoonSharpHidden]
        public static CallbackArguments Debug(
            this CallbackArguments t,
            [CallerArgumentExpression("t")] string expression = null
        )
        {
            for (var i = 0; i < t.Count; i++)
                t[i].Debug($"{expression}[{i}]");

            return t;
        }

        [MoonSharpHidden]
        public static DynValue Debug(
            this DynValue t,
            Converter<DynValue, object> converter,
            [CallerArgumentExpression("t")] string expression = null
        )
        {
            Logger.Important($"{expression} = {converter(t)}", LogType.Runtime);
            return t;
        }

        /// <summary>
        ///     Gets the dynamic value, but ensuring that the function returned will return
        ///     a table instead of a vector to retain backwards compatibility.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        [MoonSharpHidden]
        public static DynValue GetWrappedFunctionThatPacksReturnedVectors(this IUserDataDescriptor that, string str) =>
            that.Index(null, null, DynValue.NewString(str), true) is var ret &&
            ret is { Callback.ClrCallback: { } clr }
                ? DynValue.NewCallback((context, args) => PackVector(context, clr(context, args))).AsReadOnly()
                : ret;

        /// <summary>
        ///     Packs the numbers into a table.
        /// </summary>
        /// <param name="numbers">The numbers to pack.</param>
        /// <returns>The packed value.</returns>
        [MoonSharpHidden]
        public static DynValue Pack(params double[] numbers) => Pack(null, numbers);

        /// <summary>
        ///     Packs the numbers into a table.
        /// </summary>
        /// <param name="context">The script execution context.</param>
        /// <param name="numbers">The numbers to pack.</param>
        /// <returns>The packed value.</returns>
        [MoonSharpHidden]
        public static DynValue Pack(IScriptPrivateResource context, params double[] numbers) =>
            DynValue.NewTable(context?.OwnerScript, Array.ConvertAll(numbers, DynValue.NewNumber));

        // Superseded by 'SetNextItemAllowOverlap' (called before an item)
        public static DynValue SetItemAllowOverlap(ScriptExecutionContext _, CallbackArguments __) => DynValue.Nil;

        public static ImDrawListPtr GetOverlayDrawList() => ImGui.GetForegroundDrawList();

        /// <summary>
        ///     Determines whether the given global is set to <see keyword="true"/>.
        /// </summary>
        /// <param name="context">The script execution context.</param>
        /// <param name="key">The key to access.</param>
        /// <returns>The value <see keyword="true"/> if the global is <see keyword="true"/>.</returns>
        private static bool IsGlobalTrue(this IScriptPrivateResource context, string key) =>
            context.OwnerScript.Globals.RawGet(key) is { Type: DataType.Boolean, Boolean: true };

        /// <summary>
        ///     Packs the vector into the table, which includes within tuples.
        /// </summary>
        /// <param name="context">The script execution context.</param>
        /// <param name="value">The value to pack.</param>
        /// <returns>The packed value.</returns>
        private static DynValue PackVector(IScriptPrivateResource context, DynValue x) =>
            x switch
            {
                _ when context.IsGlobalTrue("imgui_disable_vector_packing") => x,
                // Uncomment below to display the control flow of this function.
                // _ when context.IsGlobalTrue("help") && value.Debug(LuaImGui.Display) is null => null,
                { Type: DataType.Tuple } => DynValue.NewTuple(Array.ConvertAll(x.Tuple, x => PackVector(context, x))),
                { UserData.Object: Vector2 v } => Pack(context, v.X, v.Y),
                { UserData.Object: Vector3 v } => Pack(context, v.X, v.Y, v.Z),
                { UserData.Object: Vector4 v } => context.IsGlobalTrue("imgui_enable_bugged_vector4_packing")
                    ? Pack(context, v.W, v.X, v.Y, v.Z)
                    : Pack(context, v.X, v.Y, v.Z, v.W),
                _ => x,
            };

        private static ref T Safe<T>(T[]? v, [CallerMemberName] string caller = " ") =>
            ref Safe(v, caller[^1] - '0', caller);

        private static ref T Safe<T>(T[]? v, int expected, [CallerMemberName] string caller = " ")
        {
            if (v is null)
                throw new ArgumentNullException(nameof(v), $"Second argument of {caller} must not be nil.");

            if (v.Length < expected)
                throw new ArgumentOutOfRangeException(
                    nameof(v),
                    $"Second argument of {caller} expected at least {expected} elements, got {v.Length}."
                );

            return ref v[0];
        }
    }
}
