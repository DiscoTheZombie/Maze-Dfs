using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class Maze : Node2D
{
    [Flags]
    public enum WallCompass
    {
        None = 0, // 0000
        N = 1, // 0001
        E = 2, // 0010 
        S = 4, // 0100
        W = 8 // 1000
    }

    public Dictionary<Vector2, WallCompass> CellWalls { get; private set; }
    public TileMap Map { get; private set; }
    public Random Rng { get; private set; }
    public Vector2 TileSize = new Vector2(64, 64); // Tile size in pixels, has to match editor tiles.

    public int
        Width = 2, // Width of map in tiles.
        Height = 2;// Height of map in tiles.


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        CellWalls = new Dictionary<Vector2, WallCompass>
            {
                {new Vector2(0, -1), WallCompass.N},
                {new Vector2(1, 0), WallCompass.E},
                {new Vector2(0, 1), WallCompass.S},
                {new Vector2(-1, 0), WallCompass.W}
            };

        Map = GetNode<TileMap>("TileMap");
        Rng = new Random();
        // Get cell size from node even after setting it in code:
        TileSize = Map.CellSize;
        // TODO: implement make maze.
        MakeMaze();
    }

    public List<Vector2> CheckNeighbours(Vector2 cell, List<Vector2> unvisited)
    {
        var result = new List<Vector2>();
        foreach (Vector2 vec in CellWalls.Keys)
            {
                if (unvisited.Contains(cell + vec))
                    {
                        result.Add(cell + vec);
                    }
            }

        return result;
    }

    public void MakeMaze()
    {
        var unvisited = new List<Vector2>();
        Stack<Vector2> stack = new Stack<Vector2>();
        Map.Clear();
        for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                    {
                        unvisited.Add(new Vector2(x, y));
                        Map.SetCellv(new Vector2(x, y), (int)(WallCompass.N
                                                               | WallCompass.E
                                                               | WallCompass.S
                                                               | WallCompass.W));
                    }
            }
        var current = new Vector2(0, 0);
        unvisited.Remove(current);
        
        // Recursive backtrack algorithm:
        while (unvisited.Any())
            {
                var neighbours = CheckNeighbours(current, unvisited);
                GD.Print($"unvisited:");
                unvisited.ForEach((v=> GD.Print(v)));
                GD.Print($"current: {current}");
                if (neighbours.Count > 0)
                    {
                        // Pick a random entry from available neighbouring tiles:
                        var next = neighbours[Rng.Next(neighbours.Count)];
                        stack.Push(current);
                        GD.Print($"Next cell is {next}");
                        var dir = next - current;
                        GD.Print($"Move in direction {dir} from  current pos: {current}");
                        
                        // Remove the wall from current cell towards direction:
                        var currentTileValue = Map.GetCellv(current);
                        var directionOfWallToBreak = (int) CellWalls[dir];
                        GD.Print($"Current tile value {currentTileValue} - {directionOfWallToBreak} =");
                        var currentWalls = Map.GetCellv(current) - (int) CellWalls[dir];
                        GD.Print($"new current tile {currentWalls}");
                        
                        // Remove wall of the target cell facing the current cell:
                        var nextTileValue = Map.GetCellv(next);
                        directionOfWallToBreak = (int) CellWalls[-dir];
                        GD.Print($"Next tile value {currentTileValue} - {directionOfWallToBreak} =");
                        var nextWalls = Map.GetCellv(next) - (int)CellWalls[-dir];
                        GD.Print($"new next tile {nextWalls}");
                        // Apply calculated values to cells:
                        Map.SetCellv(current, currentWalls);
                        Map.SetCellv(next, nextWalls);
                        current = next;
                        unvisited.Remove(current);
                    }
                else if (stack.Any())
                    {
                        current = stack.Pop();
                    }
                GD.Print($" End current: {current}\n {neighbours}");
            }
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}