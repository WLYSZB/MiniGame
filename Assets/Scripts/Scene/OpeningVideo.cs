using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

/// <summary>
/// 开场CG视频播放 - 暂用2秒空白占位
/// </summary>
public class OpeningVideo : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public Button skipButton;

    void Start()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Play();
            videoPlayer.loopPointReached += OnVideoEnd;
        }
        else
        {
            // 如果没有视频，直接跳到下一场景
            Invoke("GoToNextScene", 2f);
        }

        if (skipButton != null)
            skipButton.onClick.AddListener(SkipVideo);
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        // 视频播放完毕，进入下一场景
        SceneLoader.Instance?.LoadScene(SceneLoader.PROLOGUE_DIALOGUE);
    }

    void SkipVideo()
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
        // 按任意键跳过（可选）
        if (Input.anyKeyDown)
        {
            SkipVideo();
        }
    }
}
