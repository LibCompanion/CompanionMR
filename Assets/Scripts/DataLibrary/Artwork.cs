/*
 * CompanionMR is a Windows Mixed Reality example project for Companion.
 * Copyright (C) 2018 Dimitri Kotlovky, Andreas Sekulski
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;

namespace CompanionMR {

    /**
     * This class represents an artwork.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    [System.Serializable]
    public class Artwork {

        /**
         * Artwork ID.
         */
        public int artworkID;

        /**
         * Timestamp to reflect the alteration date of this artwork.
         */
        public long timestamp;

        /**
         * The source URL for the artwork image file.
         */
        public string image;

        /**
         * Artwork info title.
         */
        public string title;

        /**
         * Artist ID.
         */
        public int artistID;

        /**
         * Year of creation.
         */
        public string year;

        /**
         * Artstyle.
         */
        public string artStyle;

        /**
         * Detailed information.
         */
        public string detailedInfo;

        /**
         * List of video IDs for this artwork.
         */
        public List<int> videos;

        /**
         * List of audio IDs for this artwork.
         */
        public List<int> audio;

        /**
         * Reference to the artist object.
         */
        public Artist artist;

        /**
         * List of references to the video objects.
         */
        public List<Video> videoData;

        /**
         * List of references to the audio objects.
         */
        public List<Audio> audioData;

        /**
         * Local image path.
         */
        public string localImagePath;
    }
}
