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
     * This class controls the display of a 3D asset.
     *
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    public class Display3DAsset : MonoBehaviour {

        /**
         * A reference to the info board.
         */
        [Tooltip("A reference to the info board.")]
        public InfoBoard infoboard;

        /**
         * Background image of the 3D button.
         */
        private Image backgroundImage;

        /**
         * Colors of the 3D button.
         */
        private ColorBlock buttonColors;

        /**
         * Indicates whether the 3D asset is active or not.
         */
        private bool active = false;

        /**
         * Use this for initialization.
         */
        private void Start() {
            // Variable initialization (sometimes 'ActivateVisuals' is called first)
            this.backgroundImage = this.gameObject.GetComponent<Image>();
            this.buttonColors = this.GetComponent<Button>().colors;
        }

        /**
         * Toggle between activating and deactivating the 3D asset.
         */
        public void ToggleActivate() {
            if (this.active) {
                this.backgroundImage.color = new Color32(255, 255, 255, 135);
                this.buttonColors.highlightedColor = new Color32(197, 197, 197, 255);
                this.infoboard.Deactivate3D();
                this.active = false;
            } else {
                this.backgroundImage.color = new Color32(0, 255, 0, 135);
                this.buttonColors.highlightedColor = new Color32(0, 197, 0, 255);
                this.infoboard.Activate3D();
                this.active = true;
            }
        }

        /**
         * This method is called from the info board to sync the button visuals if the asset is already visible.
         */
        public void ActivateVisuals() {
            // Set variables if this is called before Start()
            if ((this.backgroundImage == null) || (this.buttonColors.Equals(ColorBlock.defaultColorBlock))) {
                this.backgroundImage = this.gameObject.GetComponent<Image>();
                this.buttonColors = this.GetComponent<Button>().colors;
            }
            this.backgroundImage.color = new Color32(0, 255, 0, 135);
            this.buttonColors.highlightedColor = new Color32(0, 197, 0, 255);
            this.active = true;
        }
    }
}
