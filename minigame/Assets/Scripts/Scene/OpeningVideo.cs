using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

/// <summary>
/// 开场CG视频播放 - 通过RawImage显示视频
/// </summary>
public class OpeningVideo : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public Button skipButton;
    public RawImage videoDisplay;  // 视频显示UI

    private bool hasTransitioned = false;

    void Start()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Play();
            videoPlayer.loopPointReached += OnVideoEnd;
            Debug.Log("视频开始播放");
        }
        else
        {
            Debug.Log("VideoPlayer为空，直接跳转");
            Invoke("GoToNextScene", 2f);
        }

        if (skipButton != null)
            skipButton.onClick.AddListener(SkipVideo);
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("视频播放完毕");
        GoToNextScene();
    }

    void SkipVideo()
    {
        if (hasTransitioned) return;
        if (videoPlayer != null && videoPlayer.isPlaying)
            videoPlayer.Stop();
        GoToNextScene();
    }

    void GoToNextScene()
    {
        if (hasTransitioned) return;
        hasTransitioned = true;
        enabled = false;
        SceneLoader.Instance?.LoadScene(SceneLoader.PROLOGUE_DIALOGUE);
    }

    void Update()
    {
        if (hasTransitioned) return;
        if (Input.anyKeyDown)
        {
            SkipVideo();
        }
    }
}
