using UnityEngine;

namespace RubikCube
{
    public static class CommonTouch
    {
        
        public enum TouchPhase
        {
            None,
            SingleBegan,
            SingleMoved,
            SingleStationary,
            SingleEnded,
            DoubleBegan,
            DoubleMoved,
            DoubleStationary,
            DoubleEnded,
        }

        private static Vector3 _prevPosition;

        private static float _prevDistance;

        private static int _prevTouchCount;
        
        public static TouchPhase GetPhase()
        {
            if (AppManager.IsEditor && !AppManager.IsUnityRemote) {
                
                if (Input.GetMouseButtonDown(0)) {
                    _prevPosition = Input.mousePosition;
                    return TouchPhase.SingleBegan;
                }
                if (Input.GetMouseButton(0)) {
                    return TouchPhase.SingleMoved;
                }
                if (Input.GetMouseButtonUp(0)) {
                    return TouchPhase.SingleEnded;
                }
            }
            
            var touchCount = Input.touchCount;

            if (touchCount == 0) {
                _prevTouchCount = 0;
                return TouchPhase.None;
            }

            if (touchCount == 1) {
                _prevTouchCount = 1;
                var phase = Input.GetTouch(0).phase;
                if (phase == UnityEngine.TouchPhase.Began) {
                    return TouchPhase.SingleBegan;
                }

                if (phase == UnityEngine.TouchPhase.Moved) {
                    return TouchPhase.SingleMoved;
                }

                if (phase == UnityEngine.TouchPhase.Stationary) {
                    return TouchPhase.SingleStationary;
                }

                if (phase == UnityEngine.TouchPhase.Ended) {
                    return TouchPhase.SingleEnded;
                }
            }

            if (touchCount > 1) {
                var phase1st = Input.GetTouch(0).phase;
                var phase2nd = Input.GetTouch(1).phase;

                if (_prevTouchCount < 2) {
                    _prevTouchCount = 2;
                    _prevDistance = GetTouchesDistance();
                    return TouchPhase.DoubleBegan;
                }

                if (phase1st == UnityEngine.TouchPhase.Stationary && phase2nd == UnityEngine.TouchPhase.Stationary) {
                    return TouchPhase.DoubleStationary;
                }

                if ((phase1st == UnityEngine.TouchPhase.Moved && phase2nd == UnityEngine.TouchPhase.Moved)
                    || (phase1st == UnityEngine.TouchPhase.Moved && phase2nd == UnityEngine.TouchPhase.Stationary)
                    || (phase1st == UnityEngine.TouchPhase.Stationary && phase2nd == UnityEngine.TouchPhase.Moved)) {
                    
                    return TouchPhase.DoubleMoved;
                }
            }
            
            return TouchPhase.None;
        }
        
        public static Vector3 Get1stPosition()
        {
            if (AppManager.IsEditor && !AppManager.IsUnityRemote) {
                return Input.mousePosition;
            }
            
            return Input.GetTouch(0).position;
        }

        public static Vector3 Get2ndPosition()
        {
            if (AppManager.IsEditor && !AppManager.IsUnityRemote) {
                return Vector3.zero;
            }
            
            if (Input.touchCount < 2) {
                return Vector3.zero;
            }
            return Input.GetTouch(1).position;
        }

        public static Vector3 Get1stDeltaPosition()
        { 
            var phase = GetPhase();

            if (AppManager.IsEditor && !AppManager.IsUnityRemote) {
                if (phase == TouchPhase.SingleBegan) {
                    return Vector3.zero;
                }
                
                if (phase == TouchPhase.SingleMoved
                    || phase == TouchPhase.SingleStationary
                    || phase == TouchPhase.SingleEnded) {
                    
                    var now   = Input.mousePosition;
                    var delta = now - _prevPosition;
                    _prevPosition = now;
                    return delta;
                }
            }

            if (Input.touchCount > 0) {
                return Input.GetTouch(0).deltaPosition;
            }
            
            return Vector3.zero;
        }

        public static Vector3 Get2ndDeltaPosition()
        {
            if (Input.touchCount > 1) {
                return Input.GetTouch(1).deltaPosition;
            }
            
            return Vector3.zero;
        }

        /// <summary>
        /// Distance between two touches. if touch counts are less than two, return 0
        /// </summary>
        /// <returns></returns>
        public static float GetTouchesDistance()
        {
            if (Input.touchCount < 2) return 0;

            var pos1 = Input.GetTouch(0).position;
            var pos2 = Input.GetTouch(1).position;
            var distance = Vector3.Distance(pos1, pos2);
            
            return distance;
        }
        
        
        /// <summary>
        /// Distance between two touches. if touch counts are less than two nor touch phase is DoubleBegan, return 0
        /// </summary>
        /// <returns></returns>
        public static float GetDeltaTouchesDistance()
        {
            if (Input.touchCount < 2) return 0;
            var phase = GetPhase();
            if (phase == TouchPhase.DoubleBegan) {
                return 0;
            }
            
            var now = GetTouchesDistance();
            var delta = now - _prevDistance;
            _prevDistance = now;

            return delta;
        }
        
    }
}