using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using YoutubePlayer;

public class VideoController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    // Start is called before the first frame update

    public string url;
    public async void Play(string videoUrl)
    {
        Controller3.instance.StartRendering(1024, ImageViewer.instance.MakeImageVoxel(1024));
        await videoPlayer.PlayYoutubeVideoAsync(videoUrl);

    }

    public async void Play()
    {
        //Controller3.instance.StartRendering(1024, ImageViewer.instance.MakeImageVoxel(1024));
        await videoPlayer.PlayYoutubeVideoAsync(url);

    }
}
