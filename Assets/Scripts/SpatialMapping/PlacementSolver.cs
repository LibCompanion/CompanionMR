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
using SUDLLOP = HoloToolkit.Unity.SpatialUnderstandingDllObjectPlacement;

namespace CompanionMR {

    /**
     * This class provides a mechanism to find specific placement targets in the spatial mapping data.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    public class PlacementSolver : LineDrawer {

        /**
         * Query states.
         */
        public enum QueryStates {
            None,
            Processing,
            Finished
        }

        /**
         * Delegate for a response callback method for placement queries.
         * 
         * @param success   indicates whether the placement query was successful
         * @param position  position result of the placement query
         */
        public delegate void ResponseDelegate(bool success, Vector3 position);

        /**
         * This struct represents a query status.
         */
        private struct QueryStatus {

            /**
             * Query state.
             */
            public QueryStates state;

            /**
             * Query name.
             */
            public string name;

            /**
             * Number of failed detections.
             */
            public int countFail;

            /**
             * Number of succeeded detections.
             */
            public int countSuccess;

            /**
             * List of placement results.
             */
            public List<SUDLLOP.ObjectPlacementResult> queryResult;

            /**
             * Indicates whether an animated box should be drawn around the placement result.
             */
            public bool drawBox;

            /**
             * Response callback method for placement queries.
             */
            public ResponseDelegate callback;

            /**
             * Reset this query status.
             */
            public void Reset() {
                this.state = QueryStates.None;
                this.name = "";
                this.countFail = 0;
                this.countSuccess = 0;
                this.queryResult = new List<SUDLLOP.ObjectPlacementResult>();
                this.drawBox = true;
                this.callback = null;
            }
        }

        /**
         * This struct represents a placement query.
         */
        private struct PlacementQuery {

            /**
             * Placement definition for this query.
             */
            public SUDLLOP.ObjectPlacementDefinition placementDefinition;

            /**
             * Placement rules for this query.
             */
            public List<SUDLLOP.ObjectPlacementRule> placementRules;

            /**
             * Placement constraints for this query.
             */
            public List<SUDLLOP.ObjectPlacementConstraint> placementConstraints;

            /**
             * Create a new placement query.
             * 
             * @param placementDefinition   placement definition
             * @param placementRules        placement rules
             * @param placementConstraints  placement constraints
             */
            public PlacementQuery(SUDLLOP.ObjectPlacementDefinition placementDefinition, List<SUDLLOP.ObjectPlacementRule> placementRules = null,
                                  List<SUDLLOP.ObjectPlacementConstraint> placementConstraints = null) {
                this.placementDefinition = placementDefinition;
                this.placementRules = placementRules;
                this.placementConstraints = placementConstraints;
            }
        }

        /**
         * This class represents a placement result.
         * 
         * @author Dimitri Kotlovsky, Andreas Sekulski
         */
        private class PlacementResult {

            /**
             * Reference to the animated box of this placement result.
             */
            public AnimatedBox box;

            /**
             * Reference to the actual placement result object.
             */
            public SUDLLOP.ObjectPlacementResult result;

            /**
             * Create a new placement result.
             * 
             * @param timeDelay     time delay for the animated box
             * @param result        placement result
             * @param drawBox       indicates whether an animated box should be drawn around the placement result
             * @param callback      callback method
             */
            public PlacementResult(float timeDelay, SUDLLOP.ObjectPlacementResult result, bool drawBox = true, ResponseDelegate callback = null) {
                if (drawBox) {
                    this.box = new AnimatedBox(timeDelay, result.Position, Quaternion.LookRotation(result.Forward, result.Up), Random.ColorHSV(0.0f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f), result.HalfDims);
                }
                if (callback != null) {
                    callback(true, new Vector3(result.Position.x, result.Position.y - result.HalfDims.y, result.Position.z));
                }

                this.result = result;
            }
        }

        /**
         * Singleton instance.
         */
        public static PlacementSolver Instance { get; private set; }

        /**
         * Indicates whether the solver is already initialted.
         */
        public bool IsSolverInitialized { get; private set; }

