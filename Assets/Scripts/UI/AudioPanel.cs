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

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CompanionMR {

    /**
     * This class represents an audio panel.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    public class AudioPanel : MonoBehaviour {

        /**
         * The title of this audio.
         */
        [Tooltip("The title of this audio.")]
        public Text audioTtitle;

        /**
         * The progress slider of this audio.
         */
        [Tooltip("The progress slider of this audio.")]
        public Slider progressSlider;

        /**
         * The current time of this audio.
         */
        [Tooltip("The current time of this audio.")]
        public Text currentTime;

        /**
         * The length of this audio.
         */
        [Tooltip("The length of this audio.")]
        public Text length;

        /**
         * The audio source script reference.
         */
        [Tooltip("The audio source script reference.")]
        public AudioSource source;

        /**
         * The audio clip of this audio.
         */
        private AudioClip clip;

        /**
         * Indicates whether the audio clip was loaded.
         */
        private bool clipLoaded;

        /**
         * Use this for initialization.
         */
        private void Start() {
            // Variable initialization
            this.clipLoaded = false;
        }

        /**
         * Update is called once per frame.
         */
        private void Update() {
            // Check if the audio data is loaded
            if (!this.clipLoaded && (this.clip != null) && (this.clip.loadState == AudioDataLoadState.Loaded)) {
                this.length.text = Utils.GetMinutes(this.clip.length) + ":" + Utils.GetSeconds(this.clip.length);
                this.clipLoaded = true;
            }

            // Update displayed play time
            if (this.source.isPlaying) {
                this.currentTime.text = Utils.GetMinutes(this.source.time) + ":" + Utils.GetSeconds(this.source.time);
                this.progressSlider.value = this.source.time / this.clip.length;
            }
        }

        /**
         * Set the data for this audio.
         * 
         * @param audio data for this audio
         */
        public void SetData(Audio audio) {

            // Load the audio
#if ENABLE_WINMD_SUPPORT
            if (System.IO.File.Exists(audio.localAudioPath)) {
                WWW www = new WWW("file:///" + audio.localAudioPath);
                StartCoroutine(WaitForWWW(www));
            } else {
                this.gameObject.SetActive(false);
                return;
            }
#endif

            // Set audio information
            this.audioTtitle.text = audio.title;
        }

        /**
         * Wait for the access request to be finished and obtain the audio data.
         * 
         * @param www   the web request/response object (used for local file access)
         * @return enumerator for usage as coroutine
         */
        private IEnumerator WaitForWWW(WWW www) {

            // Wait for the WebRequest to be finished
            yield return www;

            this.clip = www.GetAudioClip(false, true, AudioType.OGGVORBIS);
            //this.clip = www.GetAudioClipCompressed(false, AudioType.OGGVORBIS);
            this.source.clip =  this.clip;
        }
    }
}
