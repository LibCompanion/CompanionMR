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
using HoloToolkit.Unity.InputModule;
using System.Collections;
using UnityEngine;

namespace CompanionMR {

    /**
     * This class represents a controller for the Paladin 3D asset.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    public class PaladinController : MonoBehaviour, IInputClickHandler {

        /**
         * Paladin sizes.
         */
        public enum PaladinSizes {
            Small,
            Big
        }

        /**
         * Reference to the sword joint.
         */
        public Transform swordJoint;

        /**
         * Reference to the sheath joint.
         */
        public Transform sheathJoint;

        /**
         * Reference to the right hand weapon.
         */
        public Transform rightHandWeapon;

        /**
         * Reference to the left hand weapon.
         */
        public Transform leftHandWeapon;

        /**
         * Reference to the helmet.
         */
        public Transform helmet;

        /**
         * Parent object for world anchors.
         */
         [Tooltip("Parent object for world anchors.")]
        public GameObject worldAnchorParent;

        /**
         * Amount of time the scaling will take.
         */
        [Tooltip("Amount of time the scaling will take.")]
        [Range(1.0f, 5.0f)]
        public float smoothTime = 2.0f;

        /**
         * Amount of time the paladin stays dead.
         */
        [Tooltip("Amount of time the paladin stays dead.")]
        [Range(1, 5)]
        public int deathTime = 5;

        /**
         * Initial paladin size.
         */
        [Tooltip("Initial paladin size.")]
        public PaladinSizes size = PaladinSizes.Small;

        /**
         * Anchor ID of the 3D asset.
         */
        private const string ANCHOR_ID = "paladin";

        /**
         * Target scale.
         */
        private const float TARGET_SCALE = 1.0f;

        /**
         * Point of interest for UI arrows.
         */
        private static Vector3 POINT_OF_INTEREST = new Vector3(0.0f, 1.5f, 0.0f);

        /**
         * Indicates whether interpolating is in progress.
         */
        private bool interpolating;

        /**
         * The target Transform.
         */
        private Transform targetTransform;

        /**
         * Current duration of the scaling process.
         */
        private float duration;

        /**
         * Reference to the small paladin who cloned himself into this big paladin.
         */
        private GameObject smallPaladin;

        /**
         * Reference to the GameObject that contains the world anchor for the 3D asset.
         */
        private GameObject assetAnchor;

        /**
         * Reference to the world anchor manager.
         */
        private WorldAnchorManager anchorManager;

        /**
         * Update is called once per frame.
         */
        private void Update() {
            if (this.interpolating && (this.targetTransform != null)) {
                // Smoothly interpolate the position value
                this.duration += Time.deltaTime;
                float t = this.duration / this.smoothTime;
                this.transform.position = Vector3.Lerp(this.smallPaladin.transform.position, this.targetTransform.position, t);
                this.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
                if (t >= 1.0f) {
                    //this.size = PaladinSizes.Big;
                    //this.interpolating = false;
                    GameObject paladin = Instantiate(this.gameObject, this.transform.parent);
                    PaladinController ctrl = paladin.GetComponent<PaladinController>();
                    ctrl.size = PaladinSizes.Big;
                    ctrl.SetSmallPaladin(this.smallPaladin);
                    ctrl.ResetSword();
                    GameObject lookAtTarget = new GameObject();
                    lookAtTarget.transform.SetParent(paladin.transform, false);
                    lookAtTarget.transform.Translate(POINT_OF_INTEREST);
                    if (UIArrows.IsInitialized) { UIArrows.Instance.targetTransform = lookAtTarget.transform; }
                    Destroy(this.gameObject);
                }
            }
        }

        /**
         * Control the drawing of the weapon.
         */
        public void OnDrawWeapon() {
            if (this.rightHandWeapon != null) {
                this.rightHandWeapon.parent = null;                         // 1. unparent
                this.rightHandWeapon.position = this.swordJoint.position;   // 2. reposition
                this.rightHandWeapon.parent = this.swordJoint;              // 3. reparent
                this.rightHandWeapon.localPosition = Vector3.zero;          // 4. reset position
                this.rightHandWeapon.localRotation = Quaternion.identity;   // 5. reset orientation
            }
        }