        /**
         * Lits of all placement results.
         */
        private List<PlacementResult> placementResults;

        /**
         * Status of the current query.
         */
        private QueryStatus queryStatus;

        /**
         * Use this for initialization that has to be done before the Start() function of other GameObjects is invoked.
         */
        private void Awake() {
            if (PlacementSolver.Instance == null) {
                PlacementSolver.Instance = this;
            }
        }

        /**
         * This method is called when this object will be destroyed.
         */
        protected override void OnDestroy() {
            base.OnDestroy();
            if (PlacementSolver.Instance == this) {
                PlacementSolver.Instance = null;
            }
        }

        /**
         * Use this for initialization.
         */
        private void Start() {
            // Variable initialization
            this.placementResults = new List<PlacementResult>();
            this.queryStatus = new QueryStatus();
        }

        /**
         * Update is called once per frame.
         */
        private void Update() {
            // Can't do any of this till we're done with the scanning phase
            if ((!SpatialUnderstanding.IsInitialized) || (SpatialUnderstanding.Instance.ScanState != SpatialUnderstanding.ScanStates.Done)) {
                return;
            }

            // Make sure the solver has been initialized
            if (!this.IsSolverInitialized && SpatialUnderstanding.Instance.AllowSpatialUnderstanding) {
                this.InitializeSolver();
            }

            // Handle async query results
            this.ProcessPlacementResults();

            // Lines: Begin
            this.LineDraw_Begin();

            // Drawers
            bool needsUpdate = false;
            needsUpdate |= this.Draw_PlacementResults();

            // Lines: Finish up
            this.LineDraw_End(needsUpdate);
        }

        /**
         * Initialize this placement solver.
         * 
         * @return <code>true</code> if the solver was initialized successfully, <code>flase</code> otherwise
         */
        public bool InitializeSolver() {
            if (this.IsSolverInitialized || !SpatialUnderstanding.IsInitialized || !SpatialUnderstanding.Instance.AllowSpatialUnderstanding) {
                return this.IsSolverInitialized;
            }

            if (SUDLLOP.Solver_Init() == 1) {
                this.IsSolverInitialized = true;
            }

            return this.IsSolverInitialized;
        }

        /**
         * Remove all placement results.
         * 
         * @param removeAllObjects  indicates whether all instantiated placement results should also be removed
         */
        public void ClearGeometry(bool removeAllObjects = false) {
            this.placementResults.Clear();
            if (removeAllObjects && SpatialUnderstanding.IsInitialized && SpatialUnderstanding.Instance.AllowSpatialUnderstanding) {
                SUDLLOP.Solver_RemoveAllObjects();
            }
        }

        /**
         * Reset this placement solver to the initial state.
         */
        public void Clear() {
            this.ClearGeometry(true);
            this.IsSolverInitialized = false;
        }

        /**
         * Draw placement results.
         * 
         * @return <code>true</code> if the drawing is not finished and needs to be updated, <code>false</code> otherwise
         */
        private bool Draw_PlacementResults() {
            bool needsUpdate = false;

            for (int i = 0; i < this.placementResults.Count; i++) {
                if (this.placementResults[i].box != null) {
                    needsUpdate |= this.Draw_AnimatedBox(this.placementResults[i].box);
                }
            }

            return needsUpdate;
        }

