﻿/*
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

namespace CompanionMR {

    /**
     * This class represents an audio file.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    [System.Serializable]
    public class Audio {

        /**
         * Audio ID.
         */
        public int audioID;

        /**
         * Timestamp to reflect the alteration date of this audio.
         */
        public long timestamp;

        /**
         * The source URL for the actual audio file.
         */
        public string audio;

        /**
         * Audio title.
         */
        public string title;

        /**
         * Local audio path.
         */
        public string localAudioPath;
    }
}
