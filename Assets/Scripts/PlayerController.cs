using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Cinemachine.Utility;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    GameObject headInstance;
    SnakeHead snakeHead;
    BattleHead battleHead;
    CinemachineVirtualCamera cineCamera;
    // Start is called before the first frame update
    void Start()
    {
        cineCamera = GameObject.Find("VirtualCamera").GetComponent<CinemachineVirtualCamera>();
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
            battleHead.AddSheep("Sheep01");
            cineCamera.Follow = snakeHead.GetAllNodes()[0].transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
