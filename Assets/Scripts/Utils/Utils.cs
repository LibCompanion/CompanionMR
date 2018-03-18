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
     * This class provides utility methods.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    public static class Utils {

        /**
         * Delegate for a method that works with a given GameObject.
         */
        public delegate void GameObjectAction(GameObject obj);

        /**
         * Create a look-at rotation for the given forward vector.
         * 
         * @param forward           forward direction
         * @param resetOtherAxes    determines if the other axes should be reseted
         * @return a quaternion for the desired look rotation
         */
        public static Quaternion CreateLookAtRotation(Vector3 forward, bool resetOtherAxes = true) {

            // Create a quaternion for the desired look rotation
            Quaternion rotation = Quaternion.LookRotation(forward);
            if (resetOtherAxes) {
                rotation.x = 0.0f;
                rotation.z = 0.0f;
            }

            return rotation;
        }

        /**
         * Create a look-at rotation for the given points.
         * 
         * @param origin            origin point
         * @param target            target to look at
         * @param resetOtherAxes    determines if the other axes should be reset
         * @return a quaternion for the desired look rotation
         */
        public static Quaternion CreateLookAtRotation(Vector3 origin, Vector3 target, bool resetOtherAxes = true) {

            // Create a quaternion for the desired look rotation
            Quaternion rotation = Quaternion.LookRotation(target - origin);
            if (resetOtherAxes) {
                rotation.x = 0.0f;
                rotation.z = 0.0f;
            }

            return rotation;
        }

        /**
         * Draw a 2D-line into the 3D-Space.
         * 
         * @param start         start point of this line in world coordinates
         * @param end           end point of this line in world coordinates
         * @param wdith         line width
         * @param color         color of this line
         * @param lineMaterial  material of this line
         * @param parent        parent GameObject of this line
         * @return the drawn line as a GameObject
         */
        public static GameObject DrawLine(Vector3 start, Vector3 end, float width, Color color, Material lineMaterial, Transform parent) {

            // Create GameObject and add a LineRenderer component to it
            GameObject myLine = new GameObject();
            myLine.transform.position = start;
            myLine.transform.parent = parent;
            myLine.layer = 2; // IgnoreRaycast layer
            myLine.AddComponent<LineRenderer>();
            LineRenderer lr = myLine.GetComponent<LineRenderer>();

            // Tweak LineRenderer options
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.material = lineMaterial;
            lr.widthMultiplier = width;
            lr.useWorldSpace = false;

            // Set color
            Gradient gradient = new Gradient {
                mode = GradientMode.Fixed
            };
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(color, 0.0f), new GradientColorKey(color, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
            );
            lr.colorGradient = gradient;

            // Set positions
            Vector3[] positions = new Vector3[2];
            positions[0] = myLine.transform.InverseTransformPoint(start);
            positions[1] = myLine.transform.InverseTransformPoint(end);
            lr.positionCount = positions.Length;
            lr.SetPositions(positions);

            return myLine;
        }

        /**
         * Return the number of whole minutes in a given time length as a string value.
         * 
         * @param length    complete time length in seconds
         * @return the number of whole minutes as a string value
         */
        public static string GetMinutes(double length) {
            string result = "";
            int minutes = (int) length / 60;
            result = (minutes < 10) ? "0" + minutes.ToString() : minutes.ToString();
            return result;
        }

        /**
         * Return the remainder of seconds of a given time length as a string value.
         * 
         * @param length    complete time length in seconds
         * @return the remainder seconds as a string value
         */
        public static string GetSeconds(double length) {
            string result = "";
            double seconds = length % 60;
            result = (seconds < 10) ? "0" + seconds.ToString() : seconds.ToString();
            return result;
        }

        /**
         * Recursively traverse the hierarchy of a GameObject and invoke the given function.
         * 
         * @param obj   GameObject
         * @param func  function to be invoked
         */
        public static void TraverseHierarchy(GameObject obj, GameObjectAction func) {
            foreach (Transform t in obj.transform) {
                func(t.gameObject);
                TraverseHierarchy(t.gameObject, func);
            }
        }

        /**
         * Set a GameObject's layer back to default.
         * 
         * @param obj   GameObject
         */
        public static void ResetLayer(GameObject obj) {
            obj.layer = 0;
        }

        /**
         * Set GameObject's layer and the layer of all its children back to default.
         * 
         * @param obj   GameObject
         */
        public static void ResetLayerRecursively(GameObject obj) {
            Utils.TraverseHierarchy(obj, Utils.ResetLayer);
        }
    }
}
