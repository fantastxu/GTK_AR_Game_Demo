using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zhengsege.matrix
{
    public class Rotate_self : MonoBehaviour
    {
        public float speed;
        public float x;
        public float y;
        public float z;

        float num = 0;
        // Update is called once per frame
        void Update()
        {
            num += Time.deltaTime * this.speed;
            if (num > 360)
                num = 0;

            if (x == -1)
                this.transform.rotation = Quaternion.Euler(num, this.y, this.z);
            else if (y == -1)
                this.transform.rotation = Quaternion.Euler(this.x, num, this.z);
            else if (z == -1)
                this.transform.rotation = Quaternion.Euler(this.x, this.y, num);
        }
    }
}