        /**
         * Process the given placement queries.
         * 
         * @param placementName     placement description
         * @param placementList     list of placement queries
         * @param clearObjectsFirst should already detected placement results should be removed
         * @param drawBox           indicates whether an animated box should be drawn around the placement result
         * @param callback          callback method
         */
        private bool PlaceObjectAsync(string placementName, List<PlacementQuery> placementList, bool clearObjectsFirst = false, bool drawBox = true, ResponseDelegate callback = null) {
            // If we already mid-query, reject the request
            if (this.queryStatus.state != QueryStates.None) {
                return false;
            }

            // Clear geo
            if (clearObjectsFirst) {
                this.ClearGeometry();
            }

            // Mark it
            this.queryStatus.Reset();
            this.queryStatus.state = QueryStates.Processing;
            this.queryStatus.name = placementName;
            this.queryStatus.drawBox = drawBox;
            this.queryStatus.callback = callback;

            // Tell user we are processing
            Debug.Log(placementName + " (processing)");

            // Kick off a thread to do process the queries
#if ENABLE_WINMD_SUPPORT
            System.Threading.Tasks.Task.Run
#elif UNITY_EDITOR || !ENABLE_WINMD_SUPPORT
            new System.Threading.Thread
#endif
            (() => {
                // Go through the queries in the list
                for (int i = 0; i < placementList.Count; ++i) {
                    // Do the query
                    bool success = this.PlaceObject(placementName, placementList[i].placementDefinition, placementList[i].placementRules, placementList[i].placementConstraints, clearObjectsFirst, true, drawBox, callback);

                    // Mark the result
                    this.queryStatus.countSuccess = success ? (this.queryStatus.countSuccess + 1) : this.queryStatus.countSuccess;
                    this.queryStatus.countFail = !success ? (this.queryStatus.countFail + 1) : this.queryStatus.countFail;
                }

                // Done
                this.queryStatus.state = QueryStates.Finished;
            })
#if UNITY_EDITOR || !ENABLE_WINMD_SUPPORT
            .Start()
#endif
            ;

            return true;
        }

        /**
         * Process the given placement query.
         * 
         * @param placementName         placement description
         * @param placementDefinition   placement definition
         * @param placementRules        list of placement rules
         * @param placementConstraints  list of placement constraints
         * @param clearObjectsFirst     should already detected placement results should be removed
         * @param isASync               indicates whether this query is handled asynchronously
         * @param drawBox               indicates whether an animated box should be drawn around the placement result
         * @param callback              callback method
         */
        private bool PlaceObject(string placementName, SUDLLOP.ObjectPlacementDefinition placementDefinition, List<SUDLLOP.ObjectPlacementRule> placementRules = null,
                                 List<SUDLLOP.ObjectPlacementConstraint> placementConstraints = null, bool clearObjectsFirst = false, bool isASync = true, bool drawBox = true, ResponseDelegate callback = null) {

            // Clear objects (if requested)
            if (!isASync && clearObjectsFirst) {
                this.ClearGeometry();
            }

            if (!SpatialUnderstanding.IsInitialized || !SpatialUnderstanding.Instance.AllowSpatialUnderstanding) {
                return false;
            }

            // Query parameters
            int placementRuleCount = (placementRules != null) ? placementRules.Count : 0;
            System.IntPtr placementRulesPtr = ((placementRules != null) && (placementRules.Count > 0))
                ? SpatialUnderstanding.Instance.UnderstandingDLL.PinObject(placementRules.ToArray()) : System.IntPtr.Zero;
            int constraintCount = (placementConstraints != null) ? placementConstraints.Count : 0;
            System.IntPtr placementConstraintsPtr = ((placementConstraints != null) && (placementConstraints.Count > 0))
                ? SpatialUnderstanding.Instance.UnderstandingDLL.PinObject(placementConstraints.ToArray()) : System.IntPtr.Zero;
            System.IntPtr placementResultPtr = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticObjectPlacementResultPtr();

            // New query
            int success = SUDLLOP.Solver_PlaceObject(placementName, SpatialUnderstanding.Instance.UnderstandingDLL.PinObject(placementDefinition), placementRuleCount,
                                                     placementRulesPtr, constraintCount, placementConstraintsPtr, placementResultPtr);

            if (success > 0) {
                SUDLLOP.ObjectPlacementResult placementResult = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticObjectPlacementResult();
                if (!isASync) {
                    // If not running async, we can just add the results to the draw list right now
                    Debug.Log(placementName + " (1)");
                    float timeDelay = this.placementResults.Count * AnimatedBox.DelayPerItem;
                    this.placementResults.Add(new PlacementResult(timeDelay, placementResult.Clone() as SUDLLOP.ObjectPlacementResult, drawBox, callback));
                } else {
                    this.queryStatus.queryResult.Add(placementResult.Clone() as SUDLLOP.ObjectPlacementResult);
                }
                return true;
            }

            if (!isASync) {
                Debug.Log(placementName + " (0)");
            }

            return false;
        }

