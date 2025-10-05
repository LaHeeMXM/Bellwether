using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerInputManager : MonoBehaviour
{
    public static PlayerInputManager Instance;

    [Header("时间控制")]
    public float slowMotionTimeScale = 0.2f;

    [Header("镜头控制")]
    [Tooltip("在调整阶段，镜头要拉近的目标视野大小(Field of View)")]
    public float zoomedInFOV = 40f;
    [Tooltip("镜头视野变化的平滑速度")]
    public float zoomSpeed = 5f;

    [Header("高光控制")]
    [Tooltip("选中对象的外边框颜色")]
    public Color highlightColor = Color.yellow;
    [Tooltip("选中对象的外边框宽度")]
    public float highlightWidth = 4f;

    private BattleNode _selectedNode = null;
    private float _originalTimeScale = 1.0f;
    private Outline _currentOutline = null;

    // 相机相关变量
    private CinemachineVirtualCamera _cineCamera;
    private float _originalFOV;
    private bool _isZoomingIn = false;

    private Transform _originalFollowTarget;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        _cineCamera = PlayerController.cineCamera;
        if (_cineCamera != null)
        {
            _originalFOV = _cineCamera.m_Lens.FieldOfView;
            _originalFollowTarget = _cineCamera.Follow;
        }
    }

    void Update()
    {
        if (_selectedNode != null)
        {
            HandleMouseScroll();
        }

        UpdateCameraZoom();
    }

    public void StartNodeSwap(BattleNode node)
    {
        if (_selectedNode != null) return; // 防止重复触发

        _selectedNode = node;

        // --- 时间控制 ---
        _originalTimeScale = Time.timeScale;
        Time.timeScale = slowMotionTimeScale;

        // --- 高光控制 ---
        _currentOutline = _selectedNode.GetComponent<Outline>();
        if (_currentOutline != null)
        {
            _currentOutline.enabled = true; // 启用Outline组件
            _currentOutline.OutlineColor = highlightColor;
            _currentOutline.OutlineWidth = highlightWidth;
        }

        // --- 镜头控制 ---
        if (_cineCamera != null)
        {
            _isZoomingIn = true;

            // 我们需要找到被选中节点所属的那个SnakeHead，然后获取它的0号物理节点
            SnakeNode selectedNodePhysics = _selectedNode.GetHead().GetComponent<SnakeHead>().GetAllNodes()[_selectedNode.GetIndex()];
            if (selectedNodePhysics != null)
            {
                // 从相机当前的Follow目标中确定哪个是玩家蛇头
                _originalFollowTarget = _cineCamera.Follow;
                _cineCamera.Follow = selectedNodePhysics.transform;
                Debug.Log("镜头跟随目标切换为: " + selectedNodePhysics.name);
            }
        }

        Debug.Log("进入调整阶段...");
    }


    public void EndNodeSwap()
    {
        if (_selectedNode != null)
        {
            // --- 时间控制 ---
            Time.timeScale = _originalTimeScale;

            // --- 高光控制 ---
            if (_currentOutline != null)
            {
                _currentOutline.enabled = false; // 禁用Outline组件以隐藏描边
                _currentOutline = null;
            }

            // --- 镜头控制 ---
            if (_cineCamera != null)
            {
                _isZoomingIn = false;

                // 此时的蛇头可能是新的蛇头，需要重新获取
                SnakeNode newHeadNode = _selectedNode.GetHead().GetComponent<SnakeHead>().GetAllNodes()[0];
                if (newHeadNode != null)
                {
                    _cineCamera.Follow = newHeadNode.transform;
                    Debug.Log("镜头跟随目标恢复为蛇头: " + newHeadNode.name);
                }
            }

            _selectedNode = null;
            Debug.Log("调整阶段结束。");
        }
    }

    private void HandleMouseScroll()
    {
        // 获取鼠标滚轮的滚动值
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput > 0f) // 向上滚动
        {
            _selectedNode.GetHead().SwapNodeForward(_selectedNode);
        }
        else if (scrollInput < 0f) // 向下滚动
        {
            _selectedNode.GetHead().SwapNodeBackward(_selectedNode);
        }
    }

    private void UpdateCameraZoom()
    {
        if (_cineCamera == null) return;

        float targetFOV = _isZoomingIn ? zoomedInFOV : _originalFOV;

        _cineCamera.m_Lens.FieldOfView = Mathf.Lerp(
            _cineCamera.m_Lens.FieldOfView,
            targetFOV,
            Time.unscaledDeltaTime * zoomSpeed // 使用unscaledDeltaTime确保在慢动作下也能平滑变焦
        );
    }

    public void UpdateOriginalFollowTarget(Transform newHead)
    {
        // 只有在非调整模式下才更新，避免冲突
        if (_selectedNode == null)
        {
            _originalFollowTarget = newHead;
        }
    }
}
