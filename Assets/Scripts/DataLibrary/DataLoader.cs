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
using System.Collections.Generic;
using UnityEngine;

namespace CompanionMR {

    /**
     * This class provides logic to save and load information data for artworks.
     * 
     * @author Dimitri Kotlovsky, Andreas Sekulski
     */
    public class DataLoader : HoloToolkit.Unity.Singleton<DataLoader> {

        /**
         * Web protocols.
         */
        public enum Protocol {
            HTTP,
            HTTPS
        }

        /**
         * Web requests.
         */
        private enum WebRequestFormat {
            Json,
            Media
        }

        [Header("Web Service Config")]

        /**
         * Web protocol.
         */
        [Tooltip("Web protocol.")]
        public Protocol protocol = Protocol.HTTP;

        /**
         * Host name or IP address.
         */
        [Tooltip("Host name or IP address.")]
        public string host = "192.168.2.9";

        /**
         * Port number.
         */
        [Tooltip("Port number.")]
        [Range(1, 49151)]
        public int port = 8080;

        /**
         * URL path (starts with a slash).
         */
        [Tooltip("URL path (starts with a slash).")]
        public string path = "/getCompanionData";

        /**
         * Name of the directory for artist images.
         */
        public const string ARTIST_DIRECTORY = "artist";

        /**
         * Name of the directory for artwork images.
         */
        public const string ARTWORK_DIRECTORY = "artwork";

        /**
         * Name of the directory for video files.
         */
        public const string VIDEO_DIRECTORY = "video";

        /**
         * Name of the directory for audio files.
         */
        public const string AUDIO_DIRECTORY = "audio";

        /**
         * Name of the data JSON file.
         */
        public const string JSON_FILE = "data.json";

        /**
         * Name prefix for artist images.
         */
        public const string ARTIST_IMAGE_PREFIX = "artist";

        /**
         * Name prefix for artwork images.
         */
        public const string ARTWORK_IMAGE_PREFIX = "artwork";

        /**
         * Name prefix for video files.
         */
        public const string VIDEO_PREFIX = "video";

        /**
         * Name prefix for audio files.
         */
        public const string AUDIO_PREFIX = "audio";

        /**
         * Path separator.
         */
        public const string PATH_SEPARATOR = "\\";

        /**
         * Image format.
         */
        public const string IMAGE_FORMAT = ".jpg";

        /**
         * Video format.
         */
        public const string VIDEO_FORMAT = ".mp4";

        /**
         * Audio format.
         */
        public const string AUDIO_FORMAT = ".ogg";

        /**
         * Collection of all artists.
         */
        public Dictionary<int, Artist> Artists { get; private set; }

        /**
         * Collection of artworks.
         */
        public Dictionary<int, Artwork> Artworks { get; private set; }

        /**
         * Collection of videos.
         */
        public Dictionary<int, Video> Videos { get; private set; }

        /**
         * Collection of audio.
         */
        public Dictionary<int, Audio> Audio { get; private set; }

        /**
         * Indicates whether a manual IP configuration is needed.
         */
        public bool NeedsManualConfiguration { get; private set; }

        /**
         * Indicates whether the data loading process has finished.
         */
        public bool DataLoadingFinished { get; private set; }

        /**
         * Path of the directory for artist images.
         */
        public string ArtistPath { get; private set; }

        /**
         * Path of the directory for artwork images.
         */
        public string ArtworkPath { get; private set; }

        /**
         * Path of the directory for video files.
         */
        public string VideoPath { get; private set; }

        /**
         * Path of the directory for audio files.
         */
        public string AudioPath { get; private set; }

        /**
         * Path of the JSON file.
         */
        public string JsonPath { get; private set; }

        /**
         * Reference to the manual IP config utility.
         */
        private IPConfig config;

        /**
         * Reference to the debug logger.
         */
        private DebugLogger logger;

