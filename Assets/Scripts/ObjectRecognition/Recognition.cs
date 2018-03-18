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

using UnityEngine;

namespace CompanionMR {

    /**
     * This struct capsules information about a recognized artwork.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    public struct Recognition {

        /**
         * The actual artwork.
         */
        public Artwork Artwork { get; private set; }

        /**
         * Indicates whether the validation of this artwork coordinates has finished.
         */
        public bool ValidationFinished { get; set; }

        /**
         * World space point of the camera.
         */
        public Vector3 WorldSpaceCamera { get; private set; }

        /**
         * World space point of the middle of the recognized artwork.
         */
        public Vector3 WorldSpaceMiddle { get; private set; }

        /**
         * World space point of the top left corner of the recognized artwork.
         */
        public Vector3 WorldSpaceTopLeft { get; private set; }

        /**
         * World space point of the top right corner of the recognized artwork.
         */
        public Vector3 WorldSpaceTopRight { get; private set; }

        /**
         * World space point of the bottom left corner of the recognized artwork.
         */
        public Vector3 WorldSpaceBottomLeft { get; private set; }

        /**
         * World space point of the bottom right corner of the recognized artwork.
         */
        public Vector3 WorldSpaceBottomRight { get; private set; }

        /**
         * Create a recognition object.
         * 
         * @param artwork               the actual artwork
         * @param worldSpaceCamera      world space point of the camera
         * @param worldSpaceMiddle      world space point of the middle of the recognized artwork
         * @param worldSpaceTopLeft     world space point of the top left corner of the recognized artwork
         * @param worldSpaceTopRight    world space point of the top right corner of the recognized artwork
         * @param worldSpaceBottomLeft  world space point of the bottom left corner of the recognized artwork
         * @param worldSpaceBottomRight world space point of the bottom right corner of the recognized artwork
         */
        public Recognition(Artwork artwork, Vector3 worldSpaceCamera, Vector3 worldSpaceMiddle, Vector3 worldSpaceTopLeft,
                           Vector3 worldSpaceTopRight, Vector3 worldSpaceBottomLeft, Vector3 worldSpaceBottomRight) {
            this.Artwork = artwork;
            this.ValidationFinished = false;
            this.WorldSpaceCamera = worldSpaceCamera;
            this.WorldSpaceMiddle = worldSpaceMiddle;
            this.WorldSpaceTopLeft = worldSpaceTopLeft;
            this.WorldSpaceTopRight = worldSpaceTopRight;
            this.WorldSpaceBottomLeft = worldSpaceBottomLeft;
            this.WorldSpaceBottomRight = worldSpaceBottomRight;
        }
    }
}
