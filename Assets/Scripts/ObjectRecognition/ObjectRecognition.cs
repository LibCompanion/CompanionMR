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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR.WSA.Persistence;

#if ENABLE_WINMD_SUPPORT
using CW = CompanionWinRT;
#endif

namespace CompanionMR {

    /**
     * This class provides object recognition functionality with the help of the Companion framework.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class ObjectRecognition : HoloToolkit.Unity.Singleton<ObjectRecognition> {

        /**
         * Image processing methods.
         */
        public enum ImageProcessingMethod {
            FEATURE_MATCHING,
            IMAGE_HASHING,
            HYBRID_MATCHING
        }

        /**
         * Artwork scanning states.
         */
        public enum ScanState {
            None,       // no scanning
            LiveScan,   // live scanning mode with no caching
            Caching,    // caching mode (creating the cached scan areas)
            UserMode    // user mode (using the cached scan areas)
        }

        /**
         * Operations for feature matching model handling.
         */
        private enum FmModelOperation {
            Add,        // adds a model for a specific artwork
            AddNew,     // adds models for all new artworks
            Remove,     // removes the model of a specific artwork
            RemoveAll   // removes all models
        }

        /**
         * This struct capsules information about a feature matching model operation.
         */
        private struct FmModelOperationInfo {
            public int ArtworkID { get; set; }
            public FmModelOperation Operation { get; set; }
        }

        /**
         * Prefab for the scanning area.
         */
        [Tooltip("Prefab for the scanning area.")]
        public GameObject scanningArea;

        /**
         * Prefab for a recognized artwork.
         */
        [Tooltip("Prefab for a recognized artwork.")]
        public GameObject artwork;

        /**
         * Prefab for the debug photo.
         */
        [Tooltip("Prefab for the debug photo.")]
        public GameObject debugPhoto;

        /**
         * Maximum amount of different artworks that can be recognized.
         */
        [Tooltip("Maximum amount of different artworks that can be recognized.")]
        [Range(10, 100)]
        public int artworkLimit = 100;

        /**
         * Indicates whether photo capturing should always be activated if there are new artworks in the scene (only applies to Cached Mapping).
         */
        [Tooltip("Indicates whether photo captruring should always be activated if there are new artworks in the scene (only applies to Cached Mapping).")]
        public bool scanForNewArtworks = false;

        /**
         * Indicates which processing method should be used in the live scan.
         */
        [Tooltip("Indicates which processing method should be used in the live scan.")]
        public ImageProcessingMethod liveScanProcessing = ImageProcessingMethod.HYBRID_MATCHING;

        /**
         * Indicates whether the object recognition has started.
         */
        public bool ObjectRecognitionConfigured { get; private set; }

        /**
         * Current state of artwork scanning.
         */
        public ScanState CurrentScanState { get; private set; }

        /**
         * Reference to the debug logger.
         */
        private DebugLogger logger;

        /**
         * Reference to the data loader.
         */
        private DataLoader dataLoader;

        /**
         * Reference to the initializer.
         */
        private Initializer initializer;

        /**
         * Reference to the photo capture.
         */
        private PhotoCapture capture;

        /**
         * Reference to the info canvas.
         */
        private InfoCanvas infoCanvas;

        /**
         * Reference to the world anchor manager.
         */
        private HoloToolkit.Unity.WorldAnchorManager anchorManager;

        /**
         * The current camera resolution.
         */
        private Resolution cameraResolution;

#if ENABLE_WINMD_SUPPORT
        /**
         * The camera-to-world matrix of the current photo.
         */
        private Matrix4x4 cameraToWorld;

        /**
         * The projection matrix of the current photo.
         */
        private Matrix4x4 projection;

        /**
         * Indicates whether new camera matrices have been calculated.
         */
        private bool newCameraMatrices;

        /**
         * Array of active world anchors in the scene. The array index matches the artwork ID.
         */
        private bool[] worldAnchors;

        /**
         * Reference to the Companion framework configuration.
         */
        private CW.Configuration configuration;

        /**
         * Reference to the Companion match recognition.
         */
        private CW.MatchRecognition matchRecognition;

        /**
         * Reference to the Companion hash recognition.
         */
        private CW.HashRecognition hashRecognition;

        /**
         * Reference to the Companion hybrid recognition.
         */
        private CW.HybridRecognition hybridRecognition;
#endif

        /**
         * Collection of all artists.
         */
        private Dictionary<int, Artist> artists;

        /**
         * Collection of artworks.
         */
        private Dictionary<int, Artwork> artworks;

        /**
         * Collection of videos.
         */
        private Dictionary<int, Video> videos;

        /**
         * Collection of audio data.
         */
        private new Dictionary<int, Audio> audio;

