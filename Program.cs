using System.Diagnostics;

namespace DefragGPT;

class Program
{
    private static void Main(string[] args)
    {
        const int Rows = 16;
        const int Cols = 76;
        var _map = Enumerable.Repeat(0, Rows).Select(x => new char[Cols]).ToArray();

//Store
        var _bar = "Reading...";
        var _blockClusterSize = 41;
        var _fragmentCount = 1;
        var _defragged = 0;
        var _writeLoc = 0;
        var _paused = true;
        List<(int r, int c)> defraggedCoords = new();
        var _stopwatch = new Stopwatch();

        _stopwatch.Start();

        for (var i = 0; i < Rows; i++)
        for (var j = 0; j < Cols; j++)
            if (1 == Random(0, Random(1, 15)))
            {
                _fragmentCount++;

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

        int DeducePos((int, int) coord)
        {
            var (r, c) = coord;
            return 76 * r + c;
        }

        void WriteNewBlock()
        {
            _bar = "Writing...";
            _writeLoc += 1;
            if (_writeLoc >= 1216)
            {
                Console.WriteLine("WriteLoc wrapped");
                _writeLoc = 0;
            }

            var (r, c) = DeduceRc(_writeLoc);
            switch (_map[r][c])
            {
                case '▓':
                    _map[r][c] = '■';
                    defraggedCoords.Add((r, c));

                    break;
                case '■':
                    _defragged += 1;
                    defraggedCoords.Add((r, c));

                    _map[r][c] = '■';
                    WriteNewBlock();
                    break;
                default:
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
                            _bar = "Reading...";
                            break;
                        case '■':
                            _defragged += 1;
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

        (int r, int c) DeduceRc(int num)
        {
            var r = (int)Math.Floor(num / 76d);
            var c = num % 76;
            return (r, c);
        }
    }
}