        /**
         * The timestamp of the current data.
         */
        private long timestamp;

        /**
         * Indicates whether data loading coroutines have finished.
         */
        private int loadDataCounter;

        /**
         * Protocol prefix string.
         */
        private string protocolString;

        /**
         * Use this for initialization.
         */
        private void Start() {

            // Variable initialization
            this.Artworks = new Dictionary<int, Artwork>();
            this.Artists = new Dictionary<int, Artist>();
            this.Videos = new Dictionary<int, Video>();
            this.Audio = new Dictionary<int, Audio>();
            this.NeedsManualConfiguration = false;
            this.DataLoadingFinished = false;
            this.ArtistPath = "";
            this.ArtworkPath = "";
            this.VideoPath = "";
            this.AudioPath = "";
            this.JsonPath = "";
            this.config = IPConfig.Instance;
            this.logger = DebugLogger.Instance;
            this.timestamp = 0;
            this.loadDataCounter = 0;
            this.protocolString = (this.protocol == Protocol.HTTP) ? "http://" : "https:://";

            // Deactivate this script if necessary components are missing
            if ((this.config == null) || (this.logger == null)) {
                Debug.LogError("DataLoader: Script references not set properly.");
                this.enabled = false;
                return;
            }

#if ENABLE_WINMD_SUPPORT

            // Set necessary data paths
            Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            this.ArtistPath = localFolder.Path + PATH_SEPARATOR + ARTIST_DIRECTORY;
            this.ArtworkPath = localFolder.Path + PATH_SEPARATOR + ARTWORK_DIRECTORY;
            this.VideoPath = localFolder.Path + PATH_SEPARATOR + VIDEO_DIRECTORY;
            this.AudioPath = localFolder.Path + PATH_SEPARATOR + AUDIO_DIRECTORY;
            this.JsonPath = localFolder.Path + PATH_SEPARATOR + JSON_FILE;
            
            // Create image directories (if they do not already exist)
            System.IO.Directory.CreateDirectory(this.ArtistPath);
            System.IO.Directory.CreateDirectory(this.ArtworkPath);
            System.IO.Directory.CreateDirectory(this.VideoPath);
            System.IO.Directory.CreateDirectory(this.AudioPath);

            // Read current data file
            this.logger.Log("Read current data file.");
            if (System.IO.File.Exists(this.JsonPath)) {

                string json = System.IO.File.ReadAllText(this.JsonPath);
                CompanionData data = JsonUtility.FromJson<CompanionData>(json);

                // Save current timestamp
                this.timestamp = data.timestamp;

                // Load artists
                foreach (Artist artist in data.artists) {
                    artist.localImagePath = this.ArtistPath + PATH_SEPARATOR + ARTIST_IMAGE_PREFIX + artist.artistID + IMAGE_FORMAT;
                    this.Artists.Add(artist.artistID, artist);
                }

                // Load artworks
                foreach (Artwork artwork in data.artworks) {
                    artwork.localImagePath = this.ArtworkPath + PATH_SEPARATOR + ARTWORK_IMAGE_PREFIX + artwork.artworkID + IMAGE_FORMAT;
                    this.Artworks.Add(artwork.artworkID, artwork);
                }

                // Load videos
                foreach (Video video in data.videos) {
                    video.localVideoPath = this.VideoPath + PATH_SEPARATOR + VIDEO_PREFIX + video.videoID + VIDEO_FORMAT;
                    video.localImagePath = this.VideoPath + PATH_SEPARATOR + VIDEO_PREFIX + video.videoID + IMAGE_FORMAT;
                    this.Videos.Add(video.videoID, video);
                }

                // Load audio
                foreach (Audio audio in data.audio) {
                    audio.localAudioPath = this.AudioPath + PATH_SEPARATOR + AUDIO_PREFIX + audio.audioID + AUDIO_FORMAT;
                    this.Audio.Add(audio.audioID, audio);
                }

            } else {
                this.logger.Log("No current data present.");
            }

            // Load server data
            this.logger.Log("Load new data.");
            this.loadDataCounter++;
            try {
                WWW request = new WWW(this.protocolString + this.host + ":" + this.port.ToString() + this.path);
                StartCoroutine(this.WaitForWWW(WebRequestFormat.Json, request));
            } catch (System.Exception ex) {
                this.logger.Log(ex.Message);
            }
#endif
        }