        /**
         * Indicates whether the world anchors have been loaded into the scene.
         */
        private bool worldAnchorsLoaded;

        /**
         * Indicates whether we are in this room for the first time.
         */
        private bool newRoom;

        /**
         * Indicates whether there is new camera data to process.
         */
        private bool newData;

        /**
         * A collection of recognition information for each artwork.
         */
        private Dictionary<int, Recognition> recognitions;

        /**
         * A collection of all instantiated artworks.
         */
        private Dictionary<int, GameObject> instantiatedArtworks;

        /**
         * Indicates which artwork has already been recognized.
         */
        private bool[] recognizedArtworks;

        /**
         * A queue for feature matching model operations.
         */
        private Queue<FmModelOperationInfo> fmModelOperations;

        /**
         * Indicates whether a feature matching model operation is currently performed.
         */
        private bool isPerformingFmModelOperation;

#if COMP_DEBUG_IMAGE
        /**
         * Current result image.
         */
        private ResultImage resultImage;
#endif

        /**
         * Use this for initialization.
         */
        private void Start() {

            // Variable initialization
            this.ObjectRecognitionConfigured = false;
            this.CurrentScanState = ScanState.None;
#if ENABLE_WINMD_SUPPORT
            this.newCameraMatrices = false;
            this.worldAnchors = new bool[this.artworkLimit]; // initialized with false
#endif
            this.artworks = new Dictionary<int, Artwork>();
            this.artists = new Dictionary<int, Artist>();
            this.videos = new Dictionary<int, Video>();
            this.audio = new Dictionary<int, Audio>();
            this.worldAnchorsLoaded = false;
            this.newRoom = true;
            this.newData = false;
            this.recognitions = new Dictionary<int, Recognition>();
            this.instantiatedArtworks = new Dictionary<int, GameObject>();
            this.recognizedArtworks = new bool[this.artworkLimit]; // initialized with false
            this.fmModelOperations = new Queue<FmModelOperationInfo>();
            this.isPerformingFmModelOperation = false;

            // Get reference to other scripts in the scene
            this.logger = DebugLogger.Instance;
            this.dataLoader = DataLoader.Instance;
            this.initializer = Initializer.Instance;
            this.capture = CaptureManager.IsInitialized ? CaptureManager.Instance.PhotoCapture : null;
            this.infoCanvas = InfoCanvas.Instance;
            this.anchorManager = HoloToolkit.Unity.WorldAnchorManager.Instance;

            // Deactivate this script if necessary components are missing
            if ((this.scanningArea == null) || (this.artwork == null) || (this.debugPhoto == null) || (this.logger == null) || (this.initializer == null) ||
                (this.dataLoader == null) || (this.capture == null) || (this.infoCanvas == null) || (this.anchorManager == null)) {
                Debug.LogError("ObjectRecognition: Script references not set properly.");
                this.enabled = false;
                return;
            }

            // Get the current camera resolution
            this.cameraResolution = this.capture.CameraResolution;
            this.logger.Log("Set new camera resolution: " + this.cameraResolution.width + " x " + this.cameraResolution.height);

#if ENABLE_WINMD_SUPPORT
            // Create a configuration object for Companion
            this.configuration = new CW.Configuration();

            // Configure image recognition
            CW.FeatureMatching feature = new CW.FeatureMatching(CW.FeatureDetector.BRISK, CW.DescriptorMatcherType.BRUTEFORCE_HAMMING);
            CW.LSH lsh = new CW.LSH();
            CW.ShapeDetection shapeDetection = new CW.ShapeDetection();

            // Configure image processing
            this.matchRecognition = new CW.MatchRecognition(feature, CW.Scaling.SCALE_960x540);
            this.hashRecognition = new CW.HashRecognition(shapeDetection, lsh);
            this.hybridRecognition = new CW.HybridRecognition(this.hashRecognition, feature, 70);

            // Set callback methods
            this.configuration.setResultCallback(this.ResultCallback, CW.ColorFormat.RGB);
            this.configuration.setErrorCallback(this.ErrorCallback);

            // Set number of frames to skip
            this.configuration.setSkipFrame(0);

            // Set image buffer
            this.configuration.setImageBuffer(1);

            // Add source to configuaration
            CW.ImageStream stream = new CW.ImageStream(1);
            this.configuration.setSource(stream);
#endif
        }

