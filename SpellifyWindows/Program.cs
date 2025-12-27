using System;
using System.Drawing;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Spellify;

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new SpellifyContext());
    }
}

class SpellifyContext : ApplicationContext
{
    private NotifyIcon trayIcon;
    private HotKeyManager hotKey;
    private SettingsForm settingsForm;
    
    public SpellifyContext()
    {
        Settings.Load();
        
        trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "Spellify",
            Visible = true,
            ContextMenuStrip = CreateMenu()
        };
        
        hotKey = new HotKeyManager();
        hotKey.Register(Settings.HotKey, OnHotKey);
        
        if (string.IsNullOrEmpty(Settings.ApiKey))
            ShowSettings();
    }
    
    private ContextMenuStrip CreateMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add($"Исправить текст ({Settings.HotKeyDisplay})", null, (s, e) => OnHotKey());
        menu.Items.Add("-");
        menu.Items.Add("Настройки...", null, (s, e) => ShowSettings());
        menu.Items.Add("Проверить API", null, async (s, e) => await TestApi());
        menu.Items.Add("-");
        menu.Items.Add("Выход", null, (s, e) => Exit());
        return menu;
    }
    
    private void ShowSettings()
    {
        if (settingsForm == null || settingsForm.IsDisposed)
        {
            settingsForm = new SettingsForm();
            settingsForm.OnSaved += () =>
            {
                hotKey.Unregister();
                hotKey.Register(Settings.HotKey, OnHotKey);
                trayIcon.ContextMenuStrip = CreateMenu();
            };
        }
        settingsForm.Show();
        settingsForm.BringToFront();
    }
    
    private async void OnHotKey()
    {
        if (string.IsNullOrEmpty(Settings.ApiKey))
        {
            ShowSettings();
            return;
        }
        
        var text = InputSimulator.CopySelectedText();
        if (string.IsNullOrEmpty(text)) return;
        
        var result = await GeminiApi.FixText(text);
        if (result != null)
        {
            InputSimulator.PasteText(result);
        }
        else
        {
            var error = GeminiApi.LastError ?? "Неизвестная ошибка";
            if (error.Contains("429"))
                error = "Лимит запросов исчерпан. Подожди минуту.";
            else if (error.Contains("401") || error.Contains("403"))
                error = "Неверный API ключ";
            else if (error.Contains("timeout") || error.Contains("Timeout"))
                error = "Таймаут соединения";
            
            trayIcon.ShowBalloonTip(3000, "Spellify", error, ToolTipIcon.Error);
        }
    }
    
    private async Task TestApi()
    {
        if (string.IsNullOrEmpty(Settings.ApiKey))
        {
            ShowSettings();
            return;
        }
        
        var result = await GeminiApi.FixText("привет как дила");
        MessageBox.Show(
            result != null ? $"API работает!\nОтвет: {result}" : "Ошибка API",
            "Spellify",
            MessageBoxButtons.OK,
            result != null ? MessageBoxIcon.Information : MessageBoxIcon.Error
        );
    }
    
    private void Exit()
    {
        hotKey.Unregister();
        trayIcon.Visible = false;
        Application.Exit();
    }
}
