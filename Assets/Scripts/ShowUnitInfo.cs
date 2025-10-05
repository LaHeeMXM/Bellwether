using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yummy;


public class ShowUnitInfo : MonoBehaviour
{
    VinoLabel attackLabel;
    VinoLabel healthLabel;
    VinoLabel defenseLabel;

    public void SetVisible(bool visible)
    {
        attackLabel.gameObject.SetActive(visible);
        healthLabel.gameObject.SetActive(visible);
        defenseLabel.gameObject.SetActive(visible);
    }

    void Awake()
    {
        attackLabel = transform.Find("label_attack").GetComponent<VinoLabel>();
        healthLabel = transform.Find("label_health").GetComponent<VinoLabel>();
        defenseLabel = transform.Find("label_defense").GetComponent<VinoLabel>();
    }

    public void SetData(UnitAttribute data)
    {
        attackLabel.SetText(data.Attack.ToString());
        healthLabel.SetText(data.Health.ToString());
        defenseLabel.SetText(data.Defense.ToString());
    }
    void Update()
    {
        SetVisible(Input.GetKey(KeyCode.LeftShift));
    }


}
