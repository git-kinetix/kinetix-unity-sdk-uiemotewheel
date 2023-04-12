using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kinetix.UI.EmoteWheel
{
    public class AutoAlignment : MonoBehaviour
    {
        private void Awake()
        {
            transform.up = Vector3.up;
        }
    }
}
