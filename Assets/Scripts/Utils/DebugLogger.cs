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
using UnityEngine.UI;

namespace CompanionMR {

    /**
     * This class represents a debug logger.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    public class DebugLogger : HoloToolkit.Unity.Singleton<DebugLogger> {

        /**
         * Reference to the caption script.
         */
        [Tooltip("Reference to the caption script.")]
        public Text caption;

        /**
         * Reference to the text script.
         */
        [Tooltip("Reference to the text script.")]
        public Text text;

        /**
         * Reference to the FPS text.
         */
        [Tooltip("Reference to the FPS text.")]
        public Text fps;

        /**
         * Indicates whether the debug log should be visible.
         */
        [Tooltip("Indicates whether the debug log should be visible.")]
        public bool isDebugLogVisible = false;

        /**
         * Indicates whether the FPS counter should be visible.
         */
        [Tooltip("Indicates whether the FPS counter should be visible.")]
        public bool isFPSVisible = false;

        /**
         * Indicates whether the debug log was visible in the last frame.
         */
        private bool wasDebugLogVisible;

        /**
         * Indicates whether the FPS counter was visible in the last frame.
         */
        private bool wasFPSVisible;
        
        /**
         * The debug output.
         */
        private string output;

        /**
         * Indicates whether there is new text for ouptut.
         */
        private bool newText;

        /**
         * Use this for initialization.
         */
        private void Start() {
            // Variable initialization
#if COMP_DEBUG_LOG
            this.isDebugLogVisible = true;
            this.isFPSVisible = true;
#endif
            this.wasDebugLogVisible = this.isDebugLogVisible;
            this.wasFPSVisible = this.isFPSVisible;
            this.output = "";
            this.newText = false;

            // Set initial visbibility
            this.AdjustVisibility();
        }

        /**
         * Update is called once per frame.
         */
        private void Update() {

            // Output debug text
            if (this.newText && (this.text != null)) {
                Debug.Log(this.output);
                this.text.text += this.output;
                this.output = "";
                this.newText = false;
            }

            // Adjust visibility
            if ((this.isDebugLogVisible != this.wasDebugLogVisible) || (this.isFPSVisible != this.wasFPSVisible)) {
                this.AdjustVisibility();
                this.wasDebugLogVisible = this.isDebugLogVisible;
                this.wasFPSVisible = this.isFPSVisible;
            }
        }

        /**
         * Adjust the visibility of the debug log.
         */
        private void AdjustVisibility() {
            this.caption.enabled = this.isDebugLogVisible;
            this.text.enabled = this.isDebugLogVisible;
            this.fps.enabled = this.isFPSVisible;
        }

        /**
         * Add text to the debug log.
         * 
         * @param text  new text for the debug log
         */
        public void Log(string text) {
            this.output += "\r\n" + text;
            this.newText = true;
        }

        /**
         * Indicate whether the debug log should be visible.
         * 
         * @param isVisible indicates whether the debug log should be visible
         */
        public void SetIsVisible(bool isVisible) {
            this.isDebugLogVisible = isVisible;
            this.isFPSVisible = isVisible;
        }

        /**
         * Indicate whether the FPS counter should be visible.
         * 
         * @param isVisible indicates whether the FPS counter should be visible
         */
        public void IsFPSVisible(bool isVisible) {
            this.isFPSVisible = isVisible;
        }

        /**
         * Clear the debug log.
         */
        public void ClearDebug() {
            this.text.text = "";
        }
    }
}
