using System;
using System.Collections.Generic;
using System.Threading;

namespace Tetris
{
    class Program
    {
        // SETTINGS
        static int TetrisRows = 20;
        static int TetrisCols = 10;
        static int InfoCols = 10;
        static int ConsoleRows = 1 + TetrisRows + 1;
        static int ConsoleCols = 1 + TetrisCols + 1 + InfoCols + 1;
        static List<bool[,]> TetrisFigures = new List<bool[,]>()
        {
            new bool[,] // I
            {
                {true, true, true, true}
            }, 
            new bool[,] // O
            {
                {true, true },
                {true, true }
            }, 
            new bool[,] // T
            {
                {false, true, false},
                {true, true, true}
            }, 
            new bool[,] // S
            {
                {false, true, true},
                {true, true, false}
            }, 
            new bool[,] // Z
            {
                {true, true, false},
                {false, true, true}
            },
            new bool[,] // J
            {
                {true, false, false},
                {true, true, true}
            }, 
            new bool[,] // L
            {
                {false, false, true},
                {true, true, true}
            }, 
        };

        // STATE
        static int Score = 0;
        static int Frame = 0;
        static int FramesToMoveFigure = 15;
        static int CurrentFigureIndex = 2;
        static int CurrentFigureRow = 0;
        static int CurrentFigureCol = 0;
        static bool[,] TetrisField = new bool[TetrisRows, TetrisCols];

        static void Main(string[] args)
        {
            Console.Title = "Tetris";
            Console.WindowHeight = ConsoleRows + 1;
            Console.WindowWidth = ConsoleCols;
            Console.BufferHeight = ConsoleRows + 1;
            Console.BufferWidth = ConsoleCols;
            Console.CursorVisible = false;
            while (true)
            {
                Frame++;
                // USER INPUT
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey();
                    if (key.Key == ConsoleKey.Escape)
                    {
                        // Environment.Exit(0); - can be used instead of return
                        return; // return is used because of the Main() method
                    }

                    if (key.Key == ConsoleKey.LeftArrow || key.Key == ConsoleKey.A)
                    {
                        // TODO: Move current figure left
                        CurrentFigureCol--; // TODO: Out of range
                    }
                    if (key.Key == ConsoleKey.RightArrow || key.Key == ConsoleKey.D)
                    {
                        // TODO: Move current figure right
                        CurrentFigureCol++; // TODO: Out of range
                    }

                    if (key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.S)
                    {
                        Score++;
                        Frame = 1;
                        CurrentFigureRow++;
                        // TODO: Move current figure down
                    }
                    if (key.Key == ConsoleKey.Spacebar || key.Key == ConsoleKey.W || key.Key == ConsoleKey.UpArrow)
                    {
                        // TODO: Implement 90-degree rotation of the current figure
                    }
                }

                // UPDATE THE GAME STATE
                if (Frame % FramesToMoveFigure == 0)
                {
                    CurrentFigureRow++;
                    Frame = 0;
                }
                // TODO: if (Collision())
                // {
                //    AddCurrentFigure()
                //    CheckForFullLines()
                //    if (lines remove) Score++;
                // }


                // CHANGE STATE

                // REDRAW UI
                DrawBorder();
                DrawInfo();
                DrawCurrentFigure();

                Thread.Sleep(40);
            }
        }

        

        static void DrawBorder()
        {
            Console.SetCursorPosition(0, 0);

            string line = "╔";
            line += new string('═', TetrisCols);
            line += "╦";
            line += new string('═', TetrisCols);
            line += "╗";
            Console.Write(line);

            for (int i = 0; i < TetrisRows; i++)
            {
                string middleLine = "║";
                middleLine += new string(' ', TetrisCols);
                middleLine += "║";
                middleLine += new string(' ', InfoCols);
                middleLine += "║";
                Console.Write(middleLine);
            }

            string endLine = "╚";
            endLine += new string('═', TetrisCols);
            endLine += "╩";
            endLine += new string('═', TetrisCols);
            endLine += "╝";
            Console.Write(endLine);
        }

        static void DrawInfo()
        {
            Write("Score:", 1, 3 + TetrisCols);
            Write(Score.ToString(), 2, 3 + TetrisCols);

            Write("Frame:", 4, 3 + TetrisCols);
            Write(Frame.ToString(), 5, 3 + TetrisCols);
        }
        static void DrawCurrentFigure()
        {
            // to use ■

            var currentFigure = TetrisFigures[CurrentFigureIndex];
            for (int row = 0; row < currentFigure.GetLength(0); row++)
            {
                for (int col = 0; col < currentFigure.GetLength(1); col++)
                {
                    if (currentFigure[row, col])
                    {
                        Write("@", row + 1 + CurrentFigureRow, col + 1 + CurrentFigureCol);
                    }
                }
            }
        }
        static void Write(string text, int row, int col, ConsoleColor color = ConsoleColor.Red)
        {
            Console.ForegroundColor = color;
            Console.SetCursorPosition(col, row);
            Console.Write(text);
            Console.ResetColor();
        }
    }
}
