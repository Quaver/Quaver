using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using ImGuiNET;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.CoreLib;
using Wobble.Logging;

#pragma warning disable
// ReSharper disable ConvertToAutoPropertyWhenPossible InconsistentNaming MissingBlankLines UnusedMember.Global

namespace Quaver.Shared.Scripting
{
    [MoonSharpUserData]
    public static class ImGuiRedirect
    {
        // These vector functions aren't part of ImGui but are needed to maintain compatibility
        // with plugins, even if its functionality is practically useless.

        private static readonly DynValue s_pack = DynValue.NewCallback(TableModule.pack);

        public static DynValue CreateVector2 => s_pack;

        public static DynValue CreateVector3 => s_pack;

        public static DynValue CreateVector4 => s_pack;

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

        public static bool DragVector(string label, ref Vector2 v, float v_speed)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y };
            var ret = ImGui.DragInt2(label, ref span[0], v_speed);
            v.X = span[0];
            v.Y = span[1];
            return ret;
        }

        public static bool DragVector(string label, ref Vector2 v, float v_speed, int v_min)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y };
            var ret = ImGui.DragInt2(label, ref span[0], v_speed, v_min);
            v.X = span[0];
            v.Y = span[1];
            return ret;
        }

        public static bool DragVector(string label, ref Vector2 v, float v_speed, int v_min, int v_max)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y };
            var ret = ImGui.DragInt2(label, ref span[0], v_speed, v_min, v_max);
            v.X = span[0];
            v.Y = span[1];
            return ret;
        }

        public static bool DragVector(string label, ref Vector2 v, float v_speed, int v_min, int v_max, string format)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y };
            var ret = ImGui.DragInt2(label, ref span[0], v_speed, v_min, v_max, format);
            v.X = span[0];
            v.Y = span[1];
            return ret;
        }

        public static bool DragVector(
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

        public static bool DragVector(string label, ref Vector3 v, float v_speed)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z };
            var ret = ImGui.DragInt3(label, ref span[0], v_speed);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            return ret;
        }

        public static bool DragVector(string label, ref Vector3 v, float v_speed, int v_min)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z };
            var ret = ImGui.DragInt3(label, ref span[0], v_speed, v_min);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            return ret;
        }

        public static bool DragVector(string label, ref Vector3 v, float v_speed, int v_min, int v_max)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z };
            var ret = ImGui.DragInt3(label, ref span[0], v_speed, v_min, v_max);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            return ret;
        }

        public static bool DragVector(string label, ref Vector3 v, float v_speed, int v_min, int v_max, string format)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z };
            var ret = ImGui.DragInt3(label, ref span[0], v_speed, v_min, v_max, format);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            return ret;
        }

        public static bool DragVector(
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

        public static bool DragVector(string label, ref Vector4 v, float v_speed)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z, (int)v.W };
            var ret = ImGui.DragInt4(label, ref span[0], v_speed);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            v.W = span[3];
            return ret;
        }

        public static bool DragVector(string label, ref Vector4 v, float v_speed, int v_min)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z, (int)v.W };
            var ret = ImGui.DragInt4(label, ref span[0], v_speed, v_min);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            v.W = span[3];
            return ret;
        }

        public static bool DragVector(string label, ref Vector4 v, float v_speed, int v_min, int v_max)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z, (int)v.W };
            var ret = ImGui.DragInt4(label, ref span[0], v_speed, v_min, v_max);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            v.W = span[3];
            return ret;
        }

        public static bool DragVector(string label, ref Vector4 v, float v_speed, int v_min, int v_max, string format)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z, (int)v.W };
            var ret = ImGui.DragInt4(label, ref span[0], v_speed, v_min, v_max, format);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            v.W = span[3];
            return ret;
        }

        public static bool DragVector(
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

        public static bool InputInt3(string label, ref int[] v) => ImGui.InputInt2(label, ref Safe(v));

        public static bool InputInt3(string label, ref int[] v, ImGuiInputTextFlags flags) =>
            ImGui.InputInt2(label, ref Safe(v), flags);

        public static bool InputInt4(string label, ref int[] v) => ImGui.InputInt2(label, ref Safe(v));

        public static bool InputInt4(string label, ref int[] v, ImGuiInputTextFlags flags) =>
            ImGui.InputInt2(label, ref Safe(v), flags);

        public static bool InputVector(string label, ref Vector2 v)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y };
            var ret = ImGui.InputInt2(label, ref span[0]);
            v.X = span[0];
            v.Y = span[1];
            return ret;
        }

        public static bool InputVector(string label, ref Vector2 v, ImGuiInputTextFlags flags)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y };
            var ret = ImGui.InputInt2(label, ref span[0], flags);
            v.X = span[0];
            v.Y = span[1];
            return ret;
        }

        public static bool InputVector(string label, ref Vector3 v)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z };
            var ret = ImGui.InputInt3(label, ref span[0]);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            return ret;
        }

        public static bool InputVector(string label, ref Vector3 v, ImGuiInputTextFlags flags)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z };
            var ret = ImGui.InputInt3(label, ref span[0], flags);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            return ret;
        }

        public static bool InputVector(string label, ref Vector4 v)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z, (int)v.W };
            var ret = ImGui.InputInt4(label, ref span[0]);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            v.W = span[3];
            return ret;
        }

        public static bool InputVector(string label, ref Vector4 v, ImGuiInputTextFlags flags)
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

        public static bool SliderVector(string label, ref Vector2 v, int v_min, int v_max)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y };
            var ret = ImGui.SliderInt2(label, ref span[0], v_min, v_max);
            v.X = span[0];
            v.Y = span[1];
            return ret;
        }

        public static bool SliderVector(string label, ref Vector2 v, int v_min, int v_max, string format)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y };
            var ret = ImGui.SliderInt2(label, ref span[0], v_min, v_max, format);
            v.X = span[0];
            v.Y = span[1];
            return ret;
        }

        public static bool SliderVector(
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

        public static bool SliderVector(string label, ref Vector3 v, int v_min, int v_max)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z };
            var ret = ImGui.SliderInt3(label, ref span[0], v_min, v_max);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            return ret;
        }

        public static bool SliderVector(string label, ref Vector3 v, int v_min, int v_max, string format)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z };
            var ret = ImGui.SliderInt3(label, ref span[0], v_min, v_max, format);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            return ret;
        }

        public static bool SliderVector(
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

        public static bool SliderVector(string label, ref Vector4 v, int v_min, int v_max)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z, (int)v.W };
            var ret = ImGui.SliderInt4(label, ref span[0], v_min, v_max);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            v.W = span[3];
            return ret;
        }

        public static bool SliderVector(string label, ref Vector4 v, int v_min, int v_max, string format)
        {
            Span<int> span = stackalloc[] { (int)v.X, (int)v.Y, (int)v.Z, (int)v.W };
            var ret = ImGui.SliderInt4(label, ref span[0], v_min, v_max, format);
            v.X = span[0];
            v.Y = span[1];
            v.Z = span[2];
            v.W = span[3];
            return ret;
        }

        public static bool SliderVector(
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
            ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X;

        [MoonSharpHidden]
        public static DynValue Debug(this DynValue t, [CallerArgumentExpression("t")] string expression = null)
        {
            Logger.Important($"{expression} = {t} - {t.UserData?.Object}", LogType.Runtime);
            return t;
        }

        // Superseded by 'SetNextItemAllowOverlap' (called before an item)
        public static DynValue SetItemAllowOverlap(ScriptExecutionContext _, CallbackArguments __) => DynValue.Nil;

        public static ImDrawListPtr GetOverlayDrawList() => ImGui.GetForegroundDrawList();

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
