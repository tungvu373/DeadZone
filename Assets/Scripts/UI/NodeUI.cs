using UnityEngine;
using TMPro;

public class NodeUI : MonoBehaviour
{
    [Header("Main Panel")]
    public GameObject panel;

    [Header("Build Buttons")]
    public GameObject normalTowerButton;
    public GameObject fireTowerButton;
    public GameObject iceTowerButton;
    public GameObject lightningTowerButton;

    [Header("Build Texts")]
    public TextMeshProUGUI normalTowerText;
    public TextMeshProUGUI fireTowerText;
    public TextMeshProUGUI iceTowerText;
    public TextMeshProUGUI lightningTowerText;

    [Header("Tower Action Buttons")]
    public GameObject upgradeButton;
    public GameObject sellButton;

    [Header("Tower Action Texts")]
    public TextMeshProUGUI upgradeText;
    public TextMeshProUGUI sellText;

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
        Hide();
    }

    public void Show(Node node)
    {
        if (node == null || panel == null)
            return;

        panel.SetActive(true);

        if (cam != null)
        {
            panel.transform.position =
                cam.WorldToScreenPoint(node.GetBuildPosition());
        }

        bool isEmpty = node.IsEmpty();

        if (isEmpty)
        {
            ShowBuildOptions();
            HideTowerActions();
        }
        else
        {
            HideBuildOptions();
            ShowTowerActions(node);
        }
    }

    // =============================================================
    // HIỂN THỊ CÁC LOẠI THÁP CÓ THỂ XÂY
    // =============================================================

    private void ShowBuildOptions()
    {
        BuildManager manager = BuildManager.Instance;

        if (manager == null)
        {
            HideBuildOptions();
            return;
        }

        SetupBuildButton(
            normalTowerButton,
            normalTowerText,
            manager.normalTowerPrefab,
            "Normal Tower"
        );

        SetupBuildButton(
            fireTowerButton,
            fireTowerText,
            manager.fireTowerPrefab,
            "Fire Tower"
        );

        SetupBuildButton(
            iceTowerButton,
            iceTowerText,
            manager.iceTowerPrefab,
            "Ice Tower"
        );

        SetupBuildButton(
            lightningTowerButton,
            lightningTowerText,
            manager.lightningTowerPrefab,
            "Lightning Tower"
        );
    }

    private void SetupBuildButton(
        GameObject button,
        TextMeshProUGUI buttonText,
        GameObject towerPrefab,
        string towerName)
    {
        if (button == null)
            return;

        // Nếu chưa có prefab thì ẩn nút.
        if (towerPrefab == null)
        {
            button.SetActive(false);
            return;
        }

        Tower tower = towerPrefab.GetComponent<Tower>();

        // Prefab không hợp lệ thì cũng ẩn nút.
        if (tower == null || tower.data == null)
        {
            button.SetActive(false);
            return;
        }

        button.SetActive(true);

        if (buttonText != null)
        {
            buttonText.text =
                $"{towerName} ({tower.data.buildCost}v)";
        }
    }

    private void HideBuildOptions()
    {
        SetActive(normalTowerButton, false);
        SetActive(fireTowerButton, false);
        SetActive(iceTowerButton, false);
        SetActive(lightningTowerButton, false);
    }

    // =============================================================
    // HIỂN THỊ NÂNG CẤP VÀ BÁN
    // =============================================================

    private void ShowTowerActions(Node node)
    {
        if (node.tower == null)
        {
            HideTowerActions();
            return;
        }

        Tower tower = node.tower.GetComponent<Tower>();

        if (tower == null)
        {
            HideTowerActions();
            return;
        }

        bool canUpgrade = tower.CanUpgrade();

        SetActive(upgradeButton, canUpgrade);
        SetActive(sellButton, true);

        if (canUpgrade && upgradeText != null)
        {
            upgradeText.text =
                $"Upgrade Tower ({tower.GetUpgradeCost()}v)";
        }

        if (sellText != null)
        {
            sellText.text =
                $"Sell Tower ({tower.GetSellValue()}v)";
        }
    }

    private void HideTowerActions()
    {
        SetActive(upgradeButton, false);
        SetActive(sellButton, false);
    }

    private void SetActive(GameObject target, bool active)
    {
        if (target != null)
            target.SetActive(active);
    }

    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    public bool IsOpen()
    {
        return panel != null && panel.activeSelf;
    }
}