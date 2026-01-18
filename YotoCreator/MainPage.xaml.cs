using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using YotoCreator.Models;
using YotoCreator.Services;

namespace YotoCreator
{
    /// <summary>
    /// Main page for the Yoto Creator application
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly AudioFileService _audioFileService;
        private readonly ChatGptService _chatGptService;
        private readonly YotoApiService _yotoApiService;
        private ObservableCollection<AudioFile> _audioFiles;
        private ObservableCollection<Chapter> _chapters;
        private Chapter _selectedChapter;
        private byte[] _generatedIcon;
        private byte[] _generatedCover;

        public MainPage()
        {
            this.InitializeComponent();

            _audioFileService = new AudioFileService();
            _chatGptService = new ChatGptService();
            _yotoApiService = new YotoApiService();
            _audioFiles = new ObservableCollection<AudioFile>();
            _chapters = new ObservableCollection<Chapter>();

            ChaptersList.ItemsSource = _chapters;
        }

        #region Authentication

        private async void ChatGptAuthButton_Click(object sender, RoutedEventArgs e)
        {
            var apiKey = ChatGptApiKeyBox.Password;
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                ChatGptStatusText.Text = "Please enter an API key";
                ChatGptStatusText.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Red);
                return;
            }

            try
            {
                var success = await _chatGptService.AuthenticateAsync(apiKey);
                if (success)
                {
                    ChatGptStatusText.Text = "Connected";
                    ChatGptStatusText.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Green);
                }
                else
                {
                    ChatGptStatusText.Text = "Authentication failed";
                    ChatGptStatusText.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Red);
                }
            }
            catch (Exception ex)
            {
                ChatGptStatusText.Text = $"Error: {ex.Message}";
                ChatGptStatusText.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Red);
            }
        }

        private async void YotoAuthButton_Click(object sender, RoutedEventArgs e)
        {
            var apiKey = YotoApiKeyBox.Text;
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                YotoStatusText.Text = "Please enter an API key";
                YotoStatusText.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Red);
                return;
            }

            try
            {
                var success = await _yotoApiService.AuthenticateAsync(apiKey);
                if (success)
                {
                    YotoStatusText.Text = "Connected";
                    YotoStatusText.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Green);
                }
                else
                {
                    YotoStatusText.Text = "Authentication failed";
                    YotoStatusText.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Red);
                }
            }
            catch (Exception ex)
            {
                YotoStatusText.Text = $"Error: {ex.Message}";
                YotoStatusText.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Red);
            }
        }

        #endregion

        #region Chapter Management

        private void CreateChapterButton_Click(object sender, RoutedEventArgs e)
        {
            var title = NewChapterTitleBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                _ = ShowErrorDialog("Title Required", "Please enter a title for the chapter");
                return;
            }

            var chapter = new Chapter
            {
                Title = title,
                Description = NewChapterDescriptionBox.Text?.Trim(),
                Order = _chapters.Count
            };

            _chapters.Add(chapter);

            NewChapterTitleBox.Text = string.Empty;
            NewChapterDescriptionBox.Text = string.Empty;
        }

        private void ChaptersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedChapter = ChaptersList.SelectedItem as Chapter;

            if (_selectedChapter != null)
            {
                SelectedChapterPanel.Visibility = Visibility.Visible;
                ChapterAudioFilesList.ItemsSource = _selectedChapter.AudioFiles;

                // Reset icon preview
                ChapterIconPreviewBorder.Visibility = Visibility.Collapsed;
                ChapterIconPromptBox.Text = string.Empty;
            }
            else
            {
                SelectedChapterPanel.Visibility = Visibility.Collapsed;
                ChapterAudioFilesList.ItemsSource = null;
            }
        }

        private void DeleteChapterButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Chapter chapter)
            {
                _chapters.Remove(chapter);

                // Update order of remaining chapters
                for (int i = 0; i < _chapters.Count; i++)
                {
                    _chapters[i].Order = i;
                }
            }
        }

        private async void GenerateChapterIconButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedChapter == null)
            {
                await ShowErrorDialog("No Chapter Selected", "Please select a chapter first");
                return;
            }

            if (!_chatGptService.IsAuthenticated)
            {
                await ShowErrorDialog("Authentication Required", "Please authenticate with ChatGPT first");
                return;
            }

            var prompt = ChapterIconPromptBox.Text;
            if (string.IsNullOrWhiteSpace(prompt))
            {
                await ShowErrorDialog("Prompt Required", "Please enter a description for the chapter icon");
                return;
            }

            try
            {
                ChapterIconProgressRing.IsActive = true;
                GenerateChapterIconButton.IsEnabled = false;

                var iconData = await _chatGptService.GenerateImageAsync(prompt, 16, 16);
                _selectedChapter.Icon = iconData;

                // Display the generated icon
                await DisplayImage(ChapterIconPreviewImage, ChapterIconPreviewBorder, iconData);

                // Refresh the chapters list to show the updated icon
                var selectedIndex = ChaptersList.SelectedIndex;
                ChaptersList.ItemsSource = null;
                ChaptersList.ItemsSource = _chapters;
                ChaptersList.SelectedIndex = selectedIndex;
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Generation Error", ex.Message);
            }
            finally
            {
                ChapterIconProgressRing.IsActive = false;
                GenerateChapterIconButton.IsEnabled = true;
            }
        }

        private async void AddAudioToChapterButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedChapter == null)
            {
                await ShowErrorDialog("No Chapter Selected", "Please select a chapter first");
                return;
            }

            try
            {
                var files = await _audioFileService.PickAudioFilesAsync();
                foreach (var file in files)
                {
                    file.Order = _selectedChapter.AudioFiles.Count;
                    _selectedChapter.AudioFiles.Add(file);
                }

                // Refresh the audio files list
                ChapterAudioFilesList.ItemsSource = null;
                ChapterAudioFilesList.ItemsSource = _selectedChapter.AudioFiles;

                // Refresh chapters list to update the audio file count
                var selectedIndex = ChaptersList.SelectedIndex;
                ChaptersList.ItemsSource = null;
                ChaptersList.ItemsSource = _chapters;
                ChaptersList.SelectedIndex = selectedIndex;
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Error picking files", ex.Message);
            }
        }

        private void RemoveAudioFromChapterButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedChapter == null)
                return;

            var selectedItems = ChapterAudioFilesList.SelectedItems.Cast<AudioFile>().ToList();
            foreach (var item in selectedItems)
            {
                _selectedChapter.AudioFiles.Remove(item);
            }

            // Update order of remaining files
            for (int i = 0; i < _selectedChapter.AudioFiles.Count; i++)
            {
                _selectedChapter.AudioFiles[i].Order = i;
            }

            // Refresh the audio files list
            ChapterAudioFilesList.ItemsSource = null;
            ChapterAudioFilesList.ItemsSource = _selectedChapter.AudioFiles;

            // Refresh chapters list to update the audio file count
            var selectedIndex = ChaptersList.SelectedIndex;
            ChaptersList.ItemsSource = null;
            ChaptersList.ItemsSource = _chapters;
            ChaptersList.SelectedIndex = selectedIndex;
        }

        #endregion

        #region AI Image Generation

        private async void GenerateIconButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_chatGptService.IsAuthenticated)
            {
                await ShowErrorDialog("Authentication Required", "Please authenticate with ChatGPT first");
                return;
            }

            var prompt = IconPromptBox.Text;
            if (string.IsNullOrWhiteSpace(prompt))
            {
                await ShowErrorDialog("Prompt Required", "Please enter a description for the icon");
                return;
            }

            try
            {
                IconProgressRing.IsActive = true;
                GenerateIconButton.IsEnabled = false;

                _generatedIcon = await _chatGptService.GenerateImageAsync(prompt, 16, 16);

                // Display the generated icon
                await DisplayImage(IconPreviewImage, IconPreviewBorder, _generatedIcon);
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Generation Error", ex.Message);
            }
            finally
            {
                IconProgressRing.IsActive = false;
                GenerateIconButton.IsEnabled = true;
            }
        }

        private async void GenerateCoverButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_chatGptService.IsAuthenticated)
            {
                await ShowErrorDialog("Authentication Required", "Please authenticate with ChatGPT first");
                return;
            }

            var prompt = CoverPromptBox.Text;
            if (string.IsNullOrWhiteSpace(prompt))
            {
                await ShowErrorDialog("Prompt Required", "Please enter a description for the cover");
                return;
            }

            try
            {
                CoverProgressRing.IsActive = true;
                GenerateCoverButton.IsEnabled = false;

                _generatedCover = await _chatGptService.GenerateImageAsync(prompt, 1024, 1024);

                // Display the generated cover
                await DisplayImage(CoverPreviewImage, CoverPreviewBorder, _generatedCover);
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Generation Error", ex.Message);
            }
            finally
            {
                CoverProgressRing.IsActive = false;
                GenerateCoverButton.IsEnabled = true;
            }
        }

        #endregion

        #region Yoto Content Management

        private async void CreateContentButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_yotoApiService.IsAuthenticated)
            {
                await ShowErrorDialog("Authentication Required", "Please authenticate with Yoto API first");
                return;
            }

            if (string.IsNullOrWhiteSpace(ContentTitleBox.Text))
            {
                await ShowErrorDialog("Title Required", "Please enter a title for the content");
                return;
            }

            if (_chapters.Count == 0)
            {
                await ShowErrorDialog("Chapters Required", "Please add at least one chapter");
                return;
            }

            // Validate that all chapters have audio files
            var chaptersWithoutAudio = _chapters.Where(c => c.AudioFiles.Count == 0).ToList();
            if (chaptersWithoutAudio.Any())
            {
                await ShowErrorDialog("Audio Required",
                    $"All chapters must have at least one audio file. Missing audio in: {string.Join(", ", chaptersWithoutAudio.Select(c => c.Title))}");
                return;
            }

            try
            {
                ContentProgressRing.IsActive = true;
                CreateContentButton.IsEnabled = false;
                UpdateContentButton.IsEnabled = false;

                var content = new YotoContent
                {
                    Title = ContentTitleBox.Text,
                    Description = ContentDescriptionBox.Text,
                    Chapters = _chapters.ToList(),
                    Icon = _generatedIcon,
                    CoverImage = _generatedCover
                };

                var contentId = await _yotoApiService.CreateContentAsync(content);

                ContentStatusText.Text = $"Content created successfully! ID: {contentId}";
                ContentStatusText.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Creation Error", ex.Message);
            }
            finally
            {
                ContentProgressRing.IsActive = false;
                CreateContentButton.IsEnabled = true;
                UpdateContentButton.IsEnabled = true;
            }
        }

        private async void UpdateContentButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_yotoApiService.IsAuthenticated)
            {
                await ShowErrorDialog("Authentication Required", "Please authenticate with Yoto API first");
                return;
            }

            // TODO: Implement content update functionality
            await ShowErrorDialog("Not Implemented", "Update functionality will be implemented based on your specific requirements");
        }

        #endregion

        #region Helper Methods

        private async System.Threading.Tasks.Task DisplayImage(Windows.UI.Xaml.Controls.Image imageControl, Border border, byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return;

            using (var stream = new Windows.Storage.Streams.InMemoryRandomAccessStream())
            {
                await stream.WriteAsync(imageData.AsBuffer());
                stream.Seek(0);

                var bitmap = new Windows.UI.Xaml.Media.Imaging.BitmapImage();
                await bitmap.SetSourceAsync(stream);

                imageControl.Source = bitmap;
                border.Visibility = Visibility.Visible;
            }
        }

        private async System.Threading.Tasks.Task ShowErrorDialog(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK"
            };

            await dialog.ShowAsync();
        }

        #endregion
    }
}
