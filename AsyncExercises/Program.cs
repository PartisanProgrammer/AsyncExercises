using System.Diagnostics;
using DoNotEdit_1;
using Exercise_1;

/*
 * In this Exercise, we're simulating the following case:
 * We have built GTA VI.
 * The Game spawns 10 AI Pedestrians, which all spawn in random locations and need to calculate some
 * smooth Path that makes them act naturally in their environment.
 * This calculation unfortunately is Performance-Heavy (it takes 2 seconds)
 * The player is not okay with the Game freezing for 20 or even 2 seconds.
 * That means, we need to implement the Path Request asynchronously, to allow it to run in a separate thread and run in parallel.
 * If we implement this nicely, it will mean that the AI won't move for two seconds at first (which we can cover up by spawning them far away), but other than that, the player won't even notice, that these expensive calculations had to happen.
 *
 * The knowledge learned here applies to any game that you make, where you need to call an expensive method and don't want your framerate to drop.
 */

namespace Exercise_1
{
    public class Unit
    {
        public IPath _path;
        bool IsNotCurrentlyCalculating = true;
        public async void Update(){
            if (_path == null && IsNotCurrentlyCalculating){
                IsNotCurrentlyCalculating = false;
                _path = await Task.Run(() => Program.CalculatePath());
                _path.Run();
                IsNotCurrentlyCalculating = true;
            }

            _path?.Run();
            // if there is no _path
            // calculate one using Program.CalculatePath()
            // if there is a path, call Run on it (every Update)
        }
        
        async Task<IPath> CalculatePathAsync(){
            var path = Task.Run(Program.CalculatePath);
            
            return await path;
        }
    }
}



// DO NOT EDIT ANY OF THE CODE DOWN HERE!
namespace DoNotEdit_1
{
    using System.Diagnostics;
    public interface IPath
    {
        void Run();
    }

    public static class Program
    {
        private static volatile int pathsHaveBeenRequested;
        private static volatile int pathsHaveBeenRunSixtyTimes;
        private class Path : IPath
        {
            private int runCount = 0;
            public void Run()
            {
                runCount++;
                if (runCount != 60)
                    return;
                Interlocked.Increment(ref pathsHaveBeenRunSixtyTimes);
            }
        }
        public static IPath CalculatePath()
        {
            Interlocked.Increment(ref pathsHaveBeenRequested);
            Thread.Sleep(2000);
            return new Path();
        }
        static void sain()
        {
            int frameCount = 0;
            var stopWatch = Stopwatch.StartNew();
            var units = Enumerable.Range(0, 10).Select(i => new Unit()).ToArray();
            while (pathsHaveBeenRunSixtyTimes < units.Length)
            {
                Thread.Sleep(1000/60); // 60 fps
                foreach (var unit in units)
                {
                    unit.Update();
                }
                frameCount++;
            }
            stopWatch.Stop();
            
            Console.WriteLine($"It took the game {stopWatch.Elapsed} to calculate Paths on 10 Units and Run() them for 60 Frames. (Expected: ca. 3 seconds)");
            Console.WriteLine($"A total of {pathsHaveBeenRequested} Paths have been requested. (Expected: 10)");
            Console.WriteLine($"The game ran at ca. {frameCount/stopWatch.Elapsed.TotalSeconds} Frames per Second (Expected: 55+)");
        }
    }
}