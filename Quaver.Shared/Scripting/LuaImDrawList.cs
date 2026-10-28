using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Hexa.NET.ImGui;
using MoonSharp.Interpreter;

#pragma warning disable
// ReSharper disable ConvertToAutoPropertyWhenPossible InconsistentNaming MissingBlankLines UnusedMember.Global

namespace Quaver.Shared.Scripting
{
    /*
     * LuaImDrawList is adapted from ImGui.NET's generated ImDrawListPtr implementation:
     * https://github.com/mellinoe/ImGui.NET/blob/70a87022f775025b90dbe2194e44983c79de0911/src/ImGui.NET/Generated/ImDrawList.gen.cs
     *
     * Changes from the original are limited to wrapping Hexa.NET.ImGui's ImDrawListPtr,
     * preserving the Lua-facing ImGui.NET 1.91 overloads, and adapting the ImFont and
     * ImTextureID representations required by Dear ImGui 1.92.
     *
     * The MIT License (MIT)
     *
     * Copyright (c) 2017 Eric Mellino and ImGui.NET contributors
     *
     * Permission is hereby granted, free of charge, to any person obtaining a copy
     * of this software and associated documentation files (the "Software"), to deal
     * in the Software without restriction, including without limitation the rights
     * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
     * copies of the Software, and to permit persons to whom the Software is
     * furnished to do so, subject to the following conditions:
     *
     * The above copyright notice and this permission notice shall be included in all
     * copies or substantial portions of the Software.
     *
     * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
     * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
     * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
     * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
     * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
     * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
     * SOFTWARE.
     */
    [MoonSharpUserData]
    public sealed unsafe class LuaImDrawList
    {
        [MoonSharpHidden]
        internal ImDrawListPtr Pointer { get; }

        public IntPtr NativePtr => (IntPtr)(ImDrawList*)Pointer;

        public ImVector<ImDrawCmd> CmdBuffer => Pointer.CmdBuffer;

        public ImVector<ushort> IdxBuffer => Pointer.IdxBuffer;

        public ImVector<ImDrawVert> VtxBuffer => Pointer.VtxBuffer;

        public ImDrawListFlags Flags
        {
            get => Pointer.Flags;
            set
            {
                var pointer = Pointer;
                pointer.Flags = value;
            }
        }

        public uint _VtxCurrentIdx
        {
            get => Pointer.VtxCurrentIdx;
            set
            {
                var pointer = Pointer;
                pointer.VtxCurrentIdx = value;
            }
        }

        public IntPtr _Data => (IntPtr)(ImDrawListSharedData*)Pointer.Data;

        public ImDrawVertPtr _VtxWritePtr => Pointer.VtxWritePtr;

        public IntPtr _IdxWritePtr
        {
            get => (IntPtr)Pointer.IdxWritePtr;
            set
            {
                var pointer = Pointer;
                pointer.IdxWritePtr = (ushort*)value;
            }
        }

        public ImVector<Vector2> _Path => Pointer.Path;

        public ImDrawCmdHeader _CmdHeader
        {
            get => Pointer.CmdHeader;
            set
            {
                var pointer = Pointer;
                pointer.CmdHeader = value;
            }
        }

        public ImDrawListSplitter _Splitter
        {
            get => Pointer.Splitter;
            set
            {
                var pointer = Pointer;
                pointer.Splitter = value;
            }
        }

        public ImVector<Vector4> _ClipRectStack => Pointer.ClipRectStack;

        public ImVector<ImTextureRef> _TextureIdStack => Pointer.TextureStack;

        public float _FringeScale
        {
            get => Pointer.FringeScale;
            set
            {
                var pointer = Pointer;
                pointer.FringeScale = value;
            }
        }

        public string _OwnerName => Marshal.PtrToStringUTF8((IntPtr)Pointer.OwnerName);

        internal LuaImDrawList(ImDrawListPtr pointer) => Pointer = pointer;

        private static unsafe ImTextureRef TextureRef(IntPtr textureId) => new(null, textureId);

