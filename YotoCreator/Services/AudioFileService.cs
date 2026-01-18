using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using YotoCreator.Models;

namespace YotoCreator.Services
{
    /// <summary>
    /// Service for handling audio file operations
    /// </summary>
    public class AudioFileService
    {
        private readonly string[] _supportedAudioFormats = new[]
        {
            ".mp3",
            ".m4a",
            ".wav",
            ".wma",
            ".aac",
            ".flac",
            ".ogg"
        };

        /// <summary>
        /// Pick audio files from the file system
        /// </summary>
        public async Task<List<AudioFile>> PickAudioFilesAsync()
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.MusicLibrary
            };

            // Add supported audio formats
            foreach (var format in _supportedAudioFormats)
            {
                picker.FileTypeFilter.Add(format);
            }

            var files = await picker.PickMultipleFilesAsync();
            if (files == null || files.Count == 0)
                return new List<AudioFile>();

            var audioFiles = new List<AudioFile>();
            int order = 0;

            foreach (var file in files)
            {
                try
                {
                    var audioFile = await CreateAudioFileFromStorageFileAsync(file, order++);
                    audioFiles.Add(audioFile);
                }
                catch
                {
                    // Skip files that can't be processed
                    continue;
                }
            }

            return audioFiles;
        }

        /// <summary>
        /// Create AudioFile model from StorageFile
        /// </summary>
        private async Task<AudioFile> CreateAudioFileFromStorageFileAsync(StorageFile file, int order)
        {
            var properties = await file.GetBasicPropertiesAsync();
            var musicProperties = await file.Properties.GetMusicPropertiesAsync();

            var duration = musicProperties.Duration;
            var durationString = duration.Hours > 0
                ? $"{duration.Hours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}"
                : $"{duration.Minutes:D2}:{duration.Seconds:D2}";

            return new AudioFile
            {
                FileName = file.Name,
                FilePath = file.Path,
                Duration = durationString,
                FileSize = (long)properties.Size,
                Order = order,
                Token = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file)
            };
        }

        /// <summary>
        /// Get StorageFile from AudioFile model
        /// </summary>
        public async Task<StorageFile> GetStorageFileAsync(AudioFile audioFile)
        {
            if (!string.IsNullOrEmpty(audioFile.Token))
            {
                try
                {
                    return await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(audioFile.Token);
                }
                catch
                {
                    // Fall back to path-based access
                }
            }

            // Try to get file by path
            if (!string.IsNullOrEmpty(audioFile.FilePath))
            {
                try
                {
                    return await StorageFile.GetFileFromPathAsync(audioFile.FilePath);
                }
                catch
                {
                    throw new Exception($"Could not access file: {audioFile.FileName}");
                }
            }

            throw new Exception($"No valid path or token for file: {audioFile.FileName}");
        }

        /// <summary>
        /// Validate if file is a supported audio format
        /// </summary>
        public bool IsSupportedAudioFormat(string fileName)
        {
            var extension = System.IO.Path.GetExtension(fileName)?.ToLowerInvariant();
            return !string.IsNullOrEmpty(extension) && _supportedAudioFormats.Contains(extension);
        }

        /// <summary>
        /// Get total duration of all audio files
        /// </summary>
        public TimeSpan GetTotalDuration(List<AudioFile> audioFiles)
        {
            var totalSeconds = 0.0;

            foreach (var audioFile in audioFiles)
            {
                if (TimeSpan.TryParse(audioFile.Duration, out var duration))
                {
                    totalSeconds += duration.TotalSeconds;
                }
            }

            return TimeSpan.FromSeconds(totalSeconds);
        }

        /// <summary>
        /// Get total file size of all audio files
        /// </summary>
        public long GetTotalFileSize(List<AudioFile> audioFiles)
        {
            return audioFiles.Sum(f => f.FileSize);
        }

        /// <summary>
        /// Format file size for display
        /// </summary>
        public string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }
}
