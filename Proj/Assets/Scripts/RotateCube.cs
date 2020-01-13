using UnityEngine;

namespace RubikCube
{
    public class RotateCube : MonoBehaviour
    {
        public int Id { get; set; }

        public Vector3Int PositionId { get; set; }

        /// <summary>
        /// 回転完了時のposition
        /// </summary>
        private Vector3 _targetPosition;

        /// <summary>
        /// 回転完了時のrotation
        /// </summary>
        private Quaternion _targetRotation;

        private Transform _transform;

        private void Start()
        {
            _transform = transform;
        }

        /// <summary>
        /// 
        /// </summary>
        public Vector3Int RotatePositionId(Vector3Int angle, float transferValueToOrigin)
        {
            // not change
            if (angle == Vector3Int.zero)
            {
                //                Debug.Log("nothing to change");
                return PositionId;
            }

            var newPositionId = Vector3Int.zero;

            // around x axis. y,z will be changed
            if (angle.x != 0)
            {
                var cos = Mathf.RoundToInt(Mathf.Cos(angle.x * Mathf.Deg2Rad));
                var sin = Mathf.RoundToInt(Mathf.Sin(angle.x * Mathf.Deg2Rad));
                var y1 = PositionId.y - transferValueToOrigin;
                var z1 = PositionId.z - transferValueToOrigin;
                var y2 = cos * y1 - sin * z1;
                var z2 = sin * y1 + cos * z1;
                y2 += transferValueToOrigin;
                z2 += transferValueToOrigin;

                newPositionId = new Vector3Int(PositionId.x, Mathf.RoundToInt(y2), Mathf.RoundToInt(z2));
            }
            // around y axis. z,x will be changed
            else if (angle.y != 0)
            {
                var cos = Mathf.RoundToInt(Mathf.Cos(angle.y * Mathf.Deg2Rad));
                var sin = Mathf.RoundToInt(Mathf.Sin(angle.y * Mathf.Deg2Rad));
                var x1 = PositionId.x - transferValueToOrigin;
                var z1 = PositionId.z - transferValueToOrigin;
                var x2 = cos * x1 + sin * z1;
                var z2 = -sin * x1 + cos * z1;
                x2 += transferValueToOrigin;
                z2 += transferValueToOrigin;

                newPositionId = new Vector3Int(Mathf.RoundToInt(x2), PositionId.y, Mathf.RoundToInt(z2));
            }
            // around z axis. x,y will be changed
            else if (angle.z != 0)
            {
                var cos = Mathf.RoundToInt(Mathf.Cos(angle.z * Mathf.Deg2Rad));
                var sin = Mathf.RoundToInt(Mathf.Sin(angle.z * Mathf.Deg2Rad));
                var x1 = PositionId.x - transferValueToOrigin;
                var y1 = PositionId.y - transferValueToOrigin;
                var x2 = cos * x1 - sin * y1;
                var y2 = sin * x1 + cos * y1;
                x2 += transferValueToOrigin;
                y2 += transferValueToOrigin;

                newPositionId = new Vector3Int(Mathf.RoundToInt(x2), Mathf.RoundToInt(y2), PositionId.z);
            }

            return newPositionId;
        }

        /// <summary>
        /// rotation using matrix.
        /// </summary>
        /// <param name="m"></param>
        public void RotateAroundAxis(Matrix4x4 m)
        {
            var newM = m * _transform.localToWorldMatrix;
            // position
            _transform.position = new Vector3(newM.m03, newM.m13, newM.m23);
            // rotation
            Quaternion q = newM.rotation;
            var angle = q.eulerAngles;
            var x = float.Parse(angle.x.ToString("f3"));
            var y = float.Parse(angle.y.ToString("f3"));
            var z = float.Parse(angle.z.ToString("f3"));
            angle = new Vector3(x, y, z);
            _transform.rotation = Quaternion.Euler(angle);
            // scale does not change
            _transform.localScale = Vector3.one; //newM.lossyScale;
        }

        public CubeInfo Info()
        {
            var info = new CubeInfo() {
                id = Id,
                positionId = PositionId,
                transform = transform
            };

            return info;
        }

        public struct CubeInfo
        {
            public int id;
            public Vector3 positionId;
            public Transform transform;
        }
    }
}