        /**
         * Update is called once per frame.
         */
        private void Update() {

            // Load world anchors
            if (!this.worldAnchorsLoaded) {
                this.LoadWorldAnchors();
            }

            // Start the object recognition if the data library as well as the world anchors are loaded
            if (this.worldAnchorsLoaded && this.dataLoader.DataLoadingFinished && !this.ObjectRecognitionConfigured && this.initializer.ReadyToStartRecognition) {
                this.InitializeData();

                // Start cached object recognition
                if (this.CurrentScanState == ScanState.UserMode) {
                    this.StartCachedRecognition();
                }

                this.ObjectRecognitionConfigured = true;
            }

            // Instantiate corresponding GameObjects for the newly recognized artworks
            if (this.newData) {
                bool validationFinished = true;
                foreach (KeyValuePair<int, Recognition> pair in this.recognitions) {

                    int id = pair.Key;
                    Recognition info = pair.Value;

                    // Ignore this artwork if its validation process has already finished
                    if (info.ValidationFinished) {
                        continue;
                    }

                    // Check if the recognized artwork was already instantiated
                    if (this.instantiatedArtworks.ContainsKey(id)) {

                        GameObject obj = this.instantiatedArtworks[id];

                        // Check if the data was valid
                        int valid = obj.GetComponent<RecognizedArtwork>().IsValid();

                        // Artwork validation is not finished yet
                        if (valid == -1) {
                            validationFinished = false;

                        // Artwork is valid
                        } else if (valid == 1) {
                            this.logger.Log("Artwork " + id + ": valid");
                            info.ValidationFinished = true;
                            if (this.IsIdEligible(id)) {
                                this.recognizedArtworks[id] = true;
                            }
#if ENABLE_WINMD_SUPPORT
                            // Remove artwork from further object recognition cycles
                            if ((this.CurrentScanState != ScanState.LiveScan) || (this.liveScanProcessing == ImageProcessingMethod.FEATURE_MATCHING)) {
                                this.matchRecognition.removeModel(id);
                                int count = this.matchRecognition.getModels().Count;
                                this.logger.Log("Count: " + count);
                                this.capture.IsActive(count > 0);
                                this.logger.Log("Artwork " + id + " removed from Companion.");
                                if ((this.CurrentScanState == ScanState.Caching) && (count == 0)) {
                                    this.infoCanvas.SetInfoText("All artworks scanned. <i>Bloom</i> to quit.", true);
                                } else if (this.CurrentScanState == ScanState.Caching) {
                                    this.infoCanvas.SetInfoText("Artwork #" + id + " scanned. " + count + " artworks left.");
                                }
                            }
#endif
                        // Artwork is not valid
                        } else {
                            this.logger.Log("Artwork " + id + ": not valid");
                            info.ValidationFinished = true;
                            // Destroy the already instantiated artwork
                            Destroy(obj);
                            this.instantiatedArtworks.Remove(id);
                        }

                    // Instantiate a recognized artwork
                    } else {
                        this.logger.Log("Artwork " + id + ": instantiate new artwork");
                        GameObject artwork = Instantiate(this.artwork);
                        RecognizedArtwork artworkScript = artwork.GetComponent<RecognizedArtwork>();
                        artworkScript.SetNewData(info);
                        validationFinished = false;
                        this.instantiatedArtworks.Add(id, artwork);
                    }
                }

                // Validation process finished
                if (validationFinished) {
#if COMP_DEBUG_IMAGE
                    // Instantiate the photo in the scene
                    GameObject debugPhoto = Instantiate(this.debugPhoto, this.resultImage.WorldSpaceMiddle, Utils.CreateLookAtRotation(this.resultImage.WorldSpaceMiddle, this.resultImage.WorldSpaceCamera, false));
                    this.resultImage.DisplayImage(debugPhoto.GetComponentInChildren<UnityEngine.UI.Image>().gameObject, this.cameraResolution);
#endif
                    this.newData = false;
                    this.recognitions.Clear();
                    this.ProcessingStopped();
                }
            }

            // Perform feature matching model operations
            if (this.ObjectRecognitionConfigured && !this.isPerformingFmModelOperation && (this.fmModelOperations.Count > 0)) {
                this.isPerformingFmModelOperation = true;
                StartCoroutine(this.DoFeatureModelOperation(this.fmModelOperations.Dequeue()));
            }
        }

