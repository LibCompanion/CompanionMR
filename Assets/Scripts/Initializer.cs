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
     * This class initializes this application.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    public class Initializer : HoloToolkit.Unity.Singleton<Initializer> {

        /**
         * Initialization canvas text.
         */
        [Tooltip("Initialization canvas text.")]
        public UnityEngine.UI.Text text;

        /**
         * Referece to the cursor.
         */
        [Tooltip("Referece to the cursor.")]
        public GameObject cursor;

        /**
         * Referece to the main menu.
         */
        [Tooltip("Referece to the main menu.")]
        public GameObject mainMenu;

        /**
         * Indicates whether it is safe to start the object recognition.
         */
        public bool ReadyToStartRecognition { get; private set; }

        /**
         * Reference to the data loader.
         */
        private DataLoader dataLoader;

        /**
         * Reference to the manual IP config utility.
         */
        private IPConfig config;

        /**
         * Reference to the object recognition.
         */
        private ObjectRecognition objectRecognition;

        /**
         * Reference to the info canvas.
         */
        private InfoCanvas infoCanvas;

        /**
         * Indicates whether the initialization process is finished.
         */
        private bool initFinished;

        /**
         * Indicates whether the data loading is finished.
         */
        private bool dataLoadingFinished;

        /**
         * Indicates whether the object recognition has been configured.
         */
        private bool objectRecognitionConfigured;

        /**
         * Indicates whether the main menu should be displayed.
         */
        private bool openMenu;

        /**
         * Use this for initialization.
         */
        private void Start() {

            // Variable initialization
            this.ReadyToStartRecognition = false;
            this.initFinished = false;
            this.dataLoadingFinished = false;
            this.objectRecognitionConfigured = false;
            this.openMenu = false;

            // Get reference to other scripts in the scene
            this.dataLoader = DataLoader.Instance;
            this.config = IPConfig.Instance;
            this.objectRecognition = ObjectRecognition.Instance;
            this.infoCanvas = InfoCanvas.Instance;

            // Check references
            if ((this.dataLoader == null) || (this.config == null) || (this.objectRecognition == null) || (this.text == null) ||
                (this.cursor == null) || (this.mainMenu == null) || (this.infoCanvas == null)) {
                Debug.Log("Initializer: Script references not set properly.");
                this.enabled = false;
            }
        }

        /**
         * Update is called once per frame.
         */
        private void Update() {

            // Initialize modules
            if (!this.initFinished) {
                
                // Wait until the user has properly adjusted the HMD
                if (!this.ReadyToStartRecognition && HMDAdjustment.IsInitialized && HMDAdjustment.Instance.HMDAdjusted) {
                    Destroy(HMDAdjustment.Instance.gameObject);
                    this.text.enabled = true;
                    this.ReadyToStartRecognition = true;

                    // Activate the cursor
                    Utils.ResetLayerRecursively(this.cursor);
                }

                // Check data loading status
                if (this.ReadyToStartRecognition && !this.dataLoadingFinished) {
                    if (!this.dataLoader.DataLoadingFinished && !this.config.gameObject.activeInHierarchy && this.dataLoader.NeedsManualConfiguration) {
                        this.text.enabled = false;
                        this.config.gameObject.SetActive(true);
                    } else if (this.config.Connected || this.dataLoader.DataLoadingFinished) {
                        Destroy(this.config.gameObject);
                        this.text.enabled = true;
                        this.dataLoadingFinished = true;
                    }
                }

                // Start object recognition
                if (this.dataLoadingFinished && !this.objectRecognitionConfigured) {
                    this.objectRecognitionConfigured = this.objectRecognition.ObjectRecognitionConfigured;
                    this.openMenu = this.objectRecognition.CurrentScanState != ObjectRecognition.ScanState.UserMode;
                }

                // Finsish initialization
                if (this.objectRecognitionConfigured) {
                    this.initFinished = true;
                }

            } else {

                // Activate main menu if there is no cached room
                this.mainMenu.gameObject.SetActive(this.openMenu);

                // Show user instruction
                if (this.objectRecognition.CurrentScanState == ObjectRecognition.ScanState.UserMode) {
                    this.infoCanvas.SetInfoText("Please focus any artwork to display augmented information.");
                }

                // Destory this gameobject
                Destroy(this.gameObject);
            }

        }
    }
}
