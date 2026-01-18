using System;

namespace YotoCreator.Models
{
    /// <summary>
    /// Represents an audio file in the soundtrack
    /// </summary>
    public class AudioFile
    {
        /// <summary>
        /// Name of the audio file
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Full path to the audio file
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Duration of the audio file
        /// </summary>
        public string Duration { get; set; }

        /// <summary>
        /// File size in bytes
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Order in the soundtrack (0-based)
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Storage token for the file
        /// </summary>
        public string Token { get; set; }
    }
}
