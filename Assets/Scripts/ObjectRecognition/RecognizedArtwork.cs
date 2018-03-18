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
using HoloToolkit.Unity.SpatialMapping;
using UnityEngine;

namespace CompanionMR {

    /**
     * This class represents a recognized artwork.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    public class RecognizedArtwork : MonoBehaviour {

        /**
         * Prefab for the info board.
         */
        [Tooltip("Prefab for the info board.")]
        public GameObject infoBoard;

        /**
         * Prefab for the scanning area.
         */
        [Tooltip("Prefab for the scanning area.")]
        public GameObject scanningArea;

        /**
         * (Debug) Prefab for the rectangle that represents the recognized artwork's shape.
         */
        [Tooltip("(Debug) Prefab for the rectangle that represents the recognized artwork's shape.")]
        public GameObject debugRect;

        /**
         * (Debug) Material for the lines drawn on screen to visualize the recognized artwork.
         */
        [Tooltip("(Debug) Material for the lines drawn on screen to visualize the recognized artwork.")]
        public Material debugLineMat;

        /**
         * Maximum raycast length (in meters).
         */
        [Tooltip("Maximum raycast length (in meters).")]
        [Range(1.0f, 20.0f)]
        public float raycastLength = 10.0f;

        /**
         * Side of the artwork on which the info board should be displayed.
         */
        [Tooltip("Side of the artwork on which the info board should be displayed.")]
        public InfoBoard.InfoBoardPlacement infoBoardSide = InfoBoard.InfoBoardPlacement.Right;

        /**
         * Prefix for artwork anchors.
         */
        public const string ARTWORK_ANCHOR_PREFIX = "artwork";

        /**
         * Threshold for the cross product between the normals of the recognized points (artwork's corners).
         */
        private const float VALIDITY_THRESHOLD = 0.97f;

        /**
         * Reference to the debug logger.
         */
        private DebugLogger logger;

        /**
         * The data object that holds all information about the actual artwork.
         */
        private Artwork artwork;

        /**
         * Indicates whether there is new data that has to be processed.
         */
        private bool newData;

        /**
         * Indicates whether this recognized artwork is valid.
         */
        private bool valid;

        /**
         * World space coordinates of the camera.
         */
        private Vector3 worldSpaceCamera;

        /**
         * World space coordinates of the middle point of the recognized area.
         */
        private Vector3 worldSpaceMiddle;

        /**
         * World space coordinates of the top left corner of the recognized area.
         */
        private Vector3 worldSpaceTopLeft;

        /**
         * World space coordinates of the top right corner of the recognized area.
         */
        private Vector3 worldSpaceTopRight;

        /**
         * World space coordinates of the bottom right corner of the recognized area.
         */
        private Vector3 worldSpaceBottomRight;

        /**
         * World space coordinates of the bottom left corner of the recognized area.
         */
        private Vector3 worldSpaceBottomLeft;

        /**
         * Raycast hit information of the raycast from camera to the wall (middle point).
         */
        private RaycastHit hitInfoMiddle;

        /**
         * Raycast hit information of the raycast from camera to the wall (top left corner).
         */
        private RaycastHit hitInfoTopLeft;

        /**
         * Raycast hit information of the raycast from camera to the wall (top right corner).
         */
        private RaycastHit hitInfoTopRight;

        /**
         * Raycast hit information of the raycast from camera to the wall (bottom right corner).
         */
        private RaycastHit hitInfoBottomRight;

        /**
         * Raycast hit information of the raycast from camera to the wall (bottom left corner).
         */
        private RaycastHit hitInfoBottomLeft;

        /**
         * Position of the top left hit point in local coordinates (parent: world anchor).
         */
        private Vector3 topLeft;

        /**
         * Position of the top right hit point in local coordinates (parent: world anchor).
         */
        private Vector3 topRight;

        /**
         * Reference to the instantiated info board GameObject.
         */
        private GameObject instInfoBoard;

        /**
         * Reference to the instantiated world anchor GameObject.
         */
        private GameObject anchor;

        /**
         * Reference to the world anchor manager.
         */
        private WorldAnchorManager anchorManager;

        /**
         * Reference to the spatial mapping manager.
         */
        private SpatialMappingManager spatialMapping;

        /**
         * Use this for initialization.
         */
        private void Start() {

            // Variable initialization (the other variables are set by a 'SetNewData' call)
            this.valid = false;

            // Get reference to other scripts in the scene
            this.anchorManager = WorldAnchorManager.Instance;
            this.spatialMapping = SpatialMappingManager.Instance;
            this.logger = DebugLogger.Instance;

            // Deactivate this script if necessary components are missing
            if ((this.infoBoard == null) || (this.scanningArea == null) || (this.artwork == null) || (this.anchorManager == null) || (this.spatialMapping == null) || (this.logger == null)) {
                Debug.LogError("RecognizedArtwork: Script references not set properly.");
                this.enabled = false;
            }
        }

