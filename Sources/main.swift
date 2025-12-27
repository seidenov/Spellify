import Cocoa
import Carbon

struct HotKeyConfig {
    var keyCode: UInt32
    var modifiers: UInt32
    var displayName: String
    
    static let presets: [String: HotKeyConfig] = [
        "Ctrl+Shift+F": HotKeyConfig(keyCode: 3, modifiers: UInt32(controlKey | shiftKey), displayName: "Ctrl+Shift+F"),
        "Ctrl+Shift+G": HotKeyConfig(keyCode: 5, modifiers: UInt32(controlKey | shiftKey), displayName: "Ctrl+Shift+G"),
        "Ctrl+Shift+T": HotKeyConfig(keyCode: 17, modifiers: UInt32(controlKey | shiftKey), displayName: "Ctrl+Shift+T"),
        "Cmd+Shift+F": HotKeyConfig(keyCode: 3, modifiers: UInt32(cmdKey | shiftKey), displayName: "Cmd+Shift+F"),
        "Cmd+Shift+G": HotKeyConfig(keyCode: 5, modifiers: UInt32(cmdKey | shiftKey), displayName: "Cmd+Shift+G"),
        "Option+Shift+F": HotKeyConfig(keyCode: 3, modifiers: UInt32(optionKey | shiftKey), displayName: "Option+Shift+F")
    ]
}

struct Settings {
    static var apiKey: String {
        get { UserDefaults.standard.string(forKey: "GeminiAPIKey") ?? "" }
        set { UserDefaults.standard.set(newValue, forKey: "GeminiAPIKey") }
    }
    
    static var model: String {
        get { UserDefaults.standard.string(forKey: "GeminiModel") ?? "gemini-2.0-flash" }
        set { UserDefaults.standard.set(newValue, forKey: "GeminiModel") }
    }
    
    static var hotkeyName: String {
        get { UserDefaults.standard.string(forKey: "HotKeyName") ?? "Ctrl+Shift+F" }
        set { UserDefaults.standard.set(newValue, forKey: "HotKeyName") }
    }
    
    static var hotkey: HotKeyConfig {
        HotKeyConfig.presets[hotkeyName] ?? HotKeyConfig.presets["Ctrl+Shift+F"]!
    }
    
    static var geminiURL: String {
        "https://generativelanguage.googleapis.com/v1beta/models/\(model):generateContent?key=\(apiKey)"
    }
    
    static let availableModels = [
        "gemini-2.0-flash",
        "gemini-2.5-flash-lite",
        "gemini-3-flash-preview"
    ]
}

class SettingsWindowController: NSWindowController {
    static let shared = SettingsWindowController()
    
    let apiKeyField = NSTextField()
    let modelPopup = NSPopUpButton()
    let hotkeyPopup = NSPopUpButton()
    var onSave: (() -> Void)?
    
