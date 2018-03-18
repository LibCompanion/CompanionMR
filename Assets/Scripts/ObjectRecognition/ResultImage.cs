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
     * This struct capsules information about the result image.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    public struct ResultImage {

        /**
         * The actual result image.
         */
        public byte[] Image { get; private set; }

        /**
         * World space point of the camera.
         */
        public Vector3 WorldSpaceCamera { get; private set; }

        /**
         * World space point of the middle of the result image.
         */
        public Vector3 WorldSpaceMiddle { get; private set; }

        /**
         * World space point of the top left corner of the result image.
         */
        public Vector3 WorldSpaceTopLeft { get; private set; }

        /**
         * World space point of the top right corner of the result image
         */
        public Vector3 WorldSpaceTopRight { get; private set; }

        /**
         * World space point of the bottom left corner of the result image.
         */
        public Vector3 WorldSpaceBottomLeft { get; private set; }

        /**
         * Create a result image object.
         * 
         * @param image                 the actual result image
         * @param worldSpaceCamera      world space point of the camera
         * @param worldSpaceMiddle      world space point of the middle of the recognized artwork
         * @param worldSpaceTopLeft     world space point of the top left corner of the recognized artwork
         * @param worldSpaceTopRight    world space point of the top right corner of the recognized artwork
         * @param worldSpaceBottomLeft  world space point of the bottom left corner of the recognized artwork
         */
        public ResultImage(byte[] image, Vector3 worldSpaceCamera, Vector3 worldSpaceMiddle, Vector3 worldSpaceTopLeft,
                           Vector3 worldSpaceTopRight, Vector3 worldSpaceBottomLeft) {
            this.Image = new byte[image.Length];
            image.CopyTo(this.Image, 0);
            this.WorldSpaceCamera = worldSpaceCamera;
            this.WorldSpaceMiddle = worldSpaceMiddle;
            this.WorldSpaceTopLeft = worldSpaceTopLeft;
            this.WorldSpaceTopRight = worldSpaceTopRight;
            this.WorldSpaceBottomLeft = worldSpaceBottomLeft;
        }

        /**
         * Display the result image in the scene.
         * 
         * @param canvas            the canvas where the image is displayed
         * @param cameraResolution  the camera resolution
         */
        public void DisplayImage(GameObject canvas, Resolution cameraResolution) {
            // Load image data into the texture
            Texture2D texture = new Texture2D(cameraResolution.width, cameraResolution.height, TextureFormat.RGB24, false) {
                wrapMode = TextureWrapMode.Clamp
            };
            texture.LoadRawTextureData(this.Image);
            texture.Apply();

            // Create a Sprite
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            Rect tRect = new Rect(0, 0, cameraResolution.width, cameraResolution.height);
            UnityEngine.UI.Image imageScript = canvas.GetComponent<UnityEngine.UI.Image>();
            imageScript.overrideSprite = Sprite.Create(texture, tRect, pivot);

            // Resize the canvas rect
            canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(
                Vector3.Distance(this.WorldSpaceTopLeft, this.WorldSpaceTopRight),
                Vector3.Distance(this.WorldSpaceTopLeft, this.WorldSpaceBottomLeft)
            );
        }
    }
}
