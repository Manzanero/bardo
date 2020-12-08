using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class Draggable : MonoBehaviour
{
    public bool Dragged { get; private set; }

    private bool _mouseOver;
    public bool MouseOver => !GameMaster.instance.mouseOverUi && _mouseOver;

    private float _timeHoldingMouse;
    
    private void OnMouseDrag()
    {
        if (!MouseOver && _timeHoldingMouse == 0)
            return;
        
        _timeHoldingMouse += Time.deltaTime;
        if (_timeHoldingMouse < 0.25f) 
            return;
        
        GetComponent<Collider>().enabled = false;
        Dragged = true;
    }
    
    private void OnMouseEnter()
    {
        _mouseOver = true;
    }
    
    private void OnMouseExit()
    {
        _mouseOver = false;
    }
    
    private void OnMouseUp()
    {
        GetComponent<Collider>().enabled = true;
        Dragged = false;
        _timeHoldingMouse = 0;
    }
}