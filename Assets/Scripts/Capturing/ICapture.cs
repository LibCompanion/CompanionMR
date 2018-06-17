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

namespace CompanionMR {

    /**
     * Interface for capture modules.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    public interface ICapture {

        /**
         * Indicates whether the capture mode has been started or not.
         */
        bool CaptureStarted { get; }

        /**
         * This method stops the capture mode and disposes of the capture object.
         * 
         * @param action    action that should be invoked after disposing
         */
        void DisposeActive(UnityEngine.Events.UnityAction action);

        /**
         * This method disposes of the inactive capture object.
         */
        void DisposeInactive();
    }
}
