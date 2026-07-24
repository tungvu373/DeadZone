using UnityEngine;

public class NodeUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panel;          // panel chứa các nút
    public GameObject buildButton;
    public GameObject upgradeButton;
    public GameObject sellButton;

    private Camera cam;

    void Awake()
    {
        cam = Camera.main;
        Hide();
    }

    public void Show(Node node)
    {
        panel.SetActive(true);

        // Đặt menu ngay tại vị trí node trên màn hình
        panel.transform.position = cam.WorldToScreenPoint(node.GetBuildPosition());

        // Hiện nút theo trạng thái ô
        bool empty = node.IsEmpty();
        buildButton.SetActive(empty);
        upgradeButton.SetActive(!empty);
        sellButton.SetActive(!empty);

        // Nếu tower max level thì ẩn nút nâng cấp
        if (!empty)
        {
            Tower tower = node.tower.GetComponent<Tower>();
            upgradeButton.SetActive(tower.CanUpgrade());
        }
    }

    public void Hide()
    {
        panel.SetActive(false);
    }

    public bool IsOpen()
    {
        return panel.activeSelf;
    }
}