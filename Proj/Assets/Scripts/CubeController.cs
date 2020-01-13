using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using UniRx;
using UniRx.Async;

namespace RubikCube
{
    [RequireComponent(typeof(CubeCreator), typeof(CubesUtil))]
    public class CubeController : MonoBehaviour
    {
        public enum Message
        {
            None,
            CatchCube,
            ReleaseCube,
            Solve,
        }

        private enum State
        {
            None,
            TapCube,
            RotateTargets,
            Rotate,
            RotateRelease,
            Transfer,
            ContinueRotate,
            Solve,
        }

        public bool EnableRayCast { get; set; }

        [SerializeField] private Camera mainCamera;

        [SerializeField] private CubesUtil cubesUtil;

        private Subject<Message> _messageSubject = new Subject<Message>();
        public IObservable<Message> MessageAsObservable => _messageSubject;

        private ReactiveProperty<State> stateAsObservable = new ReactiveProperty<State>(State.None);

        private CompositeDisposable stateDisposable = new CompositeDisposable();

        private Vector3 _totalDragAmount = Vector3.zero;

        private List<RotateCube> _rotateCubes = new List<RotateCube>();

        private RotateCube _clickedCube;

        /// rotate axis when dragging horizontal. this should contain only -1, 0, 1
        private Vector3Int _rotateAxisScreenHorizontal;

        /// rotate axis when dragging vertical. this should contain only -1, 0, 1
        private Vector3Int _rotateAxisScreenVertical;

        private float _drag2DegreeTotal;

        private List<UserManipulateHistory> _userManipulateHistories;

        /// <summary>
        /// all cubes on game
        /// </summary>
        private List<RotateCube> _cubes;
        public List<RotateCube> Cubes => _cubes;

        private void Start()
        {
            Debug.Log("!!! CubeController Start !!!");

            _userManipulateHistories = new List<UserManipulateHistory>();

            stateAsObservable
                .Subscribe(state => {
                    stateDisposable.Clear();

                    switch (state)
                    {
                        case State.TapCube:
                            OnTapCube();
                            break;

                        case State.RotateTargets:
                            OnRotateTargets();
                            break;

                        case State.Rotate:
                            OnRotate();
                            break;

                        case State.RotateRelease:

                            break;

                        case State.Transfer:
                            OnTransfer();
                            break;

                        case State.ContinueRotate:
                            OnContinueRotate();
                            break;
                        case State.Solve:
                            OnSolve();
                            break;
                    }
                })
                .AddTo(gameObject);
        }

        private void OnDestroy()
        {
            Debug.Log("!!! OnDestroy CubeController !!!");
            stateDisposable.Dispose();
        }

        /// <summary>
        /// all rotate cubes
        /// </summary>
        /// <param name="cubes"></param>
        public void SetCubes(List<RotateCube> cubes)
        {
            _cubes = cubes;
        }

        /// <summary>
        /// destroy cubes
        /// </summary>
        public void DestroyCubes()
        {
            foreach (var cube in _cubes)
            {
                Destroy(cube.gameObject);
            }

            _cubes.Clear();
        }

        /// <summary>
        /// clear all user manipulation histories.
        /// </summary>
        public void ClearUserManipulateHistories()
        {
            _userManipulateHistories.Clear();
        }

        /// <summary>
        /// scramble cubes
        /// </summary>
        /// <returns></returns>
        public async UniTask<Unit> Scramble(int scrambleCount, bool isRetry)
        {
            return await cubesUtil.ScrambleCubes(isRetry, _cubes, scrambleCount);
        }

        /// <summary>
        /// undo
        /// </summary>
        /// <returns></returns>
        public async UniTask<bool> Undo()
        {
            if (_userManipulateHistories.Count == 0)
            {
                Debug.Log("Nothing to undo!!!!!");
                return false;
            }

            var lastHistory = _userManipulateHistories.Last();
            var cubeId = lastHistory.CubeId;
            var clickedCube = _cubes.First(cube => cube.Id == cubeId);
            var degree = lastHistory.RotateDegree;

            var cubes = new List<RotateCube>();

            if (degree.x != 0)
            {
                cubes = _cubes.Where(cube => cube.PositionId.x == clickedCube.PositionId.x).ToList();
            }
            else if (degree.y != 0)
            {
                cubes = _cubes.Where(cube => cube.PositionId.y == clickedCube.PositionId.y).ToList();
            }
            else if (degree.z != 0)
            {
                cubes = _cubes.Where(cube => cube.PositionId.z == clickedCube.PositionId.z).ToList();
            }

            await cubesUtil.RotateCubesWithAnim(cubes, degree * -1);

            cubesUtil.UpdatePositionIds(cubes, degree * -1);

            _userManipulateHistories.RemoveAt(_userManipulateHistories.Count - 1);

            return true;
        }

