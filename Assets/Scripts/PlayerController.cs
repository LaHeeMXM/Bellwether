using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Cinemachine.Utility;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private static bool _isInstantiated = false;

    GameObject headInstance;
    SnakeHead snakeHead;
    BattleHead battleHead;
    public static CinemachineVirtualCamera cineCamera;

    void Awake()
    {
        if (_isInstantiated)
        {
            Destroy(gameObject);
            return;
        }

        _isInstantiated = true;
    }


    void Start()
    {
        if (cineCamera == null)
        {
            cineCamera = GameObject.Find("VirtualCamera").GetComponent<CinemachineVirtualCamera>();
        }

        if (headInstance == null)
        {
            headInstance = Instantiate(Resources.Load<GameObject>("Head"));
            headInstance.transform.SetParent(transform);
            headInstance.transform.localPosition = Vector3.zero;
            headInstance.transform.localRotation = Quaternion.identity;

            snakeHead = headInstance.GetComponent<SnakeHead>();
            battleHead = headInstance.GetComponent<BattleHead>();
            snakeHead.isPlayer = true;
            battleHead.isPlayer = true;
            battleHead.AddSheep("ATK");
            cineCamera.Follow = snakeHead.GetAllNodes()[0].transform;

            if (snakeHead.isPlayer && snakeHead.GetAllNodes().Count > 0)
            {
                cineCamera.Follow = snakeHead.GetAllNodes()[0].transform;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
