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

using HoloToolkit.Unity.InputModule;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using MRC = UnityEngine.VR.WSA.WebCam;

namespace CompanionMR {

    /**
     * This class allows MR photo capturing with the HoloLens hardware.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    public class PhotoCapture : MonoBehaviour, ICapture {

        /**
         * Time between photos.
         */
        [Tooltip("Time between photos.")]
        [Range(0.0f, 5.0f)]
        public float timeBetweenPhotos = 2.0f;

        /**
         * Indicates whether the photo capture should start or end.
         */
        public bool ShouldCapture { get; set; }

        /**
         * The current camera resolution.
         */
        public Resolution CameraResolution { get;  private set; }

        /**
         * Reference to the debug logger.
         */
        private DebugLogger logger;

        /**
         * Resolution code that indicates the width and height of the captured photo.
         * 
         * Possible resolutions for HoloLens camera:
         *     0:  2048  x 1152 (67° H-FOV)
         *     1:  1408  x  792 (48° H-FOV)
         *     2:  1344  x  756 (67° H-FOV)
         *     3:  1280  x  720 (45° H-FOV)
         *     4:   896  x  504 (48° H-FOV)
         */
        private const int IMAGE_RESOLUTION = 1;

        /**
         * OpenCV image type. CV_8UC3 = 16 (8-bit unsigned char with 3 channels).
         */
        private const int CV_8UC3 = 16;

        /**
         * Parameters for the camera configuration.
         */
        private MRC.CameraParameters cameraParameters;

        /**
         * Reference to the GazeManager.
         */
        private GazeManager gazeManager;

        /**
         * Reference to the object recognition.
         */
        private ObjectRecognition objectRecognition;

        /**
         * Reference to MR-PhotoCapture.
         */
        private MRC.PhotoCapture photoCaptureObject;

        /**
         * Indicates whether the lastly taken photo is still being processed.
         */
        private bool isProcessing;

        /**
         * Indicates whether it has been tried to start the photo mode.
         */
        private bool tryStartPhotoMode;

        /**
         * Indicates whether it has been tried to stop the photo mode.
         */
        private bool tryStopPhotoMode;

        /**
         * Indicates whether the photo mode has been started.
         */
        public bool CaptureStarted { get; private set; }

        /**
         * Time since the last focus change.
         */
        private float timeSinceLastFocusChange;

        /**
         * Indicates whether the calculated time between captured photos should be reset.
         */
        private bool resetTime;

        /**
         * Photo count.
         */
        private int photoCount;

        /**
         * Indicates whether the capture process should wait until it is activated again.
         */
        private bool waitMode;

        /**
         * Wait counter.
         */
        private int waitCounter;

        /**
         * Tag of the currently focused object.
         */
        private string focusedObjectTag;

        /**
         * Indicates whether the capturering is active.
         */
        private bool active;

        /**
         * Action that should be invoked after disposing.
         */
        private UnityEngine.Events.UnityAction disposeAction;

