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
     * This class controls a sphere shaped area where object recognition is enabled.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    public class ScanningArea : MonoBehaviour {

        /**
         * Collider of this recognition area.
         */
        [Tooltip("Collider of this recognition area.")]
        public SphereCollider sphereCollider;

        /**
         * Mesh renderer of the anchor.
         */
        [Tooltip("Mesh renderer of the anchor.")]
        public MeshRenderer anchorRenderer;

        /**
         * Mesh renderer of the sphere.
         */
        [Tooltip("Mesh renderer of the sphere.")]
        public MeshRenderer sphereRenderer;

        /**
         * The ID of the artwork which this anchor is connected to.
         */
        [Tooltip("The ID of the artwork which this anchor is connected to.")]
        public int artworkID = -1;

        /**
         * Use this for initialization.
         */
        private void Start() {
#if COMP_DEBUG_ANCHOR
            // Visualize the anchors
            this.anchorRenderer.enabled = true;
#endif
#if COMP_DEBUG_AREA
            // Visualize the anchor spheres
            this.sphereRenderer.enabled = true;
#endif
        }

        /**
         * Activate the anchor sphere collider.
         */
        public void ActivateCollider() {
            if (this.sphereCollider != null) {
                this.sphereCollider.enabled = true;
            }
        }
    }
}
