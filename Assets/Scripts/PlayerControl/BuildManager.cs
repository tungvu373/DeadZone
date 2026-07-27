using UnityEngine;
using UnityEngine.EventSystems;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;

    [Header("Setup")]
    public GameObject towerPrefab;
    public LayerMask nodeLayerMask;
    public NodeUI nodeUI;

    private Camera cam;
    private Node hoveredNode;     // node đang trỏ chuột (highlight)
    private Node selectedNode;    // node đang mở menu

    void Awake()
    {
        Instance = this;
        cam = Camera.main;   // vẫn dùng Main Camera — Cinemachine chỉ điều khiển, không thay thế nó
    }

    void Update()
    {
        bool overUI = EventSystem.current != null &&
                      EventSystem.current.IsPointerOverGameObject();

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        // ----- Highlight khi hover (không hover khi đang trên UI hoặc đang kéo map) -----
        if (!overUI && !CameraDragController.IsDragging)   // ✅ ĐÃ SỬA
            HandleHover(mouseWorld);
        else
            ClearHover();

        // ----- CHUỘT PHẢI: mở menu -----
        if (Input.GetMouseButtonDown(1) && !overUI)
        {
            Node node = GetNodeUnderMouse(mouseWorld);
            if (node != null)
                SelectNode(node);
            else
                DeselectNode();
        }

        // ----- CHUỘT TRÁI thả ra ở chỗ trống: đóng menu -----
        // (click lên nút UI thì EventSystem xử lý, không vào đây;
        //  kéo map thì IsDragging = true, cũng không đóng menu)
        if (Input.GetMouseButtonUp(0) && !overUI && !CameraDragController.IsDragging)   // ✅ ĐÃ SỬA
        {
            DeselectNode();
        }
    }

    Node GetNodeUnderMouse(Vector3 mouseWorld)
    {
        Collider2D hit = Physics2D.OverlapPoint(mouseWorld, nodeLayerMask);
        return hit != null ? hit.GetComponent<Node>() : null;
    }

    void HandleHover(Vector3 mouseWorld)
    {
        Node node = GetNodeUnderMouse(mouseWorld);
        if (node != hoveredNode)
        {
            if (hoveredNode != null && hoveredNode != selectedNode)
                hoveredNode.ResetColor();

            hoveredNode = node;

            if (hoveredNode != null)
                hoveredNode.Highlight();
        }
    }

    void ClearHover()
    {
        if (hoveredNode != null && hoveredNode != selectedNode)
            hoveredNode.ResetColor();
        hoveredNode = null;
    }

    void SelectNode(Node node)
    {
        // Bấm phải lần nữa vào node đang chọn → đóng menu (toggle)
        if (selectedNode == node)
        {
            DeselectNode();
            return;
        }

        if (selectedNode != null)
            selectedNode.ResetColor();

        selectedNode = node;
        selectedNode.Highlight();     // giữ highlight khi menu đang mở
        nodeUI.Show(selectedNode);
    }

    void DeselectNode()
    {
        if (selectedNode != null)
            selectedNode.ResetColor();
        selectedNode = null;
        nodeUI.Hide();
    }

    // ================== CÁC HÀM GỌI TỪ NÚT UI ==================

    public void BuildTower()
    {
        if (selectedNode == null || !selectedNode.IsEmpty()) return;

        TowerData data = towerPrefab.GetComponent<Tower>().data;

        // ✅ Check tiền
        if (!GameManager.Instance.SpendMoney(data.buildCost))
        {
            Debug.Log("Không đủ vàng!");
            return;
        }

        GameObject tower = Instantiate(towerPrefab,
            selectedNode.GetBuildPosition(), Quaternion.identity);
        selectedNode.tower = tower;

        DeselectNode();
    }

    public void UpgradeTower()
    {
        if (selectedNode == null || selectedNode.IsEmpty()) return;

        Tower tower = selectedNode.tower.GetComponent<Tower>();
        if (!tower.CanUpgrade()) return;

        // ✅ Check tiền
        if (!GameManager.Instance.SpendMoney(tower.GetUpgradeCost()))
        {
            Debug.Log("Không đủ vàng để nâng cấp!");
            return;
        }

        tower.Upgrade();
        DeselectNode();
    }

    public void SellTower()
    {
        if (selectedNode == null || selectedNode.IsEmpty()) return;

        Tower tower = selectedNode.tower.GetComponent<Tower>();
        GameManager.Instance.AddMoney(tower.GetSellValue());   // ✅ hoàn tiền

        Destroy(selectedNode.tower);
        selectedNode.tower = null;

        DeselectNode();
    }
}