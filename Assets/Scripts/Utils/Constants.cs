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

namespace CompanionMR {

    /**
     * This class provides constants for Unity tags.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    public static class Tags {

        /**
         * Tag for spatial mapping meshes.
         */
        public const string TAG_SPATIAL_MAPPING = "SpatialMapping";

        /**
         * Tag for the Napoleon asset.
         */
        public const string TAG_NAPOLEON = "Napoleon";

        /**
         * Tag for scanning areas.
         */
        public const string TAG_SCANNING_AREA = "ScanningArea";
    }

    /**
     * This class provides constants for Unity triggers.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    public static class Triggers {

        /**
         * Trigger for clicked events.
         */
        public const string TRIGGER_CLICKED = "clicked";

        /**
         * Trigger for the Napoleon esteregg.
         */
        public const string TRIGGER_EASTEREGG = "easteregg";
    }
}
