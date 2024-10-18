using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using ImGuiNET;

namespace Quaver.Shared.Screens.Edit.Plugins;

/// <summary>
///     This class is for fixes to ImGui functions that behave differently to the original version
///     Check out https://github.com/ImGuiNET/ImGui.NET/issues/495
///     The file is basically just copies of the source, just with slight modification to <see cref="BeginTabItem"/>
///     to fix the null ref issue
/// </summary>
public static class ImGuiFix
{
    /// <inheritdoc cref="ImGui.BeginTabItem(string, ref bool, ImGuiTabItemFlags)"/>
    /// <remarks>
    ///     The original binding doesn't work when <see cref="p_open"/> is null reference.
    ///     However, it being a null ref has significance to imgui, as it means
    ///     the item is always shown and the selection is managed by imgui itself.
    ///     So, we fix this ourselves and wait till it gets fixed upstream.
    /// </remarks>
    public static unsafe bool BeginTabItem(string label, ref bool p_open, ImGuiTabItemFlags flags)
    {
        byte* native_label;
        int label_byteCount = 0;
        if (label != null)
        {
            label_byteCount = Encoding.UTF8.GetByteCount(label);
            if (label_byteCount > Util.StackAllocationSizeLimit)
            {
                native_label = Util.Allocate(label_byteCount + 1);
            }
            else
            {
                byte* native_label_stackBytes = stackalloc byte[label_byteCount + 1];
                native_label = native_label_stackBytes;
            }

            int native_label_offset = Util.GetUtf8(label, native_label, label_byteCount);
            native_label[native_label_offset] = 0;
        }
        else { native_label = null; }

        byte ret;
        if (Unsafe.IsNullRef(ref p_open))
        {
            ret = ImGuiNative.igBeginTabItem(native_label, (byte*)0, flags);
        }
        else
        {
            byte native_p_open_val = p_open ? (byte)1 : (byte)0;
            byte* native_p_open = &native_p_open_val;
            ret = ImGuiNative.igBeginTabItem(native_label, native_p_open, flags);
            p_open = native_p_open_val != 0;
        }

        if (label_byteCount > Util.StackAllocationSizeLimit)
        {
            Util.Free(native_label);
        }

        return ret != 0;
    }

    private static unsafe class Util
    {
        internal const int StackAllocationSizeLimit = 2048;

        public static string StringFromPtr(byte* ptr)
        {
            int characters = 0;
            while (ptr[characters] != 0)
            {
                characters++;
            }

            return Encoding.UTF8.GetString(ptr, characters);
        }

        internal static bool AreStringsEqual(byte* a, int aLength, byte* b)
        {
            for (int i = 0; i < aLength; i++)
            {
                if (a[i] != b[i]) { return false; }
            }

            if (b[aLength] != 0) { return false; }

            return true;
        }

        internal static byte* Allocate(int byteCount) => (byte*)Marshal.AllocHGlobal(byteCount);

        internal static void Free(byte* ptr) => Marshal.FreeHGlobal((IntPtr)ptr);

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        internal static int CalcSizeInUtf8(ReadOnlySpan<char> s, int start, int length)
#else
        internal static int CalcSizeInUtf8(string s, int start, int length)
#endif
        {
            if (start < 0 || length < 0 || start + length > s.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (s.Length == 0) return 0;

            fixed (char* utf16Ptr = s)
            {
                return Encoding.UTF8.GetByteCount(utf16Ptr + start, length);
            }
        }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        internal static int GetUtf8(ReadOnlySpan<char> s, byte* utf8Bytes, int utf8ByteCount)
        {
            if (s.IsEmpty)
            {
                return 0;
            }

            fixed (char* utf16Ptr = s)
            {
                return Encoding.UTF8.GetBytes(utf16Ptr, s.Length, utf8Bytes, utf8ByteCount);
            }
        }
#endif

        internal static int GetUtf8(string s, byte* utf8Bytes, int utf8ByteCount)
        {
            fixed (char* utf16Ptr = s)
            {
                return Encoding.UTF8.GetBytes(utf16Ptr, s.Length, utf8Bytes, utf8ByteCount);
            }
        }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        internal static int GetUtf8(ReadOnlySpan<char> s, int start, int length, byte* utf8Bytes, int utf8ByteCount)
#else
        internal static int GetUtf8(string s, int start, int length, byte* utf8Bytes, int utf8ByteCount)
#endif
        {
            if (start < 0 || length < 0 || start + length > s.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (s.Length == 0) return 0;

            fixed (char* utf16Ptr = s)
            {
                return Encoding.UTF8.GetBytes(utf16Ptr + start, length, utf8Bytes, utf8ByteCount);
            }
        }
    }
}