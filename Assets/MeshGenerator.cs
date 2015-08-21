using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MarchingCubes
{
    // Heavily influenced by Sebastian Lague, see https://www.youtube.com/watch?v=v7yyZZjF1z4&list=PLFt_AvWsXl0eZgMK_DT5_biRkWXftAOf9
    [ExecuteInEditMode]
    [RequireComponent(typeof(MapGenerator))]
    public class MeshGenerator : MonoBehaviour
    {
        private MapGenerator _mapGenerator;
        private SquareGrid _squareGrid;

        public void Start()
        {
            _mapGenerator = GetComponent<MapGenerator>();
        }

        public void GenerateMesh()
        {
            _mapGenerator.GenerateMap();
            _squareGrid = new SquareGrid(_mapGenerator.Map, 1);

            GetComponent<MeshFilter>().sharedMesh = _squareGrid.Mesh;
        }

        public void OnDrawGizmos()
        {
            if (_squareGrid == null)
            {
                GenerateMesh();
            }

            for (int y = 0; y < _squareGrid.Size.y; y++)
            {
                for (int x = 0; x < _squareGrid.Size.x; x++)
                {
                    Square square = _squareGrid.Squares[x, y];

                    Gizmos.color = Color.green;
                    Gizmos.DrawCube(square.SquareCenter, Vector3.one * 0.1f);

                    //Gizmos.color = Color.cyan;
                    //Gizmos.DrawCube(MoveHome(square.CenterTop.Position, square.SquareCenter), Vector3.one * 0.1f);
                    //Gizmos.DrawCube(MoveHome(square.CenterBottom.Position, square.SquareCenter), Vector3.one * 0.1f);
                    //Gizmos.DrawCube(MoveHome(square.CenterLeft.Position, square.SquareCenter), Vector3.one * 0.1f);
                    //Gizmos.DrawCube(MoveHome(square.CenterRight.Position, square.SquareCenter), Vector3.one * 0.1f);

                    //DrawCubeSpecial(square.BottomLeft, square);
                    //DrawCubeSpecial(square.TopLeft, square);
                    //DrawCubeSpecial(square.TopRight, square);
                    //DrawCubeSpecial(square.BottomRight, square);
                }
            }
        }

        //private void DrawCubeSpecial(ControlNode controlNode, Square square)
        //{
        //    Gizmos.color = controlNode.Active ? Color.black : Color.white;
        //    Gizmos.DrawCube(MoveHome(controlNode.Position, square.SquareCenter), Vector3.one * 0.1f);
        //}

        private Vector2 MoveHome(Vector2 position, Vector2 home)
        {
            return Vector2.Lerp(position, home, 0.5f);
        }

        public class SquareGrid
        {
            public SquareGrid(float[,] map, float squareSize)
            {
                Vertices = new List<Node>();
                Triangles = new List<Node>();
                SquareSize = squareSize;
                MapSize =
                    new Vector2i(
                        map.GetLength(0),
                        map.GetLength(1));
                Size = MapSize - Vector2i.One;
                WorldSize = MapSize.ToVector2() * squareSize;
                BuildControlNodes(map);
                BuildSquares();
                BuildTriangles();
                BuildMesh();
            }

            public Vector2i MapSize { get; private set; }
            public Vector2i Size { get; private set; }
            public Vector2 WorldSize { get; private set; }
            public ControlNode[,] ControlNodes { get; private set; }
            public Square[,] Squares { get; private set; }
            public List<Node> Vertices { get; set; }
            public List<Node> Triangles { get; set; }
            public float SquareSize { get; private set; }
            public Mesh Mesh { get; set; }

            private void BuildControlNodes(float[,] map)
            {
                ControlNodes = new ControlNode[MapSize.x, MapSize.y];
                for (int y = 0; y < MapSize.y; y++)
                {
                    for (int x = 0; x < MapSize.x; x++)
                    {
                        Vector2i p = new Vector2i(x, y);
                        ControlNodes[x, y] =
                            new ControlNode(
                                p,
                                map,
                                p.ToVector2() - WorldSize * 0.5f + Vector2.one * 0.5f,
                                SquareSize);
                    }
                }
            }

            private void BuildSquares()
            {
                Squares = new Square[Size.x, Size.y];

                for (int y = 0; y < Size.y; y++)
                {
                    for (int x = 0; x < Size.x; x++)
                    {
                        Vector2i p = new Vector2i(x, y);
                        Squares[x, y] =
                            new Square(
                                GetControlNode(p),
                                GetControlNode(p + Vector2i.up),
                                GetControlNode(p + Vector2i.up + Vector2i.right),
                                GetControlNode(p + Vector2i.right));
                    }
                }
            }


            private ControlNode GetControlNode(Vector2i pos)
            {
                return ControlNodes[pos.x, pos.y];
            }

            private void BuildMesh()
            {
                Mesh = new Mesh();
                Mesh.name = "Marching Cubes";
                Mesh.vertices = Vertices.Select(n => (Vector3)n.Position).ToArray();
                Mesh.triangles = Triangles.Select(n => n.VertexId.Value).ToArray();
                Mesh.RecalculateNormals();
            }

            private void BuildTriangles()
            {
                for (int y = 0; y < Size.y; y++)
                {
                    for (int x = 0; x < Size.x; x++)
                    {
                        Square square = Squares[x, y];

                        bool bit0 = square.BottomLeft.Active > 0;
                        bool bit1 = square.TopLeft.Active > 0;
                        bool bit2 = square.TopRight.Active > 0;
                        bool bit3 = square.BottomRight.Active > 0;

                        int id = (bit0 ? 1 : 0) + (bit1 ? 2 : 0) + (bit2 ? 4 : 0) + (bit3 ? 8 : 0);

                        switch (id)
                        {
                            case 0:
                                break;

                            case 1:
                                AddTriangles(square.CenterLeft, square.CenterBottom, square.BottomLeft);
                                break;

                            case 2:
                                AddTriangles(square.CenterLeft, square.TopLeft, square.CenterTop);
                                break;

                            case 3:
                                AddTriangles(square.CenterTop, square.CenterBottom, square.BottomLeft, square.TopLeft);
                                break;

                            case 4:
                                AddTriangles(square.CenterTop, square.TopRight, square.CenterRight);
                                break;

                            case 5:
                                AddTriangles(square.CenterLeft, square.CenterTop, square.TopRight, square.CenterRight, square.CenterBottom, square.BottomLeft);
                                break;

                            case 6:
                                AddTriangles(square.CenterRight, square.CenterLeft, square.TopLeft, square.TopRight);
                                break;

                            case 7:
                                AddTriangles(square.CenterRight, square.CenterBottom, square.BottomLeft, square.TopLeft, square.TopRight);
                                break;

                            case 8:
                                AddTriangles(square.CenterBottom, square.CenterRight, square.BottomRight);
                                break;

                            case 9:
                                AddTriangles(square.CenterLeft, square.CenterRight, square.BottomRight, square.BottomLeft);
                                break;

                            case 10:
                                AddTriangles(square.CenterBottom, square.CenterLeft, square.TopLeft, square.CenterTop, square.CenterRight, square.BottomRight);
                                break;

                            case 11:
                                AddTriangles(square.CenterTop, square.CenterRight, square.BottomRight, square.BottomLeft, square.TopLeft);
                                break;

                            case 12:
                                AddTriangles(square.CenterBottom, square.CenterTop, square.TopRight, square.BottomRight);
                                break;

                            case 13:
                                AddTriangles(square.CenterLeft, square.CenterTop, square.TopRight, square.BottomRight, square.BottomLeft);
                                break;

                            case 14:
                                AddTriangles(square.CenterBottom, square.CenterLeft, square.TopLeft, square.TopRight, square.BottomRight);
                                break;

                            case 15:
                                AddTriangles(square.BottomLeft, square.TopLeft, square.TopRight, square.BottomRight);
                                break;
                        }
                    }
                }
            }

            private void AddTriangles(params Node[] nodes)
            {
                foreach (Node node in nodes)
                {
                    if (!node.VertexId.HasValue)
                    {
                        node.VertexId = Vertices.Count;
                        Vertices.Add(node);
                    }
                }

                for (int i = 2; i < nodes.Length; i++)
                {
                    Triangles.Add(nodes[0]);
                    Triangles.Add(nodes[i - 1]);
                    Triangles.Add(nodes[i]);
                }
            }
        }

        public class Square
        {
            public Square(
                ControlNode bottomLeft,
                ControlNode topLeft,
                ControlNode topRight,
                ControlNode bottomRight)
            {
                SquareCenter = (bottomLeft.Position + topRight.Position) * 0.5f;
                BottomLeft = bottomLeft;
                TopLeft = topLeft;
                TopRight = topRight;
                BottomRight = bottomRight;

                CenterLeft = BottomLeft.Up;
                CenterRight = BottomRight.Up;
                CenterTop = TopLeft.Right;
                CenterBottom = BottomLeft.Right;
            }

            public Vector2 SquareCenter { get; set; }

            public ControlNode BottomLeft { get; set; }
            public ControlNode TopLeft { get; set; }
            public ControlNode TopRight { get; set; }
            public ControlNode BottomRight { get; set; }

            public Node CenterLeft { get; set; }
            public Node CenterRight { get; set; }
            public Node CenterTop { get; set; }
            public Node CenterBottom { get; set; }
        }

        public class Node
        {
            public Node(Vector2 position)
            {
                Position = position;
            }

            public Vector2 Position { get; set; }
            public int? VertexId { get; set; }
        }

        public class ControlNode : Node
        {
            public float Active { get; set; }
            public Node Right { get; set; }
            public Node Up { get; set; }

            public ControlNode(Vector2i pos, float[,] map, Vector2 position, float size)
                : base(position)
            {
                Active = map[pos.x, pos.y];

                if (pos.x == map.GetLength(0))
                {
                    Right = new Node(position + Vector2.right * size * 0.5f);
                }
                else
                {
                    Right = new Node(position + Vector2.right * size * 0.5f);
                }

                if (pos.y == map.GetLength(1))
                {
                    Up = new Node(position + Vector2.up * size * 0.5f);
                }
                else
                {
                    Up = new Node(position + Vector2.up * size * 0.5f);
                }

                //float right = pos.x <  ? map[pos.x + 1, pos.y] : Active;
                //float up = map[pos.x, pos.y + 1];

            }
        }
    }
}