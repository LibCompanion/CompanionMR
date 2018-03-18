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
using UnityEngine.Video;

namespace CompanionMR {

    /**
     * This class represents a video panel.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    public class VideoPanel : MonoBehaviour {

        /**
         * The title of this video.
         */
        [Tooltip("The title of this video.")]
        public Text videoTitle;

        /**
         * The raw image script of this video panel.
         */
        [Tooltip("The raw image script of this video panel.")]
        public RawImage rawImage;

        /**
         * The video player of this video panel.
         */
        [Tooltip("The video player script of this video panel.")]
        public VideoPlayer videoPlayer;

        /**
         * The progress slider of this video.
         */
        [Tooltip("The progress slider of this video.")]
        public Slider progressSlider;

        /**
         * The current time of this video.
         */
        [Tooltip("The current time of this video.")]
        public Text currentTime;

        /**
         * The length of this video.
         */
        [Tooltip("The length of this video.")]
        public Text length;

        /**
         * The play button script of this video panel.
         */
        [Tooltip("The play button script of this video panel.")]
        public Button playButton;

        /**
         * The pause button script of this video panel.
         */
        [Tooltip("The pause button script of this video panel.")]
        public Button pauseButton;

        /**
         * The stop button script of this video panel.
         */
        [Tooltip("The stop button script of this video panel.")]
        public Button stopButton;

        /**
         * Reference to the photo capture.
         */
        private PhotoCapture capture;

        /**
         * Thumbnail of this video.
         */
        private Texture2D videoThumbnail;

        /**
         * Update is called once per frame.
         */
        private void Update() {
            // Update displayed play time
            if (this.videoPlayer.isPlaying) {
                this.currentTime.text = Utils.GetMinutes(this.videoPlayer.time) + ":" + Utils.GetSeconds(this.videoPlayer.time);
                this.progressSlider.value = ((float) this.videoPlayer.time) / (this.videoPlayer.frameCount / this.videoPlayer.frameRate);
            }
        }

        /**
         * Set the data for this video.
         * 
         * @param video data for this video
         */
        public void SetData(Video video) {

            this.capture = CaptureManager.IsInitialized ? CaptureManager.Instance.PhotoCapture : null;

            // Load the video thumbnail as a texture
#if ENABLE_WINMD_SUPPORT
            if (System.IO.File.Exists(video.localImagePath)) {
                WWW www = new WWW("file:///" + video.localImagePath);
                StartCoroutine(WaitForWWW(www));
            } else {
                this.gameObject.SetActive(false);
                return;
            }
#endif
            // Set video information
            this.videoTitle.text = video.title;
            this.playButton.onClick.AddListener(this.PlayClicked);
            this.pauseButton.onClick.AddListener(this.PauseClicked);
            this.stopButton.onClick.AddListener(this.StopClicked);
            this.videoPlayer.url = "file://" + video.localVideoPath;
            this.length.text = video.length;

            // Set seperate AudioSource for the video sound playback (video bug)
            this.videoPlayer.SetTargetAudioSource(0, video.audioSrc);
        }

        /**
         * This function Is called if the play button was clicked.
         */
        public void PlayClicked() {
            this.capture.Wait(true);
        }

        /**
         * This function Is called if the pause button was clicked.
         */
        public void PauseClicked() {
            this.capture.Wait(false);
        }

        /**
         * This function Is called if the stop button was clicked.
         */
        public void StopClicked() {
            this.capture.Wait(false);
            this.rawImage.texture = this.videoThumbnail;
        }

        /**
         * Wait for the access request to be finished and obtain the image file.
         * 
         * @param www   the web request/response object (used for local file access)
         * @return enumerator for usage as coroutine
         */
        private IEnumerator WaitForWWW(WWW www) {

            // Wait for the WebRequest to be finished
            yield return www;

            Texture2D bmp = new Texture2D(www.texture.width, www.texture.height, TextureFormat.BC7, false);
            www.LoadImageIntoTexture(bmp);
            this.videoThumbnail = bmp;
            this.rawImage.texture = bmp;
        }

    }
}
