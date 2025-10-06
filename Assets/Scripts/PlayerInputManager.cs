using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerInputManager : MonoBehaviour
{
    public static PlayerInputManager Instance;

    [Header("ʱ�����")]
    public float slowMotionTimeScale = 0.2f;

    [Header("��ͷ����")]
    [Tooltip("�ڵ����׶Σ���ͷҪ������Ŀ����Ұ��С(Field of View)")]
    public float zoomedInFOV = 40f;
    [Tooltip("��ͷ��Ұ�仯��ƽ���ٶ�")]
    public float zoomSpeed = 5f;

    [Header("�߹����")]
    public Color highlightColor = Color.yellow;
    public Color hoverColor = Color.white;
    public float highlightWidth = 4f;

    private BattleNode _selectedNode = null;
    private BattleNode _hoveredNode = null;
    private float _originalTimeScale = 1.0f;

    private CinemachineVirtualCamera _cineCamera;
    private float _originalFOV;
    private bool _isZoomingIn = false;
    private Transform _originalFollowTarget;

    [Tooltip("�̳�Canvas��Panel��GameObject")]
    public GameObject tutorialPanel;



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

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (tutorialPanel != null)
            {
                tutorialPanel.SetActive(!tutorialPanel.activeSelf);
            }
        }

        if (_selectedNode != null)
        {
            HandleMouseScroll();
        }
        UpdateCameraZoom();
    }

    #region Node Interaction Callbacks

    public void OnNodeClicked(BattleNode node)
    {
        if (_selectedNode != null) return;

        _selectedNode = node;
        _hoveredNode = null;

        StartAdjustModeEffects();
    }

    public void OnNodeReleased()
    {
        if (_selectedNode != null)
        {
            EndAdjustModeEffects();

            _selectedNode = null;

            if (_hoveredNode != null)
            {
                SetOutline(_hoveredNode, hoverColor, highlightWidth);
            }
        }
    }

    public void OnNodeHoverEnter(BattleNode node)
    {
        _hoveredNode = node;
        if (_selectedNode == null)
        {
            SetOutline(_hoveredNode, hoverColor, highlightWidth);
        }
    }

    public void OnNodeHoverExit(BattleNode node)
    {
        if (_selectedNode == null && _hoveredNode == node)
        {
            ClearOutline(_hoveredNode);
        }

        if (_hoveredNode == node)
        {
            _hoveredNode = null;
        }
    }

    #endregion

    #region Effect Logic & Control

    private void StartAdjustModeEffects()
    {
        _originalTimeScale = Time.timeScale;
        Time.timeScale = slowMotionTimeScale;

        if (_cineCamera != null)
        {
            _isZoomingIn = true;

            SnakeNode selectedNodePhysics = _selectedNode.GetHead().GetComponent<SnakeHead>().GetAllNodes()[_selectedNode.GetIndex()];
            if (selectedNodePhysics != null)
            {
                _originalFollowTarget = _cineCamera.Follow;
                _cineCamera.Follow = selectedNodePhysics.transform;
            }
        }

        SetOutline(_selectedNode, highlightColor, highlightWidth);

        Debug.Log("��������׶�...");
    }

    private void EndAdjustModeEffects()
    {
        Time.timeScale = _originalTimeScale;

        if (_cineCamera != null)
        {
            _isZoomingIn = false;

            SnakeNode newHeadNode = _selectedNode.GetHead().GetComponent<SnakeHead>().GetAllNodes()[0];
            if (newHeadNode != null)
            {
                _cineCamera.Follow = newHeadNode.transform;
            }
        }

        ClearOutline(_selectedNode);

        Debug.Log("�����׶ν�����");
    }

    private void HandleMouseScroll()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput > 0f)
        {
            _selectedNode.GetHead().SwapNodeForward(_selectedNode);
        }
        else if (scrollInput < 0f)
        {
            _selectedNode.GetHead().SwapNodeBackward(_selectedNode);
        }
    }

    private void UpdateCameraZoom()
    {
        if (_cineCamera == null) return;
        float targetFOV = _isZoomingIn ? zoomedInFOV : _originalFOV;
        _cineCamera.m_Lens.FieldOfView = Mathf.Lerp(_cineCamera.m_Lens.FieldOfView, targetFOV, Time.unscaledDeltaTime * zoomSpeed);
    }

    public void UpdateOriginalFollowTarget(Transform newHead)
    {
        if (_selectedNode == null)
        {
            _originalFollowTarget = newHead;
        }
    }

    private void SetOutline(BattleNode node, Color color, float width)
    {
        if (node == null) return;
        var outline = node.GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = true;
            outline.OutlineColor = color;
            outline.OutlineWidth = width;
        }
    }

    private void ClearOutline(BattleNode node)
    {
        if (node == null) return;
        var outline = node.GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = false;
        }
    }

    #endregion
}