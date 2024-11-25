using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Fram.Inventory
{
    public class ItemBounce : MonoBehaviour
    {
        private Transform spriteTrans;
        public BoxCollider2D coll;

        private float gravity = -3.5f;
        private bool isGround;

        private float distance;
        private Vector2 direction;
        private Vector3 targetPos;

        private void Awake()
        {
            coll = GetComponent<BoxCollider2D>();
            spriteTrans = transform.GetChild(0);
            coll.enabled = false;
        }

        private void Update()
        {
            Bounce();
        }

        public void InitBounceItem(Vector3 target,Vector2 dir)
        {
            coll.enabled = false;
            targetPos = target;
            direction = dir;
            distance = Vector3.Distance(target, transform.position);

            spriteTrans.position += Vector3.up * 1.45f;
        }

        private void Bounce()
        {
            isGround = spriteTrans.position.y <= transform.position.y;

            if (Vector3.Distance(transform.position, targetPos) > 0.1f)
            {
                // 这里乘以distance主要是可以达到一个线性的效果，线性地到达那个目标点
                transform.position += (Vector3)direction * distance * -gravity * Time.deltaTime;
            }

            if (!isGround)
            {
                // 如果未在地上的话，因为此时spriteTrans的位置与transform的的位置竖直上有差别，所以要改变竖直方向上的高度
                spriteTrans.position += Vector3.up * gravity * Time.deltaTime;
            }
            else
            {
                spriteTrans.position = transform.position;
                coll.enabled = true;
            }
        }
    }
}

