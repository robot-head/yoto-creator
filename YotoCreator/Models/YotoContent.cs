using System;
using System.Collections.Generic;

namespace YotoCreator.Models
{
    /// <summary>
    /// Represents content for the Yoto Player
    /// </summary>
    public class YotoContent
    {
        /// <summary>
        /// Unique identifier for the content
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Title of the content
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Description of the content
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// List of audio files in the content
        /// </summary>
        public List<AudioFile> AudioFiles { get; set; }

        /// <summary>
        /// List of chapters in the content
        /// </summary>
        public List<Chapter> Chapters { get; set; }

        /// <summary>
        /// 16x16 icon image data
        /// </summary>
        public byte[] Icon { get; set; }

        /// <summary>
        /// Cover image data
        /// </summary>
        public byte[] CoverImage { get; set; }

        /// <summary>
        /// Date when the content was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date when the content was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        public YotoContent()
        {
            AudioFiles = new List<AudioFile>();
            Chapters = new List<Chapter>();
        }
    }
}