        /// <summary>
        /// user can manipulate
        /// </summary>
        public void Manipulate()
        {
            stateAsObservable.Value = State.TapCube;
        }

        /// <summary>
        /// tap cube 
        /// </summary>
        private void OnTapCube()
        {
            Observable.EveryUpdate()
                .Where(_ => EnableRayCast)
                .Where(_ => Input.GetMouseButtonDown(0))
                .Subscribe(_ => {
                    var distance = 100;
                    var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                    var hit = new RaycastHit();
                    Debug.DrawRay(ray.origin, ray.direction * distance, Color.red, 2f, false);

                    if (Physics.Raycast(ray, out hit, distance))
                    {
                        _clickedCube = hit.collider.gameObject.GetComponent<RotateCube>();

                        if (_clickedCube != null)
                        {
                            InputUtil.GetDragAmount();
                            _totalDragAmount = Vector3.zero;
                            stateAsObservable.Value = State.RotateTargets;
                            _messageSubject.OnNext(Message.CatchCube);
                        }
                        else
                        {
                            Debug.Log("could not catch cube");
                        }
                    }
                })
                .AddTo(stateDisposable);
        }

        /// <summary>
        /// axis
        /// 回転の軸と方向を決定
        /// </summary>
        private void OnRotateTargets()
        {
            Observable.EveryUpdate()
                .Subscribe(_ => {
                    var dragAmount = InputUtil.GetDragAmount();
                    _totalDragAmount.x += dragAmount.x;
                    _totalDragAmount.y += dragAmount.y;

                    var threshold = 50f;

                    if (Mathf.Abs(_totalDragAmount.x) > threshold && Mathf.Abs(_totalDragAmount.x) > Mathf.Abs(_totalDragAmount.y))
                    {
                        var cameraUp = mainCamera.transform.up;
                        Debug.Log(cameraUp.ToString());

                        if (Mathf.Abs(cameraUp.x) > Mathf.Abs(cameraUp.y) && Mathf.Abs(cameraUp.x) > Mathf.Abs(cameraUp.z))
                        {
                            _rotateAxisScreenHorizontal = cameraUp.x > 0 ? new Vector3Int(1, 0, 0) : new Vector3Int(-1, 0, 0);
                            var posIdX = _clickedCube.PositionId.x;
                            _rotateCubes = _cubes.Where(cube => cube.PositionId.x == posIdX).ToList();
                        }
                        else if (Mathf.Abs(cameraUp.y) > Mathf.Abs(cameraUp.x) && Mathf.Abs(cameraUp.y) > Mathf.Abs(cameraUp.z))
                        {
                            _rotateAxisScreenHorizontal = cameraUp.y > 0 ? new Vector3Int(0, 1, 0) : new Vector3Int(0, -1, 0);
                            var posIdY = _clickedCube.PositionId.y;
                            _rotateCubes = _cubes.Where(cube => cube.PositionId.y == posIdY).ToList();
                        }
                        else
                        {
                            _rotateAxisScreenHorizontal = cameraUp.z > 0 ? new Vector3Int(0, 0, 1) : new Vector3Int(0, 0, -1);
                            var posIdZ = _clickedCube.PositionId.z;
                            _rotateCubes = _cubes.Where(cube => cube.PositionId.z == posIdZ).ToList();
                        }

                        stateAsObservable.Value = State.Rotate;
                    }
                    else if (Mathf.Abs(_totalDragAmount.y) > threshold && Mathf.Abs(_totalDragAmount.y) > Mathf.Abs(_totalDragAmount.x))
                    {
                        var cameraRight = mainCamera.transform.right;

                        if (Mathf.Abs(cameraRight.x) > Mathf.Abs(cameraRight.y) && Mathf.Abs(cameraRight.x) > Mathf.Abs(cameraRight.z))
                        {
                            _rotateAxisScreenVertical = cameraRight.x > 0 ? new Vector3Int(1, 0, 0) : new Vector3Int(-1, 0, 0);
                            var posIdX = _clickedCube.PositionId.x;
                            _rotateCubes = _cubes.Where(cube => cube.PositionId.x == posIdX).ToList();
                        }
                        else if (Mathf.Abs(cameraRight.y) > Mathf.Abs(cameraRight.x) && Mathf.Abs(cameraRight.y) > Mathf.Abs(cameraRight.z))
                        {
                            _rotateAxisScreenVertical = cameraRight.y > 0 ? new Vector3Int(0, 1, 0) : new Vector3Int(0, -1, 0);
                            var posIdY = _clickedCube.PositionId.y;
                            _rotateCubes = _cubes.Where(cube => cube.PositionId.y == posIdY).ToList();
                        }
                        else
                        {
                            _rotateAxisScreenVertical = cameraRight.z > 0 ? new Vector3Int(0, 0, 1) : new Vector3Int(0, 0, -1);
                            var posIdZ = _clickedCube.PositionId.z;
                            _rotateCubes = _cubes.Where(cube => cube.PositionId.z == posIdZ).ToList();
                        }

                        stateAsObservable.Value = State.Rotate;
                    }
                })
                .AddTo(stateDisposable);
        }