        /**
         * Try to load the world anchors that are saved in the World Anchor Store of the HoloLens device.
         */
        private void LoadWorldAnchors() {
#if UNITY_EDITOR
            this.worldAnchorsLoaded = true;
#elif ENABLE_WINMD_SUPPORT
            WorldAnchorStore store = this.anchorManager.AnchorStore;
            if ((store != null) && HoloToolkit.Unity.SpatialUnderstanding.IsInitialized) {

                // Get spatial understanding references
                HoloToolkit.Unity.SpatialUnderstanding suScript = HoloToolkit.Unity.SpatialUnderstanding.Instance;
                GameObject suObj = suScript.gameObject;
                CachedRoom roomScript = suObj.GetComponent<CachedRoom>();

                // Get all world anchor IDs
                string[] ids = store.GetAllIds();
                this.logger.Log("Active world anchors:");

                // Try to interpret the anchors
                for (int index = 0; index < ids.Length; index++) {

                    // Print out all anchor IDs
                    string anchorID = ids[index];
                    this.logger.Log("        " + anchorID);

                    // Check if we have a cached room anchored
                    if ((roomScript != null) && anchorID.Equals(roomScript.fileName)) {
                        suScript.CleanupUnderstanding();
                        roomScript.LoadRoom();
                        this.CurrentScanState = ScanState.UserMode;

                    // Try to load the artwork anchors
                    } else if ((anchorID.Length > RecognizedArtwork.ARTWORK_ANCHOR_PREFIX.Length) && anchorID.Substring(0, RecognizedArtwork.ARTWORK_ANCHOR_PREFIX.Length).Equals(RecognizedArtwork.ARTWORK_ANCHOR_PREFIX)) {

                        // The anchor ID should have a specific format
                        try {
                            int id = int.Parse(anchorID.Substring(RecognizedArtwork.ARTWORK_ANCHOR_PREFIX.Length));
                            if (this.IsIdEligible(id)) {
                                // Instantiate the anchors far away from the camera collider to avoid early collision
                                GameObject gameObject = Instantiate(this.scanningArea, new Vector3(0.0f, 10.0f, 0.0f), Quaternion.identity);
                                this.anchorManager.AttachAnchor(gameObject, anchorID);
                                ScanningArea anchor = gameObject.GetComponent<ScanningArea>();
                                anchor.artworkID = id;
                                anchor.ActivateCollider();
                                this.worldAnchors[id] = true;
                                this.newRoom = false;
                            } else {
                                this.logger.Log("World anchor ID " + id + " not eligible.");
                            }
                        } catch (System.Exception ex) {
                            this.logger.Log("EXCEPTION: " + ex.Message);
                        }
                    }
                }

                // Remove all anchors if the room is not cached or a cached room has no cached scan areas
                if ((!this.newRoom && (this.CurrentScanState != ScanState.UserMode)) || (this.newRoom && (this.CurrentScanState == ScanState.UserMode))) {
                    this.anchorManager.RemoveAllAnchors();
                    this.TabulaRasa();
                }

                this.worldAnchorsLoaded = true;
            }
#endif
        }

        /**
         * Initialize the data that has been loaded from local or remote storage.
         */
        private void InitializeData() {
            
            // Obtain data
            this.artworks = this.dataLoader.Artworks;
            this.artists = this.dataLoader.Artists;
            this.videos = this.dataLoader.Videos;
            this.audio = this.dataLoader.Audio;
            
            // Add references
            foreach (Artwork artwork in this.artworks.Values) {
                
                // Add actual artist object reference to each artwork
                artwork.artist = (this.artists.ContainsKey(artwork.artistID)) ? this.artists[artwork.artistID] : null;

                // Add actual video object references to each artwork
                foreach (int videoID in artwork.videos) {
                    artwork.videoData.Add(this.videos[videoID]);
                }

                // Add actual audio object references to each artwork
                foreach (int audioID in artwork.audio) {
                    artwork.audioData.Add(this.audio[audioID]);
                }
            }
        }

        /**
         * Start the cached object recognition.
         */
        private void StartCachedRecognition() {
#if ENABLE_WINMD_SUPPORT
            // Set feature matching as the processing method and start Companion
            this.configuration.setProcessing(this.matchRecognition);
            this.RunCompanion();
#endif

            // Activate collision detection with world anchor spheres if the scene has cached anchors
            this.GetComponent<SphereCollider>().enabled = !this.newRoom;

            // Activate ongoing scanning for artworks outside of cached scan areas if that is the desired behaviour for uncached artworks
            if (this.scanForNewArtworks) {
                this.EnterScanningMode();
            }
        }

