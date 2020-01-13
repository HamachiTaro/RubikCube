using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using UniRx.Async;

namespace RubikCube
{
    public class CubesUtil : MonoBehaviour
    {
        private List<RandomRotateInfo> _scrambleInfos;

        public void ClearScrambleInfos()
        {
            _scrambleInfos.Clear();
        }

        public async UniTask<Unit> ScrambleCubes(bool useExistInfo, List<RotateCube> cubes, int scrambleTimes)
        {
            if (!useExistInfo || _scrambleInfos.Count == 0)
            {
                var ids = cubes.Select(cube => cube.Id).ToList();
                CreateScrambleInfos(ids, scrambleTimes);
            }
            
            foreach (var info in _scrambleInfos)
            {
                var id = info.CubeId;
                var targetCube = cubes.First(c => c.Id == id);

                var scrambleTargetCubes = new List<RotateCube>();
                var rotateDegree = info.RotateDegree;

                if (rotateDegree.x != 0)
                {
                    scrambleTargetCubes = cubes.Where(cube => cube.PositionId.x == targetCube.PositionId.x).ToList();
                }
                else if (rotateDegree.y != 0)
                {
                    scrambleTargetCubes = cubes.Where(cube => cube.PositionId.y == targetCube.PositionId.y).ToList();
                }
                else if (rotateDegree.z != 0)
                {
                    scrambleTargetCubes = cubes.Where(cube => cube.PositionId.z == targetCube.PositionId.z).ToList();
                }
                
                await RotateCubesWithAnim(scrambleTargetCubes, rotateDegree);

                UpdatePositionIds(scrambleTargetCubes, rotateDegree);

                await UniTask.Delay(System.TimeSpan.FromSeconds(0.1f));
            }

            return Unit.Default;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rotateCubes"></param>
        /// <param name="rotateDegree"></param>
        /// <returns></returns>
        public async UniTask<Unit> RotateCubesWithAnim(List<RotateCube> rotateCubes, Vector3 rotateDegree)
        {
            var totalDegree = 0f;
            bool isX = false;
            bool isY = false;
            bool isZ = false;

            if (!Mathf.Approximately(rotateDegree.x, 0f))
            {
                isX = true;
                totalDegree = rotateDegree.x;
            }
            else if (!Mathf.Approximately(rotateDegree.y, 0f))
            {
                isY = true;
                totalDegree = rotateDegree.y;
            }
            else if (!Mathf.Approximately(rotateDegree.z, 0f))
            {
                isZ = true;
                totalDegree = rotateDegree.z;
            }

            // frame count for animation.
            var frame = 10;

            if (Mathf.Abs(totalDegree) < 10)
            {
                frame = 1;
            }
            else if (Mathf.Abs(totalDegree) < 30)
            {
                frame = 3;
            }
            else if (Mathf.Abs(totalDegree) < 45)
            {
                frame = 5;
            }

            // rotate degree per frame
            var deltaDegreePerFrame = Mathf.FloorToInt(totalDegree / frame);
            var degreeList = new List<float>();

            for (int i = 0; i < frame; i++)
            {
                if (i < frame - 1)
                {
                    degreeList.Add(deltaDegreePerFrame);
                }
                else
                {
                    degreeList.Add(totalDegree - deltaDegreePerFrame * (frame - 1));
                }
            }

            foreach (var degree in degreeList)
            {
                // create a rotation Matrix
                var rotate = new Vector3(isX ? degree : 0, isY ? degree : 0, isZ ? degree : 0);
                var m = RotateMatrix(rotate);

                foreach (var cube in rotateCubes)
                {
                    cube.RotateAroundAxis(m);
                }

                await UniTask.DelayFrame(1);
            }

            return Unit.Default;
        }

        /// <summary>
        /// create rotation matrix by rotate angle vector3
        /// </summary>
        /// <param name="rotate"></param>
        /// <returns></returns>
        public Matrix4x4 RotateMatrix(Vector3 rotate)
        {
            var quaternion = Quaternion.Euler(rotate);
            var m = Matrix4x4.identity;
            m.SetTRS(Vector3.zero, quaternion, Vector3.one);

            return m;
        }

        /// <summary>
        /// update position ids which had changed by rotation
        /// </summary>
        /// <param name="rotateCubes"></param>
        /// <param name="rotateDegree"></param>
        public void UpdatePositionIds(List<RotateCube> rotateCubes, Vector3Int rotateDegree)
        {
            var dimension = GameController.Dimensions;
            var transferValueToOrigin = (dimension - 1) * 0.5f;

            foreach (var rotateCube in rotateCubes)
            {
                var positionId = rotateCube.RotatePositionId(rotateDegree, transferValueToOrigin);
                rotateCube.PositionId = positionId;
            }
        }

        /// <summary>
        /// create scramble info. keep info for retry.
        /// </summary>
        /// <param name="cubeIds"></param>
        /// <param name="count"></param>
        private void CreateScrambleInfos(List<int> cubeIds, int count)
        {
            _scrambleInfos = new List<RandomRotateInfo>();

            for (int i = 0; i < count; i++)
            {
                // scramble target cubes
                var random = Random.Range(0, cubeIds.Count);
                var id = cubeIds[random];

                // scramble degree
                var rotateDegree = Vector3Int.zero;
                var angleRandom = Random.Range(0, 6);

                var angle = GameController.ScrambleDegree;

                switch (angleRandom)
                {
                    case 0:
                        rotateDegree = new Vector3Int(angle, 0, 0);
                        break;
                    case 1:
                        rotateDegree = new Vector3Int(-angle, 0, 0);
                        break;
                    case 2:
                        rotateDegree = new Vector3Int(0, angle, 0);
                        break;
                    case 3:
                        rotateDegree = new Vector3Int(0, -angle, 0);
                        break;
                    case 4:
                        rotateDegree = new Vector3Int(0, 0, angle);
                        break;
                    case 5:
                        rotateDegree = new Vector3Int(0, 0, -angle);
                        break;
                }

                var info = new RandomRotateInfo(id, rotateDegree);
                _scrambleInfos.Add(info);
            }
        }
    }

    public class RandomRotateInfo
    {
        public int CubeId { get; }

        public Vector3Int RotateDegree { get; }

        public RandomRotateInfo(int id, Vector3Int rotateDegree)
        {
            CubeId = id;
            RotateDegree = rotateDegree;
        }
    }
}