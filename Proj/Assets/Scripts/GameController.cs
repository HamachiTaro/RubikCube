using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Async;

namespace RubikCube
{
    /// <summary>
    /// Bridge of UI and GameView
    /// </summary>
    public class GameController : MonoBehaviour
    {
        public static readonly int Dimensions = 3;
        
        public static readonly int ScrambleDegree = 90;
        
        
        private enum State
        {
            None,
            CreateCube,
            CreateSavedCube,
            Scrambled,
            UserManipulate,
            Solved,
            Retire,
            Retry,
        }

        // [SerializeField] private GameUI gameUI;

        [SerializeField] private CubeCreator cubeCreator;

        [SerializeField] private CubeController cubeController;
        //
        [SerializeField] private CameraMover cameraMover;

        private ReactiveProperty<State> _stateAsObservable;

        private CompositeDisposable _stateDisposable;

        private int _retryCount;

        private bool _isContinueGame;

        #region Unity Method

        void Start()
        {
            Debug.Log("!!! Start Game Controller !!!");

            _retryCount = 0;
            cameraMover.Controllable = false;

            _stateAsObservable = new ReactiveProperty<State>(State.None);
            _stateDisposable = new CompositeDisposable();

            _stateAsObservable
                .Subscribe(state => {
                    _stateDisposable.Clear();

                    switch (state)
                    {
                        case State.CreateCube:
                            OnCreateCubes();
                            break;

                        case State.CreateSavedCube:
                            // OnCreateSavedCube();
                            break;

                        case State.Scrambled:
                            OnScrambled();
                            break;

                        case State.UserManipulate:
                            OnManipulate();
                            break;

                        case State.Solved:
                            // OnSolved();
                            break;

                        case State.Retry:
                            // OnRetry();
                            break;
                    }
                })
                .AddTo(gameObject);

            _stateAsObservable.Value = State.CreateCube;
        }

        private void OnDestroy()
        {
            Debug.Log("!!! OnDestroy GameController !!!");
            _stateDisposable.Dispose();
        }

        #endregion

        /// <summary>
        /// Create new cubes
        /// </summary>
        private void OnCreateCubes()
        {
            var dimension = AppManager.Dimension;
            var cubes = cubeCreator.CreateCubes(dimension);
            cubeController.SetCubes(cubes);
            _stateAsObservable.Value = State.Scrambled;
        }

        /*
        /// <summary>
        /// Create cubes based on the save data
        /// </summary>
        private async UniTask<Unit> OnCreateSavedCube()
        {
            _isContinueGame = true;

            var cubeInfos = EasySaveUtil.LoadSaveData();
            var cubes     = cubeCreator.CreateCubes(cubeInfos);
            cubeController.SetCubes(cubes);

            await FadeCanvas.Instance.FadeOutAsObservable(0.3f).ToUniTask();

            _stateAsObservable.Value = State.UserManipulate;

            return Unit.Default;
        }
        */

        /**/
        /// <summary>
        /// scramble cubes. user cannot touch.
        /// </summary>
        /// <returns></returns>
        private async UniTask<Unit> OnScrambled()
        {
            var isRetry = (_retryCount > 0);

            await cubeController.Scramble(AppManager.ScrambleTimes, isRetry);

            _stateAsObservable.Value = State.UserManipulate;
            return Unit.Default;
        }
        
