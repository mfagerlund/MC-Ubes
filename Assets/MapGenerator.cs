using UnityEngine;
using Random = UnityEngine.Random;

namespace MarchingCubes
{
    // Heavily influenced by Sebastian Lague, see https://www.youtube.com/watch?v=v7yyZZjF1z4&list=PLFt_AvWsXl0eZgMK_DT5_biRkWXftAOf9
    [ExecuteInEditMode]
    public class MapGenerator : MonoBehaviour
    {
        public Vector2i size = new Vector2i(20, 15);
        [Range(0, 100)]
        public float floorPercentage = 50;

        [Range(0, 6)]
        public int cellularAutomataSteps = 4;

        public int seed = -1;

        public float[,] Map;

        public void Start()
        {
            GenerateMap();
        }

        public void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                GenerateMap();
            }
        }

        public void GenerateMap()
        {
            Map = new float[size.x, size.y];
            BuildRandomMap();
            for (int i = 0; i < cellularAutomataSteps; i++)
            {
                RunCellularAutomata();
            }
        }

        public void OnDrawGizmos()
        {
            if (Map == null || Map.GetLength(0) != size.x || Map.GetLength(1) != size.y)
            {
                GenerateMap();
            }

            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    Vector2i p = new Vector2i(x, y);
                    bool set = Map[x, y] > 0;
                    Gizmos.color = set ? Color.black : Color.white;
                    Gizmos.DrawCube(GetTileCenter(p), Vector3.one * 0.2f);
                }
            }
        }

        private void RunCellularAutomata()
        {
            for (int y = 1; y < size.y - 1; y++)
            {
                for (int x = 1; x < size.x - 1; x++)
                {
                    Vector2i p = new Vector2i(x, y);
                    RunCellularAutomata(p);
                }
            }
        }

        private void RunCellularAutomata(Vector2i p)
        {
            int hits = 0;
            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    Vector2i delta = new Vector2i(x, y);
                    if (GetMapFill(delta + p) > 0)
                    {
                        hits++;
                    }
                }
            }

            if (hits > 4)
            {
                Map[p.x, p.y] = Random.Range(0.1f, 1f);
            }
            if (hits < 4)
            {
                Map[p.x, p.y] = 0;
            }
        }

        private void BuildRandomMap()
        {
            if (seed != -1)
            {
                Random.seed = seed;
            }
            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    Vector2i p = new Vector2i(x, y);
                    Map[x, y] =
                        size.IsOnBorder(p)
                        || Random.Range(0, 100) >= floorPercentage
                            ? Random.Range(0.1f, 1)
                            : 0;
                }
            }
        }

        private Vector2 GetTileCenter(Vector2i pos)
        {
            return
                new Vector2(
                    pos.x - size.x / 2 + 0.5f,
                    pos.y - size.y / 2 + 0.5f);
        }

        private float GetMapFill(Vector2i pos)
        {
            if (!size.ContainsAsSize(pos) || size.IsOnBorder(pos))
            {
                return 1;
            }

            return Map[pos.x, pos.y];
        }
    }
}