# TimeTracker Installer

Questo modulo permette di generare l'installer Windows (`.exe`) per TimeTracker tramite **Inno Setup**.

---

## 🚀 Come Creare l'Installer

Puoi creare l'installer in due modi semplici:

### Metodo 1: Doppio clic su `build-installer.bat`
Dalla cartella principale del progetto, fai doppio clic sul file [`build-installer.bat`](file:///D:/Projects/TimeTracker/build-installer.bat).

### Metodo 2: PowerShell
Esegui da terminale:
```powershell
.\build-installer.ps1
```

Puoi anche personalizzare parametri, ad esempio la versione o il tipo di build:
```powershell
# Specifica una versione
.\build-installer.ps1 -Version "1.1.0"

# Build non self-contained (richiede .NET 8 Runtime installato sul PC di destinazione, installer molto più leggero ~10MB)
.\build-installer.ps1 -SelfContained $false
```

---

## 📁 File Generato

L'eseguibile di installazione viene creato nella cartella:
```
Installer/Output/TimeTracker-Setup-v{versione}.exe
```

---

## ✨ Funzionalità dell'Installer

1. **Self-Contained & ReadyToRun**:
   - Include il runtime .NET 8 compilato Ahead-Of-Time (ReadyToRun): l'applicazione funziona su qualsiasi PC Windows a 64 bit senza dover installare runtime o SDK esterni, con avvio immediato.
2. **Installazione Per-User o All-Users**:
   - Supporta l'installazione sia per il singolo utente (`%LOCALAPPDATA%\Programs\TimeTracker`, senza richiedere permessi di amministratore) che a livello di sistema (`Program Files`, con privilegi elevati).
3. **Scorciatoie & Icone**:
   - Crea il collegamento nel Menu Start.
   - Opzione (selezionabile) per creare il collegamento sul Desktop.
   - Utilizza l'icona ufficiale dell'applicazione ([`app_icon.ico`](file:///D:/Projects/TimeTracker/TimeTracker/Resources/app_icon.ico)).
4. **Avvio Automatico con Windows**:
   - Opzione nell'installer per avviare TimeTracker all'accesso a Windows (configura la chiave `Software\Microsoft\Windows\CurrentVersion\Run`).
5. **Supporto Multilingua**:
   - Interfaccia dell'installer disponibile sia in **Italiano** che in **Inglese**.
6. **Disinstallazione Pulita**:
   - Registrazione nei programmi installati di Windows ("App e funzionalità").
   - Rimozione pulita dei file e delle chiavi di registro di avvio.
   - Mantiene intatti i dati storici del database (`%APPDATA%\TimeTracker\timetracker.db`) per evitare la perdita accidentale delle ore registrate.
