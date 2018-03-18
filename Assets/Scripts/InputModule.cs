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

using UnityEngine.EventSystems;

namespace CompanionMR {

    /**
     * This class represents a HoloLens input module with the ability to return the last pointer event data object.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    public class InputModule : HoloLensInputModule {

        /**
         * Return the last pointer event data of the device with the given ID.
         * 
         * @param id    touch / mouse ID
         * @return pointer event data
         */
        public PointerEventData GetLastPointerEventDataPublic(int id) {
            return this.GetLastPointerEventData(id);
        }
    }
}
