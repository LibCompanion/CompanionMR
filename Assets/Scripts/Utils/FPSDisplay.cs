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
     * Simple framerate (FPS) display script.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    public class FPSDisplay : MonoBehaviour {

        /**
         * Reference to the text script that will show the framerate counter.
         */
        [Tooltip("Reference to the text script that will show the framerate counter.")]
        public Text text;

        /**
         * Buffer for framerate calculation.
         */
        private int[] buffer;

        /**
         * Buffer index.
         */
        private int bufferIndex;

        /**
         * Buffer size.
         */
        private static readonly int BUFFER_SIZE = 60;

        /**
         * Use this for initialization.
         */
        private void Start() {

            // Variable initialization
            this.buffer = new int[BUFFER_SIZE];
            this.bufferIndex = 0;

            // Deactivate this script if necessary components are missing
            if (this.text == null) {
                Debug.LogError("FPSDisplay: Script references not set properly.");
                this.enabled = false;
                return;
            }
        }

        /**
         * Update is called once per frame.
         */
        private void Update() {

            // Update buffer
            this.buffer[this.bufferIndex++] = (int) Mathf.Round(1.0f / Time.unscaledDeltaTime);
            this.bufferIndex %= BUFFER_SIZE;

            // Calculate FPS
            int sum = 0;
            for (int i = 0; i < this.buffer.Length; i++) {
                sum += this.buffer[i];
            }

            // Update text
            int fps = Mathf.Clamp(sum / BUFFER_SIZE, 0, 99);
            string str = (fps < 10) ? ("0" + fps.ToString()) : fps.ToString();
            this.text.text = "FPS: " + str;
        }
    }
}
