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
     * This class fades in and out arrows that indicate which direction the player should be facing.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    public class UIArrows : HoloToolkit.Unity.Singleton<UIArrows> {

        /**
         * Indicates how long it takes for the arrows to appear and disappear.
         */
        [Tooltip("Indicates how long it takes for the arrows to appear and disappear.")]
        [Range(0.5f, 3.0f)]
        public float fadeDuration = 1.0f;

        /**
         * The maximum angle between the desired direction and the player's current facing direction in which arrows are shown.
         */
        [Tooltip("The maximum angle between the desired direction and the player's current facing direction in which arrows are shown.")]
        [Range(5.0f, 50.0f)]
        public float showAngle = 30.0f;

        /**
         * Reference to the target transform that indicates which direction the player should be facing.
         */
        [Tooltip("Reference to the target transform that indicates which direction the player should be facing.")]
        public Transform targetTransform;

        /**
         * Reference to the camera to determine which way the player is facing.
         */
        [Tooltip("Reference to the camera to determine which way the player is facing.")]
        public Transform cameraTransform;

        /**
         * Reference to the renderers of the arrows used to fade them in and out.
         */
        [Tooltip("Reference to the renderers of the arrows used to fade them in and out.")]
        public Renderer[] arrowRenderers;

        /**
         * The name of the color property of the shader that is used to fade the arrows.
         */
        private const string MATERIAL_PROPERTY_NAME = "_Color";

        /**
         * The angle delta that indicates when to start lerping the up vector.
         */
        private const float LERP_ANGLE_DELTA = 10.0f;

        /**
         * The current alpha value of the arrows.
         */
        private float currentAlpha;

        /**
         *  Fading speed.
         */
        private float fadeSpeed;

        /**
         * Use this for initialization.
         */
        private void Start() {
            // Speed is distance (from zero alpha to one alpha) divided by time (duration).
            this.fadeSpeed = 1.0f / this.fadeDuration;
        }

        /**
         * Update is called once per frame.
         */
        private void Update() {
            if (this.targetTransform != null) {

                // Move the arrows along with the camera (i.e. players's head)
                this.transform.position = this.cameraTransform.position;

                // The forward vector of the camera
                Vector3 camForward = this.cameraTransform.forward;

                // The vector in which the player should be facing is the forward direction of the transform specified
                Vector3 desiredDirection = this.targetTransform.position - this.transform.position;

                // The difference angle between the desired facing and the current facing of the player
                float angleDelta = Vector3.Angle(desiredDirection, camForward);

                // Lerp the Up vector of the look-rotation to smooth the arrow animation around the target and the opposite direction
                Vector3 cameraUp = this.cameraTransform.up;
                Vector3 cameraDown = (-1.0f) * cameraUp;
                Vector3 desiredUp = (Vector3.Dot(this.transform.up, cameraUp) > Vector3.Dot(this.transform.up, cameraDown)) ? cameraUp : cameraDown;
                Vector3 up = Vector3.Cross(desiredDirection, camForward);
                if (angleDelta <= LERP_ANGLE_DELTA) {
                    up = Vector3.Lerp(up, desiredUp, 1.0f - (angleDelta / LERP_ANGLE_DELTA));
                }

                // Rotate the arrows torwards the target transform
                this.transform.rotation = Quaternion.LookRotation(desiredDirection, up);

                // If the difference is greater than the angle at which the arrows are shown, their target alpha is one otherwise it is zero
                // ...
                float targetAlpha = (angleDelta <= this.showAngle) ? (angleDelta / this.showAngle) : 1.0f;

                // Increment the current alpha value towards the now chosen target alpha and the calculated speed
                this.currentAlpha = Mathf.MoveTowards(this.currentAlpha, targetAlpha, this.fadeSpeed * Time.deltaTime);
                this.currentAlpha = (angleDelta <= this.showAngle) ? (angleDelta / this.showAngle) : 1.0f;

                // Deactive the arrows if we are facing the target
                if ((angleDelta <= this.showAngle) && (this.currentAlpha <= 0.1f)) {
                    this.targetTransform = null;
                }

                // Set new alpha value for the arrows
                this.SetAlpha(this.currentAlpha);

            } else {
                // Disable arrows
                if (this.currentAlpha != 0.0f) {
                    this.currentAlpha = 0.0f;
                    this.SetAlpha(this.currentAlpha);
                }
            }
        }

        /**
         * Set the arrow material to the given alpha value.
         * 
         * @alpha   target alpha value
         */
        private void SetAlpha(float alpha) {
            // Go through all the arrow renderers and set the given property of their material to the current alpha
            for (int i = 0; i < this.arrowRenderers.Length; i++) {
                Color color = this.arrowRenderers[i].material.GetColor(MATERIAL_PROPERTY_NAME);
                this.arrowRenderers[i].material.SetColor(MATERIAL_PROPERTY_NAME, new Color(color.r, color.g, color.b, alpha));
            }
        }

        /**
         * Turn the arrows on or off.
         * 
         * @param enable    <code>true</code> enables the arrows; <code>false</code> disables them
         */
        public void Enable(bool enable) {
            this.gameObject.SetActive(enable);
        }
    }
}
