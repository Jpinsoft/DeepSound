# DeepSound for macOS

DeepSound is a cross-platform steganography tool and audio converter that hides secret data into audio files. 
The application also enables you to extract secret files directly from audio files.

**This is the macOS-compatible port** of the original [DeepSound](https://github.com/Jpinsoft/DeepSound) Windows application, built with .NET 9 and Avalonia UI for cross-platform compatibility.

## Features

- **Audio Steganography**: Hide secret files inside WAV audio files using LSB (Least Significant Bit) encoding
- **AES-256 Encryption**: Optionally encrypt hidden files with a password for enhanced security
- **Cross-Platform**: Runs on macOS, Windows, and Linux
- **Modern UI**: Built with Avalonia UI for a native look and feel

## Supported Formats

- **Carrier Files**: WAV (PCM audio)
- **Secret Files**: Any file type

## Screenshots

<div align="center">
<picture>
<img alt="DeepSound for macOS" width="600" src="https://github.com/Jpinsoft/DeepSound/assets/28184960/59478086-1eb9-402a-b4cb-c3f96239cdeb">
</picture>
</div>

## Installation

### Download Pre-built Release

Download the latest pre-built macOS application from the [Releases](https://github.com/MeAmCat/DeepSound-Mac/releases) page:
- **DeepSoundMac-macOS-arm64.zip** for Apple Silicon Macs (M1, M2, M3, etc.)
- **DeepSoundMac-macOS-x64.zip** for Intel Macs

After downloading:
1. Extract the ZIP file
2. Open Terminal and navigate to the extracted folder
3. Run the application:
   ```bash
   ./DeepSoundMac.App
   ```
   Or make it executable first if needed:
   ```bash
   chmod +x DeepSoundMac.App
   ./DeepSoundMac.App
   ```

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later (only required for building from source)

### Build from Source

1. Clone the repository:
   ```bash
   git clone https://github.com/MeAmCat/DeepSound-Mac.git
   cd DeepSound-Mac
   ```

2. Build the application:
   ```bash
   dotnet build DeepSoundMac.sln
   ```

3. Run the application:
   ```bash
   dotnet run --project src/DeepSoundMac.App/DeepSoundMac.App.csproj
   ```

### Publish as Standalone Application

To create a standalone macOS application:

```bash
# For macOS ARM64 (Apple Silicon)
dotnet publish src/DeepSoundMac.App/DeepSoundMac.App.csproj -c Release -r osx-arm64 --self-contained

# For macOS x64 (Intel)
dotnet publish src/DeepSoundMac.App/DeepSoundMac.App.csproj -c Release -r osx-x64 --self-contained

# For Windows
dotnet publish src/DeepSoundMac.App/DeepSoundMac.App.csproj -c Release -r win-x64 --self-contained

# For Linux
dotnet publish src/DeepSoundMac.App/DeepSoundMac.App.csproj -c Release -r linux-x64 --self-contained
```

## Usage

### Hiding Files (Encoding)

1. Open the application
2. Click "Browse..." to select a WAV audio file as the carrier
3. Add secret files you want to hide using the "Add Files..." button
4. (Optional) Enable encryption and enter a password
5. Select an output directory
6. Click "Encode and Save" to create the output file

### Extracting Files (Decoding)

1. Open the application
2. Click "Browse..." to select a WAV file containing hidden data
3. If the file is encrypted, enter the password
4. Click "Extract Hidden Files"
5. Click "Save Extracted Files" to save the extracted files

## Technical Details

- **Steganography Method**: LSB (Least Significant Bit) encoding - modifies the least significant bit of each audio sample to store hidden data
- **Encryption**: AES-256 with PBKDF2 key derivation (100,000 iterations, SHA-256)
- **Maximum Capacity**: Approximately 1 bit per sample (e.g., a 44.1kHz stereo WAV file can hide about 5.5KB per second of audio)

## Project Structure

```
DeepSound-Mac/
├── src/
│   ├── DeepSoundMac.App/          # Avalonia UI Application
│   │   ├── Views/                  # XAML views
│   │   ├── ViewModels/             # MVVM view models
│   │   └── Assets/                 # Application resources
│   └── DeepSoundMac.Core/          # Core library
│       ├── Audio/                  # Audio file handling
│       ├── Encryption/             # AES encryption
│       ├── Models/                 # Data models
│       └── Steganography/          # Steganography engine
├── Localization/                   # Localization resources
└── Files/                          # Product information
```

## Releases

This project uses GitHub Actions to automatically build and release macOS applications. When a version tag is pushed (e.g., `v1.0.0`), the workflow will:
1. Build and test the application
2. Publish for both macOS ARM64 (Apple Silicon) and x64 (Intel)
3. Create ZIP archives of the builds
4. Automatically create a GitHub release with the artifacts

To create a new release:
```bash
git tag v1.0.0
git push origin v1.0.0
```

## Contributing

If you find DeepSound useful and would like to improve this app, you are welcome. 
The GitHub Issue tracker is the preferred channel for bug reports and improvement requests.

## License

This project is based on the original [DeepSound](https://github.com/Jpinsoft/DeepSound) project.

## Acknowledgments

- Original DeepSound application by [Jpinsoft](https://github.com/Jpinsoft)
- Built with [Avalonia UI](https://avaloniaui.net/) for cross-platform support
- Uses [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/) for MVVM patterns
