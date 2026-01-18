# Yoto Creator

A Windows UWP application for authoring custom content for Yoto Players with AI-generated artwork.

## Features

- **Audio File Management**: Pick and organize multiple audio files to create custom soundtracks
- **AI-Powered Icon Generation**: Generate 16x16 full-color icons using ChatGPT/DALL-E
- **AI-Powered Cover Images**: Create custom cover artwork using AI image generation
- **Yoto API Integration**: Authenticate and manage content directly on the Yoto platform
- **Modern UWP Interface**: Clean, intuitive Windows 10/11 interface

## Project Structure

```
YotoCreator/
├── YotoCreator.sln             - Visual Studio solution file
├── YotoCreator/
│   ├── App.xaml                - Application definition
│   ├── App.xaml.cs             - Application lifecycle management
│   ├── MainPage.xaml           - Main UI layout
│   ├── MainPage.xaml.cs        - Main page logic
│   ├── Package.appxmanifest    - UWP app manifest
│   ├── YotoCreator.csproj      - Project file
│   ├── Assets/                 - Application assets
│   ├── Models/                 - Data models
│   │   ├── AudioFile.cs
│   │   └── YotoContent.cs
│   ├── Properties/             - Assembly information
│   │   ├── AssemblyInfo.cs
│   │   └── Default.rd.xml
│   └── Services/               - Business logic services
│       ├── AudioFileService.cs
│       ├── ChatGptService.cs
│       └── YotoApiService.cs
```

## Requirements

- Windows 10 version 17763 or higher
- Visual Studio 2019 or later with UWP development workload
- OpenAI API key (for image generation)
- Yoto API key (for content management)

## Getting Started

1. Open `YotoCreator.sln` in Visual Studio
2. Restore NuGet packages
3. Add image assets to the `YotoCreator/Assets/` folder (see Assets/README.txt)
4. Build and run the application
5. Enter your API keys for ChatGPT and Yoto
6. Start creating content!

## Documentation

For detailed usage instructions and API integration notes, see [YotoCreator/README.md](YotoCreator/README.md)

## License

This project is provided as-is for development purposes.