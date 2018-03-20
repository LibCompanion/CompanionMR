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

using UnityEngine;
using HoloToolkit.Unity;
using System.Collections.Generic;
using System;

using ShapeComp = HoloToolkit.Unity.SpatialUnderstandingDllShapes.ShapeComponent;
using ShapeConst = HoloToolkit.Unity.SpatialUnderstandingDllShapes.ShapeConstraint;
using ShapeCompConst = HoloToolkit.Unity.SpatialUnderstandingDllShapes.ShapeComponentConstraint;

namespace CompanionMR {

    /**
     * This class creates and manages shape definitions for spatial understanding.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    public class ShapeDefinition : Singleton<ShapeDefinition> {

        /**
         * Indicates whether all shape definitions has been created.
         */
        public bool HasCreatedShapes { get; private set; }

        /**
         * Use this for initialization.
         */
        private void Start() {
            if (SpatialUnderstanding.IsInitialized) {
                SpatialUnderstanding.Instance.ScanStateChanged += this.OnScanStateChanged;
            }
        }

        /**
         * This method is called when this object will be destroyed.
         */
        protected override void OnDestroy() {
            base.OnDestroy();
            if (SpatialUnderstanding.IsInitialized) {
                SpatialUnderstanding.Instance.ScanStateChanged -= this.OnScanStateChanged;
            }
        }

        /**
         * Create shape definitions.
         */
        public void CreateShapes() {
            if (this.HasCreatedShapes || !SpatialUnderstanding.IsInitialized || !SpatialUnderstanding.Instance.AllowSpatialUnderstanding) {
                return;
            }

            // Create definitions and analyze
            this.CreateCustomShapeDefinitions();
            SpatialUnderstandingDllShapes.ActivateShapeAnalysis();
        }

        /**
         * This method is called when the SpatialUnderstanding state has changed.
         */
        private void OnScanStateChanged() {
            // If we are leaving the None state, go ahead and register shapes now
            if (SpatialUnderstanding.IsInitialized && (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Done)) {
                // Create definitions and analyze
                this.CreateShapes();
            }
        }

        /**
         * Add a new shape definition.
         * 
         * @param shapeName         shape name
         * @param shapeComponents   list of shape components
         * @param shapeConstraints  list of shape constraints
         */
        private void AddShape(string shapeName, List<ShapeComp> shapeComponents, List<ShapeConst> shapeConstraints = null) {

            if (!SpatialUnderstanding.IsInitialized || !SpatialUnderstanding.Instance.AllowSpatialUnderstanding) {
                return;
            }

            IntPtr shapeComponentsPtr = (shapeComponents == null) ? IntPtr.Zero : SpatialUnderstanding.Instance.UnderstandingDLL.PinObject(shapeComponents.ToArray());
            IntPtr shapeConstraintsPtr = (shapeConstraints == null) ? IntPtr.Zero : SpatialUnderstanding.Instance.UnderstandingDLL.PinObject(shapeConstraints.ToArray());
            int shapeComponentCount = (shapeComponents == null) ? 0 : shapeComponents.Count;
            int shapeConstraintCount = (shapeConstraints == null) ? 0 : shapeConstraints.Count;

            if (SpatialUnderstandingDllShapes.AddShape(shapeName, shapeComponentCount, shapeComponentsPtr, shapeConstraintCount, shapeConstraintsPtr) == 0) {
                Debug.LogError("Failed to create custom shape description.");
            }
        }