        /**
         * Process the placement results asynchronously.
         */
        private void ProcessPlacementResults() {
            // Check it
            if (this.queryStatus.state != QueryStates.Finished) {
                return;
            }
            if (!SpatialUnderstanding.IsInitialized || !SpatialUnderstanding.Instance.AllowSpatialUnderstanding) {
                return;
            }

            // We will reject any above or below the ceiling/floor
            SpatialUnderstandingDll.Imports.QueryPlayspaceAlignment(SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceAlignmentPtr());
            SpatialUnderstandingDll.Imports.PlayspaceAlignment alignment = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceAlignment();

            // Copy over the results
            //for (int i = 0; i < this.queryStatus.queryResult.Count; ++i) {
            //    if ((this.queryStatus.queryResult[i].Position.y < alignment.CeilingYValue) && (this.queryStatus.queryResult[i].Position.y > alignment.FloorYValue)) {
            //        float timeDelay = this.placementResults.Count * AnimatedBox.DelayPerItem;
            //        this.placementResults.Add(new PlacementResult(timeDelay, this.queryStatus.queryResult[i].Clone() as SUDLLOP.ObjectPlacementResult));
            //    }
            //}
            if ((this.queryStatus.queryResult.Count > 0) && (this.queryStatus.queryResult[0].Position.y < alignment.CeilingYValue) && (this.queryStatus.queryResult[0].Position.y > alignment.FloorYValue)) {
                float timeDelay = this.placementResults.Count * AnimatedBox.DelayPerItem;
                this.placementResults.Add(new PlacementResult(timeDelay, this.queryStatus.queryResult[0].Clone() as SUDLLOP.ObjectPlacementResult, this.queryStatus.drawBox, this.queryStatus.callback));
            } else {
                this.queryStatus.callback(false, Vector3.zero);
            }

            // Text
            Debug.Log(this.queryStatus.name + " (" + this.placementResults.Count + "/" + (this.queryStatus.countSuccess + this.queryStatus.countFail) + ")");

            // Mark done
            this.queryStatus.Reset();
        }

        /**
         * Query the SpatialUnderstanding DLL for a specific placement.
         * 
         * @param pos       position near which the object should be placed
         * @param drawBox   indicates whether an animated box should be drawn around the placement result
         * @param callback  callback method
         */
        public void Query_OnFloor_NearPoint(Vector3 pos, bool drawBox = true, ResponseDelegate callback = null) {
            List<PlacementQuery> placementQueries = new List<PlacementQuery>();
            for (int i = 0; i < 4; ++i) {
                float halfDimSize = 0.25f;
                SUDLLOP.ObjectPlacementDefinition placementDefinition = SUDLLOP.ObjectPlacementDefinition.Create_OnFloor(new Vector3(halfDimSize, halfDimSize * 3.6f, halfDimSize));
                List<SUDLLOP.ObjectPlacementRule> rules = new List<SUDLLOP.ObjectPlacementRule> {
                    SUDLLOP.ObjectPlacementRule.Create_AwayFromOtherObjects(halfDimSize * 3.0f),
                    //SUDLLOP.ObjectPlacementRule.Create_AwayFromPosition(CameraCache.Main.transform.position, 1.0f)
                };
                List<SUDLLOP.ObjectPlacementConstraint> constraints = new List<SUDLLOP.ObjectPlacementConstraint> {
                    SUDLLOP.ObjectPlacementConstraint.Create_NearPoint(pos, 0.5f, 2.0f)
                };
                placementQueries.Add(new PlacementQuery(placementDefinition, rules, constraints));
            }
            this.PlaceObjectAsync("OnFloor - NearPoint/AwayFromMe", placementQueries, false, drawBox, callback);
        }
    }
}
