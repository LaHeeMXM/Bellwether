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
    [Tooltip("ѡ�ж������߿���ɫ")]
    public Color highlightColor = Color.yellow;
    [Tooltip("ѡ�ж������߿���")]
    public float highlightWidth = 4f;

    private BattleNode _selectedNode = null;
    private float _originalTimeScale = 1.0f;
    private Outline _currentOutline = null;

    // �����ر���
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
        if (_selectedNode != null) return; // ��ֹ�ظ�����

        _selectedNode = node;

        // --- ʱ����� ---
        _originalTimeScale = Time.timeScale;
        Time.timeScale = slowMotionTimeScale;

        // --- �߹���� ---
        _currentOutline = _selectedNode.GetComponent<Outline>();
        if (_currentOutline != null)
        {
            _currentOutline.enabled = true; // ����Outline���
            _currentOutline.OutlineColor = highlightColor;
            _currentOutline.OutlineWidth = highlightWidth;
        }

        // --- ��ͷ���� ---
        if (_cineCamera != null)
        {
            _isZoomingIn = true;

            // ������Ҫ�ҵ���ѡ�нڵ��������Ǹ�SnakeHead��Ȼ���ȡ����0������ڵ�
            SnakeNode selectedNodePhysics = _selectedNode.GetHead().GetComponent<SnakeHead>().GetAllNodes()[_selectedNode.GetIndex()];
            if (selectedNodePhysics != null)
            {
                // �������ǰ��FollowĿ����ȷ���ĸ��������ͷ
                _originalFollowTarget = _cineCamera.Follow;
                _cineCamera.Follow = selectedNodePhysics.transform;
                Debug.Log("��ͷ����Ŀ���л�Ϊ: " + selectedNodePhysics.name);
            }
        }

        Debug.Log("��������׶�...");
    }


    public void EndNodeSwap()
    {
        if (_selectedNode != null)
        {
            // --- ʱ����� ---
            Time.timeScale = _originalTimeScale;

            // --- �߹���� ---
            if (_currentOutline != null)
            {
                _currentOutline.enabled = false; // ����Outline������������
                _currentOutline = null;
            }

            // --- ��ͷ���� ---
            if (_cineCamera != null)
            {
                _isZoomingIn = false;

                // ��ʱ����ͷ�������µ���ͷ����Ҫ���»�ȡ
                SnakeNode newHeadNode = _selectedNode.GetHead().GetComponent<SnakeHead>().GetAllNodes()[0];
                if (newHeadNode != null)
                {
                    _cineCamera.Follow = newHeadNode.transform;
                    Debug.Log("��ͷ����Ŀ��ָ�Ϊ��ͷ: " + newHeadNode.name);
                }
            }

            _selectedNode = null;
            Debug.Log("�����׶ν�����");
        }
    }

    private void HandleMouseScroll()
    {
        // ��ȡ�����ֵĹ���ֵ
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput > 0f) // ���Ϲ���
        {
            _selectedNode.GetHead().SwapNodeForward(_selectedNode);
        }
        else if (scrollInput < 0f) // ���¹���
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
            Time.unscaledDeltaTime * zoomSpeed // ʹ��unscaledDeltaTimeȷ������������Ҳ��ƽ���佹
        );
    }

    public void UpdateOriginalFollowTarget(Transform newHead)
    {
        // ֻ���ڷǵ���ģʽ�²Ÿ��£������ͻ
        if (_selectedNode == null)
        {
            _originalFollowTarget = newHead;
        }
    }
}
