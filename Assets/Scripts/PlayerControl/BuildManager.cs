using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;

    [Header("Tower Prefabs")]
    public GameObject normalTowerPrefab;
    public GameObject fireTowerPrefab;

    [Tooltip("Có thể để trống và thêm sau.")]
    public GameObject iceTowerPrefab;

    [Tooltip("Có thể để trống và thêm sau.")]
    public GameObject lightningTowerPrefab;

    [Header("Setup")]
    public LayerMask nodeLayerMask;
    public NodeUI nodeUI;

    private Camera cam;
    private Node hoveredNode;
    private Node selectedNode;

    private void Awake()
    {
        Instance = this;
        cam = Camera.main;
    }

    private void Update()
    {
        if (cam == null)
            return;

        // Chỉ chặn thao tác thế giới khi chuột nằm trên UI tương tác.
        bool overUI = IsPointerOverInteractiveUI();

        Vector3 mouseWorld =
            cam.ScreenToWorldPoint(Input.mousePosition);

        mouseWorld.z = 0f;

        if (!overUI && !CameraDragController.IsDragging)
        {
            HandleHover(mouseWorld);
        }
        else
        {
            ClearHover();
        }

        // Chuột phải: chọn node.
        if (Input.GetMouseButtonDown(1) && !overUI)
        {
            Node node = GetNodeUnderMouse(mouseWorld);

            if (node != null)
                SelectNode(node);
            else
                DeselectNode();
        }

        // Chuột trái vào chỗ trống: đóng menu.
        if (Input.GetMouseButtonUp(0) &&
            !overUI &&
            !CameraDragController.IsDragging)
        {
            DeselectNode();
        }
    }

    private bool IsPointerOverInteractiveUI()
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData pointerData =
            new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            Selectable selectable =
                result.gameObject.GetComponentInParent<Selectable>();

            if (selectable != null &&
                selectable.gameObject.activeInHierarchy &&
                selectable.interactable)
            {
                return true;
            }
        }

        return false;
    }

    private Node GetNodeUnderMouse(Vector3 mouseWorld)
    {
        Collider2D hit = Physics2D.OverlapPoint(
            mouseWorld,
            nodeLayerMask
        );

        return hit != null ? hit.GetComponent<Node>() : null;
    }

    private void HandleHover(Vector3 mouseWorld)
    {
        Node node = GetNodeUnderMouse(mouseWorld);

        if (node == hoveredNode)
            return;

        if (hoveredNode != null && hoveredNode != selectedNode)
            hoveredNode.ResetColor();

        hoveredNode = node;

        if (hoveredNode != null)
            hoveredNode.Highlight();
    }

    private void ClearHover()
    {
        if (hoveredNode != null && hoveredNode != selectedNode)
            hoveredNode.ResetColor();

        hoveredNode = null;
    }

    private void SelectNode(Node node)
    {
        if (selectedNode == node)
        {
            DeselectNode();
            return;
        }

        if (selectedNode != null)
        {
            selectedNode.ResetColor();
            ShowTowerRange(selectedNode, false);
        }

        selectedNode = node;
        selectedNode.Highlight();

        if (nodeUI != null)
            nodeUI.Show(selectedNode);

        ShowTowerRange(selectedNode, true);
    }

    private void DeselectNode()
    {
        if (selectedNode != null)
        {
            selectedNode.ResetColor();
            ShowTowerRange(selectedNode, false);
        }

        selectedNode = null;

        if (nodeUI != null)
            nodeUI.Hide();
    }

    private void ShowTowerRange(Node node, bool show)
    {
        if (node == null || node.tower == null)
            return;

        Tower tower = node.tower.GetComponent<Tower>();

        if (tower != null)
            tower.ShowRange(show);
    }

    public void BuildNormalTower()
    {
        BuildTower(normalTowerPrefab);
    }

    public void BuildFireTower()
    {
        BuildTower(fireTowerPrefab);
    }

    public void BuildIceTower()
    {
        BuildTower(iceTowerPrefab);
    }

    public void BuildLightningTower()
    {
        BuildTower(lightningTowerPrefab);
    }

    private void BuildTower(GameObject towerPrefab)
    {
        if (selectedNode == null || !selectedNode.IsEmpty())
            return;

        if (towerPrefab == null)
        {
            Debug.LogWarning("Prefab tháp này chưa được thiết lập.");
            return;
        }

        Tower prefabTower = towerPrefab.GetComponent<Tower>();

        if (prefabTower == null || prefabTower.data == null)
        {
            Debug.LogError(
                $"Prefab {towerPrefab.name} chưa có Tower hoặc TowerData."
            );
            return;
        }
      
        GameObject tower = Instantiate(towerPrefab,
            selectedNode.GetBuildPosition(), Quaternion.identity);
        selectedNode.tower = tower;

        GameObject newTower = Instantiate(
            towerPrefab,
            selectedNode.GetBuildPosition(),
            Quaternion.identity
        );

        selectedNode.tower = newTower;
        DeselectNode();
    }

    public void UpgradeTower()
    {
        if (selectedNode == null || selectedNode.IsEmpty())
            return;

        Tower tower = selectedNode.tower.GetComponent<Tower>();

        if (tower == null || !tower.CanUpgrade())
            return;

        if (!GameManager.Instance.SpendMoney(
                tower.GetUpgradeCost()))
        {
            Debug.Log("Không đủ vàng để nâng cấp!");
            return;
        }

        tower.Upgrade();
        DeselectNode();
    }

    public void SellTower()
    {
        if (selectedNode == null || selectedNode.IsEmpty())
            return;

        Tower tower = selectedNode.tower.GetComponent<Tower>();

        if (tower == null)
            return;

        GameManager.Instance.AddMoney(tower.GetSellValue());

        GameObject towerToDestroy = selectedNode.tower;
        selectedNode.tower = null;

        Destroy(towerToDestroy);
        DeselectNode();
    }
}