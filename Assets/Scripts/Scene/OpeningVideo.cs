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
    public RawImage videoDisplay;    // 视频显示UI
    public Image fadeOverlay;        // 淡入淡出遮罩

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
            skipButton.onClick.AddListener(OnSkipButtonClicked);
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("视频播放完毕");
        SceneLoader.Instance?.LoadScene(SceneLoader.PROLOGUE_DIALOGUE);
    }

    // public方法，可以被OnClick调用
    public void OnSkipButtonClicked()
    {
        SkipVideo();
    }

    public void SkipVideo()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
            videoPlayer.Stop();
        SceneLoader.Instance?.LoadScene(SceneLoader.PROLOGUE_DIALOGUE);
    }

    void GoToNextScene()
    {
        SceneLoader.Instance?.LoadScene(SceneLoader.PROLOGUE_DIALOGUE);
    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            SkipVideo();
        }
    }
}