    init() {
        let window = NSWindow(
            contentRect: NSRect(x: 0, y: 0, width: 400, height: 240),
            styleMask: [.titled, .closable],
            backing: .buffered,
            defer: false
        )
        window.title = "Настройки Spellify"
        window.center()
        
        let contentView = NSView(frame: window.contentView!.bounds)
        
        let apiLabel = NSTextField(labelWithString: "API ключ Gemini:")
        apiLabel.frame = NSRect(x: 20, y: 190, width: 360, height: 20)
        contentView.addSubview(apiLabel)
        
        apiKeyField.frame = NSRect(x: 20, y: 160, width: 360, height: 24)
        apiKeyField.placeholderString = "Введите API ключ"
        apiKeyField.stringValue = Settings.apiKey
        contentView.addSubview(apiKeyField)
        
        let modelLabel = NSTextField(labelWithString: "Модель:")
        modelLabel.frame = NSRect(x: 20, y: 125, width: 360, height: 20)
        contentView.addSubview(modelLabel)
        
        modelPopup.frame = NSRect(x: 20, y: 95, width: 360, height: 25)
        modelPopup.addItems(withTitles: Settings.availableModels)
        if let index = Settings.availableModels.firstIndex(of: Settings.model) {
            modelPopup.selectItem(at: index)
        }
        contentView.addSubview(modelPopup)
        
        let hotkeyLabel = NSTextField(labelWithString: "Сочетание клавиш:")
        hotkeyLabel.frame = NSRect(x: 20, y: 60, width: 360, height: 20)
        contentView.addSubview(hotkeyLabel)
        
        hotkeyPopup.frame = NSRect(x: 20, y: 30, width: 360, height: 25)
        let hotkeyNames = Array(HotKeyConfig.presets.keys).sorted()
        hotkeyPopup.addItems(withTitles: hotkeyNames)
        if let index = hotkeyNames.firstIndex(of: Settings.hotkeyName) {
            hotkeyPopup.selectItem(at: index)
        }
        contentView.addSubview(hotkeyPopup)
        
        let saveButton = NSButton(title: "Сохранить", target: nil, action: #selector(save))
        saveButton.frame = NSRect(x: 280, y: 5, width: 100, height: 30)
        saveButton.bezelStyle = .rounded
        saveButton.keyEquivalent = "\r"
        contentView.addSubview(saveButton)
        
        window.contentView = contentView
        super.init(window: window)
        saveButton.target = self
    }
    
    required init?(coder: NSCoder) { fatalError() }
    
    @objc func save() {
        let oldHotkey = Settings.hotkeyName
        Settings.apiKey = apiKeyField.stringValue.trimmingCharacters(in: .whitespacesAndNewlines)
        Settings.model = modelPopup.titleOfSelectedItem ?? "gemini-2.0-flash"
        Settings.hotkeyName = hotkeyPopup.titleOfSelectedItem ?? "Ctrl+Shift+F"
        
        if oldHotkey != Settings.hotkeyName {
            onSave?()
        }
        window?.close()
    }
    
    func showIfNeeded() {
        if Settings.apiKey.isEmpty { show() }
    }
    
    func show() {
        apiKeyField.stringValue = Settings.apiKey
        if let index = Settings.availableModels.firstIndex(of: Settings.model) {
            modelPopup.selectItem(at: index)
        }
        let hotkeyNames = Array(HotKeyConfig.presets.keys).sorted()
        if let index = hotkeyNames.firstIndex(of: Settings.hotkeyName) {
            hotkeyPopup.selectItem(at: index)
        }
        showWindow(nil)
        NSApp.activate(ignoringOtherApps: true)
    }
}

class AlertHelper {
    static func showAccessibilityAlert() {
        DispatchQueue.main.async {
            let alert = NSAlert()
            alert.messageText = "Требуется разрешение"
            alert.informativeText = "Spellify нужен доступ к Универсальному доступу для чтения выделенного текста.\n\nНажмите «Открыть настройки» и добавьте Spellify в список."
            alert.alertStyle = .warning
            alert.addButton(withTitle: "Открыть настройки")
            alert.addButton(withTitle: "Отмена")
            
            NSApp.activate(ignoringOtherApps: true)
            
            if alert.runModal() == .alertFirstButtonReturn {
                NSWorkspace.shared.open(URL(string: "x-apple.systempreferences:com.apple.preference.security?Privacy_Accessibility")!)
            }
        }
    }
}

class AppDelegate: NSObject, NSApplicationDelegate {
    var statusItem: NSStatusItem!
    var hotKeyRef: EventHotKeyRef?
    var hotkeyMenuItem: NSMenuItem?
    
    func applicationDidFinishLaunching(_ notification: Notification) {
        setupMenuBar()
        registerHotKey()
        
        SettingsWindowController.shared.onSave = { [weak self] in
            self?.reregisterHotKey()
        }
        
        DispatchQueue.main.asyncAfter(deadline: .now() + 0.5) {
            SettingsWindowController.shared.showIfNeeded()
        }
    }
    
