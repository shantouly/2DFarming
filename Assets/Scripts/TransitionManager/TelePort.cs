using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fram.Transition
{
    public class TelePort : MonoBehaviour
    {
        [SceneName]
        public string sceneToGo;

        public Vector3 posToGo;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                EventHandler.CallTransitionEvent(sceneToGo, posToGo);
            }
        }
    }
}

