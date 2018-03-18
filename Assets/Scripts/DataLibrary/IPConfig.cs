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

using System.Collections;
using UnityEngine;

namespace CompanionMR {

    /**
     * Utility for connecting to the data providing web service by IP address from inside application at runtime.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    public class IPConfig : HoloToolkit.Unity.Singleton<IPConfig> {

        /**
         * The maximum length of characters in a IPv4 address (000.000.000.000).
         */
        private const int MaximumCharacterLength = 15;

        /**
         * IP address text field.
         */
        [Tooltip("IP address text field.")]
        public UnityEngine.UI.Text ipAddress;

        /**
         * Connection indicator.
         */
        [Tooltip("Connection indicator.")]
        public UnityEngine.UI.Image connectionIndicator;

        /**
         * IP address.
         */
        public string IpAddress { get { return this.ipAddress.text; } }

        /**
         * Indicates whether the connection was successful yet.
         */
        public bool Connected { get; private set; }

        /**
         * Constant string that represents the "connected" status.
         */
        private const string CONNECTED = "Connected";

        /**
         * Constant string that represents the "not connected" status.
         */
        private const string NOT_CONNECTED = "Not Connected";

        /**
         * Indicates whether the data loader is trying to connect to the given IP address.
         */
        private bool isTryingToConnect;

        /**
         * Reference to the data loader.
         */
        private DataLoader dataLoader;

        /**
         * Use this for initialization that has to be done before the Start() function of other GameObjects is invoked.
         */
        protected override void Awake() {
            base.Awake();
            this.ipAddress.text = NOT_CONNECTED;
        }

        /**
         * Use this for initialization.
         */
        private void Start() {

            // Variable initialization
            this.Connected = false;
            this.isTryingToConnect = false;

            // Get reference to other scripts in the scene
            this.dataLoader = DataLoader.Instance;

            // Deactivate this script if necessary components are missing
            if (this.dataLoader == null) {
                Debug.LogError("IPConfig: Script references not set properly.");
                this.enabled = false;
                return;
            }
        }

        /**
         * Try to connect to the given IP address.
         */
        public void Connect() {
            string ip = this.ipAddress.text;
            DebugLogger.Instance.Log(ip[ip.Length - 1].ToString());
            DebugLogger.Instance.Log("" + ip[ip.Length - 1].Equals('.'));
            if (!this.isTryingToConnect && !ip.Equals(NOT_CONNECTED) && (ip.Length >= 7) && !ip[ip.Length - 1].Equals('.')) {
                this.isTryingToConnect = true;
                this.connectionIndicator.color = Color.yellow;
            }
        }

        /**
         * Indicates whether this config is ready for a connection attempt.
         * 
         * @return <code>true</code> if this config is ready for a connection attempt, <code>false</code> otherwise
         */
        public bool IsReady() {
            return this.isTryingToConnect;
        }

        /**
         * This method should be called to indicate whether the last connection attempt was successful or not.
         * 
         * @param success   indicates whether the last connection attempt was successful or not
         */
        public void ConnectionSuccessful(bool success) {
            if (success) {
                this.connectionIndicator.color = Color.green;
                this.ipAddress.text = CONNECTED;
                this.Connected = true;
            } else {
                this.connectionIndicator.color = Color.red;
                this.isTryingToConnect = false;
            }
        }

        /**
         * Add a character to the IP address.
         * 
         * @param character input character that should be added to the IP address
         */
        public void AddCharacter(string character) {
            if (!this.isTryingToConnect) {
                if (this.ipAddress.text.Equals(NOT_CONNECTED)) {
                    this.ipAddress.text = string.Empty;
                }

                if (this.ipAddress.text.Length < MaximumCharacterLength) {
                    this.ipAddress.text += character;
                }
            }
        }

        /**
         * Delete last character.
         */
        public void DeleteLastCharacter() {
            if (!this.isTryingToConnect) {
                if (!string.IsNullOrEmpty(this.ipAddress.text)) {
                    this.ipAddress.text = this.ipAddress.text.Substring(0, this.ipAddress.text.Length - 1);
                }
            }
        }

        /**
         * Clear IP address string.
         */
        public void ClearIpAddressString() {
            if (!this.isTryingToConnect) {
                this.ipAddress.text = string.Empty;
            }
        }
    }
}
