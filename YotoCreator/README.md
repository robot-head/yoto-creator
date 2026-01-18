# Yoto Creator - Windows UWP Application

A Windows UWP application for creating custom content for Yoto Player with AI-generated artwork.

## Features

- **Audio File Management**: Pick and organize audio files from your filesystem to create custom soundtracks
- **ChatGPT Integration**: Authenticate and generate AI-powered images using OpenAI's DALL-E
- **Icon Generation**: Create 16x16 full-color icons for Yoto Player display
- **Cover Image Generation**: Generate custom cover images for your content
- **Yoto API Integration**: Authenticate and manage content on the Yoto platform

## Project Structure

```
YotoCreator/
├── App.xaml                    - Application definition
├── App.xaml.cs                 - Application entry point and lifecycle
├── MainPage.xaml               - Main UI layout
├── MainPage.xaml.cs            - Main page code-behind
├── Package.appxmanifest        - UWP app manifest
├── YotoCreator.csproj          - Project file
├── Assets/                     - Application assets (icons, splash screen)
├── Models/
│   ├── AudioFile.cs            - Audio file data model
│   └── YotoContent.cs          - Yoto content data model
├── Properties/
│   ├── AssemblyInfo.cs         - Assembly metadata
│   └── Default.rd.xml          - Runtime directives
└── Services/
    ├── AudioFileService.cs     - Audio file picking and management
    ├── ChatGptService.cs       - OpenAI/ChatGPT API integration
    └── YotoApiService.cs       - Yoto API integration
```

## Requirements

- Windows 10 version 17763 or higher
- Visual Studio 2019 or later with UWP workload
- .NET Native compilation support
- OpenAI API key for ChatGPT integration
- Yoto API key for content management

## Setup

1. Open the solution in Visual Studio
2. Restore NuGet packages:
   - Microsoft.NETCore.UniversalWindowsPlatform (6.2.14)
   - Newtonsoft.Json (13.0.3)
3. Add actual image assets to the Assets/ folder (see Assets/README.txt)
4. Build and run the application

## Usage

### 1. Authentication

#### ChatGPT API
1. Enter your OpenAI API key in the ChatGPT API field
2. Click "Connect" to authenticate
3. Wait for confirmation

#### Yoto API
1. Enter your Yoto API key in the Yoto API field
2. Click "Connect" to authenticate
3. Wait for confirmation

### 2. Adding Audio Files

1. Click "Pick Audio Files" button
2. Select one or more audio files from your filesystem
3. Supported formats: MP3, M4A, WAV, WMA, AAC, FLAC, OGG
4. Files will appear in the list with duration information
5. Use "Remove Selected" or "Clear All" to manage the list

### 3. Generating AI Images

#### Icon Generation
1. Enter a description of the icon you want in the icon prompt field
2. Click "Generate Icon"
3. Wait for the AI to generate the 16x16 icon
4. Preview will appear once generated

#### Cover Image Generation
1. Enter a description of the cover image in the cover prompt field
2. Click "Generate Cover"
3. Wait for the AI to generate the cover image
4. Preview will appear once generated

### 4. Creating Yoto Content

1. Enter a title for your content
2. Optionally enter a description
3. Ensure you have:
   - At least one audio file added
   - Authenticated with Yoto API
   - (Optional) Generated icon and cover images
4. Click "Create New Content"
5. Wait for the content to be uploaded
6. Success message will show the content ID

## API Integration Notes

### OpenAI API
- Uses DALL-E 3 for image generation
- Uses GPT-4 for text generation
- Images are generated at requested dimensions
- Note: 16x16 images may need to be resized from DALL-E's minimum size

### Yoto API
- Base URL: `https://api.yotoplay.com/v1`
- Requires bearer token authentication
- Supports content creation, updating, and deletion
- Handles multipart file uploads for audio, icons, and covers

## Notes

- This is a skeleton application with basic structure and functionality
- Asset images need to be created and added to the Assets folder
- API endpoints and authentication flows may need adjustment based on actual Yoto API documentation
- Error handling is implemented but may need enhancement for production use
- Image resizing functionality for 16x16 icons may need to be added

## Development

To extend this application:

1. **Add new models**: Create classes in the Models folder
2. **Add new services**: Create service classes in the Services folder
3. **Add new pages**: Create XAML/code-behind pairs in the Views folder
4. **Update UI**: Modify MainPage.xaml or create new pages
5. **Add features**: Implement additional Yoto API endpoints as needed

## License

This project is provided as-is for development purposes.
