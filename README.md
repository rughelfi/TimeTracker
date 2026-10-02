# TimeTracker ⏱️

TimeTracker è un'applicazione desktop moderna e leggera per Windows, sviluppata in **WPF (.NET 8)**, pensata per tracciare il tempo dedicato alle diverse attività quotidiane con il minimo attrito.

---

## 🚀 Caratteristiche Principali

- **Widget compatto e discreto:** Floating bar per inserire rapidamente l'attività corrente e avviare il timer.
- **Icona nella System Tray:** Riduzione nell'area di notifica con menu contestuale per avvio/pausa, visualizzazione rapida e accesso alle impostazioni.
- **Autocompletamento intelligente:** Suggerimenti in tempo reale sulle attività passate durante la digitazione.
- **Raggruppamento attività:** Riconoscimento automatico e raggruppamento per somiglianza delle attività registrate.
- **Esportazione dati:** Esportazione del log delle attività in formato **Excel (.xlsx)** e **CSV**.
- **Promemoria acustici:** Notifiche e segnali sonori configurabili (con suoni personalizzati o sintetizzatore acustico integrato).
- **Archiviazione locale sicura:** Database SQLite locale memorizzato in `%APPDATA%\TimeTracker\timetracker.db`.
- **Avvio con Windows:** Possibilità di avviare automaticamente l'applicazione all'accesso dell'utente.

---

## 📦 Installazione

Puoi scaricare e installare TimeTracker in pochi secondi:

1. **Scarica l'installer ufficiale:**
   - ⬇️ **[Download TimeTracker-Setup-v1.0.0.exe](https://github.com/rughelfi/TimeTracker/releases/latest/download/TimeTracker-Setup-v1.0.0.exe)** (disponibile nella sezione **[GitHub Releases](https://github.com/rughelfi/TimeTracker/releases)**).
2. **Esegui l'installer:**
   - **Nessun prerequisito:** Include già il runtime .NET 8 (Self-Contained & ReadyToRun per Windows a 64 bit).
   - **Nessun diritto di amministratore richiesto:** Può essere installato sia per il singolo utente (`%LOCALAPPDATA%`) che per tutti gli utenti del sistema.
   - Crea automaticamente i collegamenti nel Menu Start e (opzionalmente) sul Desktop con l'icona ufficiale.
   - Opzione per configurare l'avvio automatico all'accesso a Windows.

Per compilare autonomamente l'installer dal codice sorgente, consulta [Installer README](Installer/README.md).

---

## 🛠️ Compilazione da Sorgente

### Prerequisiti
- **Windows 10 / 11 (x64)**
- **.NET 8 SDK** (o superiore)

### Esecuzione in modalità Debug
```powershell
dotnet run --project TimeTracker/TimeTracker.csproj
```

### Generazione dell'Installer
Fai doppio clic su `build-installer.bat` oppure esegui da PowerShell:
```powershell
.\build-installer.ps1
```

---

## 🏗️ Struttura della Soluzione

```
TimeTracker/
├── TimeTracker/           # Progetto WPF principale (.NET 8)
│   ├── Converters/        # Value converter XAML
│   ├── Data/              # Entity Framework Core DbContext (SQLite)
│   ├── Models/            # Modelli di dati (ActivityEntry, AppSettings)
│   ├── Resources/         # Icone e stili XAML
│   ├── Services/          # Servizi (Timer, TrayIcon, Export, Autocomplete, Grouping)
│   ├── ViewModels/        # ViewModels (CommunityToolkit.Mvvm)
│   └── Views/             # Finestre WPF (Widget, Log, Settings)
├── Inspector/             # Utility per ispezione / diagnostica
├── Installer/             # Script Inno Setup e tool per generare il file di setup
├── build-installer.bat    # Script batch per build immediata dell'installer
└── build-installer.ps1    # Script PowerShell per packaging e compilazione
```

---

## 📄 Licenza

Questo progetto è rilasciato sotto Licenza MIT.