        /**
         * Update is called once per frame.
         */
        private void Update() {

            // Find the recognized artwork in the 3D world by raycasting from the camera world position through the pixel coordinates (in world space)
            if (this.newData) {

                // Hitting an antiOccultationLayer
                if (Physics.Raycast(this.worldSpaceCamera, this.worldSpaceMiddle - this.worldSpaceCamera, out this.hitInfoMiddle, this.raycastLength)
                    && Physics.Raycast(this.worldSpaceCamera, this.worldSpaceTopLeft - this.worldSpaceCamera, out this.hitInfoTopLeft, this.raycastLength, this.spatialMapping.LayerMask)
                    && Physics.Raycast(this.worldSpaceCamera, this.worldSpaceTopRight - this.worldSpaceCamera, out this.hitInfoTopRight, this.raycastLength, this.spatialMapping.LayerMask)
                    && Physics.Raycast(this.worldSpaceCamera, this.worldSpaceBottomRight - this.worldSpaceCamera, out this.hitInfoBottomRight, this.raycastLength, this.spatialMapping.LayerMask)
                    && Physics.Raycast(this.worldSpaceCamera, this.worldSpaceBottomLeft - this.worldSpaceCamera, out this.hitInfoBottomLeft, this.raycastLength, this.spatialMapping.LayerMask)) {

                    // Calculate cross and dot products for validation
                    Vector3 cross1 = Vector3.Cross(this.hitInfoTopRight.point - this.hitInfoTopLeft.point, this.hitInfoMiddle.point - this.hitInfoTopLeft.point).normalized;
                    Vector3 cross2 = Vector3.Cross(this.hitInfoBottomRight.point - this.hitInfoTopRight.point, this.hitInfoMiddle.point - this.hitInfoTopRight.point).normalized;
                    Vector3 cross3 = Vector3.Cross(this.hitInfoBottomLeft.point - this.hitInfoBottomRight.point, this.hitInfoMiddle.point - this.hitInfoBottomRight.point).normalized;
                    Vector3 cross4 = Vector3.Cross(this.hitInfoTopLeft.point - this.hitInfoBottomLeft.point, this.hitInfoMiddle.point - this.hitInfoBottomLeft.point).normalized;
                    float dot1 = Vector3.Dot(cross1, cross3);
                    float dot2 = Vector3.Dot(cross2, cross4);
                    this.logger.Log(this.name + " Corner validity 1: " + dot1 * 100 + "%");
                    this.logger.Log(this.name + " Corner validity 2: " + dot2 * 100 + "%");

                    // Approve validity
                    if ((dot1 > VALIDITY_THRESHOLD) && (dot2 > VALIDITY_THRESHOLD)) {

                        this.valid = true;

                        // Calculate the position and rotation of the world anchor prefab
                        Vector3 position = this.hitInfoMiddle.point;
                        Quaternion rotation = Utils.CreateLookAtRotation(this.hitInfoMiddle.normal);
                        this.anchor = Instantiate(this.scanningArea, position, rotation);
                        this.anchor.transform.Translate(new Vector3(0.0f, 0.0f, 1.5f));

                        this.anchorManager.RemoveAnchor(this.name);
                        this.anchorManager.AttachAnchor(this.anchor, this.name);

#if COMP_DEBUG_RAYS
                        // Draw lines to represent light rays
                        if (this.debugLineMat != null) {
                            Color blue = new Color(0, 0, 255);
                            Utils.DrawLine(this.worldSpaceCamera, this.worldSpaceMiddle, 0.01f, blue, this.debugLineMat, this.anchor.transform);
                            Utils.DrawLine(this.worldSpaceCamera, this.worldSpaceTopLeft, 0.01f, blue, this.debugLineMat, this.anchor.transform);
                            Utils.DrawLine(this.worldSpaceCamera, this.worldSpaceTopRight, 0.01f, blue, this.debugLineMat, this.anchor.transform);
                            Utils.DrawLine(this.worldSpaceCamera, this.worldSpaceBottomRight, 0.01f, blue, this.debugLineMat, this.anchor.transform);
                            Utils.DrawLine(this.worldSpaceCamera, this.worldSpaceBottomLeft, 0.01f, blue, this.debugLineMat, this.anchor.transform);

                            Color red = new Color(255, 0, 0);
                            Utils.DrawLine(this.worldSpaceMiddle, this.hitInfoMiddle.point, 0.01f, red, this.debugLineMat, this.anchor.transform);
                            Utils.DrawLine(this.worldSpaceTopLeft, this.hitInfoTopLeft.point, 0.01f, red, this.debugLineMat, this.anchor.transform);
                            Utils.DrawLine(this.worldSpaceTopRight, this.hitInfoTopRight.point, 0.01f, red, this.debugLineMat, this.anchor.transform);
                            Utils.DrawLine(this.worldSpaceBottomRight, this.hitInfoBottomRight.point, 0.01f, red, this.debugLineMat, this.anchor.transform);
                            Utils.DrawLine(this.worldSpaceBottomLeft, this.hitInfoBottomLeft.point, 0.01f, red, this.debugLineMat, this.anchor.transform);
                        }
#endif
#if COMP_DEBUG_RECT
                        // Instantiate a rectangle around the recognized artwork
                        if (this.debugRect != null) {
                            position = this.hitInfoTopLeft.point;
                            rotation = Utils.CreateLookAtRotation(-1 * this.hitInfoMiddle.normal);
                            GameObject rect = Instantiate(this.debugRect, position, rotation, this.anchor.transform);
                            LineRenderer lr = rect.GetComponent<LineRenderer>();

                            Vector3[] positions = new Vector3[5];
                            positions[0] = rect.transform.InverseTransformPoint(this.hitInfoTopLeft.point);
                            positions[1] = rect.transform.InverseTransformPoint(this.hitInfoTopRight.point);
                            positions[2] = rect.transform.InverseTransformPoint(this.hitInfoBottomRight.point);
                            positions[3] = rect.transform.InverseTransformPoint(this.hitInfoBottomLeft.point);
                            positions[4] = rect.transform.InverseTransformPoint(this.hitInfoTopLeft.point);
                            lr.positionCount = positions.Length;
                            lr.SetPositions(positions);
                        }
#endif

                        // Calculate the position and rotation of the info board
                        position = (this.infoBoardSide == InfoBoard.InfoBoardPlacement.Right) ? this.hitInfoTopRight.point : this.hitInfoTopLeft.point;
                        rotation = Utils.CreateLookAtRotation(this.hitInfoMiddle.normal);

                        // Instantiate the info board
                        this.instInfoBoard = Instantiate(this.infoBoard, position, rotation, this.anchor.transform);
                        this.instInfoBoard.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f); // hide the info board until all data is loaded
                        InfoBoard board = this.instInfoBoard.GetComponent<InfoBoard>();

                        // Translate the info board relatively to the transform's local axes
                        if (this.infoBoardSide == InfoBoard.InfoBoardPlacement.Right) {
                            this.instInfoBoard.transform.Translate(-0.1f, 0.1f, 0.1f);
                        } else {
                            this.instInfoBoard.transform.Translate(0.1f, 0.1f, 0.1f);
                        }

                        // Transform world positions into the world anchors local position
                        this.topLeft = this.anchor.transform.InverseTransformPoint(this.hitInfoTopLeft.point);
                        this.topRight = this.anchor.transform.InverseTransformPoint(this.hitInfoTopRight.point);

                        // Set data for the info board
                        board.SetData(this, this.infoBoardSide, this.artwork, this.hitInfoMiddle.point);

                    } else {
                        this.logger.Log(this.name + ": cross product invalid.");
                    }

                } else {
                    this.logger.Log(this.name + ": raycasting failed.");
                }

                this.newData = false;
            }
        }

        /**
         * Set newly recognized data for this artwork.
         * 
         * @param info  recognition infomation
         */
        public void SetNewData(Recognition info) {
            this.artwork = info.Artwork;
            this.name = ARTWORK_ANCHOR_PREFIX + this.artwork.artworkID;
            this.worldSpaceCamera = info.WorldSpaceCamera;
            this.worldSpaceMiddle = info.WorldSpaceMiddle;
            this.worldSpaceTopLeft = info.WorldSpaceTopLeft;
            this.worldSpaceTopRight = info.WorldSpaceTopRight;
            this.worldSpaceBottomLeft = info.WorldSpaceBottomLeft;
            this.worldSpaceBottomRight = info.WorldSpaceBottomRight;
            this.newData = true;
        }

        /**
         * Indicate whether the calculated coordinates are valid or not, respectively if the validation is not finished yet.
         * 
         * @return Returns <code>1</code> if the calculated coordinates are valid, <code>0</code> if they are not and <code>-1</code> if the validation process is not finished yet.
         */
        public int IsValid() {
            int valid = -1;
            if (!this.newData) {
                valid = (this.valid) ? 1 : 0;
            }
            return valid;
        }

        /**
         * This method is called when this GameObject will be destroyed.
         */
        private void OnDestroy() {
#if ENABLE_WINMD_SUPPORT
            this.anchorManager.AnchorStore.Delete(this.name);
#endif
        }

        /**
         * Switch the placement of the info board.
         * 
         * @param placement determines on which side the info board should be placed
         */
        public void SwitchSides(InfoBoard.InfoBoardPlacement placement) {

            // Determine the placement
            this.infoBoardSide = placement;

            // Set position and rotation
            this.instInfoBoard.transform.position = (this.infoBoardSide == InfoBoard.InfoBoardPlacement.Right)
                ? this.anchor.transform.TransformPoint(this.topRight) : this.anchor.transform.TransformPoint(this.topLeft);
            if (this.infoBoardSide == InfoBoard.InfoBoardPlacement.Right) {
                this.instInfoBoard.transform.Translate(-0.1f, 0.1f, 0.1f);
            } else {
                this.instInfoBoard.transform.Translate(0.1f, 0.1f, 0.1f);
            }

        }
    }
}