        /**
         * Control the sheathing of the weapon.
         */
        public void OnSheathWeapon() {
            if (this.rightHandWeapon != null) {
                this.rightHandWeapon.parent = null;                         // 1. unparent
                this.rightHandWeapon.position = this.sheathJoint.position;  // 2. reposition
                this.rightHandWeapon.parent = this.sheathJoint;             // 3. reparent
                this.rightHandWeapon.localPosition = Vector3.zero;          // 4. reset position
                this.rightHandWeapon.localRotation = Quaternion.identity;   // 5. reset orientation
            }
        }

        /**
         * Activate the ragdoll system.
         */
        private void Die() {

            // Disable the animation component
            Animator animator = this.GetComponent<Animator>();
            if (animator) {
                animator.enabled = false;
            }

            // Enable colliders and rigidbodies
            Collider[] colliders = this.GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders) {
                Rigidbody rigid = col.GetComponent<Rigidbody>();
                if (rigid != null) {
                    rigid.isKinematic = false;
                    rigid.useGravity = true;
                }
            }

            // Release the right hand weapons
            if (this.rightHandWeapon != null) {
                this.rightHandWeapon.parent = null;
            }

            // Release the right left weapons
            if (this.leftHandWeapon != null) {
                this.leftHandWeapon.parent = null;
            }

            // Release the helmet
            if (this.helmet != null) {
                this.helmet.parent = null;
            }

