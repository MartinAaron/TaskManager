using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaskSManage
{
    public class Program
    {
        private static ConcurrentDictionary<int, Tuple<Task, CancellationTokenSource>> keyValuePairs = new ConcurrentDictionary<int, Tuple<Task, CancellationTokenSource>>();
        private static List<Task> list = new List<Task>();
        public async static Task Main(string[] args)
        {
            await Task.Factory.StartNew(StartNewTaskOfThree, creationOptions: TaskCreationOptions.LongRunning);

            var isStop = true;
            while (isStop)
            {
                var command = Console.ReadLine();
                if (int.TryParse(command, out int currentId))
                {
                    await CancelTask(currentId);
                }
                else
                {
                    await CancelAllTask();
                }
            }
        }

        public async static Task StartNewTaskOfThree()
        {
            for (int i = 0; i < 3; i++)
            {
                var source = new CancellationTokenSource();
                var task = Task.Factory.StartNew(async state =>
               await Run(source.Token), source.Token, creationOptions: TaskCreationOptions.LongRunning);

                keyValuePairs.TryAdd(i, new Tuple<Task, CancellationTokenSource>(task, source));
                list.Add(task);
            }

            await Task.CompletedTask;
        }

        public static async Task Run(CancellationToken cancellation)
        {
            while (!cancellation.IsCancellationRequested)
            {
                Console.WriteLine($"Child Task {Task.CurrentId} is Running");

                Thread.Sleep(TimeSpan.FromSeconds(3));
            }
            await Task.CompletedTask;
        }

        public static async Task CancelTask(int currentId)
        {
            var source = keyValuePairs[currentId].Item2;
            source.Cancel();

            await Task.CompletedTask;
        }

        public static async Task CancelAllTask()
        {
            foreach (var item in keyValuePairs)
            {
                item.Value.Item2.Cancel();
            }
            Console.WriteLine("Cancel All Tasks Complete");
            await Task.CompletedTask;
            Console.WriteLine($"total open Tasks：{list.Count}");
        }
    }
}
