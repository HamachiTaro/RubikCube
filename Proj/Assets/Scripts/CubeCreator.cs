using System.Collections.Generic;
using UnityEngine;

namespace RubikCube
{
    public class CubeCreator : MonoBehaviour
    {
        [SerializeField] private RotateCube outerCube;

        /// <summary>
        /// create new cubes
        /// </summary>
        /// <param name="dimensions"></param>
        /// <returns></returns>
        public List<RotateCube> CreateCubes(int dimensions)
        {
            var startZ = -dimensions * 0.5f + 0.5f;
            var startY = -dimensions * 0.5f + 0.5f;
            var startX = -dimensions * 0.5f + 0.5f;

            var id = 0;
            var cubes = new List<RotateCube>();

            for (var z = 0; z < dimensions; z++)
            {
                for (var y = 0; y < dimensions; y++)
                {
                    for (var x = 0; x < dimensions; x++)
                    {
                        var isOuter = (x == 0 || y == 0 || z == 0
                            || x == dimensions - 1 || y == dimensions - 1 || z == dimensions - 1);

                        if (!isOuter) continue;

                        var cube = Instantiate(outerCube, transform);
                        var posX = startX + x;
                        var posY = startY + y;
                        var posZ = startZ + z;
                        cube.transform.position = new Vector3(posX, posY, posZ);

                        cube.PositionId = new Vector3Int(x, y, z);
                        cube.Id = id;
                        cubes.Add(cube);

                        id++;
                    }
                }
            }

            return cubes;
        }
    }
}