        /**
         * Run Companion processing.
         */
        private void RunCompanion() {
#if ENABLE_WINMD_SUPPORT
            // Run Companion on a background thread
            System.Threading.Tasks.Task.Run(() => {
                try {
                    this.configuration.run();
                } catch (System.Exception ex) {
                    this.logger.Log("EXCEPTION: " + ex.Message);
                }
            });
#endif
        }

#if ENABLE_WINMD_SUPPORT
        /**
         * Result callback method for the Companion processing.
         * 
         * @param results   list of results that represent the recognized artworks
         * @param image     the processed image with visually markers for the recognized artworks
         */
        public void ResultCallback(IList<CW.Result> results, byte[] image) {

            // Ignore the result if the camera matrices of the processed photo were not obtained successfully
            if (!this.newCameraMatrices || (results.Count == 0)) {
                this.logger.Log("No results.");
                this.ProcessingStopped();
                return;
            }

            // Reset camera matrices for this photo
            this.newCameraMatrices = false;

            bool newResults = false;
            Vector3 worldSpaceCamera;
            Vector3 worldSpaceMiddle;
            Vector3 worldSpaceTopLeft;
            Vector3 worldSpaceTopRight;
            Vector3 worldSpaceBottomLeft;
            Vector3 worldSpaceBottomRight;

            CW.Point upperLeftCorner;
            upperLeftCorner.x = 0;
            upperLeftCorner.y = 0;

            CW.Point upperRightCorner;
            upperRightCorner.x = 0;
            upperRightCorner.y = 0;

            CW.Point bottomRightCorner;
            bottomRightCorner.x = 0;
            bottomRightCorner.y = 0;

            CW.Point bottomLeftCorner;
            bottomLeftCorner.x = 0;
            bottomLeftCorner.y = 0;

            // ToDo: MultiplyPoint3x4 !!
            worldSpaceCamera = this.cameraToWorld.MultiplyPoint(new Vector3(0, 0, 0/*, 1*/)); // camera location in world space

            // float casting
            float image_width = this.cameraResolution.width;
            float image_height = this.cameraResolution.height;
            
            foreach (CW.Result result in results) {
                
                int artworkID = result.getID();

                // Ignore results for artworks, the ID of which is not within the eligible range
                if ((result.getType() != CW.ResultType.RECOGNITION) || !this.IsIdEligible(artworkID)) {
                    this.logger.Log("Artwork " + artworkID + " has an ineligible ID.");
                    continue;
                }

                // Ignore results for artworks that were already recognized
                if (this.recognizedArtworks[artworkID]) {
                    this.logger.Log("Artwork " + artworkID + " already recognized.");
                    continue;
                }

                this.logger.Log("Corners found for artwork " + artworkID + ".");

                upperLeftCorner = result.getFrame().getUpperLeftCorner();
                upperRightCorner = result.getFrame().getUpperRightCorner();
                bottomRightCorner = result.getFrame().getLowerRightCorner();
                bottomLeftCorner = result.getFrame().getLowerLeftCorner();

                float middleX = upperLeftCorner.x + ((upperRightCorner.x - upperLeftCorner.x) / 2.0f);
                float middleY = upperLeftCorner.y + ((bottomLeftCorner.y - upperLeftCorner.y) / 2.0f);
                Vector2 ImagePosZeroToOne = new Vector2(middleX / image_width, 1.0f - (middleY / image_height));
                Vector2 ImagePosProjected = ((ImagePosZeroToOne * 2.0f) - new Vector2(1.0f, 1.0f)); // -1 to 1 space
                Vector3 CameraSpacePos = UnProjectVector(this.projection, new Vector3(ImagePosProjected.x, ImagePosProjected.y, 1.0f));
                worldSpaceMiddle = this.cameraToWorld.MultiplyPoint(CameraSpacePos); // ray point in world space

                ImagePosZeroToOne = new Vector2(upperLeftCorner.x / image_width, 1.0f - (upperLeftCorner.y / image_height));
                ImagePosProjected = ((ImagePosZeroToOne * 2.0f) - new Vector2(1.0f, 1.0f)); // -1 to 1 space
                CameraSpacePos = UnProjectVector(this.projection, new Vector3(ImagePosProjected.x, ImagePosProjected.y, 1.0f));
                worldSpaceTopLeft = this.cameraToWorld.MultiplyPoint(CameraSpacePos); // ray point in world space

                ImagePosZeroToOne = new Vector2(upperRightCorner.x / image_width, 1.0f - (upperRightCorner.y / image_height));
                ImagePosProjected = ((ImagePosZeroToOne * 2.0f) - new Vector2(1.0f, 1.0f)); // -1 to 1 space
                CameraSpacePos = UnProjectVector(this.projection, new Vector3(ImagePosProjected.x, ImagePosProjected.y, 1.0f));
                worldSpaceTopRight = this.cameraToWorld.MultiplyPoint(CameraSpacePos); // ray point in world space

                ImagePosZeroToOne = new Vector2(bottomRightCorner.x / image_width, 1.0f - (bottomRightCorner.y / image_height));
                ImagePosProjected = ((ImagePosZeroToOne * 2.0f) - new Vector2(1.0f, 1.0f)); // -1 to 1 space
                CameraSpacePos = UnProjectVector(this.projection, new Vector3(ImagePosProjected.x, ImagePosProjected.y, 1.0f));
                worldSpaceBottomRight = this.cameraToWorld.MultiplyPoint(CameraSpacePos); // ray point in world space

                ImagePosZeroToOne = new Vector2(bottomLeftCorner.x / image_width, 1.0f - (bottomLeftCorner.y / image_height));
                ImagePosProjected = ((ImagePosZeroToOne * 2.0f) - new Vector2(1.0f, 1.0f)); // -1 to 1 space
                CameraSpacePos = UnProjectVector(this.projection, new Vector3(ImagePosProjected.x, ImagePosProjected.y, 1.0f));
                worldSpaceBottomLeft = this.cameraToWorld.MultiplyPoint(CameraSpacePos); // ray point in world space

#if COMP_DEBUG_IMAGE
                // Calculate image corners in world space coordinates
                ImagePosZeroToOne = new Vector2(0.5f, 0.5f);
                ImagePosProjected = ((ImagePosZeroToOne * 2.0f) - new Vector2(1.0f, 1.0f)); // -1 to 1 space
                CameraSpacePos = UnProjectVector(this.projection, new Vector3(ImagePosProjected.x, ImagePosProjected.y, 1.0f));
                Vector3 imageMiddle = this.cameraToWorld.MultiplyPoint(CameraSpacePos); // ray point in world space

                ImagePosZeroToOne = new Vector2(0.0f, 1.0f);
                ImagePosProjected = ((ImagePosZeroToOne * 2.0f) - new Vector2(1.0f, 1.0f)); // -1 to 1 space
                CameraSpacePos = UnProjectVector(this.projection, new Vector3(ImagePosProjected.x, ImagePosProjected.y, 1.0f));
                Vector3 imageTopLeft = this.cameraToWorld.MultiplyPoint(CameraSpacePos); // ray point in world space

                ImagePosZeroToOne = new Vector2(1.0f, 1.0f);
                ImagePosProjected = ((ImagePosZeroToOne * 2.0f) - new Vector2(1.0f, 1.0f)); // -1 to 1 space
                CameraSpacePos = UnProjectVector(this.projection, new Vector3(ImagePosProjected.x, ImagePosProjected.y, 1.0f));
                Vector3 imageTopRight = this.cameraToWorld.MultiplyPoint(CameraSpacePos); // ray point in world space

                ImagePosZeroToOne = new Vector2(0.0f, 0.0f);
                ImagePosProjected = ((ImagePosZeroToOne * 2.0f) - new Vector2(1.0f, 1.0f)); // -1 to 1 space
                CameraSpacePos = UnProjectVector(this.projection, new Vector3(ImagePosProjected.x, ImagePosProjected.y, 1.0f));
                Vector3 imageBottomLeft = this.cameraToWorld.MultiplyPoint(CameraSpacePos); // ray point in world space

                // Save result image
                this.resultImage = new ResultImage(image, worldSpaceCamera, imageMiddle, imageTopLeft, imageTopRight, imageBottomLeft);
#endif

                // Save information about the recognized artwork
                Artwork artwork = this.artworks[artworkID];
                Recognition recognition = new Recognition(artwork, worldSpaceCamera, worldSpaceMiddle, worldSpaceTopLeft, worldSpaceTopRight, worldSpaceBottomLeft, worldSpaceBottomRight);
                this.recognitions.Add(artworkID, recognition);
                
                newResults = true;
            }

            // Check if the found results were ignored because they were already recognized before
            if (!newResults) {
                this.logger.Log("Artworks already recognized.");
                this.ProcessingStopped();
            } else {
                // Beginn processing of new data
                this.newData = true;
            }
        }
#endif

