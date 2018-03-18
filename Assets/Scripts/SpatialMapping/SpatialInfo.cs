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
using HoloToolkit.Unity;
using HoloToolkit.Examples.InteractiveElements;

namespace CompanionMR {

    /**
     * This class displays text information during spatial mapping.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    public class SpatialInfo : MonoBehaviour, HoloToolkit.Unity.InputModule.IInputClickHandler {

        /**
         * Spatial mapping validation states.
         */
        public enum State {
            Undefined,
            AfterFinalizing,
            AfterTopology
        }

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
         * Reference to the main menu.
         */
        [Tooltip("Reference to the main menu.")]
        public GameObject mainMenu;

        /**
         * Reference to the Cached Mapping button.
         */
        [Tooltip("Reference to the Cached Mapping button.")]
        public Interactive cachedMappingButton;

        /**
         * Reference to the Remap button.
         */
        [Tooltip("Reference to the Remap button.")]
        public Interactive remapButton;

        /**
         * Reference to the Live Mapping button.
         */
        [Tooltip("Reference to the Live Mapping button.")]
        public Interactive liveMappingButton;

        /**
         * Reference to the Save Mesh button.
         */
        [Tooltip("Reference to the Save Mesh button.")]
        public Interactive saveMeshButton;

        /**
         * Reference to the SpatialUnderstanding instance.
         */
        private SpatialUnderstanding spatialUnderstanding;

        /**
         * Current state.
         */
        private State state;

        /**
         * Use this for initialization.
         */
        private void Start() {

            // Variable initialization
            this.state = State.Undefined;

            // Check references
            if ((this.mainText == null) || (this.infoText == null) || (this.mainMenu == null) || (this.cachedMappingButton == null) ||
                (this.remapButton == null) && (this.liveMappingButton == null) || (this.saveMeshButton == null)) {
                Debug.Log("SpatialInfo: Script references not set properly.");
                this.enabled = false;
            }

#if UNITY_EDITOR
            this.enabled = false;
#else
            // Attach this GameObject as a listener for input click events
            if (HoloToolkit.Unity.InputModule.InputManager.IsInitialized) {
                HoloToolkit.Unity.InputModule.InputManager.Instance.AddGlobalListener(this.gameObject);
            }

            // Request reference to the SpatialUnderstanding instance
            this.spatialUnderstanding = SpatialUnderstanding.Instance;
            if (this.spatialUnderstanding == null) {
                Destroy(this.gameObject);
            } else {
                // Attach this Component as a listener for SpatialUnderstanding state changes
                this.spatialUnderstanding.ScanStateChanged += this.OnScanStateChanged;
            }
#endif
        }

        /**
         * This method is called when this object will be destroyed.
         */
        private void OnDestroy() {
#if !UNITY_EDITOR
            if (HoloToolkit.Unity.InputModule.InputManager.IsInitialized) {
                HoloToolkit.Unity.InputModule.InputManager.Instance.RemoveGlobalListener(this.gameObject);
            }
            if (this.spatialUnderstanding != null) {
                this.spatialUnderstanding.ScanStateChanged -= this.OnScanStateChanged;
            }
#endif
        }

        /**
         * This method is called when the SpatialUnderstanding state has changed.
         */
        private void OnScanStateChanged() {
            // Analyze the scan state and display the according message to the user
            if ((this.spatialUnderstanding != null) && this.spatialUnderstanding.AllowSpatialUnderstanding) {
                switch (this.spatialUnderstanding.ScanState) {
                    case SpatialUnderstanding.ScanStates.Scanning:
                        this.infoText.text = "When ready, <i>air-tap</i> to finalize the scan.";
                        this.infoText.enabled = true;
                        break;
                    case SpatialUnderstanding.ScanStates.Finishing:
                        this.infoText.enabled = false;
                        this.mainText.text = "Finalizing scan (please wait).";
                        this.mainText.enabled = true;
                        break;
                    case SpatialUnderstanding.ScanStates.Done:
                        this.state = State.AfterFinalizing;
                        this.mainText.enabled = false;
                        this.infoText.text = "<i>Air-tap</i> to query the topology.";
                        this.infoText.enabled = true;
                        break;
                }
            }
        }

        /**
         * Receive input click events.
         * 
         * @param eventData input click event data
         */
        public void OnInputClicked(HoloToolkit.Unity.InputModule.InputClickedEventData eventData) {
            if ((this.spatialUnderstanding != null) && (this.spatialUnderstanding.ScanState == SpatialUnderstanding.ScanStates.Scanning) && !this.spatialUnderstanding.ScanStatsReportStillWorking) {
                this.spatialUnderstanding.RequestFinishScan();
            } else if ((this.spatialUnderstanding != null) && (this.spatialUnderstanding.ScanState == SpatialUnderstanding.ScanStates.Done) && (this.state == State.AfterFinalizing)) {
                this.state = State.AfterTopology;
                if (ShapeDetection.Instance != null) { ShapeDetection.Instance.FindAllTables(); }
                this.infoText.text = "Examine the augmented space. <i>Air-tap</i> to show the menu.";
            } else if ((this.spatialUnderstanding != null) && (this.spatialUnderstanding.ScanState == SpatialUnderstanding.ScanStates.Done) && (this.state == State.AfterTopology)) {
                this.state = State.Undefined;
                this.infoText.enabled = false;
                this.cachedMappingButton.gameObject.SetActive(false);
                this.liveMappingButton.gameObject.SetActive(false);
                this.remapButton.gameObject.SetActive(true);
                this.saveMeshButton.gameObject.SetActive(true);
                this.mainMenu.SetActive(true);
            }
        }

        /**
         * Destroy this GameObject.
         */
        public void DestroyGameObject() {
            Destroy(this.gameObject);
        }
    }
}
