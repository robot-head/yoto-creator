using System.Collections.Generic;

namespace YotoCreator.Models
{
    /// <summary>
    /// Represents a chapter in the Yoto content
    /// </summary>
    public class Chapter
    {
        /// <summary>
        /// Title of the chapter
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Description of the chapter
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 16x16 icon image data for the chapter
        /// </summary>
        public byte[] Icon { get; set; }

        /// <summary>
        /// List of audio files in this chapter
        /// </summary>
        public List<AudioFile> AudioFiles { get; set; }

        /// <summary>
        /// Order in the content (0-based)
        /// </summary>
        public int Order { get; set; }

        public Chapter()
        {
            AudioFiles = new List<AudioFile>();
        }
    }
}
