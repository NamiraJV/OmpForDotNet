namespace OmpForDotNet.Utility.Settings
{
    public static class OpenMPConstants
    {
        public static int OMP_NUM_THREADS { get; } = 4;

        //directive parameters
        public static readonly string NUM_THREADS_OPTION = "num_threads";
        public static readonly string FIRST_PRIVATE_OPTION = "firstprivate";
        public static readonly string SCHEDULE_OPTION = "schedule";
        public static readonly string CRITICAL_DIRECTIVE = "critical";

        // types that for loop variable can have
        public static string[] ForLoopVariableTypes { get; } = new string[] { "int", "double", "float" };

        // counters for generated variables to avoid duplicate variables in a project
        public static int GeneratedVariablesCount { get; set; } = 0;
        public static int GeneratedTaskVariablesCount { get; set; } = 0;
        public static int GeneratedTaskListVariablesCount { get; set; } = 0;
    }
}