            // Disable ragdoll after a couple of seconds
            StartCoroutine(this.DisableRagdoll());
        }

        /**
         * Disable the ragdoll system.
         */
        private IEnumerator DisableRagdoll() {
            //yield return new WaitForSeconds(2);
            //Collider[] colliders = this.GetComponentsInChildren<Collider>();
            //foreach (Collider col in colliders) {
            //    Rigidbody rigid = col.GetComponent<Rigidbody>();
            //    if (rigid != null) {
            //        rigid.isKinematic = true;
            //        rigid.useGravity = false;
            //    }
            //}

            yield return new WaitForSeconds(this.deathTime);

            // Make a new paladin
            this.smallPaladin.SetActive(true);

            // Destroy the weapons if they exist
            if (this.leftHandWeapon != null) {
                Destroy(this.leftHandWeapon.gameObject);
            }
            if (this.rightHandWeapon != null) {
                Destroy(this.rightHandWeapon.gameObject);
            }
            if (this.helmet != null) {
                Destroy(this.helmet.gameObject);
            }

            // Destroy this paladin
            Destroy(this.gameObject);
        }

        /**
         * This method is called when this object will be destroyed.
         */
        private void OnDestroy() {
            // Destory the world anchor parent
            if (this.assetAnchor != null) {
                Destroy(this.assetAnchor);
            }

            // Destroy the weapons if they exist
            if (this.leftHandWeapon != null) {
                Destroy(this.leftHandWeapon.gameObject);
            }
            if (this.rightHandWeapon != null) {
                Destroy(this.rightHandWeapon.gameObject);
            }
            if (this.helmet != null) {
                Destroy(this.helmet.gameObject);
            }
        }

        /**
         * This method is called when the user has clicked on this GameObject.
         * 
         * @param eventData     input click event data
         */
        public void OnInputClicked(InputClickedEventData eventData) {
            if ((this.size == PaladinSizes.Small) && (!this.interpolating)) {
#if UNITY_EDITOR
                GameObject paladin = Instantiate(this.gameObject);
                PaladinController ctrl = paladin.GetComponent<PaladinController>();
                ctrl.ResetSword();
                ctrl.Grow(this.transform.parent, this.gameObject);
                this.gameObject.SetActive(false);
#else
                this.anchorManager = WorldAnchorManager.Instance;
                bool found = false;

                // Check if we have already instantiated a world anchor
                if (this.assetAnchor != null) {
                    GameObject paladin = Instantiate(this.gameObject, this.assetAnchor.transform);
                    PaladinController ctrl = paladin.GetComponent<PaladinController>();
                    ctrl.ResetSword();
                    ctrl.Grow(this.assetAnchor.transform, this.gameObject);
                    this.gameObject.SetActive(false);
                    found = true;
                } else if (this.anchorManager != null) {
                    // Check if a world anchor for this asset exists
                    UnityEngine.VR.WSA.Persistence.WorldAnchorStore store = this.anchorManager.AnchorStore;
                    if (store != null) {
                        string[] ids = store.GetAllIds();
                        for (int index = 0; (index < ids.Length) && !found; index++) {
                            if (ids[index].Equals(ANCHOR_ID)) {
                                this.assetAnchor = Instantiate(this.worldAnchorParent);
                                this.anchorManager.AttachAnchor(this.assetAnchor, ANCHOR_ID);
                                GameObject paladin = Instantiate(this.gameObject, this.assetAnchor.transform);
                                PaladinController ctrl = paladin.GetComponent<PaladinController>();
                                ctrl.ResetSword();
                                ctrl.Grow(this.assetAnchor.transform, this.gameObject);
                                this.gameObject.SetActive(false);
                                found = true;
                            }
                        }
                    }
                }

                // Try to query SpatialUnderstanding if a world anchor was not found
                if (!found && (this.anchorManager != null)) {
                    SpatialUnderstanding suInstance = SpatialUnderstanding.Instance;
                    PlacementSolver solver = PlacementSolver.Instance;
                    if ((suInstance != null) && (solver != null) && InputManager.IsInitialized && (suInstance.ScanState == SpatialUnderstanding.ScanStates.Done) && suInstance.AllowSpatialUnderstanding) {
                        InputManager.Instance.PushInputDisable();
                        solver.Query_OnFloor_NearPoint(this.transform.parent.position, false, this.HandlePlacementQuery);
                    }
                }
#endif
            } else if ((this.size == PaladinSizes.Big) && (!this.interpolating)) {
                this.Die();
            }
        }

        /**
         * Enlarge this paladin to its full height.
         * 
         * @param transform     the target transform parent
         * @param smallPaladin  a reference to the small paladin who cloned himself into this big paladin
         */
        private void Grow(Transform transform, GameObject smallPaladin) {
            this.smallPaladin = smallPaladin;
            this.targetTransform = transform;
            this.interpolating = true;
            this.gameObject.GetComponent<SphereCollider>().enabled = false;
        }

        /**
         * Reset the sword position.
         */
        private void ResetSword() {
            if (this.rightHandWeapon != null) {
                this.rightHandWeapon.transform.parent = this.sheathJoint;
                this.rightHandWeapon.transform.localPosition = Vector3.zero;
                this.rightHandWeapon.transform.localRotation = Quaternion.identity;
            }
        }

        /**
         * Handle the placement query.
         * 
         * @param success   indicates whether the placement query was successful
         * @param position  position result of the placement query
         */
        private void HandlePlacementQuery(bool success, Vector3 position) {
            if (success) {
                this.assetAnchor = Instantiate(this.worldAnchorParent, position, this.transform.rotation);
                this.anchorManager.RemoveAnchor(ANCHOR_ID);
                this.anchorManager.AttachAnchor(this.assetAnchor, ANCHOR_ID);
                GameObject paladin = Instantiate(this.gameObject, this.assetAnchor.transform);
                PaladinController ctrl = paladin.GetComponent<PaladinController>();
                ctrl.ResetSword();
                ctrl.Grow(this.assetAnchor.transform, this.gameObject);
                this.gameObject.SetActive(false);
            } else if (InfoCanvas.IsInitialized) {
                InfoCanvas.Instance.SetInfoText("An adequate floor position couldn't be found.");
            }

            // Activate input again
            if (InputManager.IsInitialized) {
                InputManager.Instance.PopInputDisable();
            }
        }

        /**
         * Set the reference to the small paladin who cloned himself into this big paladin.
         * 
         * @param smallPaladin  a reference to the small paladin who cloned himself into this big paladin
         */
        public void SetSmallPaladin(GameObject smallPaladin) {
            this.smallPaladin = smallPaladin;
        }
    }
}
