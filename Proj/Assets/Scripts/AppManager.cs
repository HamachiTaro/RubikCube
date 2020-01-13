using UnityEngine;

namespace RubikCube
{
    public class AppManager : SingletonMonoBehaviour<AppManager>
    {
        public const int ScrambleDegree = 90;
        
        public const string TimerFormat = @"mm\:ss\.ff";

        public const float DialogOpenAnimDuration = 0.3f;

        public const float ScreenDragCoefficient = 0.2f;

        public const string EsDimension = "dimension";
        
        public const string EsCubeInfos = "cubeInfos";

        public const string SceneTitle = "Title";
        
        public const string SceneGame = "Game";
        
        /// Android
        private static readonly bool IsAndroid = Application.platform == RuntimePlatform.Android;
        
        /// iOS
        private static readonly bool IsIOS = Application.platform == RuntimePlatform.IPhonePlayer;
        
        /// editor
        public static readonly bool IsEditor = !IsAndroid && !IsIOS;
        
        /// make this true for unity remote 
        public static bool IsUnityRemote = false;
        
        // public enum DimensionType
        // {
        //     Two,
        //     Three,
        //     Four,
        //     Five,
        //     Six,
        // }
        
        // public DimensionType Dimension { get; set; }

        public static int ScrambleTimes = 10;

        public bool StartBySaveData { get; set; }
        
        // public int DimensionToInt(DimensionType dimensionType)
        // {
        //     var dimension = 0;
        //     switch (dimensionType) {
        //         case DimensionType.Two:
        //             dimension = 2;
        //             break;
        //         case DimensionType.Three:
        //             dimension = 3;
        //             break;
        //         case DimensionType.Four:
        //             dimension = 4;
        //             break;
        //         case DimensionType.Five:
        //             dimension = 5;
        //             break;
        //         case DimensionType.Six:
        //             dimension = 6;
        //             break;
        //     }
        //     return dimension;
        // }


        public static int Dimension = 3;

        private void Start()
        {
            DontDestroyOnLoad(this);
        }
    }

}