        /// <summary>
        /// user can touch.
        /// </summary>
        private void OnManipulate()
        {
            // gameUI.StateToGameStart();

            cameraMover.Controllable = true;

            cubeController.Manipulate();
            cubeController.EnableRayCast = true;

            cubeController.MessageAsObservable
                .Where(message => message == CubeController.Message.Solve)
                .Subscribe(_ => _stateAsObservable.Value = State.Solved)
                .AddTo(_stateDisposable);
            
            cubeController.MessageAsObservable
                .Where(message => message == CubeController.Message.CatchCube)
                .Subscribe(_ => cameraMover.Controllable = false)
                .AddTo(_stateDisposable);
            
            cubeController.MessageAsObservable
                .Where(message => message == CubeController.Message.ReleaseCube)
                .Subscribe(_ => cameraMover.Controllable = true)
                .AddTo(_stateDisposable);

            // --- UI events ---

            /*
            gameUI.MessageAsObservable
                .Where(message => message == GameUI.Message.Pause)
//                .Do(_ => Debug.Log("receive message of Pause."))
                .Subscribe(_ => {
                    cubeController.EnableRayCast = false;
                    cameraMover.Controllable     = false;
                })
                .AddTo(_stateDisposable);

            gameUI.MessageAsObservable
                .Where(message => message == GameUI.Message.Resume)
//                .Do(_ => Debug.Log("receive message of Resume."))
                .Subscribe(_ => {
                    cubeController.EnableRayCast = true;
                    cameraMover.Controllable     = true;
                })
                .AddTo(_stateDisposable);

            gameUI.MessageAsObservable
                .Where(message => message == GameUI.Message.Undo)
//                .Do(_ => Debug.Log("receive message of Undo."))
                .SelectMany(_ => cubeController.Undo().ToObservable())
                .Subscribe(success => {
                    // TODO sound.
//                    if (success) Debug.Log("did undo");
//                    else Debug.Log("did not undo");
                })
                .AddTo(_stateDisposable);

            gameUI.MessageAsObservable
                .Where(message => message == GameUI.Message.Retry)
//                .Do(_ => Debug.Log("receive message to Retry. Retry this game"))
                .Subscribe(_ => { _stateAsObservable.Value = State.Retry; })
                .AddTo(_stateDisposable);

            gameUI.MessageAsObservable
                .Where(message => message == GameUI.Message.TransitionToTitle)
//                .Do(_ => Debug.Log("receive message to Title. Go to title scene"))
                .Subscribe(_ => SceneManager.LoadScene(AppManager.SceneTitle))
                .AddTo(_stateDisposable);

            gameUI.MessageAsObservable
                .Where(message => message == GameUI.Message.SavePlayData)
                .Subscribe(_ => {
//                    Debug.Log("Save play data to local.");
                    var cubes = cubeController.Cubes;
                    EasySaveUtil.SaveCubeInfos(cubes);
                })
                .AddTo(_stateDisposable);
                */
        }
        

        /*
        private async UniTask<Unit> OnSolved()
        {
            gameUI.StateToSolved();

            cameraMover.Controllable = false;
            await cameraMover.SolvedAnimation();

            gameUI.StateToResult();

            gameUI.MessageAsObservable
                .Where(message => message == GameUI.Message.Retry)
//                .Do(_ => Debug.Log("receive message to Retry. Retry this game"))
                .Subscribe(message => _stateAsObservable.Value = State.Retry)
                .AddTo(_stateDisposable);

            gameUI.MessageAsObservable
                .Where(message => message == GameUI.Message.TransitionToTitle)
//                .Do(_ => Debug.Log("receive message to Title. Go to title scene"))
                .SelectMany(_ => FadeCanvas.Instance.FadeInAsObservable(0.3f))
                .Subscribe(_ => SceneManager.LoadScene(AppManager.SceneTitle))
                .AddTo(_stateDisposable);

            gameUI.MessageAsObservable
                .Where(message => message == GameUI.Message.DeletePlayData)
                .Subscribe(_ => {
//                    Debug.Log("delete play data. delete play data. delete play data");
                    _isContinueGame = false;
                    EasySaveUtil.DeleteSaveData();
                })
                .AddTo(_stateDisposable);

            return Unit.Default;
        }
        */

        /*
        private async UniTask<Unit> OnRetry()
        {
            await FadeCanvas.Instance.FadeInAsObservable(0.3f).ToUniTask();

            _retryCount++;
            // Destroy cubes and re-create
            cubeController.DestroyCubes();
            cubeController.ClearUserManipulateHistories();

            cameraMover.ToInitialPosition();

            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

            if (_isContinueGame) {
                _stateAsObservable.Value = State.CreateSavedCube;
            }
            else {
                _stateAsObservable.Value = State.CreateCube;
            }

            return Unit.Default;
        }
        */
    }
}