        /**
         * Use this for initialization that has to be done before the Start() method of other GameObjects is invoked.
         */
        private void Awake() {
            // Set the camera resolution
            this.CameraResolution = MRC.PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).ElementAt(IMAGE_RESOLUTION);
        }

        /**
         * Use this for initialization.
         */
        private void Start() {

            // Variable initialization
            this.ShouldCapture = false;
            this.photoCaptureObject = null;
            this.isProcessing = false;
            this.tryStartPhotoMode = false;
            this.tryStopPhotoMode = false;
            this.CaptureStarted = false;
            this.timeSinceLastFocusChange = 0.0f;
            this.resetTime = false;
            this.photoCount = 1;
            this.waitMode = false;
            this.waitCounter = 0;
            this.focusedObjectTag = "";
            this.active = false;
            this.disposeAction = null;

            // Get reference to other scripts in the scene
            this.logger = DebugLogger.Instance;
            this.objectRecognition = ObjectRecognition.Instance;
            this.gazeManager = GazeManager.Instance;

            // Deactivate this script if necessary components are missing
            if ((this.logger == null) || (this.objectRecognition == null) || (this.gazeManager == null)) {
                Debug.LogError("PhotoCapture: Script references not set properly.");
                this.enabled = false;
                return;
            }

            // Subscibe to notifications about focused objects
            this.gazeManager.FocusedObjectChanged += this.OnFocusedObjectChanged;

            // Set camera parameters and create a MR-PhotoCapture object
            this.cameraParameters = new MRC.CameraParameters {
                hologramOpacity = 0.0f,
                cameraResolutionWidth = this.CameraResolution.width,
                cameraResolutionHeight = this.CameraResolution.height,
                pixelFormat = MRC.CapturePixelFormat.BGRA32
            };
            MRC.PhotoCapture.CreateAsync(false, this.OnPhotoCaptureCreated);
        }

        /**
         * Update is called once per frame.
         */
        private void Update() {

            // Ignore action commands if disposing was initialized
            if (this.disposeAction != null) {
                this.ShouldCapture = false;
            }

            // Check if we are in photo mode
            if (this.CaptureStarted) {

                // Stop photo mode if necessary
                if (!this.ShouldCapture && !this.tryStopPhotoMode && !this.isProcessing && (this.photoCaptureObject != null)) {
                    this.tryStopPhotoMode = true;
                    this.photoCaptureObject.StopPhotoModeAsync(this.OnStoppedPhotoMode);
                }

                // Process photo capturing
                if (this.ShouldCapture) {

                    // Calculate the time since the last focus change and reset it if the focus has actually changed
                    this.timeSinceLastFocusChange += Time.deltaTime;
                    if (this.resetTime) {
                        this.timeSinceLastFocusChange = 0.0f;
                        this.resetTime = false;
                    }

                    // Take a new photo if the photo mode is started and the focus has not changed for a specific time
                    if (this.active && !this.waitMode && !this.isProcessing && (this.timeSinceLastFocusChange > this.timeBetweenPhotos) && (this.photoCaptureObject != null) && this.focusedObjectTag.Equals(Tags.TAG_SPATIAL_MAPPING)) {
                        this.resetTime = true;
                        this.isProcessing = true;
                        this.photoCaptureObject.TakePhotoAsync(this.OnCapturedPhotoToMemory);
                        this.logger.Log("Took photo " + this.photoCount);
                        this.photoCount++;
                        // ToDo: Implement fallback if the app comes back from sleep mode. Try-Catch ?probably? does not work!
                    }
                }

            } else if (!this.tryStartPhotoMode) {

                // Start photo mode
                if (this.ShouldCapture) {
                    this.StartPhotoMode();

                // Dispose of photo capture object
                } else if (this.disposeAction != null) {
                    this.DisposeInactive();
                    this.disposeAction();
                }
            }
        }

        /**
         * Callback method that is called after the MR-PhotoCapture object has been created.
         * 
         * @param captureObject reference to MR-PhotoCapture
         */
        private void OnPhotoCaptureCreated(MRC.PhotoCapture captureObject) {
            // Save the current capture object
            if (captureObject != null) {
                this.photoCaptureObject = captureObject;
            } else {
                this.logger.Log("Failed to create MR-PhotoCapture instance.");
            }
        }

        /**
         * Start the photo capture mode.
         */
        private void StartPhotoMode() {
            // Try to start photo mode
            if (this.photoCaptureObject != null) {
                this.logger.Log("Trying to start photo mode...");
                this.tryStartPhotoMode = true;
                this.photoCaptureObject.StartPhotoModeAsync(this.cameraParameters, (result) => {
                    if (result.success) {
                        this.CaptureStarted = true;
                        this.logger.Log("Photo mode started.");
                    } else {
                        this.logger.Log("Unable to start photo mode.");
                    }
                    this.tryStartPhotoMode = false;
                });
            }
        }

        /**
         * Callback method that is called after the photo has been taken.
         * 
         * @param result            the result of the capturing
         * @param photoCaptureFrame the captured frame
         */
        private void OnCapturedPhotoToMemory(MRC.PhotoCapture.PhotoCaptureResult result, MRC.PhotoCaptureFrame photoCaptureFrame) {

            if (result.success) {

                Matrix4x4 cameraToWorld;
                Matrix4x4 projection;
                if (photoCaptureFrame.TryGetCameraToWorldMatrix(out cameraToWorld) && photoCaptureFrame.TryGetProjectionMatrix(out projection)) {
                    this.objectRecognition.SetMatrices(cameraToWorld, projection);
                }

                // TODO: flip Mat with cv::flip(oldmat, newmat, flipcode);
                // TODO: is opencv able to work with alpha value?

                // Copy the raw IMFMediaBuffer data into our empty byte list.
                List<byte> imageBufferList = new List<byte>();
                photoCaptureFrame.CopyRawImageDataIntoBuffer(imageBufferList);

                // The HoloLens uses the BGRA32 format. So our stride will be 4 since
                // we have a byte for each rgba channel. The raw image data will also
                // be flipped so we access our pixel data in the reverse order.
                int oldStride = 4; // BGRA32
                int newStride = 3; // BGR24
                int j = 0;
                byte[] imageData = new byte[this.CameraResolution.width * this.CameraResolution.height * newStride];
                for (int i = 0; i < imageBufferList.Count; i += oldStride) {
                    imageData[j + 0] = imageBufferList[i + 0]; // b
                    imageData[j + 1] = imageBufferList[i + 1]; // g
                    imageData[j + 2] = imageBufferList[i + 2]; // r
                    j += newStride;
                }

                // Dispose of the current photoCaptureFrame to avoid AccessViolation Exceptions (Unity Bug)
                photoCaptureFrame.Dispose();

                // OpenCV image type: CV_8UC3 = 16 (8-bit unsigned char with 3 channels)
                this.objectRecognition.AddImage(this.CameraResolution.width, this.CameraResolution.height, CV_8UC3, imageData);

            } else {
                this.logger.Log("Failed to save photo to memory.");
                this.isProcessing = false;
            }
        }

        /**
         * Callback method that is called after the photo mode has been stopped.
         * 
         * @param result    the photo capture result 
         */
        private void OnStoppedPhotoMode(MRC.PhotoCapture.PhotoCaptureResult result) {
            if (result.success) {
                this.CaptureStarted = false;
            } else {
                this.logger.Log("Failed to stop photo mode.");
            }
            this.tryStopPhotoMode = false;
        }

        /**
         * Should be called after the processing of a single photo has stopped.
         */
        public void ProcessingStopped() {
            this.isProcessing = false;
        }

        /**
         * Activate or deactivate the photo capturing.
         * 
         * @param active    dertemines whether the photo capture should be activated or deactivated 
         */
        public void IsActive(bool active) {
            this.active = active;
            this.logger.Log("Photo capture status: " + (this.active ? "enabled" : "disabled"));
        }

        /**
         * Activate the wait mode.
         * 
         * @param activate  dertemines whether the wait mode should be acivated or diactivated
         */
        public void Wait(bool activate) {
            if (activate) {
                this.waitCounter++;
                this.waitMode = true;
                this.logger.Log("Wait counter: " + this.waitCounter);
            } else {
                this.waitCounter--;
                if (this.waitCounter <= 0) {
                    this.waitCounter = 0;
                    this.waitMode = false;
                }
                this.logger.Log("Wait counter: " + this.waitCounter);
            }
        }

        /**
         * Indicate whether the capturing process is in wait mode.
         * 
         * @return <code>true</code> if the capturing process is paused, <code>false</code> otherwise
         */
        public bool IsWaiting() {
            return this.waitMode && !this.isProcessing;
        }

        /**
         * This method is called if the focused object has changed.
         * 
         * @param previousObject    object that was previously being focused
         * @param newObject         new object being focused
         */
        private void OnFocusedObjectChanged(GameObject previousObject, GameObject newObject) {
            //this.log.AddText("previousObject: " + ((previousObject != null) ? previousObject.name : "null") + "\r\nnewObject: " + ((newObject != null) ? newObject.name : "null"));
            this.focusedObjectTag = (newObject != null) ? newObject.tag : "null";
            this.resetTime = true;
        }

        /**
         * Stop the photo capture mode and dispose of the capturing object.
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
            this.logger.Log("Photo capture object disposed.");
            this.photoCaptureObject.Dispose();
            this.photoCaptureObject = null;
        }
    }
}
