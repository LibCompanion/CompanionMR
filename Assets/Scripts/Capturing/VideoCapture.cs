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

using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using MRC = UnityEngine.VR.WSA.WebCam;

namespace CompanionMR {

    /**
     * This class allows MR video capturing with the HoloLens hardware.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    public class VideoCapture : MonoBehaviour, ICapture {

        /**
         * The current video capture resolution.
         */
        public Resolution VideoResolution { get; private set; }

        /**
         * The current video capture framerate.
         */
        public float VideoFramerate { get; private set; }

        /**
         * Indicates whether the video capturing should start or end.
         */
        public bool ShouldCapture { get; set; }

        /**
         * Reference to the debug logger.
         */
        private DebugLogger logger;

        /**
         * Codes that indicate which video resolution and framerate should be chosen for video capture.
         * 
         * Possible resolutions and framerates for HoloLens camera:
         *     0:  1408  x  792 (48° H-FOV)
         *         0: 29.97003 fps
         *         1: 24 fps
         *         2: 20 fps
         *         3: 15 fps
         *         4:  5 fps
         *     1:  1344  x  756 (67° H-FOV)
         *         0: 29.97003 fps
         *         1: 24 fps
         *         2: 20 fps
         *         3: 15 fps
         *         4:  5 fps
         *     2:  1280  x  720 (45° H-FOV)
         *         0: 30 fps
         *         1: 24 fps
         *         2: 20 fps
         *         3: 15 fps
         *         4:  5 fps
         *     3:  896  x  504 (48° H-FOV)
         *         0: 29.97003 fps
         *         1: 24 fps
         *         2: 20 fps
         *         3: 15 fps
         *         4:  5 fps
         */
        private const int VIDEO_RESOLUTION = 0;
        private const int VIDEO_FPS = 0;

        /**
         * Determines if audio should be recorded.
         */
        private const MRC.VideoCapture.AudioState AUDIO_STATE = MRC.VideoCapture.AudioState.ApplicationAudio;

        /**
         * Parameters for the camera configuration.
         */
        private MRC.CameraParameters cameraParameters;

        /**
         * Reference to MR-VideoCapture.
         */
        private MRC.VideoCapture videoCaptureObject;

        /**
         * Indicates whether it has been tried to start the video recording.
         */
        private bool tryStartRecording;

        /**
         * Indicates whether it has been tried to stop the video recording.
         */
        private bool tryStopRecording;

        /**
         * Indicates whether video recording is currently active.
         */
        private bool isRecording;

        /**
         * Indicates whether it has been tried to start the video mode.
         */
        private bool tryStartVideoMode;

        /**
         * Indicates whether the video mode has been started.
         */
        public bool CaptureStarted { get; private set; }

        /**
         * Action that should be invoked after disposing.
         */
        private UnityEngine.Events.UnityAction disposeAction;

        /**
         * Naming index for video files.
         */
        private int namingIndex;

        /**
         * Use this for initialization that has to be done before the Start() method of other GameObjects is invoked.
         */
        private void Awake() {
            // Set the video resolution and framerate
            try {
                List<Resolution> resolutions = MRC.VideoCapture.SupportedResolutions.ToList();
                resolutions.RemoveRange(4, 16); // remove all resolution duplicates
                this.VideoResolution = resolutions.OrderByDescending((res) => res.width * res.height).ElementAt(VIDEO_RESOLUTION);
                this.VideoFramerate = MRC.VideoCapture.GetSupportedFrameRatesForResolution(this.VideoResolution).OrderByDescending((fps) => fps).ElementAt(VIDEO_FPS);
            } catch (System.Exception ex) {
                Debug.Log(ex.Message);
                this.gameObject.SetActive(false);
            }
        }

        /**
         * Use this for initialization.
         */
        private void Start() {

            // Variable initialization
            this.ShouldCapture = false;
            this.videoCaptureObject = null;
            this.tryStartRecording = false;
            this.tryStopRecording = false;
            this.isRecording = false;
            this.tryStartVideoMode = false;
            this.CaptureStarted = false;
            this.disposeAction = null;
            this.namingIndex = 0;

            // Get reference to other scripts in the scene
            this.logger = DebugLogger.Instance;

            // Deactivate this script if necessary components are missing
            if (this.logger == null) {
                Debug.LogError("VideoCapture: Script references not set properly.");
                this.enabled = false;
                return;
            }

            // Set camera parameters and create a MR-VideoCapture object
            this.cameraParameters = new MRC.CameraParameters {
                hologramOpacity = 1.0f,
                frameRate = this.VideoFramerate,
                cameraResolutionWidth = this.VideoResolution.width,
                cameraResolutionHeight = this.VideoResolution.height,
                pixelFormat = MRC.CapturePixelFormat.BGRA32
            };
            MRC.VideoCapture.CreateAsync(true, this.OnVideoCaptureCreated);
        }