        /**
         * Update is called once per frame.
         */
        private void Update() {
            // Check if the data is loaded
            if ((this.loadDataCounter == 0) && !this.DataLoadingFinished) {
                this.logger.Log("Data loading finished.");
                this.DataLoadingFinished = true;
                this.enabled = false;
            }
        }

        /**
         * Wait for the web request to be finished and delegate the response to a seperate function.
         * 
         * @param www           the web request/response object
         * @param format        the format of the web request
         * @param completePath  (optional) complete image path
         * @param imageName     (optional) the name of the image
         * 
         * @return enumerator for usage as coroutine
         */
        private IEnumerator WaitForWWW(WebRequestFormat format, WWW www, string completePath = "", string imageName = "") {

            // Wait for the WebRequest to be finished
            yield return www;

            // Forward the response to the specific functions
            switch (format) {
                case WebRequestFormat.Json:
                    this.ParseJSON(www);
                    break;
                case WebRequestFormat.Media:
                    this.SaveMedia(www, completePath, imageName);
                    break;
            }
        }

        /**
         * Parse the JSON file that is returned by the web request.
         * 
         * @param www   the web request/response object
         */
        private void ParseJSON(WWW www) {

            // Success
            if (string.IsNullOrEmpty(www.error)) {

                if (this.NeedsManualConfiguration) {
                    this.config.ConnectionSuccessful(true);
                }

                // Parse the json file
                this.logger.Log("Parse the json file.");
                CompanionData data = JsonUtility.FromJson<CompanionData>(www.text);

                // Update data
                if (data.timestamp > this.timestamp) {

                    try {
                        this.UpdateArtists(data.artists);
                        this.UpdateArtworks(data.artworks);
                        this.UpdateVideos(data.videos);
                        this.UpdateAudio(data.audio);
                    } catch (System.Exception ex) {
                        this.logger.Log(ex.Message);
                        return;
                    }

                    // Save the new json to disk
                    System.IO.File.WriteAllText(this.JsonPath, www.text);

                } else {
                    this.logger.Log("Current data is up to date.");
                }

                this.loadDataCounter--;

            // Error
            } else {
                this.logger.Log("Parsing json error: " + www.error);
                if (this.timestamp != 0) {
                    this.logger.Log("Use last known json instead.");
                    this.loadDataCounter--;
                } else {
                    this.logger.Log("Wait for manual IP config.");
                    if (this.NeedsManualConfiguration) {
                        this.config.ConnectionSuccessful(false);
                    } else {
                        this.NeedsManualConfiguration = true;
                    }
                    StartCoroutine(this.WaitForIP());
                }
            }
        }

        /**
         * Wait for the manual IP configuration to be finished.
         * 
         * @return enumerator for usage as coroutine
         */
        private IEnumerator WaitForIP() {

            // Wait until IP was configured manually
            yield return new WaitUntil(this.config.IsReady);
            this.host = this.config.IpAddress;

            // New request
            this.logger.Log("New IP configured manually. Try again...");
            try {
                WWW request = new WWW(this.protocolString + this.host + ":" + this.port.ToString() + this.path);
                StartCoroutine(this.WaitForWWW(WebRequestFormat.Json, request));
            } catch (System.Exception ex) {
                this.logger.Log(ex.Message);
                this.config.ConnectionSuccessful(false);
                StartCoroutine(this.WaitForIP());
            }

        }

