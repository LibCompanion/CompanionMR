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
using UnityEngine;

namespace CompanionMR {

    /**
     * This class represents a controller for the Napoleon 3D asset.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    public class NapoleonController : MonoBehaviour, IInputClickHandler {

        /**
         * Animator.
         */
        private Animator animator;

        /**
         * Use this for initialization.
         */
        private void Start() {
            this.animator = this.GetComponent<Animator>();
        }

        /**
         * This method is called when the user has clicked on this GameObject.
         * 
         * @param eventData     input click event data
         */
        public void OnInputClicked(InputClickedEventData eventData) {
            // Activate animation trigger
            this.animator.SetTrigger(Triggers.TRIGGER_CLICKED);
        }
    }
}
