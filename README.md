<p align="center">
  <img src="logo.png" width="128" height="128" alt="Spellify">
</p>

<h1 align="center">Spellify</h1>

Утилита для исправления орфографии. Выделяешь текст, жмёшь хоткей — получаешь исправленную версию. Под капотом Gemini API.

## Как работает

1. Выделяешь текст где угодно
2. Жмёшь хоткей (по умолчанию `Ctrl+Shift+F` на macOS, `Ctrl+Q` на Windows)
3. Текст заменяется на исправленный

## Установка

### macOS

Качаешь DMG из [релизов](https://github.com/seidenov/Spellify/releases), перетаскиваешь в Applications. При первом запуске попросит API ключ и доступ к Accessibility.

### Windows

Качаешь `Spellify-Setup.exe` из [релизов](https://github.com/seidenov/Spellify/releases) и устанавливаешь. Или скачай portable версию `Spellify.exe`.

## Сборка из исходников

### macOS

```bash
./create-dmg.sh
```

### Windows

Требуется [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
cd SpellifyWindows
dotnet publish -c Release -o dist
```

## Настройка

API ключ берёшь тут: [Google AI Studio](https://aistudio.google.com/apikey)

В настройках можно поменять модель и хоткей.

## Ограничения

Gemini API работает не во всех странах. [Список](https://ai.google.dev/gemini-api/docs/available-regions).

## Лицензия

MIT
