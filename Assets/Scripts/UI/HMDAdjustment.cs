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
using HoloToolkit.Unity.InputModule;

namespace CompanionMR {

    /**
     * This class controls the HMD adjustment procedure.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    public class HMDAdjustment : HoloToolkit.Unity.Singleton<HMDAdjustment>, IInputClickHandler {

        /**
         * Reference to the HMD adjustment canvas.
         */
        [Tooltip("Reference to the HMD adjustment canvas.")]
        public GameObject HMDAdjustmentCanvas;

        /**
         * Indicates whether the HMD adjustment has been finished or not. 
         */
        public bool HMDAdjusted { get; private set; }

        /**
         * Use this for initialization.
         */
        private void Start() {
            if (InputManager.IsInitialized) {
                InputManager.Instance.AddGlobalListener(this.gameObject);
            }
        }

        /**
         * This method is called when the user has performed an air-tap gesture.
         * 
         * @param eventData     input click event data
         */
        public void OnInputClicked(InputClickedEventData eventData) {
            if (!this.HMDAdjusted) {
                // Stopp receiving events
                if (InputManager.IsInitialized) {
                    InputManager.Instance.RemoveGlobalListener(this.gameObject);
                }

                // Destroy the HMD adjustment canvas
                if (this.HMDAdjustmentCanvas != null) {
                    Destroy(this.HMDAdjustmentCanvas);
                }

                // Mark the finished adjustment
                this.HMDAdjusted = true;
            }
        }
    }
}
