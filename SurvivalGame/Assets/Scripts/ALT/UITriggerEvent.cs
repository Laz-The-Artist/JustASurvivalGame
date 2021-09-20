using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITriggerEvent : MonoBehaviour
{
    public bool isHovered = false;
    public GameObject Tooltip;

    void Start()
    {

    }


    void Update()
    {
        if (isHovered) {
            Tooltip.SetActive(true);
        } else {
            Tooltip.SetActive(false);
        }
    }

    void OnMouseEnter() {
        isHovered = true;
        Debug.Log("kurva vagy");
    }

    void OnMouseExit() {
        isHovered = false;
    }
}