        /**
         * Create custom shape definitions.
         */
        private void CreateCustomShapeDefinitions() {

            if (!SpatialUnderstanding.IsInitialized || !SpatialUnderstanding.Instance.AllowSpatialUnderstanding) {
                return;
            }

            List<ShapeComp> shapeComponents;
            List<ShapeConst> shapeConstraints;

            // Table
            shapeComponents = new List<ShapeComp>() { new ShapeComp(new List<ShapeCompConst>() {
                ShapeCompConst.Create_SurfaceHeight_Between(0.55f, 1.0f),
                ShapeCompConst.Create_SurfaceCount_Min(1),
                ShapeCompConst.Create_SurfaceArea_Min(0.35f),
                ShapeCompConst.Create_RectangleLength_Min(0.75f),
                ShapeCompConst.Create_RectangleWidth_Min(0.5f),
            })};
            this.AddShape("Table", shapeComponents);

            // AllSurfaces
            shapeComponents = new List<ShapeComp>() { new ShapeComp(new List<ShapeCompConst>() {
                ShapeCompConst.Create_SurfaceHeight_Between(0.15f, 1.75f)
            })};
            this.AddShape("All Surfaces", shapeComponents);

            // Sittable
            shapeComponents = new List<ShapeComp>() { new ShapeComp(new List<ShapeCompConst>() {
                ShapeCompConst.Create_SurfaceHeight_Between(0.2f, 0.6f),
                ShapeCompConst.Create_SurfaceCount_Min(1),
                ShapeCompConst.Create_SurfaceArea_Min(0.035f)
            })};
            this.AddShape("Sittable", shapeComponents);

            // Chair
            shapeComponents = new List<ShapeComp>() { new ShapeComp(new List<ShapeCompConst>() {
                ShapeCompConst.Create_SurfaceHeight_Between(0.25f, 0.6f),
                ShapeCompConst.Create_SurfaceCount_Min(1),
                ShapeCompConst.Create_SurfaceArea_Min(0.035f),
                ShapeCompConst.Create_IsRectangle(),
                ShapeCompConst.Create_RectangleLength_Between(0.1f, 0.5f),
                ShapeCompConst.Create_RectangleWidth_Between(0.1f, 0.4f),
                ShapeCompConst.Create_SurfaceNotPartOfShape("Couch"),
            })};
            this.AddShape("Chair", shapeComponents);

            // LargeSurface
            shapeComponents = new List<ShapeComp>() { new ShapeComp(new List<ShapeCompConst>() {
                ShapeCompConst.Create_SurfaceHeight_Between(0.3f, 0.75f),
                ShapeCompConst.Create_SurfaceCount_Min(1),
                ShapeCompConst.Create_SurfaceArea_Min(0.35f),
                ShapeCompConst.Create_RectangleLength_Min(0.75f),
                ShapeCompConst.Create_RectangleWidth_Min(0.5f),
            })};
            this.AddShape("Large Surface", shapeComponents);

            // EmptyTable
            shapeComponents = new List<ShapeComp>() { new ShapeComp(new List<ShapeCompConst>() {
                ShapeCompConst.Create_SurfaceHeight_Between(0.3f, 0.75f),
                ShapeCompConst.Create_SurfaceCount_Min(1),
                ShapeCompConst.Create_SurfaceArea_Min(0.35f),
                ShapeCompConst.Create_RectangleLength_Min(0.75f),
                ShapeCompConst.Create_RectangleWidth_Min(0.5f),
            })};
            shapeConstraints = new List<ShapeConst>() { ShapeConst.Create_NoOtherSurface() };
            this.AddShape("Large Empty Surface", shapeComponents, shapeConstraints);

            // "Couch"
            shapeComponents = new List<ShapeComp>() {
                // Seat
                new ShapeComp(new List<ShapeCompConst>() {
                    ShapeCompConst.Create_SurfaceHeight_Between(0.2f, 0.6f),
                    ShapeCompConst.Create_SurfaceCount_Min(1),
                    ShapeCompConst.Create_SurfaceArea_Min(0.3f),
                    ShapeCompConst.Create_IsRectangle(),
                    ShapeCompConst.Create_RectangleLength_Between(0.4f, 3.0f),
                    ShapeCompConst.Create_RectangleWidth_Min(0.3f),
                }),
                // Back
                new ShapeComp(new List<ShapeCompConst>() {
                    ShapeCompConst.Create_SurfaceHeight_Between(0.6f, 1.0f),
                    ShapeCompConst.Create_SurfaceCount_Min(1),
                    ShapeCompConst.Create_IsRectangle(0.3f),
                    ShapeCompConst.Create_RectangleLength_Between(0.4f, 3.0f),
                    ShapeCompConst.Create_RectangleWidth_Min(0.05f),
                })
            };
            shapeConstraints = new List<ShapeConst>() {
                ShapeConst.Create_RectanglesSameLength(0, 1, 0.6f),
                ShapeConst.Create_RectanglesParallel(0, 1),
                ShapeConst.Create_RectanglesAligned(0, 1, 0.3f),
                ShapeConst.Create_AtBackOf(1, 0),
            };
            this.AddShape("Couch", shapeComponents, shapeConstraints);

            // Mark it
            this.HasCreatedShapes = true;
        }

        /**
         * Remove all shape definitions.
         */
        public void Clear() {
            this.HasCreatedShapes = false;
            SpatialUnderstandingDllShapes.RemoveAllShapes();
        }
    }
}