        /**
         * Save the media file that is returned by the web request.
         * 
         * @param www           the web request/response object
         * @param completePath  complete media path
         * @param mediaName     the name of the media file
         */
        private void SaveMedia(WWW www, string completePath, string mediaName) {

            // Success
            if (string.IsNullOrEmpty(www.error)) {
                byte[] media = www.bytes;
                System.IO.File.WriteAllBytes(completePath, media);
                this.logger.Log("Saved media: " + mediaName);
                this.loadDataCounter--;

            // Error
            } else {
                this.logger.Log("Download media error: " + www.error);
                this.loadDataCounter--;

                // non 404 error (try again)
                if (!www.error.Equals("404 Not Found")) {
                    WWW request = new WWW(www.url);
                    StartCoroutine(this.WaitForWWW(WebRequestFormat.Media, request, completePath, mediaName));
                }
            }

        }

        /**
         * Update the artist data.
         * 
         * @param artists   list of artists
         */
        private void UpdateArtists(List<Artist> artists) {

            // Update artists
            foreach (Artist remoteArtist in artists) {

                int id = remoteArtist.artistID;
                string imageName = ARTIST_IMAGE_PREFIX + id + IMAGE_FORMAT;
                string imagePath = this.ArtistPath + PATH_SEPARATOR + imageName;

                // Check if this artist already exists
                if (this.Artists.ContainsKey(id)) {

                    Artist localArtist = this.Artists[id];
                    if (remoteArtist.timestamp > localArtist.timestamp) {

                        // Update artist
                        this.logger.Log("Update artist: " + id);
                        remoteArtist.localImagePath = imagePath;
                        this.Artists[id] = remoteArtist;
                        this.loadDataCounter++;

                        // Save image
                        WWW request = new WWW(remoteArtist.image);
                        StartCoroutine(this.WaitForWWW(WebRequestFormat.Media, request, imagePath, imageName));

                    } else {
                        this.logger.Log(ARTIST_IMAGE_PREFIX + id + " is up to date.");
                    }

                } else {

                    // Add new artist
                    this.logger.Log("Add new artist: " + id);
                    remoteArtist.localImagePath = imagePath;
                    this.Artists.Add(id, remoteArtist);
                    this.loadDataCounter++;

                    // Save image
                    WWW request = new WWW(remoteArtist.image);
                    StartCoroutine(this.WaitForWWW(WebRequestFormat.Media, request, imagePath, imageName));
                }
            }

        }

        /**
         * Update the artwork data.
         * 
         * @param artworks  list of artworks
         */
        private void UpdateArtworks(List<Artwork> artworks) {

            // Update artworks
            foreach (Artwork remoteArt in artworks) {

                int id = remoteArt.artworkID;
                string imageName = ARTWORK_IMAGE_PREFIX + id + IMAGE_FORMAT;
                string imagePath = this.ArtworkPath + PATH_SEPARATOR + imageName;

                // Check if this artwork already exists
                if (this.Artworks.ContainsKey(id)) {

                    Artwork localArt = this.Artworks[id];
                    if (remoteArt.timestamp > localArt.timestamp) {

                        // Update artist
                        this.logger.Log("Update artwork: " + id);
                        remoteArt.localImagePath = imagePath;
                        this.Artworks[id] = remoteArt;
                        this.loadDataCounter++;

                        // Save image
                        WWW request = new WWW(remoteArt.image);
                        StartCoroutine(this.WaitForWWW(WebRequestFormat.Media, request, imagePath, imageName));

                    } else {
                        this.logger.Log(ARTWORK_IMAGE_PREFIX + id + " is up to date.");
                    }

                } else {

                    // Add new artwork
                    this.logger.Log("Add new artwork: " + id);
                    remoteArt.localImagePath = imagePath;
                    this.Artworks.Add(id, remoteArt);
                    this.loadDataCounter++;

                    // Save image
                    WWW request = new WWW(remoteArt.image);
                    StartCoroutine(this.WaitForWWW(WebRequestFormat.Media, request, imagePath, imageName));
                }
            }

        }

