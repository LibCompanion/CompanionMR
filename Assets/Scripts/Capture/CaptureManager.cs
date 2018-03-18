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
     * This class manages the photo and video capturing with the HoloLens hardware.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    [RequireComponent(typeof(PhotoCapture))]
    [RequireComponent(typeof(VideoCapture))]
    public class CaptureManager : HoloToolkit.Unity.Singleton<CaptureManager> {

        /**
         * Capture modes.
         */
        public enum CaptureMode {
            None,
            PhotoMode,
            VideoMode
        }

        /**
         * Indicates which capture mode should be used.
         */
        [Tooltip("Indicates which capture mode should be used.")]
        public CaptureMode captureMode = CaptureMode.PhotoMode;

        /**
         * Indicates which capture mode is currently used.
         */
        public CaptureMode CurrentMode { get; private set; }

        /**
         * Reference to the photo capture.
         */
        public PhotoCapture PhotoCapture { get; private set; }

        /**
         * Reference to the video capture.
         */
        public VideoCapture VideoCapture { get; private set; }

        /**
         * Reference to the debug logger.
         */
        private DebugLogger logger;

        /**
         * Indicates which capture mode is going to be used next.
         */
        private CaptureMode NextMode { get; set; }

        /**
         * Action that should be invoked after disposing the photo capture.
         */
        private UnityEngine.Events.UnityAction disposeAction;

        /**
         * Indicates whether the modes are currently switching or not.
         */
        private bool isSwitching;

        /**
         * Use this for initialization that has to be done before the Start() method of other GameObjects is invoked.
         */
        protected override void Awake() {
            base.Awake();

            // Get reference to other scripts in the scene
            this.PhotoCapture = this.gameObject.GetComponent<PhotoCapture>();
            this.VideoCapture = this.gameObject.GetComponent<VideoCapture>();
        }

        /**
         * Use this for initialization.
         */
        private void Start() {

            // Variable initialization
            this.CurrentMode = CaptureMode.None;
            this.NextMode = CaptureMode.None;
            this.isSwitching = false;
            this.disposeAction = null;

            // Get reference to other scripts in the scene
            this.logger = DebugLogger.Instance;

            // Deactivate this script if necessary components are missing
            if ((this.logger == null) || (this.PhotoCapture == null) || (this.VideoCapture == null)) {
                Debug.LogError("CaptureManager: Script references not set properly.");
                this.enabled = false;
                return;
            }

        }

        /**
         * Update is called once per frame.
         */
        private void Update() {

            // Switching between modes
            if (this.isSwitching) {

                // Wait until the switching process has finished
                switch (this.NextMode) {
                    case CaptureMode.None:
                        this.isSwitching = this.PhotoCapture.CaptureStarted || this.VideoCapture.CaptureStarted;
                        break;
                    case CaptureMode.PhotoMode:
                        if (!this.VideoCapture.CaptureStarted) {
                            this.PhotoCapture.ShouldCapture = true;
                        }
                        this.isSwitching = !this.PhotoCapture.CaptureStarted;
                        break;
                    case CaptureMode.VideoMode:
                        if (!this.PhotoCapture.CaptureStarted) {
                            this.VideoCapture.ShouldCapture = true;
                        }
                        this.isSwitching = !this.VideoCapture.CaptureStarted;
                        break;
                }

            } else {

                // Deactivate all modes if disposing was initialized
                if (this.disposeAction != null) {
                    this.captureMode = CaptureMode.None;
                    this.NextMode = CaptureMode.None;
                }

                // Switch modes
                if (this.captureMode != this.CurrentMode) {
                    this.NextMode = this.captureMode;
                    this.isSwitching = true;
                    switch (this.NextMode) {
                        case CaptureMode.None:
                            switch (this.CurrentMode) {
                                case CaptureMode.PhotoMode:
                                    this.PhotoCapture.ShouldCapture = false;
                                    if (this.disposeAction != null) {
                                        this.VideoCapture.DisposeInactive();
                                        this.PhotoCapture.DisposeActive(this.disposeAction);
                                    }
                                    break;
                                case CaptureMode.VideoMode:
                                    this.VideoCapture.ShouldCapture = false;
                                    if (this.disposeAction != null) {
                                        this.PhotoCapture.DisposeInactive();
                                        this.VideoCapture.DisposeActive(this.disposeAction);
                                    }
                                    break;
                            }
                            break;
                        case CaptureMode.PhotoMode:
                            switch (this.CurrentMode) {
                                case CaptureMode.None:
                                    this.PhotoCapture.ShouldCapture = true;
                                    break;
                                case CaptureMode.VideoMode:
                                    this.VideoCapture.ShouldCapture = false;
                                    break;
                            }
                            break;
                        case CaptureMode.VideoMode:
                            switch (this.CurrentMode) {
                                case CaptureMode.None:
                                    this.VideoCapture.ShouldCapture = true;
                                    break;
                                case CaptureMode.PhotoMode:
                                    this.PhotoCapture.ShouldCapture = false;
                                    break;
                            }
                            break;
                    }

                    // Save current mode
                    this.CurrentMode = this.NextMode;
                }
            }
        }

        /**
         * Completely deactivate the capturing.
         * 
         * @param action    action that should be invoked after disposing
         */
        public void Dispose(UnityEngine.Events.UnityAction action) {
            this.disposeAction = action;
        }

        /**
         * Start video recording.
         */
        public void StartVideoRecording() {
            this.captureMode = CaptureMode.VideoMode;
        }

        /**
         * Stop video recording.
         */
        public void StoptVideoRecording() {
            this.captureMode = CaptureMode.PhotoMode;
        }
    }
}
