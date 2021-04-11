using System.Threading.Tasks;

namespace IceColdMirror {
    public static class Helpers {
        public static async Task<T> AwaitWithTimeout<T>(this Task<T> task, int timeoutMs) {
            await Task.WhenAny(task, Task.Delay(timeoutMs));

            if (!task.IsCompleted)
                throw new System.Exception("Task timedout");

            return await task;
        }
    }
}
