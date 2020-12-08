using System;
using UnityEngine;

public class CameraOperate : MonoBehaviour
{
    public float minY = 10f;
    public float maxY = 80f;
    
    [Tooltip("Mouse wheel rolling control lens please enter, the speed of the back")]
    [Range(0.1f, 2f)] 
    public float scrollSpeed = 1f;
    [Tooltip("Right mouse button control lens X axis rotation speed")]
    [Range(0.1f, 2f)] 
    public float rotateXSpeed = 1f;
    [Tooltip("Right mouse button control lens Y axis rotation speed")]
    [Range(0.1f, 2f)] 
    public float rotateYSpeed = 1f;
    [Tooltip("Mouse wheel press, lens translation speed")]
    // [Range(0.1f, 2f)] 
    // public float moveSpeed = 1f;
    private float _moveSpeed = 1f;
    public bool operate = true;
    
    private bool _isRotate;
    private bool _isMove;
    private Transform _mTransform;
    private Vector3 _traStart;
    private Vector3 _mouseStart;
    private bool _isDown;

    
    private void Start()
    {
        _mTransform = transform;
    }

    private void Update()
    {
        var position = _mTransform.position;
        var height = position.y;
        _moveSpeed = 0.2f + height * 0.01f;
        
        if (!operate) 
            return;
        if (_isRotate && Input.GetMouseButtonUp(2))
            _isRotate = false;
        if (_isMove && Input.GetMouseButtonUp(1))
            _isMove = false;
        
        if (!Input.GetMouseButton(2))
            _isRotate = false;
        if (!Input.GetMouseButton(1))
            _isMove = false;
        if (_isRotate && _isMove)
            return;
        if (Input.mousePosition.y >= Screen.height + 1 || 
            Input.mousePosition.y <= - 1 || 
            Input.mousePosition.x >= Screen.width + 1 || 
            Input.mousePosition.x <= - 1)
            return;

        if (_isRotate && !_isMove) 
        {
            var offset = Input.mousePosition - _mouseStart;
            
            // whether the lens is facing down
            if (_isDown)
            {   var rot = _traStart + new Vector3(offset.y * 0.3f * rotateYSpeed, -offset.x * 0.3f * rotateXSpeed, 0);
                rot.x = Mathf.Clamp(rot.x, 0f, 90f);
                _mTransform.rotation = Quaternion.Euler(rot);
            }
            else
            {
                var rotNotDown = _traStart + new Vector3(-offset.y * 0.3f * rotateYSpeed, offset.x * 0.3f * rotateXSpeed, 0);
                rotNotDown.x = Mathf.Clamp(rotNotDown.x, 0f, 90f); 
                _mTransform.rotation = Quaternion.Euler(rotNotDown);
            }
        }

        else if (Input.GetMouseButtonDown(2) && !_isMove)
        {
            _isRotate = true;
            _mouseStart = Input.mousePosition;
            _traStart = _mTransform.rotation.eulerAngles;
            _isDown = _mTransform.up.y < -0.0001f;
        }

        if (_isMove && !_isRotate)
        {
            var offset = Input.mousePosition - _mouseStart;
            var sceneRotationY = transform.rotation.eulerAngles.y;
            var sceneForward = Quaternion.Euler(0, sceneRotationY, 0) * Vector3.forward;
            var sceneRight = Quaternion.Euler(0, sceneRotationY, 0) * Vector3.right;
            _mTransform.position = _traStart + _moveSpeed * -offset.y * 0.1f * sceneForward +
                                               _moveSpeed * -offset.x * 0.1f * sceneRight;
        }

        else if (Input.GetMouseButtonDown(1) && !_isRotate)
        {
            _isMove = true;
            _mouseStart = Input.mousePosition;
            _traStart = _mTransform.position;
        }
        
        // scroll
        var scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Math.Abs(scroll) < 0.0001f)
            return;
        
        position += scrollSpeed * scroll * 1000f * Time.deltaTime * Vector3.down;
        position.y = Mathf.Clamp(position.y, minY, maxY);
        _mTransform.position = position;
    }
}