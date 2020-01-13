using UnityEngine;

namespace RubikCube
{
    public static class InputUtil
    {
        public static Vector3 GetDragAmount()
        {
            return CommonTouch.Get1stDeltaPosition();
        }

        public static float GetInputH()
        {
            if (AppManager.IsEditor && !AppManager.IsUnityRemote)
            {
                return Input.GetAxis("Horizontal");
            }

            if (CommonTouch.GetPhase() != CommonTouch.TouchPhase.SingleMoved)
            {
                return 0f;
            }

            var deltaPos = CommonTouch.Get1stDeltaPosition();
            var value = deltaPos.x * AppManager.ScreenDragCoefficient;
            value = Mathf.Clamp(value, -5f, 5f);

            return value;
        }

        public static float GetInputV()
        {
            if (AppManager.IsEditor && !AppManager.IsUnityRemote)
            {
                return Input.GetAxis("Vertical");
            }

            if (CommonTouch.GetPhase() != CommonTouch.TouchPhase.SingleMoved)
            {
                return 0f;
            }

            var deltaPos = CommonTouch.Get1stDeltaPosition();
            var value = deltaPos.y * AppManager.ScreenDragCoefficient;
            value = Mathf.Clamp(value, -5f, 5f);

            return value;
        }

        public static float GetPinch()
        {
            if (AppManager.IsEditor && !AppManager.IsUnityRemote)
            {
                return -Input.GetAxis("Mouse ScrollWheel");
            }

            var deltaDistance = CommonTouch.GetDeltaTouchesDistance();
            var value = deltaDistance / 1000f;
            value = Mathf.Clamp(value, -1f, 1f);

            return value;
        }
    }
}