    func setupMenuBar() {
        statusItem = NSStatusBar.system.statusItem(withLength: NSStatusItem.variableLength)
        statusItem.button?.title = "✏️"
        
        let menu = NSMenu()
        hotkeyMenuItem = NSMenuItem(title: "Исправить текст (\(Settings.hotkeyName))", action: #selector(fixSelectedText), keyEquivalent: "")
        menu.addItem(hotkeyMenuItem!)
        menu.addItem(NSMenuItem.separator())
        menu.addItem(NSMenuItem(title: "Настройки...", action: #selector(showSettings), keyEquivalent: ","))
        menu.addItem(NSMenuItem(title: "Проверить API", action: #selector(testAPI), keyEquivalent: ""))
        menu.addItem(NSMenuItem(title: "Универсальный доступ", action: #selector(openAccessibility), keyEquivalent: ""))
        menu.addItem(NSMenuItem.separator())
        menu.addItem(NSMenuItem(title: "Выход", action: #selector(quit), keyEquivalent: "q"))
        statusItem.menu = menu
    }
    
    @objc func showSettings() {
        SettingsWindowController.shared.show()
    }
    
    @objc func openAccessibility() {
        NSWorkspace.shared.open(URL(string: "x-apple.systempreferences:com.apple.preference.security?Privacy_Accessibility")!)
    }
    
    func reregisterHotKey() {
        if let ref = hotKeyRef {
            UnregisterEventHotKey(ref)
            hotKeyRef = nil
        }
        registerHotKey()
        hotkeyMenuItem?.title = "Исправить текст (\(Settings.hotkeyName))"
    }
    
    func registerHotKey() {
        let config = Settings.hotkey
        
        var hotKeyID = EventHotKeyID()
        hotKeyID.signature = OSType(0x54584658)
        hotKeyID.id = 1
        
        RegisterEventHotKey(config.keyCode, config.modifiers, hotKeyID, GetApplicationEventTarget(), 0, &hotKeyRef)
        
        var eventType = EventTypeSpec(eventClass: OSType(kEventClassKeyboard), eventKind: UInt32(kEventHotKeyPressed))
        InstallEventHandler(GetApplicationEventTarget(), { (_, _, _) -> OSStatus in
            DispatchQueue.main.async {
                (NSApp.delegate as? AppDelegate)?.fixSelectedText()
            }
            return noErr
        }, 1, &eventType, nil, nil)
    }
    
    @objc func testAPI() {
        if Settings.apiKey.isEmpty {
            SettingsWindowController.shared.show()
            return
        }
        callGemini(text: "привет как дила") { result in
            DispatchQueue.main.async {
                let alert = NSAlert()
                switch result {
                case .success(let fixed):
                    alert.messageText = "API работает"
                    alert.informativeText = "Ответ: \(fixed)"
                case .failure(let error):
                    alert.messageText = "Ошибка API"
                    alert.informativeText = error.localizedDescription
                }
                alert.runModal()
            }
        }
    }
    
    @objc func fixSelectedText() {
        if Settings.apiKey.isEmpty {
            SettingsWindowController.shared.show()
            return
        }
        
        let pasteboard = NSPasteboard.general
        pasteboard.clearContents()
        
        let src = CGEventSource(stateID: .hidSystemState)
        let keyDown = CGEvent(keyboardEventSource: src, virtualKey: 0x08, keyDown: true)
        let keyUp = CGEvent(keyboardEventSource: src, virtualKey: 0x08, keyDown: false)
        keyDown?.flags = .maskCommand
        keyUp?.flags = .maskCommand
        keyDown?.post(tap: .cghidEventTap)
        keyUp?.post(tap: .cghidEventTap)
        
        DispatchQueue.main.asyncAfter(deadline: .now() + 0.15) { [weak self] in
            guard let text = pasteboard.string(forType: .string), !text.isEmpty else {
                AlertHelper.showAccessibilityAlert()
                return
            }
            
            self?.callGemini(text: text) { result in
                DispatchQueue.main.async {
                    switch result {
                    case .success(let fixed):
                        pasteboard.clearContents()
                        pasteboard.setString(fixed, forType: .string)
                        
                        let src = CGEventSource(stateID: .hidSystemState)
                        let keyDown = CGEvent(keyboardEventSource: src, virtualKey: 0x09, keyDown: true)
                        let keyUp = CGEvent(keyboardEventSource: src, virtualKey: 0x09, keyDown: false)
                        keyDown?.flags = .maskCommand
                        keyUp?.flags = .maskCommand
                        keyDown?.post(tap: .cghidEventTap)
                        keyUp?.post(tap: .cghidEventTap)
                        
                    case .failure(let error):
                        self?.showNotification(title: "Ошибка", message: error.localizedDescription)
                    }
                }
            }
        }
    }
    
    func callGemini(text: String, completion: @escaping (Result<String, Error>) -> Void) {
        guard let url = URL(string: Settings.geminiURL) else { return }
        
        var request = URLRequest(url: url)
        request.httpMethod = "POST"
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")
        request.timeoutInterval = 30
        
        let prompt = "Исправь орфографические и пунктуационные ошибки. Верни ТОЛЬКО исправленный текст:\n\n\(text)"
        let body: [String: Any] = [
            "contents": [["parts": [["text": prompt]]]],
            "generationConfig": ["temperature": 0.1, "maxOutputTokens": 2048]
        ]
        request.httpBody = try? JSONSerialization.data(withJSONObject: body)
        
        URLSession.shared.dataTask(with: request) { data, response, error in
            if let error = error {
                completion(.failure(NSError(domain: "Spellify", code: -1, userInfo: [NSLocalizedDescriptionKey: error.localizedDescription])))
                return
            }
            
            if let httpResponse = response as? HTTPURLResponse {
                if httpResponse.statusCode == 429 {
                    completion(.failure(NSError(domain: "Spellify", code: 429, userInfo: [NSLocalizedDescriptionKey: "Лимит запросов исчерпан. Подожди минуту."])))
                    return
                }
                if httpResponse.statusCode == 401 || httpResponse.statusCode == 403 {
                    completion(.failure(NSError(domain: "Spellify", code: httpResponse.statusCode, userInfo: [NSLocalizedDescriptionKey: "Неверный API ключ"])))
                    return
                }
            }
            
            guard let data = data,
                  let json = try? JSONSerialization.jsonObject(with: data) as? [String: Any] else {
                completion(.failure(NSError(domain: "Spellify", code: -1, userInfo: [NSLocalizedDescriptionKey: "Ошибка парсинга ответа"])))
                return
            }
            
            if let error = json["error"] as? [String: Any], let message = error["message"] as? String {
                completion(.failure(NSError(domain: "Spellify", code: -1, userInfo: [NSLocalizedDescriptionKey: message])))
                return
            }
            
            guard let candidates = json["candidates"] as? [[String: Any]],
                  let content = candidates.first?["content"] as? [String: Any],
                  let parts = content["parts"] as? [[String: Any]],
                  let text = parts.first?["text"] as? String else {
                completion(.failure(NSError(domain: "Spellify", code: -1, userInfo: [NSLocalizedDescriptionKey: "Неожиданный формат ответа"])))
                return
            }
            completion(.success(text.trimmingCharacters(in: .whitespacesAndNewlines)))
        }.resume()
    }
    
    @objc func quit() { NSApp.terminate(nil) }
    
    func showNotification(title: String, message: String) {
        DispatchQueue.main.async {
            let alert = NSAlert()
            alert.messageText = title
            alert.informativeText = message
            alert.alertStyle = .warning
            alert.addButton(withTitle: "OK")
            NSApp.activate(ignoringOtherApps: true)
            alert.runModal()
        }
    }
}

let app = NSApplication.shared
app.delegate = AppDelegate()
app.setActivationPolicy(.accessory)
app.run()
