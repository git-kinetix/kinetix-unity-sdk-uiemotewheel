using UnityEngine;
using UnityEngine.InputSystem;

namespace Kinetix.UI.EmoteWheel
{
    public class AutoRotationInput : MonoBehaviour
    {
        // CACHE
        private Vector3 dir;
        private Camera  cam;

        private void Awake()
        {
            cam = Camera.main;
        }
        
        private void Update()
        {
            Vector3 tempMouse = new Vector3(Mouse.current.position.x.ReadValue(), Mouse.current.position.y.ReadValue() );
            dir = tempMouse - cam.WorldToScreenPoint(transform.position);
            float angle        = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90.0f;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
}