        public int _CalcCircleAutoSegmentCount(float radius) => Pointer._CalcCircleAutoSegmentCount(radius);
        public void _ClearFreeMemory() => Pointer._ClearFreeMemory();
        public void _OnChangedClipRect() => Pointer._OnChangedClipRect();
        public void _OnChangedTextureID() => Pointer._OnChangedTexture();
        public void _OnChangedVtxOffset() => Pointer._OnChangedVtxOffset();
        public void _PathArcToFastEx(Vector2 center, float radius, int minSample, int maxSample, int step) =>
            Pointer._PathArcToFastEx(center, radius, minSample, maxSample, step);
        public void _PathArcToN(Vector2 center, float radius, float minAngle, float maxAngle, int segments) =>
            Pointer._PathArcToN(center, radius, minAngle, maxAngle, segments);
        public void _PopUnusedDrawCmd() => Pointer._PopUnusedDrawCmd();
        public void _ResetForNewFrame() => Pointer._ResetForNewFrame();
        public void _TryMergeDrawCmds() => Pointer._TryMergeDrawCmds();

        public void AddBezierCubic(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, uint color, float thickness) =>
            Pointer.AddBezierCubic(p1, p2, p3, p4, color, thickness, 0);

        public void AddBezierCubic(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, uint color, float thickness, int segments) =>
            Pointer.AddBezierCubic(p1, p2, p3, p4, color, thickness, segments);

        public void AddBezierQuadratic(Vector2 p1, Vector2 p2, Vector2 p3, uint color, float thickness) =>
            Pointer.AddBezierQuadratic(p1, p2, p3, color, thickness, 0);

        public void AddBezierQuadratic(Vector2 p1, Vector2 p2, Vector2 p3, uint color, float thickness, int segments) =>
            Pointer.AddBezierQuadratic(p1, p2, p3, color, thickness, segments);

        public unsafe void AddCallback(IntPtr callback, IntPtr callbackData) =>
            Pointer.AddCallback(Marshal.GetDelegateForFunctionPointer<ImDrawCallback>(callback), (void*)callbackData);

        public void AddCircle(Vector2 center, float radius, uint color) => Pointer.AddCircle(center, radius, color, 0, 1f);
        public void AddCircle(Vector2 center, float radius, uint color, int segments) => Pointer.AddCircle(center, radius, color, segments, 1f);
        public void AddCircle(Vector2 center, float radius, uint color, int segments, float thickness) => Pointer.AddCircle(center, radius, color, segments, thickness);
        public void AddCircleFilled(Vector2 center, float radius, uint color) => Pointer.AddCircleFilled(center, radius, color, 0);
        public void AddCircleFilled(Vector2 center, float radius, uint color, int segments) => Pointer.AddCircleFilled(center, radius, color, segments);
        public void AddConcavePolyFilled(ref Vector2 points, int pointCount, uint color) => Pointer.AddConcavePolyFilled(ref points, pointCount, color);
        public void AddConvexPolyFilled(ref Vector2 points, int pointCount, uint color) => Pointer.AddConvexPolyFilled(ref points, pointCount, color);
        public void AddDrawCmd() => Pointer.AddDrawCmd();
        public void AddEllipse(Vector2 center, Vector2 radius, uint color) => Pointer.AddEllipse(center, radius, color, 0f, 0, 1f);
        public void AddEllipse(Vector2 center, Vector2 radius, uint color, float rotation) => Pointer.AddEllipse(center, radius, color, rotation, 0, 1f);
        public void AddEllipse(Vector2 center, Vector2 radius, uint color, float rotation, int segments) => Pointer.AddEllipse(center, radius, color, rotation, segments, 1f);
        public void AddEllipse(Vector2 center, Vector2 radius, uint color, float rotation, int segments, float thickness) => Pointer.AddEllipse(center, radius, color, rotation, segments, thickness);
        public void AddEllipseFilled(Vector2 center, Vector2 radius, uint color) => Pointer.AddEllipseFilled(center, radius, color, 0f, 0);
        public void AddEllipseFilled(Vector2 center, Vector2 radius, uint color, float rotation) => Pointer.AddEllipseFilled(center, radius, color, rotation, 0);
        public void AddEllipseFilled(Vector2 center, Vector2 radius, uint color, float rotation, int segments) => Pointer.AddEllipseFilled(center, radius, color, rotation, segments);
        public void AddLine(Vector2 p1, Vector2 p2, uint color) => Pointer.AddLine(p1, p2, color, 1f);
        public void AddLine(Vector2 p1, Vector2 p2, uint color, float thickness) => Pointer.AddLine(p1, p2, color, thickness);
        public void AddNgon(Vector2 center, float radius, uint color, int segments) => Pointer.AddNgon(center, radius, color, segments, 1f);
        public void AddNgon(Vector2 center, float radius, uint color, int segments, float thickness) => Pointer.AddNgon(center, radius, color, segments, thickness);
        public void AddNgonFilled(Vector2 center, float radius, uint color, int segments) => Pointer.AddNgonFilled(center, radius, color, segments);
        public void AddPolyline(ref Vector2 points, int pointCount, uint color, ImDrawFlags flags, float thickness) =>
            Pointer.AddPolyline(ref points, pointCount, color, flags, thickness);
        public void AddQuad(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, uint color) => Pointer.AddQuad(p1, p2, p3, p4, color, 1f);
        public void AddQuad(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, uint color, float thickness) => Pointer.AddQuad(p1, p2, p3, p4, color, thickness);
        public void AddQuadFilled(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, uint color) => Pointer.AddQuadFilled(p1, p2, p3, p4, color);
        public void AddTriangle(Vector2 p1, Vector2 p2, Vector2 p3, uint color) => Pointer.AddTriangle(p1, p2, p3, color, 1f);
        public void AddTriangle(Vector2 p1, Vector2 p2, Vector2 p3, uint color, float thickness) => Pointer.AddTriangle(p1, p2, p3, color, thickness);
        public void AddTriangleFilled(Vector2 p1, Vector2 p2, Vector2 p3, uint color) => Pointer.AddTriangleFilled(p1, p2, p3, color);

