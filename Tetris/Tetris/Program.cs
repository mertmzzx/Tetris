using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
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
                {true, true, false},
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
        static string ScoresFile = "scores.txt";
        private static int[] ScorePerLines = { 9, 40, 100, 300, 1200 };

        // STATE
        static int HighScore = 0;
        static int Score = 0;
        static int Frame = 0;
        static int Level = 1;
        static int FramesToMoveFigure = 16;
        static bool[,] CurrentFigure = null;
        static int CurrentFigureRow = 0;
        static int CurrentFigureCol = 0;
        static bool[,] TetrisField = new bool[TetrisRows, TetrisCols];
        static Random Random = new Random();

        static void Main(string[] args)
        {
            new Thread(PlayMusic).Start();

            if (File.Exists(ScoresFile))
            {
                var allScores = File.ReadAllLines(ScoresFile);

                foreach (var score in allScores)
                {
                    var match = Regex.Match(score, @" => (?<score>[0-9]+)");
                    HighScore = Math.Max(HighScore, int.Parse(match.Groups["score"].Value));
                }
            }

            Console.Title = "Tetris";
            Console.CursorVisible = false;
            Console.WindowHeight = ConsoleRows + 1;
            Console.WindowWidth = ConsoleCols;
            Console.BufferHeight = ConsoleRows + 1;
            Console.BufferWidth = ConsoleCols;
            CurrentFigure = TetrisFigures[Random.Next(0, TetrisFigures.Count)];
            while (true)
            {
                Frame++;
                UpdateLevel();

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
                        if (CurrentFigureCol >= 1)
                        {
                            CurrentFigureCol--;
                        }
                    }
                    if (key.Key == ConsoleKey.RightArrow || key.Key == ConsoleKey.D)
                    {
                        if (CurrentFigureCol < TetrisCols - CurrentFigure.GetLength(1))
                        {
                            CurrentFigureCol++;
                        }
                    }

                    if (key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.S)
                    {
                        Score += Level;
                        Frame = 1;
                        CurrentFigureRow++;
                    }
                    if (key.Key == ConsoleKey.Spacebar || key.Key == ConsoleKey.W || key.Key == ConsoleKey.UpArrow)
                    {
                        RotateCurrentFigure();
                    }
                }

                // UPDATE THE GAME STATE
                if (Frame % (FramesToMoveFigure - Level) == 0)
                {
                    CurrentFigureRow++;
                    Frame = 0;
                }

                if (Collision(CurrentFigure))
                {
                    AddCurrentFigureToTetrisField();

                    int lines = CheckForFullLines();
                    Score += ScorePerLines[lines] * Level;

                    CurrentFigure = TetrisFigures[Random.Next(0, TetrisFigures.Count)];
                    CurrentFigureRow = 0;
                    CurrentFigureCol = 0;

                    if (Collision(CurrentFigure))
                    {
                        File.AppendAllLines(ScoresFile, new List<string>
                        {
                            $"[{DateTime.Now.ToLongTimeString()}] {Environment.UserName} => {Score}"
                        });

                        var scoreAsString = Score.ToString();
                        scoreAsString += new string(' ', 7 - scoreAsString.Length);

                        Write("╔═════════════╗", 5, 4);
                        Write("║ GAME        ║", 6, 4);
                        Write("║      OVER!  ║", 7, 4);
                        Write("║             ║", 8, 4);
                        Write($"║ {scoreAsString}     ║", 9, 4);
                        Write("╚═════════════╝", 10, 4);

                        Thread.Sleep(100000);
                        return;
                    }
                }

                // REDRAW UI
                DrawBorder();
                DrawInfo();
                DrawCurrentFigure();
                DrawTetrisField();

                Thread.Sleep(40);
            }
        }

        static void UpdateLevel()
        {
            if (Score <= 0)
            {
                Level = 1;
                return;
            }

            Level = (int)Math.Log10(Score) - 1;
            if (Level < 1)
            {
                Level = 1;
            }

            if (Level > 10)
            {
                Level = 10;
            }
        }

        static void RotateCurrentFigure()
        {
            var newFigure = new bool[CurrentFigure.GetLength(1), CurrentFigure.GetLength(0)];

            for (int row = 0; row < CurrentFigure.GetLength(0); row++)
            {
                for (int col = 0; col < CurrentFigure.GetLength(1); col++)
                {
                    newFigure[col, CurrentFigure.GetLength(0) - row - 1] = CurrentFigure[row, col];
                }
            }

            if (!Collision(newFigure))
            {
                CurrentFigure = newFigure;
            }
        }

        static int CheckForFullLines()
        {
            int lines = 0;

            for (int row = 0; row < TetrisField.GetLength(0); row++)
            {
                bool rowIsFull = true;
                for (int col = 0; col < TetrisField.GetLength(1); col++)
                {
                    if (TetrisField[row, col] == false)
                    {
                        rowIsFull = false;
                        break;
                    }
                }

                if (rowIsFull)
                {
                    for (int rowToMove = row; rowToMove >= 1; rowToMove--)
                    {
                        for (int col = 0; col < TetrisField.GetLength(1); col++)
                        {
                            TetrisField[rowToMove, col] = TetrisField[rowToMove - 1, col];
                        }
                    }

                    lines++;
                }
            }

            return lines;
        }

        static void AddCurrentFigureToTetrisField()
        {
            for (int row = 0; row < CurrentFigure.GetLength(0); row++)
            {
                for (int col = 0; col < CurrentFigure.GetLength(1); col++)
                {
                    if (CurrentFigure[row, col])
                    {
                        TetrisField[CurrentFigureRow + row, CurrentFigureCol + col] = true;
                    }
                }
            }
        }

        static bool Collision(bool[,] figure)
        {
            if (CurrentFigureCol > TetrisCols - figure.GetLength(1))
            {
                return true;
            }

            if (CurrentFigureRow + figure.GetLength(0) == TetrisRows)
            {
                return true;
            }

            for (int row = 0; row < figure.GetLength(0); row++)
            {
                for (int col = 0; col < figure.GetLength(1); col++)
                {
                    if (figure[row, col] && TetrisField[CurrentFigureRow + row + 1, CurrentFigureCol + col])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        static void DrawBorder()
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.SetCursorPosition(0, 0);

            string line = "╔";
            line += new string('═', TetrisCols);
            line += "╦";
            line += new string('═', InfoCols);
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
            endLine += new string('═', InfoCols);
            endLine += "╝";
            Console.Write(endLine);
        }

        static void DrawInfo()
        {
            if (Score > HighScore)
            {
                HighScore = Score;
            }

            Write("Level:", 1, 3 + TetrisCols);
            Write(Level.ToString(), 2, 3 + TetrisCols);

            Write("Score:", 4, 3 + TetrisCols);
            Write(Score.ToString(), 5, 3 + TetrisCols);

            Write("Best:", 7, 3 + TetrisCols);
            Write(Score.ToString(), 8, 3 + TetrisCols);

            // Write("Frame:", 7, 3 + TetrisCols);
            // Write(Frame.ToString() + " / " + (FramesToMoveFigure - Level).ToString(), 8, 3 + TetrisCols);

            // Write("Position:", 10, 3 + TetrisCols);
            // Write($"{CurrentFigureRow}, {CurrentFigureCol}", 11, 3 + TetrisCols);

            Write("Controls:", 13, 3 + TetrisCols);
            Write($"    ▲", 14, 3 + TetrisCols);
            Write($"  ◄   ►", 15, 3 + TetrisCols);
            Write($"    ▼", 16, 3 + TetrisCols);
        }

        static void DrawCurrentFigure()
        {
            // ■ can be used instead of @

            for (int row = 0; row < CurrentFigure.GetLength(0); row++)
            {
                for (int col = 0; col < CurrentFigure.GetLength(1); col++)
                {
                    if (CurrentFigure[row, col])
                    {
                        Write("@", row + 1 + CurrentFigureRow, col + 1 + CurrentFigureCol, ConsoleColor.Yellow);
                    }
                }
            }
        }

        static void DrawTetrisField()
        {
            for (int row = 0; row < TetrisRows; row++) //TetrisField.GetLength(0)
            {
                for (int col = 0; col < TetrisCols; col++) //TetrisField.GetLength(1)
                {
                    if (TetrisField[row, col])
                    {
                        Write("@", row + 1, col + 1);
                    }

                }
            }
        }

        static void Write(string text, int row, int col, ConsoleColor color = ConsoleColor.DarkYellow)
        {
            Console.ForegroundColor = color;
            Console.SetCursorPosition(col, row);
            Console.Write(text);
            Console.ResetColor();
        }

        static void PlayMusic()
        {
            while (true)
            {
                const int soundLenght = 100;
                Console.Beep(1320, soundLenght * 4);
                Console.Beep(990, soundLenght * 2);
                Console.Beep(1056, soundLenght * 2);
                Console.Beep(1188, soundLenght * 2);
                Console.Beep(1320, soundLenght);
                Console.Beep(1188, soundLenght);
                Console.Beep(1056, soundLenght * 2);
                Console.Beep(990, soundLenght * 2);
                Console.Beep(880, soundLenght * 4);
                Console.Beep(880, soundLenght * 2);
                Console.Beep(1056, soundLenght * 2);
                Console.Beep(1320, soundLenght * 4);
                Console.Beep(1188, soundLenght * 2);
                Console.Beep(1056, soundLenght * 2);
                Console.Beep(990, soundLenght * 6);
                Console.Beep(1056, soundLenght * 2);
                Console.Beep(1188, soundLenght * 4);
                Console.Beep(1320, soundLenght * 4);
                Console.Beep(1056, soundLenght * 4);
                Console.Beep(880, soundLenght * 4);
                Console.Beep(880, soundLenght * 4);
                Thread.Sleep(soundLenght * 2);
                Console.Beep(1188, soundLenght * 4);
                Console.Beep(1408, soundLenght * 2);
                Console.Beep(1760, soundLenght * 4);
                Console.Beep(1584, soundLenght * 2);
                Console.Beep(1408, soundLenght * 2);
                Console.Beep(1320, soundLenght * 6);
                Console.Beep(1056, soundLenght * 2);
                Console.Beep(1320, soundLenght * 4);
                Console.Beep(1188, soundLenght * 2);
                Console.Beep(1056, soundLenght * 2);
                Console.Beep(990, soundLenght * 4);
                Console.Beep(990, soundLenght * 2);
                Console.Beep(1056, soundLenght * 2);
                Console.Beep(1188, soundLenght * 4);
                Console.Beep(1320, soundLenght * 4);
                Console.Beep(1056, soundLenght * 4);
                Console.Beep(880, soundLenght * 4);
                Console.Beep(880, soundLenght * 4);
                Thread.Sleep(soundLenght * 4);
                Console.Beep(1320, soundLenght * 4);
                Console.Beep(990, soundLenght * 2);
                Console.Beep(1056, soundLenght * 2);
                Console.Beep(1188, soundLenght * 2);
                Console.Beep(1320, soundLenght);
                Console.Beep(1188, soundLenght);
                Console.Beep(1056, soundLenght * 2);
                Console.Beep(990, soundLenght * 2);
                Console.Beep(880, soundLenght * 4);
                Console.Beep(880, soundLenght * 2);
                Console.Beep(1056, soundLenght * 2);
                Console.Beep(1320, soundLenght * 4);
                Console.Beep(1188, soundLenght * 2);
                Console.Beep(1056, soundLenght * 2);
                Console.Beep(990, soundLenght * 6);
                Console.Beep(1056, soundLenght * 2);
                Console.Beep(1188, soundLenght * 4);
                Console.Beep(1320, soundLenght * 4);
                Console.Beep(1056, soundLenght * 4);
                Console.Beep(880, soundLenght * 4);
                Console.Beep(880, soundLenght * 4);
                Thread.Sleep(soundLenght * 2);
                Console.Beep(1188, soundLenght * 4);
                Console.Beep(1408, soundLenght * 2);
                Console.Beep(1760, soundLenght * 4);
                Console.Beep(1584, soundLenght * 2);
                Console.Beep(1408, soundLenght * 2);
                Console.Beep(1320, soundLenght * 6);
                Console.Beep(1056, soundLenght * 2);
                Console.Beep(1320, soundLenght * 4);
                Console.Beep(1188, soundLenght * 2);
                Console.Beep(1056, soundLenght * 2);
                Console.Beep(990, soundLenght * 4);
                Console.Beep(990, soundLenght * 2);
                Console.Beep(1056, soundLenght * 2);
                Console.Beep(1188, soundLenght * 4);
                Console.Beep(1320, soundLenght * 4);
                Console.Beep(1056, soundLenght * 4);
                Console.Beep(880, soundLenght * 4);
                Console.Beep(880, soundLenght * 4);
                Thread.Sleep(soundLenght * 4);
                Console.Beep(660, soundLenght * 8);
                Console.Beep(528, soundLenght * 8);
                Console.Beep(594, soundLenght * 8);
                Console.Beep(495, soundLenght * 8);
                Console.Beep(528, soundLenght * 8);
                Console.Beep(440, soundLenght * 8);
                Console.Beep(419, soundLenght * 8);
                Console.Beep(495, soundLenght * 8);
                Console.Beep(660, soundLenght * 8);
                Console.Beep(528, soundLenght * 8);
                Console.Beep(594, soundLenght * 8);
                Console.Beep(495, soundLenght * 8);
                Console.Beep(528, soundLenght * 4);
                Console.Beep(660, soundLenght * 4);
                Console.Beep(880, soundLenght * 8);
                Console.Beep(838, soundLenght * 16);
                Console.Beep(660, soundLenght * 8);
                Console.Beep(528, soundLenght * 8);
                Console.Beep(594, soundLenght * 8);
                Console.Beep(495, soundLenght * 8);
                Console.Beep(528, soundLenght * 8);
                Console.Beep(440, soundLenght * 8);
                Console.Beep(419, soundLenght * 8);
                Console.Beep(495, soundLenght * 8);
                Console.Beep(660, soundLenght * 8);
                Console.Beep(528, soundLenght * 8);
                Console.Beep(594, soundLenght * 8);
                Console.Beep(495, soundLenght * 8);
                Console.Beep(528, soundLenght * 4);
                Console.Beep(660, soundLenght * 4);
                Console.Beep(880, soundLenght * 8);
                Console.Beep(838, soundLenght * 16);
            }
        }
    }
}
