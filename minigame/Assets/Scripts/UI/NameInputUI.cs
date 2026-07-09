using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 起名弹窗 - 输入角色名字
/// </summary>
public class NameInputUI : MonoBehaviour
{
    public GameObject panel;
    public TMP_InputField nameInput;
    public Button confirmButton;
    private System.Action<string> onConfirm;

    void Awake() => panel.SetActive(false);

    public void Show(System.Action<string> callback)
    {
        onConfirm = callback;
        nameInput.text = "";
        panel.SetActive(true);
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnConfirm);
    }

    void OnConfirm()
    {
        string name = nameInput.text.Trim();
        if (string.IsNullOrEmpty(name))
            name = "垃圾袋";
        panel.SetActive(false);
        onConfirm?.Invoke(name);
    }
}
