using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class Draggable : MonoBehaviour
{
    public bool Dragged { get; private set; }
    public bool MouseOver { get; private set; }

    private float _timeHoldingMouse;
    
    private void OnMouseDrag()
    {
        _timeHoldingMouse += Time.deltaTime;
        if (!(_timeHoldingMouse > 0.25f)) 
            return;
        GetComponent<Collider>().enabled = false;
        Dragged = true;
    }
    
    private void OnMouseEnter()
    {
        MouseOver = true;
    }
    
    private void OnMouseExit()
    {
        MouseOver = false;
    }
    
    private void OnMouseUp()
    {
        GetComponent<Collider>().enabled = true;
        Dragged = false;
        _timeHoldingMouse = 0;
    }
}