using UnityEngine;
using UnityEngine.Video;

namespace YoutubePlayer
{
    public class SimpleYoutubeVideo : MonoBehaviour
    {
        public string videoUrl;
        public static SimpleYoutubeVideo instance;
        private void Awake()
        {
            instance = this;
        }
        public async void Play()
        {
            Debug.Log("Loading video...");
            var videoPlayer = GetComponent<VideoPlayer>();
            await videoPlayer.PlayYoutubeVideoAsync(videoUrl);
        }
    }
}