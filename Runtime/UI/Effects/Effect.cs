using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kinetix.UI.EmoteWheel
{
    public abstract class Effect : MonoBehaviour
    {
        public abstract void Play();
        public abstract void Stop();
    }
}

