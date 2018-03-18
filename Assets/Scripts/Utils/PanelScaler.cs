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
     * This class smoothly scales a panel.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    [RequireComponent(typeof(RectTransform))]
    public class PanelScaler : MonoBehaviour {

        /**
         * Minimum scale.
         */
        [Tooltip("Minimum scale.")]
        [Range(0.0f, 1.0f)]
        public float minScale = 0.0f;

        /**
         * Maximum scale.
         */
        [Tooltip("Maximum scale.")]
        [Range(0.0f, 1.0f)]
        public float maxScale = 1.0f;

        /**
         * Amount of time the scaling will take.
         */
        [Tooltip("Amount of time the scaling will take.")]
        [Range(0.0f, 1.0f)]
        public float smoothTime = 0.1f;

        /**
         * RectTransform of this panel.
         */
        private RectTransform rect;

        /**
         * Current target scale.
         */
        private float targetScale;

        /**
         * Indicates whether interpolating is in progress. 
         */
        private bool interpolating;

        /**
         * Scaling velocity.
         */
        private float scalingVelocity;

        /**
         * Use this for initialization.
         */
        private void Start() {
            // Variable initialization
            this.rect = this.gameObject.GetComponent<RectTransform>();
            this.targetScale = this.maxScale;
            this.interpolating = false;
            this.scalingVelocity = 0.0f;
        }

        /**
         * Update is called once per frame.
         */
        private void Update() {
            // Interpolates between current and target scale
            if (this.interpolating) {

                // Smoothly interpolate the scaling value of this panel
                float newScale = Mathf.SmoothDamp(this.rect.localScale.x, this.targetScale, ref this.scalingVelocity, this.smoothTime);

                // Clamp the scale if the gap is infinitesimaly small
                if (Mathf.Abs(this.targetScale - newScale) <= 0.1f) {
                    newScale = this.targetScale;
                    this.interpolating = false;
                }

                // Set new scale
                this.rect.localScale = new Vector3(newScale, newScale, newScale);
            }
        }

        /**
         * Scale this panel to the minimum scale.
         */
        public void Minimize() {
            this.targetScale = this.minScale;
            this.interpolating = true;
        }

        /**
         * Scale this panel to the maximum scale.
         */
        public void Maximize() {
            this.targetScale = this.maxScale;
            this.interpolating = true;
        }

        /**
         * Toggle between minimum and maximum scale.
         */
        public void ToggleScale() {
            this.targetScale = (this.targetScale == this.maxScale) ? this.minScale : this.maxScale;
            this.interpolating = true;
        }
    }
}
