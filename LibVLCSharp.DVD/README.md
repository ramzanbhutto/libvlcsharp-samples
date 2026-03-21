# LibVLCSharp DVD Player Sample
A cross-platform DVD player sample built with LibVLCSharp and Avalonia UI.

---

## Requirements
- .NET 10 SDK
- Avalonia 11.3.12
- LibVLCSharp 3.9.6
- LibVLCSharp.Avalonia 3.9.6
- VideoLAN.LibVLC.Windows 3.0.23 (Windows)
- VideoLAN.LibVLC.Mac 3.1.3.1 (macOS)
- **Linux only:** `vlc-plugin-dvd` and `libdvdcss` (required for DVD playback)
```bash
sudo pacman -S vlc-plugin-dvd libdvdcss   # Arch Linux
```
---

## Test DVD ISO (linux)
This sample was tested using the **Big Buck Bunny NTSC DVD ISO** — a free and legal open-source film by the Blender Foundation.
- Download: [Big-Buck-Bunny](https://ia801907.us.archive.org/13/items/BigBuckBunny/big-buck-bunny-NTSC.iso) 
- Verify ISO: ls -lh ~/big-buck-bunny-NTSC.iso 

### Mount and inspect
```bash
sudo mkdir -p /mnt/dvd
sudo mount -o loop ~/big-buck-bunny-NTSC.iso /mnt/dvd
ls /mnt/dvd   # should contain VIDEO_TS folder
```

### Test with system VLC first
```bash
vlc dvd:///mnt/dvd
vlc /mnt/dvd/VIDEO_TS/VTS_01_1.VOB  # or play specific title
```

### Unmount when done
```bash
sudo umount /mnt/dvd
```
---

## Test DVD ISO (windows)
**Prerequisites**
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- No separate VLC installation needed, as `VideoLAN.LibVLC.Windows` NuGet package bundles the native libraries

**Run**
```bash
cd LibVLCSharp.DVD
dotnet run
```

**DVD Source Options**

- **Physical DVD drive:** `dvd:///D:/` (replace `D` with your drive letter)
- **ISO file:** right-click the `.iso` in File Explorer → **Mount**, then use the assigned drive letter: `dvd:///E:/`
- **VIDEO_TS folder:** `dvd:///C:/path/to/VIDEO_TS`

> **Note:** Encrypted commercial DVDs require `libdvdcss`. The NuGet package does not bundle it. Unencrypted ISOs (e.g. Big Buck Bunny) work without it.

---

## Testing on macOS

**Prerequisites**
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- No separate VLC installation needed, as `VideoLAN.LibVLC.Mac` NuGet package bundles the native libraries

**Run**
```bash
cd LibVLCSharp.DVD
dotnet run
```

**DVD Source Options**

- **Physical DVD drive** — `dvd:///dev/rdisk2` (verify disk number with `diskutil list`)
- **ISO file** — mount via Finder (double-click) or terminal:
```bash
  hdiutil attach ~/your-file.iso
```
  Then use `dvd:///Volumes/VOLUME_NAME/` (volume name shown after mount)
- **VIDEO_TS folder** — `dvd:///path/to/VIDEO_TS`

> **Note:** Encrypted commercial DVDs require `libdvdcss`, installable via Homebrew: `brew install libdvdcss`. Unencrypted ISOs work without it.

---

## Project Setup

### Create the project
```bash
dotnet new install Avalonia.Templates
dotnet new avalonia.app -n LibVLCSharp.DVD
```

### Add packages
```bash
cd LibVLCSharp.DVD # go to the project directory
dotnet add package LibVLCSharp
dotnet add package LibVLCSharp.Avalonia
dotnet add package VideoLAN.LibVLC.Windows
dotnet add package VideoLAN.LibVLC.Mac
# don't need of linux nuget package as linux uses system-installed VLC
```

### Add to solution
```bash
cd ~/libvlcsharp-samples # go to the root directory
dotnet sln libvlcsharp-samples-windows.sln add LibVLCSharp.DVD/LibVLCSharp.DVD.csproj
dotnet sln libvlcsharp-samples-mac.sln add LibVLCSharp.DVD/LibVLCSharp.DVD.csproj
```

---

## Run
```bash
dotnet run
```
> Then click **📂 Open ISO**, select your DVD ISO file and it starts playing.

---

## Features

- DVD ISO file picker
- Play / Pause
- Seek via timestamp slider
- Volume control
- Minimize, fullscreen and close via custom title bar

---

## Key Implementation Notes

- `Core.Initialize()` must be called before Avalonia starts in `Program.cs`
- DVD URI format: `dvd://<absolute-path-to-iso>`
- `NavigationMode` in LibVLCSharp 3.9.6 has no `Menu` value, so use `Popup` instead, and cast to `(uint)` since `Navigate()` expects `uint`
- Slider `PointerPressed`/`PointerReleased` must be registered via `AddHandler(..., handledEventsToo: true)` in the constructor — XAML event binding doesn't fire on Slider thumb interaction in Avalonia
- `Dispatcher.UIThread.Post()` is required when updating the UI from libvlc's `TimeChanged` event since it fires on a background thread
- On Linux, `vlc-plugin-dvd` must be installed separately — without it DVD ISO playback fails silently
