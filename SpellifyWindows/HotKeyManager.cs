#nullable enable
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Spellify;

class HotKeyManager : IDisposable
{
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    
    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    
    private const int HOTKEY_ID = 1;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint MOD_ALT = 0x0001;
    
    private HotKeyWindow window;
    private Action? callback;
    
    public void Register(Keys keys, Action onPressed)
    {
        callback = onPressed;
        window = new HotKeyWindow(OnHotKeyPressed);
        
        uint modifiers = 0;
        if (keys.HasFlag(Keys.Control)) modifiers |= MOD_CONTROL;
        if (keys.HasFlag(Keys.Shift)) modifiers |= MOD_SHIFT;
        if (keys.HasFlag(Keys.Alt)) modifiers |= MOD_ALT;
        
        var key = keys & Keys.KeyCode;
        RegisterHotKey(window.Handle, HOTKEY_ID, modifiers, (uint)key);
    }
    
    public void Unregister()
    {
        if (window != null)
        {
            UnregisterHotKey(window.Handle, HOTKEY_ID);
            window.Dispose();
        }
    }
    
    private void OnHotKeyPressed() => callback?.Invoke();
    
    public void Dispose() => Unregister();
    
    private class HotKeyWindow : NativeWindow, IDisposable
    {
        private const int WM_HOTKEY = 0x0312;
        private Action onHotKey;
        
        public HotKeyWindow(Action callback)
        {
            onHotKey = callback;
            CreateHandle(new CreateParams());
        }
        
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY)
                onHotKey();
            base.WndProc(ref m);
        }
        
        public void Dispose() => DestroyHandle();
    }
}
