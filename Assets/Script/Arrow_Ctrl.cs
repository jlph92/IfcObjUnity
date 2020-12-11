using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Arrow_Ctrl : MonoBehaviour
{
    public float dragSpeed = 10.0f;
    public GameObject Highlight;
    public GameObject Single_Axis_Control;
    private Vector3 prev_pos;

    public delegate void Dragged(object sender, Vector3 position);

    public event Dragged OnDragged;

    void Start()
    {
        Highlight.SetActive(false);
    }

    public void Activate(bool active)
    {
        GetComponentInChildren<CapsuleCollider>().enabled = active;
    }

    void OnMouseEnter()
    {
        Highlight.SetActive(true);
        prev_pos = Input.mousePosition;
    }

    void OnMouseOver()
    {
        Highlight.SetActive(true);
    }

    void OnMouseExit()
    {
        Highlight.SetActive(false);
    }

    void dragging()
    {
        Highlight.SetActive(true);
        //Debug.Log(System.String.Format("{0}, {1}", Input.mousePosition, prev_pos));

        Vector3 prev_pos_clip = prev_pos;
        prev_pos_clip.z = Camera.main.nearClipPlane;

        Vector3 current_pos_clip = Input.mousePosition;
        current_pos_clip.z = Camera.main.nearClipPlane;

        Vector3 world_prev = Camera.main.ScreenToWorldPoint(prev_pos_clip);
        Vector3 world_current = Camera.main.ScreenToWorldPoint(current_pos_clip);

        float delta = Vector3.Dot(world_current - world_prev, Single_Axis_Control.transform.up);
        Vector3 pos = Single_Axis_Control.transform.position;

        pos += Single_Axis_Control.transform.up * (delta * dragSpeed);
        Single_Axis_Control.transform.position = pos;
        prev_pos = Input.mousePosition;

        OnDragged(this, Single_Axis_Control.transform.position);
    }

    void OnMouseDrag()
    {
        dragging();
    }
}
