using UnityEngine;

namespace Kinetix.UI.EmoteWheel
{
    public class AutoRotation : MonoBehaviour
    {
        [SerializeField] private float speed = 1.0f;
        
        private void Update()
        {
            transform.Rotate(transform.forward, speed * Time.deltaTime);
        }
    }
}

