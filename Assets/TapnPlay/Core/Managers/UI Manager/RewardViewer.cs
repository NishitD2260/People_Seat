using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardViewer : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI countText;

    public void SetupInfo(Sprite icon, string count)
    {
        this.icon.sprite = icon;
        this.countText.text = count;
        gameObject.SetActive(true);
    }
}
