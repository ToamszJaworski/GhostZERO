using Mirror;
using System;
using UnityEngine;

public class CameraController : NetworkBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private float sensivity;

    private float _cameraRotation;

    private Transform _transform;

    [SyncVar] private float _yRot;
    [SyncVar] private float _xRot;

    private void Start()
    {
        _transform = transform;

        Cursor.lockState = CursorLockMode.Locked;

        if (!hasAuthority)
        {
            _camera.enabled = false;
            _camera.GetComponent<AudioListener>().enabled = false;
        }
    }

    private void Update()
    {
        if(hasAuthority)
        {
            float _rotX = Input.GetAxis("Mouse X");
            float _rotY = -Input.GetAxis("Mouse Y");

            _transform.Rotate(0, _rotX * sensivity * Time.deltaTime, 0);

            _cameraRotation += _rotY * sensivity * Time.deltaTime;

            _cameraRotation = Mathf.Clamp(_cameraRotation, -90f, 90f);

            _camera.transform.localRotation = Quaternion.Euler(_cameraRotation, 0, 0);

            if (_cameraRotation != _yRot || _transform.rotation.y != _xRot)
                CmdRoationSync(_cameraRotation, _transform.eulerAngles.y);
        }
        else
        {
            if(_transform.rotation.y != _xRot)
                _transform.rotation = Quaternion.Euler(0, _xRot, 0);

            if(_camera.transform.localRotation.x != _yRot)
                _camera.transform.localRotation = Quaternion.Euler(_yRot, 0, 0);
        }
    }

    [Command]
    private void CmdRoationSync(float _yRot, float _xRot)
    {
        this._yRot = _yRot;
        this._xRot = _xRot;
    }
}