        public void AddRect(Vector2 min, Vector2 max, uint color) => Pointer.AddRect(min, max, color, 0f, ImDrawFlags.None, 1f);
        public void AddRect(Vector2 min, Vector2 max, uint color, float rounding) => Pointer.AddRect(min, max, color, rounding, ImDrawFlags.None, 1f);
        public void AddRect(Vector2 min, Vector2 max, uint color, float rounding, ImDrawFlags flags) => Pointer.AddRect(min, max, color, rounding, flags, 1f);
        public void AddRect(Vector2 min, Vector2 max, uint color, float rounding, ImDrawFlags flags, float thickness) => Pointer.AddRect(min, max, color, rounding, flags, thickness);
        public void AddRectFilled(Vector2 min, Vector2 max, uint color) => Pointer.AddRectFilled(min, max, color, 0f, ImDrawFlags.None);
        public void AddRectFilled(Vector2 min, Vector2 max, uint color, float rounding) => Pointer.AddRectFilled(min, max, color, rounding, ImDrawFlags.None);
        public void AddRectFilled(Vector2 min, Vector2 max, uint color, float rounding, ImDrawFlags flags) => Pointer.AddRectFilled(min, max, color, rounding, flags);
        public void AddRectFilledMultiColor(Vector2 min, Vector2 max, uint upperLeft, uint upperRight, uint bottomRight, uint bottomLeft) =>
            Pointer.AddRectFilledMultiColor(min, max, upperLeft, upperRight, bottomRight, bottomLeft);

