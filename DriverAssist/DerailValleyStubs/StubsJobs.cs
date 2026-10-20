// Minimal DV.Logic.Job fakes: only what DriverAssist compiles against.
using System.Collections.Generic;
using UnityEngine;

namespace DV.Logic.Job
{
    public class JobsManager : MonoBehaviour
    {
        public List<Job> currentJobs = new List<Job>();

        public void RegisterGeneratedJob() { }
        public void UnregisterJob() { }
        public void TakeJob() { }
        public void AbandonJob() { }
        public void CompleteTheJob() { }
    }

    public class Job
    {
        public string ID;
        public string jobType;
        public JobChainData chainData = new JobChainData();
        public List<Task> tasks = new List<Task>();
    }

    public class JobChainData
    {
        public string chainOriginYardId;
        public string chainDestinationYardId;
    }

    public class Task
    {
        public int state;
        public TaskType InstanceTaskType;

        public TaskData GetTaskData() => new TaskData();
    }

    public class TaskData
    {
        public List<Task> nestedTasks = new List<Task>();
        public Track startTrack;
        public Track destinationTrack;
        public TaskState state;
    }

    public class Track
    {
        public TrackID ID = new TrackID();
    }

    public class TrackID
    {
        public string FullDisplayID;
    }

    public enum TaskType
    {
        Transport,
        Warehouse,
        Sequential,
        Parallel,
    }

    public enum TaskState
    {
        Done,
    }
}