        /// <summary>
        /// dragging cube
        /// </summary>
        private void OnRotate()
        {
            _drag2DegreeTotal = 0f;

            Observable.EveryUpdate()
                .Where(_ => Input.GetMouseButton(0))
                .Subscribe(_ => {
                    var isHorizontal = false;
                    var drag2Degree = 0f;
                    var dragAmount = InputUtil.GetDragAmount();

                    if (_rotateAxisScreenHorizontal.sqrMagnitude != 0)
                    {
                        drag2Degree = -dragAmount.x * 0.3f;
                        isHorizontal = true;
                    }
                    else
                    {
                        drag2Degree = dragAmount.y * 0.3f;
                    }

                    drag2Degree = Mathf.RoundToInt(drag2Degree);

                    // create rotation matrix
                    var sign = 0;
                    var rotateAngle = Vector3.zero;

                    if (_rotateAxisScreenHorizontal.x != 0 || _rotateAxisScreenVertical.x != 0)
                    {
                        sign = isHorizontal ? _rotateAxisScreenHorizontal.x : _rotateAxisScreenVertical.x;
                        rotateAngle = new Vector3(drag2Degree * sign, 0f, 0f);
                    }
                    else if (_rotateAxisScreenHorizontal.y != 0 || _rotateAxisScreenVertical.y != 0)
                    {
                        sign = isHorizontal ? _rotateAxisScreenHorizontal.y : _rotateAxisScreenVertical.y;
                        rotateAngle = new Vector3(0f, drag2Degree * sign, 0f);
                    }
                    else if (_rotateAxisScreenHorizontal.z != 0 || _rotateAxisScreenVertical.z != 0)
                    {
                        sign = isHorizontal ? _rotateAxisScreenHorizontal.z : _rotateAxisScreenVertical.z;
                        rotateAngle = new Vector3(0f, 0f, drag2Degree * sign);
                    }

                    rotateAngle = new Vector3(Mathf.RoundToInt(rotateAngle.x), Mathf.RoundToInt(rotateAngle.y), Mathf.RoundToInt(rotateAngle.z));
                    var matrix = cubesUtil.RotateMatrix(rotateAngle);

                    foreach (var cube in _rotateCubes)
                    {
                        cube.RotateAroundAxis(matrix);
                    }

                    _drag2DegreeTotal += drag2Degree * sign;
                    _drag2DegreeTotal %= 360f;
                })
                .AddTo(stateDisposable);

            Observable.EveryUpdate()
                .Where(_ => Input.GetMouseButtonUp(0))
                .Subscribe(_ => {
                    stateAsObservable.Value = State.Transfer;
                    _messageSubject.OnNext(Message.ReleaseCube);
                })
                .AddTo(stateDisposable);
        }

