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

using HoloToolkit.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CompanionMR {

    /**
     * This class represents an info board for artworks.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    public class InfoBoard : MonoBehaviour {

        /**
         * Placement areas of the info board.
         */
        public enum InfoBoardPlacement {
            Left,
            Right
        }

        /**
         * Prefab for the artwork 2 3D-asset.
         */
        [Tooltip("Prefab for the artwork 2 3D-asset.")]
        public GameObject artwork2Asset;

        /**
         * Prefab for the artwork 3 3D-asset.
         */
        [Tooltip("Prefab for the artwork 3 3D-asset.")]
        public GameObject artwork3Asset;

        /**
         * Parent object for world anchors.
         */
        [Tooltip("Parent object for world anchors.")]
        public GameObject worldAnchorParent;

        /**
         * The RectTransform of the canvas of this info baord.
         */
        [Tooltip("The RectTransform of the canvas of this info baord.")]
        public RectTransform canvasRect;

        /**
         * The info button of this canvas.
         */
        [Tooltip("The info button of this canvas.")]
        public GameObject infoButton;

        /**
         * The cancle button of this canvas.
         */
        [Tooltip("The cancle button of this canvas.")]
        public GameObject cancleButton;

        /**
         * The switch button of this canvas.
         */
        [Tooltip("The switch button of this canvas.")]
        public GameObject switchButton;

        /**
         * The 3D button of this canvas.
         */
        [Tooltip("The 3D button of this canvas.")]
        public GameObject _3DButton;

        /**
         * The main panel of this canvas.
         */
        [Tooltip("The main panel of this canvas.")]
        public GameObject panel;

        /**
         * The title text of this info board.
         */
        [Tooltip("The title text of this info board.")]
        public Text titleText;

        /**
         * The artist image of this info board.
         */
        [Tooltip("The artist image of this info board.")]
        public Image artistImage;

        /**
         * The name of the artist of this info board.
         */
        [Tooltip("The name of the artist of this info board.")]
        public Text artistName;

        /**
         * The year of this info board.
         */
        [Tooltip("The year of this info board.")]
        public Text year;

        /**
         * The art style of this info board.
         */
        [Tooltip("The art style of this info board.")]
        public Text artStyle;

        /**
         * The detailed information of this info board.
         */
        [Tooltip("The detailed information of this info board.")]
        public Text detailedInfo;

        /**
         * Panel where the videos of this info board should be displayed.
         */
        [Tooltip("Panel where the videos of this info board should be displayed.")]
        public GameObject videoPanel;

        /**
         * Panel where the audio data of this info board should be displayed.
         */
        [Tooltip("Panel where the audio data of this info board should be displayed.")]
        public GameObject audioPanel;

        /**
         * Prefab panel that can be added for video playback.
         */
        [Tooltip("Prefab panel that can be added for video playback.")]
        public GameObject videoPrefab;

        /**
         * Prefab panel that can be added for audio playback.
         */
        [Tooltip("Prefab panel that can be added for audio playback.")]
        public GameObject audioPrefab;

        /**
         * Prefab of an audio source object to play video sound seperately from the video (unity bug).
         */
        [Tooltip("Prefab of an audio source object to play video sound seperately from the video (unity bug).")]
        public GameObject audioSourcePrefab;

        /**
         * Prefix for 3D asset anchors.
         */
        private const string ASSET_ANCHOR_PREFIX = "asset";

        /**
         * Anchor ID of artwork 2's 3D asset.
         */
        private const string ANCHOR_ID_M2 = ASSET_ANCHOR_PREFIX + "2";

        /**
         * Anchor ID of artwork 3's 3D asset.
         */
        private const string ANCHOR_ID_M3 = ASSET_ANCHOR_PREFIX + "3";

        /**
         * Point of interest for UI arrows (info board).
         */
        private static Vector3 POINT_OF_INTEREST_BO = new Vector3(-0.1f, -0.05f, 0.0f);

        /**
         * Point of interest for UI arrows (artwork 2).
         */
        private static Vector3 POINT_OF_INTEREST_M2 = new Vector3(0.0f, 0.4f, 0.0f);

        /**
         * Point of interest for UI arrows (artwork 3).
         */
        private static Vector3 POINT_OF_INTEREST_M3 = new Vector3(0.0f, 1.5f, 0.0f);

        /**
         * The recognized artwork of this canvas.
         */
        private RecognizedArtwork recognizedArtwork;

        /**
         * Current info board placement.
         */
        private InfoBoardPlacement currentPlacement;

        /**
         * Reference to the world anchor manager.
         */
        private WorldAnchorManager anchorManager;

        /**
         * Reference to the debug logger.
         */
        private DebugLogger logger;

        /**
         * Reference to the info canvas.
         */
        private InfoCanvas infoCanvas;

        /**
         * Reference to the UI arrows.
         */
        private UIArrows arrows;

        /**
         * The data object that holds all information about the actual artwork.
         */
        private Artwork artwork;

        /**
         * Position of the middle hit point of the recognized artwork on the wall in world coordinates.
         */
        private Vector3 middlePoint;

        /**
         * Reference to the instantiated 3D asset.
         */
        private GameObject instantiated3DAsset;

        /**
         * Reference to the GameObject that contains the world anchor for the 3D asset.
         */
        private GameObject assetAnchor;

        /**
         * Place the info board to the other side of the artwork.
         */
        public void SwitchSides() {

            if (this.recognizedArtwork != null) {

                // Determine the new placement
                this.currentPlacement = (this.currentPlacement == InfoBoardPlacement.Left) ? InfoBoardPlacement.Right : InfoBoardPlacement.Left;

                // Switch info board position
                this.recognizedArtwork.SwitchSides(this.currentPlacement);

                // Set position and rotation
                Vector2 oldPivot = this.canvasRect.pivot;
                if (this.currentPlacement == InfoBoardPlacement.Right) {
                    this.canvasRect.pivot = new Vector2(0.0f, 1.0f);
                    this.canvasRect.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                    this.canvasRect.localEulerAngles = new Vector3(this.canvasRect.localEulerAngles.x, 190.0f, this.canvasRect.localEulerAngles.z);
                } else {
                    this.canvasRect.pivot = new Vector2(1.0f, 1.0f);
                    this.canvasRect.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                    this.canvasRect.localEulerAngles = new Vector3(this.canvasRect.localEulerAngles.x, 170.0f, this.canvasRect.localEulerAngles.z);
                }

                // Switch sides inside the canvas
                RectTransform rectInfo = (RectTransform) this.infoButton.transform;
                RectTransform rectCancle = (RectTransform) this.cancleButton.transform;
                RectTransform rectSwitch = (RectTransform) this.switchButton.transform;
                RectTransform rect3D = (RectTransform) this._3DButton.transform;
                List<RectTransform> buttonRects = new List<RectTransform> {
                    rectInfo,
                    rectCancle,
                    rectSwitch,
                    rect3D
                };

                // Adjust the button positions
                foreach (RectTransform rect in buttonRects) {
                    // Flip the horizontal anchor values
                    float anchorsX = (rect.anchorMin.x + 1) % 2;
                    rect.anchorMin = new Vector2(anchorsX, 1.0f);
                    rect.anchorMax = new Vector2(anchorsX, 1.0f);

                    // Flip the horizontal position
                    float positionX = rect.localPosition.x * (-1);
                    rect.localPosition = new Vector3(positionX, -25.0f, 0.0f);
                }

                // Adjust the panels pivot to switch the scaling direction
                RectTransform rectPanel = (RectTransform) this.panel.transform;
                float pivotX = (oldPivot.x + 1) % 2;
                rectPanel.pivot = new Vector2(pivotX, 1.0f);
            }
        }

        /**
         * Set the data for this info board.
         * 
         * @param recognizedArtwork the recognized artwork information
         * @param placement         initial info board placement
         * @param artwork           data artwork for this info board
         * @param middlePoint       middle point of the recognized artwork
         */
        public void SetData(RecognizedArtwork recognizedArtwork, InfoBoardPlacement placement, Artwork artwork, Vector3 middlePoint) {
            this.anchorManager = WorldAnchorManager.Instance;
            this.logger = DebugLogger.Instance;
            this.infoCanvas = InfoCanvas.Instance;
            this.arrows = UIArrows.Instance;
            this.recognizedArtwork = recognizedArtwork;
            this.currentPlacement = placement;
            this.artwork = artwork;
            this.middlePoint = middlePoint;
            StartCoroutine(Initialize());
        }

        /**
         * Initialize this info board.
         * 
         * @return enumerator for usage as coroutine
         */
        private IEnumerator Initialize() {

            Artist artist = this.artwork.artist;
            string artistName = "-";

            // Load the artist image as a sprite
            if (artist != null) {
#if ENABLE_WINMD_SUPPORT
                if (System.IO.File.Exists(artist.localImagePath)) {
                    WWW www = new WWW("file:///" + artist.localImagePath);
                    //StartCoroutine(WaitForWWW(www));
                    Texture2D bmp = new Texture2D(www.texture.width, www.texture.height, TextureFormat.BC7, false);
                    www.LoadImageIntoTexture(bmp);

                    Vector2 pivot = new Vector2(0.5f, 0.5f);
                    Rect tRect = new Rect(0, 0, 160, 200);
                    this.artistImage.overrideSprite = Sprite.Create(bmp, tRect, pivot);
                    this.artistImage.gameObject.SetActive(true);
                }
#endif
                artistName = artist.name;
            }

            // Set artwork information
            this.titleText.text = this.artwork.title;
            this.artistName.text = artistName;
            this.year.text = this.artwork.year;
            this.artStyle.text = this.artwork.artStyle;
            this.detailedInfo.text = this.artwork.detailedInfo;

            // Set video data
            if (this.artwork.videoData.Count > 0) { this.videoPanel.SetActive(true); }
            foreach (Video video in this.artwork.videoData) {

                // Instantiate and set AudioSource (unity bug)
                GameObject audioSrcInst = Instantiate(this.audioSourcePrefab);
                AudioSource audioSrc = audioSrcInst.GetComponent<AudioSource>();
                video.audioSrc = audioSrc;

                // Instantiate a video panel and initialize with the video data
                GameObject videoInst = Instantiate(this.videoPrefab, this.videoPanel.transform);
                VideoPanel videoScript = videoInst.GetComponent<VideoPanel>();
                videoScript.SetData(video);

                yield return null;
            }

            // Set audio data
            if (this.artwork.audioData.Count > 0) { this.audioPanel.SetActive(true); }
            foreach (Audio audio in this.artwork.audioData) {
                GameObject audioInst = Instantiate(this.audioPrefab, this.audioPanel.transform);
                AudioPanel audioScript = audioInst.GetComponent<AudioPanel>();
                audioScript.SetData(audio);
                yield return null;
            }

            // 3D asset calculations
            this.Perform3DAssetCalculations();

            yield return null;

            // Display info board
            this.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

            // Notify the user about the new instantiated object
            GameObject lookAtTarget = new GameObject();
            lookAtTarget.transform.SetParent(this.gameObject.transform, false);
            lookAtTarget.transform.Translate(POINT_OF_INTEREST_BO);
            this.arrows.targetTransform = lookAtTarget.transform;
        }

        /**
         * Perform necessary calculations for the 3D asset of this artwork.
         */
        private void Perform3DAssetCalculations() {
            bool found = false;
#if ENABLE_WINMD_SUPPORT
            // Check if a world anchor of this artwork exists
            UnityEngine.VR.WSA.Persistence.WorldAnchorStore store = this.anchorManager.AnchorStore;
            if (store != null) {
                string[] ids = store.GetAllIds();

                // Try to find the world anchor of the 3D asset
                this.logger.Log("Find 3D assets's world anchor:");
                for (int index = 0; (index < ids.Length) && !found; index++) {
                    string anchorID = ids[index];

                    // The anchor ID should have a specific format
                    if ((anchorID.Length > ASSET_ANCHOR_PREFIX.Length) && anchorID.Substring(0, ASSET_ANCHOR_PREFIX.Length).Equals(ASSET_ANCHOR_PREFIX)) {
                        try {
                            int id = int.Parse(anchorID.Substring(ASSET_ANCHOR_PREFIX.Length));
                            if (id == this.artwork.artworkID) {
                                this.logger.Log(" -> found: " + anchorID);
                                this._3DButton.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                                this._3DButton.GetComponent<Button>().interactable = true;
                                found = true;
                            }
                        } catch (System.Exception ex) {
                            this.logger.Log("EXCEPTION: " + ex.Message);
                        }
                    }
                }
            }
#endif
            // Try to query SpatialUnderstanding if a world anchor was not found
            if (!found) {
                SpatialUnderstanding suInstance = SpatialUnderstanding.Instance;
                ShapeDetection shapeDetection = ShapeDetection.Instance;
                PlacementSolver solver = PlacementSolver.Instance;
                HoloToolkit.Unity.InputModule.InputManager inputManager = HoloToolkit.Unity.InputModule.InputManager.Instance;
                if ((suInstance != null) && (solver != null) && (shapeDetection != null) && (inputManager != null) && (suInstance.ScanState == SpatialUnderstanding.ScanStates.Done) && suInstance.AllowSpatialUnderstanding) {
                    switch (this.artwork.artworkID) {
                        case 2:
                            inputManager.PushInputDisable();
                            this._3DButton.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                            shapeDetection.FindTable(this.middlePoint, this.HandleArtwork2);
                            break;
                        case 3:
                            inputManager.PushInputDisable();
                            this._3DButton.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                            solver.Query_OnFloor_NearPoint(this.middlePoint, false, this.HandleArtwork3);
                            break;
                    }
                }
            }
        }

        /**
         * Handle the the placement query for artwork 2.
         * 
         * @param success   indicates whether the placement query was successful
         * @param position  position result of the placement query
         * @param alignment user space alignment
         */
        private void HandleArtwork2(bool success, Vector3 position, SpatialUnderstandingDll.Imports.PlayspaceAlignment alignment) {
            if (success) {
                // Calculate the rotation of the 3D asset
                int maxIndex = 0;
                Vector3 artworkForward = this.transform.forward;
                Vector3[] vectors = new Vector3[] {
                    alignment.BasisX,
                    alignment.BasisX * (-1),
                    alignment.BasisZ,
                    alignment.BasisZ * (-1)
                };
                float[] dots = new float[] {
                    Vector3.Dot(alignment.BasisX, artworkForward),
                    Vector3.Dot(alignment.BasisX * (-1), artworkForward),
                    Vector3.Dot(alignment.BasisZ, artworkForward),
                    Vector3.Dot(alignment.BasisZ * (-1), artworkForward)
                };
                for (int i = 1; i < dots.Length; i++) {
                    maxIndex = (dots[maxIndex] >= dots[i]) ? maxIndex : i;
                }

                // Instantiate the world anchor and the 3D asset
                this.assetAnchor = Instantiate(this.worldAnchorParent, position, Quaternion.LookRotation(vectors[maxIndex], alignment.BasisY));
                string anchorID = ANCHOR_ID_M2;
                this.anchorManager.RemoveAnchor(anchorID);
                this.anchorManager.AttachAnchor(this.assetAnchor, anchorID);
                this.instantiated3DAsset = Instantiate(this.artwork2Asset, this.assetAnchor.transform);
                this._3DButton.GetComponent<Button>().interactable = true;
                this._3DButton.GetComponent<Display3DAsset>().ActivateVisuals();
            } else {
                string output = "An adequate table surface couldn't be found.";
                this.logger.Log(output);
                this.infoCanvas.SetInfoText(output);
            }

            // Activate input again
            if (HoloToolkit.Unity.InputModule.InputManager.IsInitialized) {
                HoloToolkit.Unity.InputModule.InputManager.Instance.PopInputDisable();
            }
        }

        /**
         * Handle the the placement query for artwork 3.
         * 
         * @param success   indicates whether the placement query was successful
         * @param position  position result of the placement query
         */
        private void HandleArtwork3(bool success, Vector3 position) {
            if (success) {
                // Instantiate the world anchor and the 3D object
                this.assetAnchor = Instantiate(this.worldAnchorParent, position, this.transform.rotation);
                string anchorID = ANCHOR_ID_M3;
                this.anchorManager.RemoveAnchor(anchorID);
                this.anchorManager.AttachAnchor(this.assetAnchor, anchorID);
                this.instantiated3DAsset = Instantiate(this.artwork3Asset, this.assetAnchor.transform);
                this._3DButton.GetComponent<Button>().interactable = true;
                this._3DButton.GetComponent<Display3DAsset>().ActivateVisuals();
            } else {
                string output = "An adequate floor position couldn't be found.";
                this.logger.Log(output);
                this.infoCanvas.SetInfoText(output);
            }

            // Activate input again
            if (HoloToolkit.Unity.InputModule.InputManager.IsInitialized) {
                HoloToolkit.Unity.InputModule.InputManager.Instance.PopInputDisable();
            }
        }

        ///**
        // * Wait for the access request to be finished and obtain the image data.
        // * 
        // * @param www   the web request/response object (used for local file access)
        // * @return enumerator for usage as coroutine
        // */
        //private IEnumerator WaitForWWW(WWW www) {

        //    // Wait for the WebRequest to be finished
        //    yield return www;

        //    Texture2D bmp = new Texture2D(www.texture.width, www.texture.height, TextureFormat.BC7, false);
        //    www.LoadImageIntoTexture(bmp);

        //    Vector2 pivot = new Vector2(0.5f, 0.5f);
        //    Rect tRect = new Rect(0, 0, 160, 200);
        //    this.artistImage.overrideSprite = Sprite.Create(bmp, tRect, pivot);
        //    this.artistImage.gameObject.SetActive(true);
        //}

        /**
         * Activate the 3D asset.
         */
        public void Activate3D() {

            // Get the right prefab and anchor ID
            string anchorID = "";
            GameObject prefab = null;
            Vector3 arrowTranslation = Vector3.zero;
            switch (this.artwork.artworkID) {
                case 2:
                    anchorID = ANCHOR_ID_M2;
                    prefab = this.artwork2Asset;
                    arrowTranslation = POINT_OF_INTEREST_M2;
                    break;
                case 3:
                    anchorID = ANCHOR_ID_M3;
                    prefab = this.artwork3Asset;
                    arrowTranslation = POINT_OF_INTEREST_M3;
                    break;
            }

            // Create a new world anchor and synchronize it before binding
            if (this.assetAnchor == null) {
                this.assetAnchor = Instantiate(this.worldAnchorParent);
                this.anchorManager.AttachAnchor(this.assetAnchor, anchorID);
            }

            // Initialize the 3D artwork
            this.instantiated3DAsset = Instantiate(prefab, this.assetAnchor.transform);

            // Notify the user about the new instantiated object
            GameObject lookAtTarget = new GameObject();
            lookAtTarget.transform.SetParent(this.instantiated3DAsset.transform, false);
            lookAtTarget.transform.Translate(arrowTranslation);
            this.arrows.targetTransform = lookAtTarget.transform;
        }

        /**
         * Deactivate the 3D asset.
         */
        public void Deactivate3D() {
            Destroy(this.instantiated3DAsset);
        }

    }
}
