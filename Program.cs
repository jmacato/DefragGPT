using System.Diagnostics;


internal class Program
{
    private const int Rows = 16;
    private const int Cols = 76;
    private static readonly char[][] _map = Enumerable.Repeat(0, Rows).Select(x => new char[Cols]).ToArray();

    //Store
    private static string _bar = "Reading...";
    private static int _blockClusterSize = 41;
    private static int _fragmentCount;
    private static int _defragged;
    private static int _writeLoc;
    private static bool _paused;
    private static readonly List<(int r, int c)> defraggedCoords = new();
    private static Stopwatch _stopwatch = new Stopwatch();

    private static void Main(string[] args)
    {
        _stopwatch.Start();

        for (var i = 0; i < Rows; i++)
        for (var j = 0; j < Cols; j++)
            if (1 == Random(0, Random(1, 15)))
            {
                if (!_paused) IncrementFragments();

                _map[i][j] = '■';
            }
            else if (3 == Random(0, Random(50, 200)))
            {
                _map[i][j] = 'X';
            }
            else if (2 == Random(0, Random(15, 1000)))
            {
                _map[i][j] = 'B';
            }
            else
            {
                _map[i][j] = '▓';
            }

        _map[0][0] = 'X';

        ;

        int DeducePos((int, int) coord)
        {
            var (r, c) = coord;
            return 76 * r + c;
        }

        ;

        void WriteNewBlock()
        {
            SetBar("Writing...");
            IncrementWriteloc();
            if (_writeLoc >= 1216)
            {
                Console.WriteLine("WriteLoc wrapped");
                ResetWriteLoc();
            }

            var (r, c) = DeduceRc(_writeLoc);
            switch (_map[r][c])
            {
                case '▓':
                    // Console.BackgroundColor = ConsoleColor.Yellow;
                    _map[r][c] = '■';
                    defraggedCoords.Add((r, c));

                    break;
                case '■':
                    IncrementDefragged();
                    // Console.BackgroundColor = ConsoleColor.Yellow;
                    defraggedCoords.Add((r, c));

                    _map[r][c] = '■';
                    WriteNewBlock();
                    break;
                default:
                    // Console.BackgroundColor = ConsoleColor.Black;
                    WriteNewBlock();
                    break;
            }
        }


        var missLog = new int[0];

        void ClearMissLog()
        {
            for (var i = 0; i < missLog.Length; i++)
            {
                var (r, c) = DeduceRc(missLog[i]);
                _map[r][c] = '▓';
            }

            missLog = new int[0];
        }

        while (true)
        {
            if (!_paused)
            {
                var (r, c) = DeduceRc(_writeLoc);
                var s = Random(r, 15);
                var t = Random(0, 75);
                if (!(DeducePos((s, t)) <= _writeLoc + 5))
                {
                    switch (_map[s][t])
                    {
                        case '▓':
                            Array.Resize(ref missLog, missLog.Length + 1);
                            missLog[^1] = DeducePos((s, t));
                            _map[s][t] = 'r';
                            SetBar("Reading...");
                            break;
                        case '■':
                            IncrementDefragged();
                            _map[s][t] = 'r';
                            Thread.Sleep(250);
                            WriteNewBlock();
                            _map[s][t] = '▓';
                            if (missLog.Length > 0)
                            {
                                WriteNewBlock();
                                ClearMissLog();
                            }

                            break;
                    }

                    if (missLog.Length >= 5)
                    {
                        WriteNewBlock();
                        ClearMissLog();
                    }
                }
            }

            Thread.Sleep(100);
            Console.Clear();

            var diskMapStr = "";
            for (var i = 0; i < Rows; i++)
            {
                for (var j = 0; j < Cols; j++)
                {
                    var cur = _map[i][j];

                    if (defraggedCoords.Any(x => x.r == i && x.c == j))
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    else Console.ForegroundColor = (ConsoleColor)(-1);

                    Console.Write(cur);
                }

                Console.WriteLine();
            }

            Console.SetCursorPosition(0, 0);
            var progressBarCount = (int)(Math.Floor(_defragged / (decimal)_fragmentCount) * 34);
            var progBar = new string('█', progressBarCount) + new string('▒', 34 - progressBarCount);


            var legend = $@"
┌────────────── Status ──────────────┐ ┌────────────── Legend ──────────────┐
│ Cluster {_writeLoc.ToString(),-6}               {_defragged / _fragmentCount,5:P0} │ │ ■ - Used          ▓ - Unused       │
│ {progBar} │ │ r - Reading       W - Writing      │
│        Elapsed Time:   {_stopwatch.Elapsed.Hours}:{_stopwatch.Elapsed.Minutes:00}        │ │ B - Bad           X - Unmovable    │
│         Full Optimization          │ │ Drive C:    1 block = {_blockClusterSize} clusters  │
└────────────────────────────────────┘ └────────────────────────────────────┘";


            Console.SetCursorPosition(10, 15);
            Console.WriteLine(legend);
            Console.WriteLine(_bar);
        }

        void SetBar(string s)
        {
            _bar = s;
        }

        void IncrementWriteloc()
        {
            _writeLoc++;
        }

        void ResetWriteLoc()
        {
            _writeLoc = 0;
        }

        void IncrementFragments()
        {
            _fragmentCount++;
        }

        void IncrementDefragged()
        {
            _defragged++;
        }

        void PauseDefrag()
        {
            _paused = true;
        }

        void UnpauseDefrag()
        {
            _paused = false;
        }

        int Random(int min, int max)
        {
            return System.Random.Shared.Next(min, max);
        }
    }

    private static (int r, int c) DeduceRc(int num)
    {
        var r = (int)Math.Floor(num / 76d);
        var c = num % 76;
        return (r, c);
    }
}