        public void AddImage(IntPtr textureId, Vector2 min, Vector2 max) => Pointer.AddImage(TextureRef(textureId), min, max, Vector2.Zero, Vector2.One, uint.MaxValue);
        public void AddImage(IntPtr textureId, Vector2 min, Vector2 max, Vector2 uvMin) => Pointer.AddImage(TextureRef(textureId), min, max, uvMin, Vector2.One, uint.MaxValue);
        public void AddImage(IntPtr textureId, Vector2 min, Vector2 max, Vector2 uvMin, Vector2 uvMax) => Pointer.AddImage(TextureRef(textureId), min, max, uvMin, uvMax, uint.MaxValue);
        public void AddImage(IntPtr textureId, Vector2 min, Vector2 max, Vector2 uvMin, Vector2 uvMax, uint color) => Pointer.AddImage(TextureRef(textureId), min, max, uvMin, uvMax, color);
        public void AddImageQuad(IntPtr textureId, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4) =>
            Pointer.AddImageQuad(TextureRef(textureId), p1, p2, p3, p4, Vector2.Zero, Vector2.UnitX, Vector2.One, Vector2.UnitY, uint.MaxValue);
        public void AddImageQuad(IntPtr textureId, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Vector2 uv1) =>
            Pointer.AddImageQuad(TextureRef(textureId), p1, p2, p3, p4, uv1, Vector2.UnitX, Vector2.One, Vector2.UnitY, uint.MaxValue);
        public void AddImageQuad(IntPtr textureId, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Vector2 uv1, Vector2 uv2) =>
            Pointer.AddImageQuad(TextureRef(textureId), p1, p2, p3, p4, uv1, uv2, Vector2.One, Vector2.UnitY, uint.MaxValue);
        public void AddImageQuad(IntPtr textureId, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Vector2 uv1, Vector2 uv2, Vector2 uv3) =>
            Pointer.AddImageQuad(TextureRef(textureId), p1, p2, p3, p4, uv1, uv2, uv3, Vector2.UnitY, uint.MaxValue);
        public void AddImageQuad(IntPtr textureId, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4) =>
            Pointer.AddImageQuad(TextureRef(textureId), p1, p2, p3, p4, uv1, uv2, uv3, uv4, uint.MaxValue);
        public void AddImageQuad(IntPtr textureId, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4, uint color) =>
            Pointer.AddImageQuad(TextureRef(textureId), p1, p2, p3, p4, uv1, uv2, uv3, uv4, color);
        public void AddImageRounded(IntPtr textureId, Vector2 min, Vector2 max, Vector2 uvMin, Vector2 uvMax, uint color, float rounding) =>
            Pointer.AddImageRounded(TextureRef(textureId), min, max, uvMin, uvMax, color, rounding, ImDrawFlags.None);
        public void AddImageRounded(IntPtr textureId, Vector2 min, Vector2 max, Vector2 uvMin, Vector2 uvMax, uint color, float rounding, ImDrawFlags flags) =>
            Pointer.AddImageRounded(TextureRef(textureId), min, max, uvMin, uvMax, color, rounding, flags);

        public void AddText(Vector2 position, uint color, string text) => Pointer.AddText(position, color, text);

        public unsafe void AddText(LuaImFont font, float fontSize, Vector2 position, uint color, string text)
        {
            ImFont* nativeFont = font.Pointer;
            Pointer.AddText(nativeFont, fontSize, position, color, text);
        }

        public unsafe void AddText(
            LuaImFont font,
            float fontSize,
            Vector2 position,
            uint color,
            string text,
            float wrapWidth
        )
        {
            ImFont* nativeFont = font.Pointer;
            Pointer.AddText(nativeFont, fontSize, position, color, text, wrapWidth);
        }

        public unsafe void AddText(
            LuaImFont font,
            float fontSize,
            Vector2 position,
            uint color,
            string text,
            float wrapWidth,
            ref Vector4 cpuFineClipRect
        )
        {
            ImFont* nativeFont = font.Pointer;
            Pointer.AddText(nativeFont, fontSize, position, color, text, wrapWidth, ref cpuFineClipRect);
        }

        public void ChannelsMerge() => Pointer.ChannelsMerge();
        public void ChannelsSetCurrent(int channel) => Pointer.ChannelsSetCurrent(channel);
        public void ChannelsSplit(int count) => Pointer.ChannelsSplit(count);
        public LuaImDrawList CloneOutput() => new(Pointer.CloneOutput());
        public void Destroy() => Pointer.Destroy();
        public Vector2 GetClipRectMin()
        {
            var clipRect = Pointer.CmdHeader.ClipRect;
            return new Vector2(clipRect.X, clipRect.Y);
        }

        public Vector2 GetClipRectMax()
        {
            var clipRect = Pointer.CmdHeader.ClipRect;
            return new Vector2(clipRect.Z, clipRect.W);
        }