        /**
         * Update is called once per frame.
         */
        private void Update() {

            // Ignore action commands if disposing was initialized
            if (this.disposeAction != null) {
                this.ShouldCapture = false;
            }

            // Check if we are in video mode
            if (this.CaptureStarted) {

                // Stop recording
                if (!this.ShouldCapture && this.isRecording && !this.tryStopRecording && (this.videoCaptureObject != null)) {
                    this.tryStopRecording = true;
                    this.videoCaptureObject.StopRecordingAsync((result) => {
                        if (result.success) {
                            this.logger.Log("Stopped video recording.");
                            this.isRecording = false;
                            this.videoCaptureObject.StopVideoModeAsync(this.OnStoppedVideoMode);
                        } else {
                            this.logger.Log("Failed to stop recording.");
                        }
                        this.tryStopRecording = false;
                    });

                // Start recording
                } else if (this.ShouldCapture && !this.isRecording && !this.tryStartRecording && (this.videoCaptureObject != null)) {
                    string timeStamp = Time.time.ToString().Replace(".", "").Replace(":", "");
                    string filename = string.Format("testVideo_{0}_{1}.mp4", this.namingIndex++, timeStamp);
                    string filepath = System.IO.Path.Combine(Application.persistentDataPath, filename);
                    filepath = filepath.Replace("/", @"\");
                    this.videoCaptureObject.StartRecordingAsync(filepath, this.OnStartedRecordingVideo);
                    this.tryStartRecording = true;
                }

            } else if (!this.tryStartVideoMode) {

                // Start video capture mode
                if (this.ShouldCapture) {
                    this.StartVideoMode();

                // Dispose video capture object
                } else if (this.disposeAction != null) {
                    this.DisposeInactive();
                    this.disposeAction();
                }
            }
        }

        /**
         * Callback method that is called after the MR-VideoCapture object has been created.
         * 
         * @param captureObject reference to MR-VideoCapture
         */
        private void OnVideoCaptureCreated(MRC.VideoCapture captureObject) {
            // Save the current capture object
            if (captureObject != null) {
                this.videoCaptureObject = captureObject;
            } else {
                this.logger.Log("Failed to create MR-VideoCapture instance.");
            }
        }

        /**
         * Start the video capture mode.
         */
        private void StartVideoMode() {
            // Try to start video mode
            if (this.videoCaptureObject != null) {
                this.logger.Log("Trying to start video mode...");
                this.tryStartVideoMode = true;
                this.videoCaptureObject.StartVideoModeAsync(this.cameraParameters, AUDIO_STATE, (result) => {
                    if (result.success) {
                        this.CaptureStarted = true;
                        this.logger.Log("Video mode started.");
                    } else {
                        this.logger.Log("Unable to start video mode.");
                    }
                    this.tryStartVideoMode = false;
                });
            }
        }

        /**
         * Callback method that is called when the video has started recording.
         * 
         * @param result    the result of the capturing
         */
        private void OnStartedRecordingVideo(MRC.VideoCapture.VideoCaptureResult result) {
            if (result.success) {
                this.logger.Log("Started video recording.");
                this.isRecording = true;
            } else {
                this.logger.Log("Failed to start video recording.");
            }
            this.tryStartRecording = false;
        }

        /**
         * Callback method that is called after the video mode has been stopped.
         * 
         * @param result    the video capture result 
         */
        private void OnStoppedVideoMode(MRC.VideoCapture.VideoCaptureResult result) {
            if (result.success) {
                this.CaptureStarted = false;
            } else {
                this.logger.Log("Failed to stop video mode.");
            }
        }

        /**
         * Stop the capture mode and dispose of the capturing object.
         * 
         * @param action    action that should be invoked after disposing
         */
        public void DisposeActive(UnityEngine.Events.UnityAction action) {
            this.disposeAction = action;
        }

        /**
         * This method disposes of the inactive capturing object.
         */
        public void DisposeInactive() {
            this.logger.Log("Video capture object disposed.");
            this.videoCaptureObject.Dispose();
            this.videoCaptureObject = null;
        }
    }
}