        /**
         * Use the given camera projection matrix to calculate the 3D coordinates of a given point.
         * 
         * @param proj  the camera projection matrix
         * @param to    the point that is going to be projected
         * @return the projected point in camera space
         */
        public static Vector3 UnProjectVector(Matrix4x4 proj, Vector3 to) {
            Vector3 from = new Vector3(0, 0, 0);
            var axsX = proj.GetRow(0);
            var axsY = proj.GetRow(1);
            var axsZ = proj.GetRow(2);
            from.z = to.z / axsZ.z;
            from.y = (to.y - (from.z * axsY.z)) / axsY.y;
            from.x = (to.x - (from.z * axsX.z)) / axsX.x;
            return from;
        }

        /**
         * Error callback method for the Companion processing.
         * 
         * @param errorMessage  a specific error message
         */
        public void ErrorCallback(string errorMessage) {
            this.logger.Log(errorMessage);
        }

        /**
         * Should be called after the processing of a single photo has stopped.
         */
        private void ProcessingStopped() {
            this.capture.ProcessingStopped();
        }

        /**
         * Set the transformation matrices of the current photo.
         * 
         * @param cameraToWorld camera-to-world matrix of the current photo
         * @param projection    projection matrix of the current photo
         */
        public void SetMatrices(Matrix4x4 cameraToWorld, Matrix4x4 projection) {
#if ENABLE_WINMD_SUPPORT
            this.cameraToWorld = cameraToWorld;
            this.projection = projection;
            this.newCameraMatrices = true;
#endif
        }

