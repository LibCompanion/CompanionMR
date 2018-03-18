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
using UnityEngine.UI;

namespace CompanionMR {

    /**
     * This class resizes a text panel dynamically.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    [RequireComponent(typeof(LayoutElement))]
    public class ResizeTextPanel : MonoBehaviour {

        /**
         * RectTransform of the text.
         */
         [Tooltip("RectTransform of the text.")]
        public RectTransform textRect;

        /**
         * Reference to the layout settings of this panel.
         */
        private LayoutElement layoutElement;

        /**
         * Use this for initialization.
         */
        private void Start() {
            this.layoutElement = this.gameObject.GetComponent<LayoutElement>();
        }

        /**
         * Update is called once per frame.
         */
        private void Update() {
            if (this.layoutElement != null) {
                this.layoutElement.preferredHeight = this.textRect.sizeDelta.y * this.textRect.localScale.y;
            }
        }
    }
}