        /**
         * Update the video data.
         * 
         * @param videos    list of videos
         */
        private void UpdateVideos(List<Video> videos) {

            // Update videos
            foreach (Video remoteVideo in videos) {

                int id = remoteVideo.videoID;
                string videoName = VIDEO_PREFIX + id + VIDEO_FORMAT;
                string videoPath = this.VideoPath + PATH_SEPARATOR + videoName;
                string imageName = VIDEO_PREFIX + id + IMAGE_FORMAT;
                string imagePath = this.VideoPath + PATH_SEPARATOR + imageName;

                // Check if this video already exists
                if (this.Videos.ContainsKey(id)) {

                    Video localVideo = this.Videos[id];
                    if (remoteVideo.timestamp > localVideo.timestamp) {

                        // Update video
                        this.logger.Log("Update video: " + id);
                        remoteVideo.localVideoPath = videoPath;
                        remoteVideo.localImagePath = imagePath;
                        this.Videos[id] = remoteVideo;
                        this.loadDataCounter += 2;

                        // Save video
                        WWW request = new WWW(remoteVideo.video);
                        StartCoroutine(this.WaitForWWW(WebRequestFormat.Media, request, videoPath, videoName));

                        // Save image
                        request = new WWW(remoteVideo.image);
                        StartCoroutine(this.WaitForWWW(WebRequestFormat.Media, request, imagePath, imageName));

                    } else {
                        this.logger.Log(VIDEO_PREFIX + id + " is up to date.");
                    }

                } else {

                    // Add new video
                    this.logger.Log("Add new video: " + id);
                    remoteVideo.localVideoPath = videoPath;
                    remoteVideo.localImagePath = imagePath;
                    this.Videos.Add(id, remoteVideo);
                    this.loadDataCounter += 2;

                    // Save video
                    WWW request = new WWW(remoteVideo.video);
                    StartCoroutine(this.WaitForWWW(WebRequestFormat.Media, request, videoPath, videoName));

                    // Save image
                    request = new WWW(remoteVideo.image);
                    StartCoroutine(this.WaitForWWW(WebRequestFormat.Media, request, imagePath, imageName));
                }
            }

        }

        /**
         * Update the audio data.
         * 
         * @param audioData list of audio data
         */
        private void UpdateAudio(List<Audio> audioData) {

            // Update audio
            foreach (Audio remoteAudio in audioData) {

                int id = remoteAudio.audioID;
                string audioName = AUDIO_PREFIX + id + AUDIO_FORMAT;
                string audioPath = this.AudioPath + PATH_SEPARATOR + audioName;

                // Check if this audio already exists
                if (this.Audio.ContainsKey(id)) {

                    Audio localAudio = this.Audio[id];
                    if (remoteAudio.timestamp > localAudio.timestamp) {

                        // Update artist
                        this.logger.Log("Update audio: " + id);
                        remoteAudio.localAudioPath = audioPath;
                        this.Audio[id] = remoteAudio;
                        this.loadDataCounter++;

                        // Save audio
                        WWW request = new WWW(remoteAudio.audio);
                        StartCoroutine(this.WaitForWWW(WebRequestFormat.Media, request, audioPath, audioName));

                    } else {
                        this.logger.Log(AUDIO_PREFIX + id + " is up to date.");
                    }

                } else {

                    // Add new audio
                    this.logger.Log("Add new audio: " + id);
                    remoteAudio.localAudioPath = audioPath;
                    this.Audio.Add(id, remoteAudio);
                    this.loadDataCounter++;

                    // Save audio
                    WWW request = new WWW(remoteAudio.audio);
                    StartCoroutine(this.WaitForWWW(WebRequestFormat.Media, request, audioPath, audioName));
                }
            }

        }

    }
}