        /**
         * Add an image to the Companion framework for processing.
         * 
         * @param width     the image width
         * @param height    the image height
         * @param type      the OpenCV type of the image
         * @param imageData the actual image data
         */
        public void AddImage(int width, int height, int type, byte[] imageData) {
#if ENABLE_WINMD_SUPPORT
            try {
                this.configuration.getSource().addImage(width, height, type, imageData);
            } catch (System.Exception ex) {
                this.logger.Log("EXCEPTION: " + ex.Message);
            }
#elif UNITY_EDITOR
            this.ProcessingStopped();
#endif
        }

        /**
         * Perform a feature matching model operation.
         *
         * @param operationInfo information about the operation
         * @return IEnumerator that makes this method work as a coroutine
         */
        private IEnumerator DoFeatureModelOperation(FmModelOperationInfo operationInfo) {

            // Stop photo capture
            this.capture.Wait(true);
            yield return new WaitUntil(this.capture.IsWaiting);

#if ENABLE_WINMD_SUPPORT
            // Perform the feature matching model operation
            switch (operationInfo.Operation) {
                case FmModelOperation.Add:
                    this.AddFeatureMatchingModel(operationInfo.ArtworkID);
                    break;
                case FmModelOperation.Remove:
                    this.RemoveFeatureMatchingModel(operationInfo.ArtworkID);
                    break;
                case FmModelOperation.RemoveAll:
                    this.matchRecognition.clearModels();
                    break;
                case FmModelOperation.AddNew:
                    this.AddNewArtworks();
                    break;
            }

            // Resume photo capture if necessary
            this.capture.IsActive(this.matchRecognition.getModels().Count > 0);
#endif
            this.capture.Wait(false);
            this.isPerformingFmModelOperation = false;
        }

        /**
         * Add a feature matching model.
         * 
         * @param artworkID     artwork for which the corresponding model should be created
         */
        private void AddFeatureMatchingModel(int artworkID) {
#if ENABLE_WINMD_SUPPORT
            // Create feature matching model
            if (this.artworks.ContainsKey(artworkID)) {
                Artwork artwork = this.artworks[artworkID];
                CW.FeatureMatchingModel fModel = new CW.FeatureMatchingModel(artwork.localImagePath, artworkID);

                // Add feature matching model
                this.matchRecognition.addModel(fModel);
                this.logger.Log("Artwork " + artworkID + " added to Companion.");
            }
#endif
        }

        /**
         * Remove a feature matching model.
         * 
         * @param artworkID     artwork for which the corresponding model should be removed
         */
        private void RemoveFeatureMatchingModel(int artworkID) {
#if ENABLE_WINMD_SUPPORT
            // Remove feature matching model
            if (this.artworks.ContainsKey(artworkID)) {
                this.matchRecognition.removeModel(artworkID);
                this.logger.Log("Artwork " + artworkID + " removed from Companion.");
            }
#endif
        }

        /**
         * Add new artworks to the object recognition process.
         */
        private void AddNewArtworks() {
#if ENABLE_WINMD_SUPPORT
            // Add artworks to the object recognition that are not anchored in the scene
            foreach (Artwork artwork in this.artworks.Values) {
                if (this.IsIdEligible(artwork.artworkID) && !this.worldAnchors[artwork.artworkID]) {
                    this.AddFeatureMatchingModel(artwork.artworkID);
                }
            }
#endif
        }

        /**
         * This method is called if some other collider enters this collider.
         * 
         * @param other the other collider
         */
        private void OnTriggerEnter(Collider other) {
            if (other.gameObject.tag.Equals(Tags.TAG_SCANNING_AREA)) {
                int id = other.transform.parent.GetComponent<ScanningArea>().artworkID;
                this.fmModelOperations.Enqueue(
                    new FmModelOperationInfo {
                        ArtworkID = id,
                        Operation = FmModelOperation.Add
                    }
                );
            }
        }

        /**
         * This method is called if some other collider exists this collider.
         * 
         * @param other the other collider
         */
        private void OnTriggerExit(Collider other) {
            if (other.gameObject.tag.Equals(Tags.TAG_SCANNING_AREA)) {
                int id = other.transform.parent.GetComponent<ScanningArea>().artworkID;
                this.fmModelOperations.Enqueue(
                    new FmModelOperationInfo {
                        ArtworkID = id,
                        Operation = FmModelOperation.Remove
                    }
                );
            }
        }

