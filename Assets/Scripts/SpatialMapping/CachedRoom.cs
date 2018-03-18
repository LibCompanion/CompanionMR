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
using System.Collections.Generic;
using UnityEngine;

namespace CompanionMR {

    /**
     * This class manages to save and laod a spatial mapping room mesh.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    public class CachedRoom : SpatialMappingSource {

        /**
         * The file name to use when saving and loading room meshes.
         */
        [Tooltip("The file name to use when saving and loading room meshes.")]
        public string fileName = "room";

        /**
         * Reference to the debug logger.
         */
        private DebugLogger logger;
        
        /**
         * Reference to the current room mesh.
         */
        private SpatialUnderstandingCustomMesh currentMesh;

        /**
         * Use this for initialization.
         */
        private void Start() {
            // Variable initialization
            this.logger = DebugLogger.Instance;

            // Get reference to other scripts in the scene
            this.currentMesh = this.gameObject.GetComponent<SpatialUnderstandingCustomMesh>();

            // Deactivate this script if necessary components are missing
            if ((this.logger == null) || (this.currentMesh == null)) {
                Debug.LogError("CachedRoom: Script references not set properly.");
                this.enabled = false;
            }
        }

        /**
         * Save the current room mesh and load it as a new spatial mapping source.
         */
        public void SaveRoom() {
            if (SpatialUnderstanding.IsInitialized && WorldAnchorManager.IsInitialized) {

                // Attach world anchor
                WorldAnchorManager.Instance.AttachAnchor(this.gameObject, this.fileName);

                // Save spatial meshes to file
                this.currentMesh.SaveSpatialMeshes(this.fileName);
            }
        }

        /**
         * Load the saved room.
         */
        public void LoadRoom() {
            if (SpatialMappingManager.IsInitialized && WorldAnchorManager.IsInitialized) {

                // Attach world anchor
                WorldAnchorManager.Instance.AttachAnchor(this.gameObject, this.fileName);

                // Set this cached room as the new spatial mapping source
                SpatialMappingManager.Instance.SetSpatialMappingSource(this);

                // Cleanup
                this.Cleanup();

                // Try loading the file
                try {
                    IList<Mesh> storedMeshes = MeshSaver.Load(this.fileName);
                    for (int iMesh = 0; iMesh < storedMeshes.Count; iMesh++) {
                        SurfaceObject surface = this.CreateSurfaceObject(storedMeshes[iMesh], "storedmesh-" + iMesh, this.transform, iMesh);
                        this.AddSurfaceObject(surface);
                    }
                } catch {
                    this.logger.Log("Failed to load " + this.fileName + ".");
                }
            }
        }
    }
}
