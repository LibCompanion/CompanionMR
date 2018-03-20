/*
 * Original work:
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 * 
 * Modified work:
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
using HoloToolkit.Unity;
using HoloToolkit.Examples.SpatialUnderstandingFeatureOverview;

namespace CompanionMR {

    /**
     * This class provides a shape detection mechanism.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    public class ShapeDetection : LineDrawer {

        /**
         * Delegate for a response callback method for shape queries.
         * 
         * @param success   indicates whether the placement query was successful
         * @param position  position result of the placement query
         * @param alignment user space alignment
         */
        public delegate void ResponseDelegate(bool success, Vector3 position, SpatialUnderstandingDll.Imports.PlayspaceAlignment alignment);

        /**
         * Singleton instance.
         */
        public static ShapeDetection Instance { get; private set; }

        /**
         * Maximum amount of detected shapes.
         */
        private const int MAX_SHAPES = 32;

        /**
         * Maximum distance of a detected shape from a specified position.
         */
        private const float MAX_DISTANCE = 2.0f;

        /**
         * Minimum height of a detected shape.
         */
        private const float MIN_HEIGHT = 0.55f;

        /**
         * Maximum height of a detected shape.
         */
        private const float MAX_HEIGHT = 1.5f;

        /**
         * Minimum width of the detected shape.
         */
        private const float MIN_WIDTH = 0.35f;

        /**
         * Minimum depth of the detected shape.
         */
        private const float MIN_DEPTH = 0.35f;

        /**
         * Upper boundry for distance calculations.
         */
        private const float NULL_DISTANCE = 10000.0f;

        /**
         * List of animated boxes to represent the detected shapes.
         */
        private List<AnimatedBox> lineBoxList;

        /**
         * Shape result objects.
         */
        private SpatialUnderstandingDllShapes.ShapeResult[] resultsShape;

        /**
         * Use this for initialization that has to be done before the Start() function of other GameObjects is invoked.
         */
        private void Awake() {
            if (ShapeDetection.Instance == null) {
                ShapeDetection.Instance = this;
            }
        }

        /**
         * This method is called when this object will be destroyed.
         */
        protected override void OnDestroy() {
            base.OnDestroy();
            if (ShapeDetection.Instance == this) {
                ShapeDetection.Instance = null;
            }
        }

        /**
         * Use this for initialization.
         */
        private void Start() {
            // Variable initialization
            this.lineBoxList = new List<AnimatedBox>();
            this.resultsShape = new SpatialUnderstandingDllShapes.ShapeResult[MAX_SHAPES];
        }

        /**
         * Update is called once per frame.
         */
        private void Update() {

            // Lines: Begin
            this.LineDraw_Begin();

            // Drawers
            bool needsUpdate = false;
            needsUpdate |= this.Draw_LineBoxList();

            // Lines: Finish up
            this.LineDraw_End(needsUpdate);
        }

        /**
         * Draw the animated boxes.
         * 
         * @return <code>true</code> if the drawing is not finished and needs to be updated, <code>false</code> otherwise
         */
        private bool Draw_LineBoxList() {
            bool needsUpdate = false;
            for (int i = 0; i < this.lineBoxList.Count; i++) {
                needsUpdate |= this.Draw_AnimatedBox(this.lineBoxList[i]);
            }
            return needsUpdate;
        }

        /**
         * Remove all detected shapes.
         */
        public void ClearGeometry() {
            this.lineBoxList = new List<AnimatedBox>();
            this.resultsShape = new SpatialUnderstandingDllShapes.ShapeResult[MAX_SHAPES];
        }

        /**
         * Try to detect a table shape near the given position.
         * 
         * @param nearPos   position near which the shape should be detected
         * @param callback  callback method    
         */
        public void FindTable(Vector3 nearPos, ResponseDelegate callback) {
            this.Query_Shape_FindShapeHalfDims("Table", nearPos, false, callback);
        }

        /**
         * Try to detect all table shapes in the scene.
         */
        public void FindAllTables() {
            this.Query_Shape_FindShapeHalfDims("Table", null, true, null);
        }

        /**
         * Query the SpatialUnderstanding DLL for a specific shape.
         * 
         * @param shapeName     name of the shape that should be detected
         * @param nearPos       position near which the shape should be detected
         * @param drawBox       indicates whether an animated box should be drawn around the shape
         * @param callback      callback method
         */
        private void Query_Shape_FindShapeHalfDims(string shapeName, Vector3? nearPos, bool drawBox, ResponseDelegate callback) {
            // Check query permission
            if (!SpatialUnderstanding.IsInitialized || !SpatialUnderstanding.Instance.AllowSpatialUnderstanding) {
                return;
            }

            // Query
            System.IntPtr resultsShapePtr = SpatialUnderstanding.Instance.UnderstandingDLL.PinObject(this.resultsShape);
            int shapeCount = SpatialUnderstandingDllShapes.QueryShape_FindShapeHalfDims(shapeName, this.resultsShape.Length, resultsShapePtr);

            // Output
            this.HandleResults("Find Shape Min/Max '" + shapeName + "'", shapeCount, Color.blue, new Vector3(0.25f, 0.025f, 0.25f), nearPos, drawBox, callback);
        }

        /**
         * Handle the shape query results.
         * 
         * @param visDesc           visual description
         * @param shapeCount        number of detected shapes
         * @param color             color of the animated box
         * @param defaultHalfDims   default shape dimensions
         * @param nearPos           position near which the shape should be detected
         * @param drawBox           indicates whether an animated box should be drawn around the shape
         * @param callback          callback method
         */
        private void HandleResults(string visDesc, int shapeCount, Color color, Vector3 defaultHalfDims, Vector3? nearPos, bool drawBox, ResponseDelegate callback) {
            // Check query permission
            if (!SpatialUnderstanding.IsInitialized || !SpatialUnderstanding.Instance.AllowSpatialUnderstanding) {
                return;
            }

            // Alignment information
            SpatialUnderstandingDll.Imports.QueryPlayspaceAlignment(SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceAlignmentPtr());
            SpatialUnderstandingDll.Imports.PlayspaceAlignment alignment = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceAlignment();

            // Process results
            float foundDistance = NULL_DISTANCE;
            Vector3 foundPosition = Vector3.zero;
            for (int i = 0; i < shapeCount; i++) {
                bool valid = true;
                Vector3 halfSize = (this.resultsShape[i].halfDims.sqrMagnitude < 0.01f) ? defaultHalfDims : this.resultsShape[i].halfDims;
                Vector3 position = new Vector3(this.resultsShape[i].position.x, this.resultsShape[i].position.y - halfSize.y, this.resultsShape[i].position.z);

                // Check distance
                if ((nearPos != null) && (callback != null)) {
                    
                    // Check MIN_DISTANCE, MIN_HEIGHT and MAX_HEIGHT
                    float distance = Vector3.Distance(nearPos.Value, position);
                    valid = (distance <= MAX_DISTANCE) && (position.y >= (alignment.FloorYValue + MIN_HEIGHT)) && (position.y <= (alignment.FloorYValue + MAX_HEIGHT));

                    // Ceck MIN_WIDTH and MIN_DEPTH
                    float width = (this.resultsShape[i].halfDims.x >= this.resultsShape[i].halfDims.z) ? this.resultsShape[i].halfDims.x : this.resultsShape[i].halfDims.z;
                    float depth = (this.resultsShape[i].halfDims.x < this.resultsShape[i].halfDims.z) ? this.resultsShape[i].halfDims.x : this.resultsShape[i].halfDims.z;
                    valid = valid && (width >= (MIN_WIDTH * 0.5)) && (depth >= (MIN_DEPTH * 0.5));

                    // Save position and distance if it is optimal
                    if (valid) {
                        foundDistance = Mathf.Min(foundDistance, distance);
                        foundPosition = (foundDistance == distance) ? position : foundPosition;
                    }
                }

                // Draw an animated box
                if (drawBox && valid) {
                    float timeDelay = this.lineBoxList.Count * AnimatedBox.DelayPerItem;
                    this.lineBoxList.Add(new AnimatedBox(timeDelay, this.resultsShape[i].position, Quaternion.LookRotation(alignment.BasisZ, alignment.BasisY), color, halfSize));
                }
            }

            // Send the position and rotation information to the callback function
            if ((nearPos != null) && (callback != null) && (shapeCount != 0) && (foundDistance < NULL_DISTANCE)) {
                Debug.Log("Shape distance: " + foundDistance);
                callback(true, foundPosition, alignment);
            } else if ((nearPos != null) && (callback != null)) {
                callback(false, Vector3.zero, alignment);
            }

            Debug.Log(string.Format("{0} ({1})", visDesc, shapeCount));
        }
    }
}
