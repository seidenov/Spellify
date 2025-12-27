#nullable enable
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Spellify;

static class InputSimulator
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll")]
    private static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [DllImport("user32.dll")]
    private static extern bool CloseClipboard();

    [DllImport("user32.dll")]
    private static extern bool EmptyClipboard();

    [DllImport("user32.dll")]
    private static extern IntPtr GetClipboardData(uint uFormat);

    [DllImport("user32.dll")]
    private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GlobalLock(IntPtr hMem);

    [DllImport("kernel32.dll")]
    private static extern bool GlobalUnlock(IntPtr hMem);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

    [DllImport("user32.dll")]
    private static extern uint MapVirtualKey(uint uCode, uint uMapType);

    private const uint CF_UNICODETEXT = 13;
    private const uint GMEM_MOVEABLE = 0x0002;
    private const uint INPUT_KEYBOARD = 1;
    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const uint KEYEVENTF_SCANCODE = 0x0008;
    private const uint MAPVK_VK_TO_VSC = 0;

    private const ushort VK_CONTROL = 0x11;
    private const ushort VK_C = 0x43;
    private const ushort VK_V = 0x56;

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public UIntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx, dy;
        public uint mouseData, dwFlags, time;
        public UIntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct HARDWAREINPUT
    {
        public uint uMsg;
        public ushort wParamL, wParamH;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)] public MOUSEINPUT mi;
        [FieldOffset(0)] public KEYBDINPUT ki;
        [FieldOffset(0)] public HARDWAREINPUT hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public InputUnion u;
    }

    private static INPUT Key(ushort vk, bool up)
    {
        var scan = (ushort)MapVirtualKey(vk, MAPVK_VK_TO_VSC);
        return new INPUT
        {
            type = INPUT_KEYBOARD,
            u = new InputUnion
            {
                ki = new KEYBDINPUT
                {
                    wVk = vk,
                    wScan = scan,
                    dwFlags = up ? KEYEVENTF_KEYUP : 0,
                    time = 0,
                    dwExtraInfo = UIntPtr.Zero
                }
            }
        };
    }

    private static void WaitForRelease()
    {
        // Ждём 500мс чтобы пользователь точно отпустил клавиши
        Thread.Sleep(500);
    }

    private static void SendCtrlC()
    {
        var inputs = new INPUT[4];
        inputs[0] = Key(VK_CONTROL, false);
        inputs[1] = Key(VK_C, false);
        inputs[2] = Key(VK_C, true);
        inputs[3] = Key(VK_CONTROL, true);
        
        uint sent = SendInput(4, inputs, Marshal.SizeOf<INPUT>());
    }

    private static void SendCtrlV()
    {
        var inputs = new INPUT[4];
        inputs[0] = Key(VK_CONTROL, false);
        inputs[1] = Key(VK_V, false);
        inputs[2] = Key(VK_V, true);
        inputs[3] = Key(VK_CONTROL, true);
        
        SendInput(4, inputs, Marshal.SizeOf<INPUT>());
    }

    private static string? GetClipboardText()
    {
        for (int i = 0; i < 10; i++)
        {
            if (OpenClipboard(IntPtr.Zero))
            {
                try
                {
                    var hData = GetClipboardData(CF_UNICODETEXT);
                    if (hData == IntPtr.Zero) return null;
                    var ptr = GlobalLock(hData);
                    if (ptr == IntPtr.Zero) return null;
                    try { return Marshal.PtrToStringUni(ptr); }
                    finally { GlobalUnlock(hData); }
                }
                finally { CloseClipboard(); }
            }
            Thread.Sleep(20);
        }
        return null;
    }

    private static void SetClipboardText(string text)
    {
        for (int i = 0; i < 10; i++)
        {
            if (OpenClipboard(IntPtr.Zero))
            {
                try
                {
                    EmptyClipboard();
                    var bytes = (text.Length + 1) * 2;
                    var hGlobal = GlobalAlloc(GMEM_MOVEABLE, (UIntPtr)bytes);
                    if (hGlobal == IntPtr.Zero) return;
                    var ptr = GlobalLock(hGlobal);
                    if (ptr != IntPtr.Zero)
                    {
                        Marshal.Copy(text.ToCharArray(), 0, ptr, text.Length);
                        Marshal.WriteInt16(ptr, text.Length * 2, 0);
                        GlobalUnlock(hGlobal);
                        SetClipboardData(CF_UNICODETEXT, hGlobal);
                    }
                    return;
                }
                finally { CloseClipboard(); }
            }
            Thread.Sleep(20);
        }
    }

    private static void ClearClipboard()
    {
        for (int i = 0; i < 10; i++)
        {
            if (OpenClipboard(IntPtr.Zero))
            {
                EmptyClipboard();
                CloseClipboard();
                return;
            }
            Thread.Sleep(20);
        }
    }

    public static string? CopySelectedText()
    {
        WaitForRelease();
        ClearClipboard();
        
        SendCtrlC();
        
        Thread.Sleep(300);
        
        return GetClipboardText();
    }
    
    public static void PasteText(string text)
    {
        SetClipboardText(text);
        Thread.Sleep(100);
        
        SendCtrlV();
    }
}
