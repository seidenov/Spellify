using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Spellify;

class SettingsForm : Form
{
    private TextBox apiKeyBox;
    private ComboBox modelBox;
    private ComboBox hotkeyBox;
    
    public event Action? OnSaved;
    
    public SettingsForm()
    {
        Text = "Настройки Spellify";
        Size = new Size(420, 280);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        
        var apiLabel = new Label { Text = "API ключ Gemini:", Location = new Point(20, 20), AutoSize = true };
        apiKeyBox = new TextBox 
        { 
            Location = new Point(20, 45), 
            Size = new Size(360, 25),
            Text = Settings.ApiKey
        };
        
        var modelLabel = new Label { Text = "Модель:", Location = new Point(20, 80), AutoSize = true };
        modelBox = new ComboBox
        {
            Location = new Point(20, 105),
            Size = new Size(360, 25),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        modelBox.Items.AddRange(Settings.AvailableModels);
        modelBox.SelectedItem = Settings.Model;
        
        var hotkeyLabel = new Label { Text = "Сочетание клавиш:", Location = new Point(20, 140), AutoSize = true };
        hotkeyBox = new ComboBox
        {
            Location = new Point(20, 165),
            Size = new Size(360, 25),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        hotkeyBox.Items.AddRange(Settings.HotKeyPresets.Keys.ToArray());
        hotkeyBox.SelectedItem = Settings.HotKeyPresets.FirstOrDefault(x => x.Value == Settings.HotKey).Key ?? "Ctrl+Shift+F";
        
        var saveButton = new Button
        {
            Text = "Сохранить",
            Location = new Point(280, 205),
            Size = new Size(100, 30)
        };
        saveButton.Click += (s, e) => Save();
        
        Controls.AddRange(new Control[] { apiLabel, apiKeyBox, modelLabel, modelBox, hotkeyLabel, hotkeyBox, saveButton });
        
        AcceptButton = saveButton;
    }
    
    private void Save()
    {
        Settings.ApiKey = apiKeyBox.Text.Trim();
        Settings.Model = modelBox.SelectedItem?.ToString() ?? "gemini-2.0-flash";
        Settings.HotKey = Settings.HotKeyPresets[hotkeyBox.SelectedItem?.ToString() ?? "Ctrl+Shift+F"];
        Settings.Save();
        OnSaved?.Invoke();
        Close();
    }
}
