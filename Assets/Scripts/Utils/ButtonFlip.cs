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
     * This class is used to activate or deactivate a button with a vertical flip animation.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(UnityEngine.UI.Button))]
    public class ButtonFlip : MonoBehaviour {

        /**
         * Local vertical rotation value for the active state.
         */
        [Tooltip("Local vertical rotation value for the active state.")]
        [Range(0.0f, 180.0f)]
        public float activeRotation = 0.0f;

        /**
         * Local vertical rotation value for the inactive state.
         */
        [Tooltip("Local vertical rotation value for the inactive state.")]
        [Range(0.0f, 180.0f)]
        public float inactiveRotation = 180.0f;

        /**
         * Duration of the flip animation.
         */
        [Tooltip("Duration of the flip animation.")]
        [Range(0.1f, 1.0f)]
        public float smoothTime = 0.1f;

        /**
         * The RectTransform of this button.
         */
        private RectTransform rect;

        /**
         * Current target rotation.
         */
        private float targetRotation;

        /**
         * Indicates whether interpolating is in progress.
         */
        private bool interpolating;

        /**
         * The exact half of the active and inactive rotations.
         */
        private float halfRotation;

        /**
         * Current rotation velocity.
         */
        private float rotationVelocity;

        /**
         * Use this for initialization.
         */
        private void Start() {
            // Variable initialization
            this.rect = this.gameObject.GetComponent<RectTransform>();
            this.targetRotation = this.inactiveRotation;
            this.interpolating = false;
            this.halfRotation = Mathf.Abs(this.activeRotation - this.inactiveRotation) / 2.0f;
            this.rotationVelocity = 0.0f;
        }

        /**
         * Update is called once per frame.
         */
        private void Update() {
            // Interpolates between the active or inactive rotation
            if (this.interpolating) {
                float newRotation = Mathf.SmoothDamp(this.rect.localEulerAngles.y, this.targetRotation, ref this.rotationVelocity, this.smoothTime);

                // Hide the button after the half rotation by scaling it to a zero scale
                if (((this.targetRotation == this.inactiveRotation) && (newRotation >= this.halfRotation))
                    || ((this.targetRotation == this.activeRotation) && (newRotation <= this.halfRotation))) {
                    this.rect.localScale = (this.targetRotation == this.inactiveRotation) ? new Vector3(0.0f, 0.0f, 0.0f) : new Vector3(1.0f, 1.0f, 1.0f);
                }

                // Clamp the rotation if the gap is infinitesimaly small
                if (Mathf.Abs(this.targetRotation - newRotation) <= 0.1f) {
                    newRotation = this.targetRotation;
                    this.interpolating = false;
                }

                // Set new rotation
                this.rect.localEulerAngles = new Vector3(this.rect.localRotation.x, newRotation, this.rect.localRotation.z);
            }
        }

        /**
         * Activate this button.
         */
        public void Activate() {
            if (!this.interpolating) {
                this.targetRotation = this.activeRotation;
                this.interpolating = true;
            }
        }

        /**
         * Deactivate this button.
         */
        public void Deactivate() {
            if (!this.interpolating) {
                this.targetRotation = this.inactiveRotation;
                this.interpolating = true;
            }
        }

        /**
         * Toggle between activating and deactivating this button.
         */
        public void ToggleRotation() {
            if (!this.interpolating) {
                this.targetRotation = (this.targetRotation == this.inactiveRotation) ? this.activeRotation : this.inactiveRotation;
                this.interpolating = true;
            }
        }
    }
}
