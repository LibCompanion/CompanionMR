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
     * This class controls a canvas that displays informational text to the user.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    public class InfoCanvas : HoloToolkit.Unity.Singleton<InfoCanvas> {

        /**
         * Reference to the main text.
         */
        [Tooltip("Reference to the main text.")]
        public Text mainText;

        /**
         * Reference to the info text.
         */
        [Tooltip("Reference to the info text.")]
        public Text infoText;

        /**
         * Display time of the text.
         */
        [Tooltip("Display time of the text.")]
        [Range(1, 10)]
        public int displayTime = 8;

        /**
         * Indicates whether the info text shuould be updated.
         */
        private bool newInfoText;

        /**
         * Time that has passed after a new text has been displayed.
         */
        private float timeAfterText;

        /**
         * Indicates whether the info text should stay or disappear after <code>displayTime</code>.
         */
        private bool stay;

        /**
         * Use this for initialization.
         */
        private void Start() {
            // Variable initialization
            this.newInfoText = false;
            this.timeAfterText = 0.0f;
            this.stay = false;
        }

        /**
         * Update is called once per frame.
         */
        private void Update() {
            // Delete the info text after the specified time
            if (this.newInfoText && !this.stay) {
                this.timeAfterText += Time.deltaTime;
                if (this.timeAfterText >= this.displayTime) {
                    this.infoText.enabled = false;
                    this.timeAfterText = 0.0f;
                    this.newInfoText = false;
                }
            } else if (this.stay) {
                this.timeAfterText = 0.0f;
                this.newInfoText = false;
            }
        }

        /**
         * Set the main text.
         * 
         * @param text  main text
         */
        public void SetMainText(string text) {
            this.mainText.text = text;
            this.mainText.enabled = true;
        }

        /**
         * Set the info text.
         * 
         * @param text  info text
         */
        public void SetInfoText(string text) {
            this.SetInfoText(text, false);
        }

        /**
         * Set the info text.
         * 
         * @param text  info text
         * @param stay  indicates whether the info text should stay or disappear after a specific time
         */
        public void SetInfoText(string text, bool stay) {
            this.infoText.text = text;
            this.infoText.enabled = true;
            this.newInfoText = true;
            this.timeAfterText = 0.0f;
            this.stay = stay;
        }
    }
}