        /**
         * This method should be called to activate the ongoing scanning for artworks.
         * 
         * @param cachedMapping indicates whether cached mapping or live mapping was started
         */
        public void EnterScanningMode(bool cachedMapping) {
            this.CurrentScanState = (cachedMapping) ? ScanState.Caching : ScanState.LiveScan;
#if ENABLE_WINMD_SUPPORT
            if (this.CurrentScanState == ScanState.LiveScan) {
                // Choose processing method for live scanning
                switch (this.liveScanProcessing) {
                    case ImageProcessingMethod.FEATURE_MATCHING:
                        this.configuration.setProcessing(this.matchRecognition);
                        break;
                    case ImageProcessingMethod.IMAGE_HASHING:
                        this.configuration.setProcessing(this.hashRecognition);
                        break;
                    case ImageProcessingMethod.HYBRID_MATCHING:
                        this.configuration.setProcessing(this.hybridRecognition);
                        break;
                    default:
                        this.logger.Log("Image processing method not found.");
                        return;
                }
            } else {
                // The processing method for cached scanning should always be feature matching
                this.configuration.setProcessing(this.matchRecognition);
            }

            // Start Companion
            this.RunCompanion();

            // Add all artworks models to processing
            foreach (Artwork artwork in this.artworks.Values) {
                int artworkID = artwork.artworkID;
                if (this.IsIdEligible(artworkID)) {
                    this.matchRecognition.addModel(new CW.FeatureMatchingModel(artwork.localImagePath, artworkID));
                    this.hashRecognition.addModel(artwork.localImagePath, artworkID);
                    this.hybridRecognition.addModel(artwork.localImagePath, artworkID);
                    this.logger.Log("Artwork " + artworkID + " added to Companion.");
                }
            }

            // Activate photo capture
            this.capture.IsActive(true);
#endif
        }

        /**
         * This method should be called to activate the ongoing scanning for artworks.
         */
        public void EnterScanningMode() {
#if ENABLE_WINMD_SUPPORT
            this.fmModelOperations.Enqueue(
                new FmModelOperationInfo {
                    ArtworkID = -1,
                    Operation = FmModelOperation.AddNew
                }
            );
#endif
        }

        /**
         * This method should be called to deactivate the ongoing scanning for artworks.
         */
        public void LeaveScanningMode() {
#if ENABLE_WINMD_SUPPORT
            this.fmModelOperations.Enqueue(
                new FmModelOperationInfo {
                    ArtworkID = -1,
                    Operation = FmModelOperation.RemoveAll
                }
            );
#endif
        }

        /**
         * Reload the scene.
         */
        public void TabulaRasa() {
#if ENABLE_WINMD_SUPPORT
            WorldAnchorStore store = this.anchorManager.AnchorStore;
            if ((store != null) && CaptureManager.IsInitialized) {
                store.Clear();
                CaptureManager.Instance.Dispose(() => { UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name); });
            }
#endif
        }

        /**
         * Print out artworks that were not anchored yet.
         */
        public void PrintMissingAnchors() {
#if ENABLE_WINMD_SUPPORT
            foreach (Artwork artwork in this.artworks.Values) {
                if (this.IsIdEligible(artwork.artworkID) && !this.worldAnchors[artwork.artworkID]) {
                    this.logger.Log("Artwork " + artwork.artworkID + " was not anchored yet.");
                }
            }
#endif
        }

        /**
         * Easter egg activation method for the Napoleon asset.
         */
        public void EasterEgg() {
            GameObject nap = GameObject.FindWithTag(Tags.TAG_NAPOLEON);
            if (nap != null) {
                Animator anim = nap.GetComponent<Animator>();
                if (anim != null) {
                    anim.SetLayerWeight(1, 0.0f);
                    anim.SetTrigger(Triggers.TRIGGER_EASTEREGG);
                }
            }
        }

        /**
         * Print out all world anchors.
         */
        public void PrintAnchors() {
#if ENABLE_WINMD_SUPPORT
            WorldAnchorStore store = this.anchorManager.AnchorStore;
            if (store != null) {
                string[] ids = store.GetAllIds();
                for (int i = 0; i < ids.Length; i++) {
                    this.logger.Log(ids[i]);
                }
            }
#endif
        }

        /**
         * Check if the given ID lies within the eligible range.
         * 
         * @param id    artwork ID
         * @return  <code>true</code> if the ID is eligible, <code>flase</code> otherwise
         */
        private bool IsIdEligible(int id) {
            return (id >= 0) && (id <= (this.artworkLimit - 1));
        }
    }
}
