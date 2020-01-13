using UnityEngine;

namespace RubikCube
{
    public class CameraMover : MonoBehaviour
    {
        [SerializeField] private float angularSpeed = 1f;

        [SerializeField] private float scrollSpeed = 20f;

        [SerializeField] private int maxElevation;

        [SerializeField] private int miniElevation;

        [SerializeField] private Transform center;

        [SerializeField] private Vector3 initialPosition;

        private PolarCoordinate polarCoordinate;

        public bool Controllable { get; set; }

        private Transform _transform;

        void Start()
        {
            _transform = transform;
            ToInitialPosition();
        }

        void Update()
        {
            if (!Controllable) return;

            var inputH = InputUtil.GetInputH();
            var inputV = InputUtil.GetInputV();
            var inputPinch = -InputUtil.GetPinch();

            // horizontal input for azimuth
            var dA = inputH * angularSpeed;
            // vertical input for elevation
            var dE = inputV * angularSpeed;
            // change radius. scroll up to go near
            var dR = inputPinch * scrollSpeed;

            Move(dA, dE, dR);
        }

        /// <summary>
        /// Move camera
        /// </summary>
        /// <param name="deltaAzimuth"></param>
        /// <param name="deltaElevation"></param>
        /// <param name="deltaRadius"></param>
        private void Move(float deltaAzimuth, float deltaElevation, float deltaRadius)
        {
            polarCoordinate.Azimuth += Mathf.Deg2Rad * deltaAzimuth;
            polarCoordinate.Elevation += Mathf.Deg2Rad * deltaElevation;
            polarCoordinate.Elevation =
                Mathf.Clamp(polarCoordinate.Elevation, Mathf.Deg2Rad * miniElevation, Mathf.Deg2Rad * maxElevation);
            polarCoordinate.Radius += deltaRadius;

            var updatedPos = polarCoordinate.PolarToCartesian();
            transform.position = updatedPos;

            transform.LookAt(center);
        }

        public void ToInitialPosition()
        {
            polarCoordinate = new PolarCoordinate(initialPosition);
            transform.position = polarCoordinate.PolarToCartesian();
            transform.LookAt(center);
        }

        // /// <summary>
        // /// todo change animation using polar coordinates
        // /// </summary>
        // /// <returns></returns>
        // public async UniTask<Unit> SolvedAnimation()
        // {
        //     // move to initialPosition. but y is zero.
        //     var targetPos = new Vector3(initialPosition.x, 0f, initialPosition.z);
        //
        //     await transform.DOMove(targetPos, 0.5f)
        //         .SetEase(Ease.Linear)
        //         .OnUpdate(() => transform.LookAt(center))
        //         .OnCompleteAsObservable().ToUniTask();
        //
        //     await UniTask.Delay(TimeSpan.FromSeconds(0.3f));
        //
        //     var deg = 0f;
        //     var temp = 0f;
        //     await DOTween.To(() => deg, x => deg = x, 720f, 2f)
        //         .OnUpdate(() => {
        //             var delta = deg - temp;
        //             RotateAroundAxis(new Vector3(0f, delta, 0f));
        //             temp = deg;
        //         })
        //         .OnCompleteAsObservable().ToUniTask();
        //
        //     return Unit.Default;
        // }

        private void RotateAroundAxis(Vector3 rotate)
        {
            // create a rotation Matrix
            var quaternion = Quaternion.Euler(rotate);
            var m = Matrix4x4.identity;
            m.SetTRS(Vector3.zero, quaternion, Vector3.one);

            var newM = m * _transform.localToWorldMatrix;
            // position
            _transform.position = new Vector3(newM.m03, newM.m13, newM.m23);
            // rotation
            Quaternion q = newM.rotation;
            _transform.rotation = q;
            // scale does not change
            _transform.localScale = newM.lossyScale;
        }

        internal class PolarCoordinate
        {
            float _radius;
            internal float Radius {
                get => _radius;
                set { _radius = Mathf.Clamp(value, 6, 15); }
            }

            internal float Azimuth { get; set; }

            internal float Elevation { get; set; }

            internal PolarCoordinate(Vector3 cartesian)
            {
                Radius = cartesian.magnitude;
                CartesianToPolar(cartesian);
            }

            /// <summary>
            /// cartesian coordinates to polar
            /// </summary>
            /// <param name="cartesianPos"></param>
            internal void CartesianToPolar(Vector3 cartesianPos)
            {
                // 方位角
                Azimuth = Mathf.Atan2(cartesianPos.z, cartesianPos.x);
                // 仰角
                Elevation = Mathf.Asin(cartesianPos.y / Radius);
            }

            /// <summary>
            /// polar to cartesian 
            /// </summary>
            /// <returns></returns>
            internal Vector3 PolarToCartesian()
            {
                var t = Radius * Mathf.Cos(Elevation);
                return new Vector3(t * Mathf.Cos(Azimuth), Radius * Mathf.Sin(Elevation), t * Mathf.Sin(Azimuth));
            }
        }
    }
}