        /// <summary>
        /// released cube goes to suitable position.
        /// </summary>
        /// <returns></returns>
        private async UniTask<Unit> OnTransfer()
        {
            var degree = 0;

            if (-360f <= _drag2DegreeTotal && _drag2DegreeTotal <= -315f)
            {
                degree = -360;
            }
            else if (-315f <= _drag2DegreeTotal && _drag2DegreeTotal <= -225f)
            {
                degree = -270;
            }
            else if (-225f <= _drag2DegreeTotal && _drag2DegreeTotal <= -135f)
            {
                degree = -180;
            }
            else if (-135f <= _drag2DegreeTotal && _drag2DegreeTotal <= -45f)
            {
                degree = -90;
            }
            else if (-45f <= _drag2DegreeTotal && _drag2DegreeTotal <= 45f)
            {
                degree = 0;
            }
            else if (45f < _drag2DegreeTotal && _drag2DegreeTotal <= 135f)
            {
                degree = 90;
            }
            else if (135f <= _drag2DegreeTotal && _drag2DegreeTotal <= 225f)
            {
                degree = 180;
            }
            else if (225f < _drag2DegreeTotal && _drag2DegreeTotal <= 315f)
            {
                degree = 270;
            }
            else if (315f < _drag2DegreeTotal && _drag2DegreeTotal <= 360f)
            {
                degree = 360;
            }

            var positionIdDegree = Vector3Int.zero;
            var differenceDegree = Vector3.zero;

            if (_rotateAxisScreenHorizontal.x != 0 || _rotateAxisScreenVertical.x != 0)
            {
                positionIdDegree.x = degree;
                differenceDegree.x = degree - Mathf.RoundToInt(_drag2DegreeTotal);
            }
            else if (_rotateAxisScreenHorizontal.y != 0 || _rotateAxisScreenVertical.y != 0)
            {
                positionIdDegree.y = degree;
                differenceDegree.y = degree - Mathf.RoundToInt(_drag2DegreeTotal);
            }
            else if (_rotateAxisScreenHorizontal.z != 0 || _rotateAxisScreenVertical.z != 0)
            {
                positionIdDegree.z = degree;
                differenceDegree.z = degree - Mathf.RoundToInt(_drag2DegreeTotal);
            }

            await cubesUtil.RotateCubesWithAnim(_rotateCubes, differenceDegree);

            cubesUtil.UpdatePositionIds(_rotateCubes, positionIdDegree);

            // keep history to undo. 
            var id = _clickedCube.Id;

            if (positionIdDegree == Vector3Int.zero)
            {
                Debug.Log("no need to save");
            }
            else
            {
                var history = new UserManipulateHistory(positionIdDegree, id);
                _userManipulateHistories.Add(history);
            }

            // check solved
            var solved = CheckSolve();

            if (solved)
            {
                Debug.Log("Solveーーーーーーー！！！！！！");
                Debug.Log("Solveーーーーーーー！！！！！！");
                Debug.Log("Solveーーーーーーー！！！！！！");
                Debug.Log("Solveーーーーーーー！！！！！！");
                Debug.Log("Solveーーーーーーー！！！！！！");
                stateAsObservable.Value = State.Solve;
            }
            else
            {
                stateAsObservable.Value = State.ContinueRotate;
            }

            return Unit.Default;
        }

        private void OnContinueRotate()
        {
            _rotateAxisScreenHorizontal = Vector3Int.zero;
            _rotateAxisScreenVertical = Vector3Int.zero;
            _totalDragAmount = Vector3.zero;

            _rotateCubes.Clear();
            _clickedCube = null;

            stateAsObservable.Value = State.TapCube;
        }

        private void OnSolve()
        {
            _messageSubject.OnNext(Message.Solve);
        }

        /// <summary>
        /// check solved. 
        /// </summary>
        /// <returns></returns>
        private bool CheckSolve()
        {
            var prevAngle = Vector3.zero;

            for (int i = 0; i < _cubes.Count; i++)
            {
                if (i == 0)
                {
                    prevAngle = _cubes[i].transform.rotation.eulerAngles;
                    continue;
                }

                var angle = _cubes[i].transform.rotation.eulerAngles;

                if (prevAngle != angle)
                {
                    //                    Debug.Log("not solved");
                    return false;
                }

                prevAngle = angle;
            }

            return true;
        }

        /// <summary>
        /// user manipulate information. be used for undo.
        /// </summary>
        private class UserManipulateHistory
        {
            internal Vector3Int RotateDegree { get; }

            internal int CubeId { get; }

            internal UserManipulateHistory(Vector3Int rotateDegree, int cubeId)
            {
                RotateDegree = rotateDegree;
                CubeId = cubeId;
            }
        }
    }
}