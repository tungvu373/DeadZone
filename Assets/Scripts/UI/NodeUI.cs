using UnityEngine;
using TMPro;

public class NodeUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panel;
    public GameObject buildButton;
    public GameObject upgradeButton;
    public GameObject sellButton;

    [Header("Texts (TextMeshPro)")]
    public TextMeshProUGUI buildText;
    public TextMeshProUGUI upgradeText;
    public TextMeshProUGUI sellText;

    [Header("Data")]
    public TowerData towerData;

    private Camera cam;

    void Awake()
    {
        cam = Camera.main;
        Hide();
    }

    public void Show(Node node)
    {
        panel.SetActive(true);
        panel.transform.position = cam.WorldToScreenPoint(node.GetBuildPosition());

        bool empty = node.IsEmpty();
        buildButton.SetActive(empty);
        upgradeButton.SetActive(!empty);
        sellButton.SetActive(!empty);

        if (empty)
        {
            buildText.text = $"Set Tower ({towerData.buildCost}v)";
        }
        else
        {
            Tower tower = node.tower.GetComponent<Tower>();
            upgradeButton.SetActive(tower.CanUpgrade());
            if (tower.CanUpgrade())
                upgradeText.text = $"Update Tower ({tower.GetUpgradeCost()}v)";
            sellText.text = $"Sell Tower ({tower.GetSellValue()}v)";
        }
    }

    public void Hide() => panel.SetActive(false);
    public bool IsOpen() => panel.activeSelf;
}