        public void PathArcTo(Vector2 center, float radius, float minAngle, float maxAngle) => Pointer.PathArcTo(center, radius, minAngle, maxAngle, 0);
        public void PathArcTo(Vector2 center, float radius, float minAngle, float maxAngle, int segments) => Pointer.PathArcTo(center, radius, minAngle, maxAngle, segments);
        public void PathArcToFast(Vector2 center, float radius, int minOf12, int maxOf12) => Pointer.PathArcToFast(center, radius, minOf12, maxOf12);
        public void PathBezierCubicCurveTo(Vector2 p2, Vector2 p3, Vector2 p4) => Pointer.PathBezierCubicCurveTo(p2, p3, p4, 0);
        public void PathBezierCubicCurveTo(Vector2 p2, Vector2 p3, Vector2 p4, int segments) => Pointer.PathBezierCubicCurveTo(p2, p3, p4, segments);
        public void PathBezierQuadraticCurveTo(Vector2 p2, Vector2 p3) => Pointer.PathBezierQuadraticCurveTo(p2, p3, 0);
        public void PathBezierQuadraticCurveTo(Vector2 p2, Vector2 p3, int segments) => Pointer.PathBezierQuadraticCurveTo(p2, p3, segments);
        public void PathClear() => Pointer.PathClear();
        public void PathEllipticalArcTo(Vector2 center, Vector2 radius, float rotation, float minAngle, float maxAngle) =>
            Pointer.PathEllipticalArcTo(center, radius, rotation, minAngle, maxAngle, 0);
        public void PathEllipticalArcTo(Vector2 center, Vector2 radius, float rotation, float minAngle, float maxAngle, int segments) =>
            Pointer.PathEllipticalArcTo(center, radius, rotation, minAngle, maxAngle, segments);
        public void PathFillConcave(uint color) => Pointer.PathFillConcave(color);
        public void PathFillConvex(uint color) => Pointer.PathFillConvex(color);
        public void PathLineTo(Vector2 position) => Pointer.PathLineTo(position);
        public void PathLineToMergeDuplicate(Vector2 position) => Pointer.PathLineToMergeDuplicate(position);
        public void PathRect(Vector2 min, Vector2 max) => Pointer.PathRect(min, max, 0f, ImDrawFlags.None);
        public void PathRect(Vector2 min, Vector2 max, float rounding) => Pointer.PathRect(min, max, rounding, ImDrawFlags.None);
        public void PathRect(Vector2 min, Vector2 max, float rounding, ImDrawFlags flags) => Pointer.PathRect(min, max, rounding, flags);
        public void PathStroke(uint color) => Pointer.PathStroke(color, ImDrawFlags.None, 1f);
        public void PathStroke(uint color, ImDrawFlags flags) => Pointer.PathStroke(color, flags, 1f);
        public void PathStroke(uint color, ImDrawFlags flags, float thickness) => Pointer.PathStroke(color, flags, thickness);
        public void PopClipRect() => Pointer.PopClipRect();
        public void PopTextureID() => Pointer.PopTexture();
        public void PrimQuadUV(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 uvA, Vector2 uvB, Vector2 uvC, Vector2 uvD, uint color) =>
            Pointer.PrimQuadUV(a, b, c, d, uvA, uvB, uvC, uvD, color);
        public void PrimRect(Vector2 a, Vector2 b, uint color) => Pointer.PrimRect(a, b, color);
        public void PrimRectUV(Vector2 a, Vector2 b, Vector2 uvA, Vector2 uvB, uint color) => Pointer.PrimRectUV(a, b, uvA, uvB, color);
        public void PrimReserve(int indexCount, int vertexCount) => Pointer.PrimReserve(indexCount, vertexCount);
        public void PrimUnreserve(int indexCount, int vertexCount) => Pointer.PrimUnreserve(indexCount, vertexCount);
        public void PrimVtx(Vector2 position, Vector2 uv, uint color) => Pointer.PrimVtx(position, uv, color);
        public void PrimWriteIdx(ushort index) => Pointer.PrimWriteIdx(index);
        public void PrimWriteVtx(Vector2 position, Vector2 uv, uint color) => Pointer.PrimWriteVtx(position, uv, color);
        public void PushClipRect(Vector2 min, Vector2 max) => Pointer.PushClipRect(min, max, false);
        public void PushClipRect(Vector2 min, Vector2 max, bool intersectWithCurrent) => Pointer.PushClipRect(min, max, intersectWithCurrent);
        public void PushClipRectFullScreen() => Pointer.PushClipRectFullScreen();
        public void PushTextureID(IntPtr textureId) => Pointer.PushTexture(TextureRef(textureId));
    }
}
