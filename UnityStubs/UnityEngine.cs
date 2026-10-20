// Minimal UnityEngine fakes: only what Sage.Game/Mod compile against.
// Signatures mirror the real Unity API; bodies are never run (CI compile only).
using System;

namespace UnityEngine
{
    public struct Vector3
    {
        public float x;
        public float y;
        public float z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static readonly Vector3 zero = new Vector3(0, 0, 0);
        public static float Distance(Vector3 a, Vector3 b) => 0f;
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public struct Vector2
    {
        public float x;
        public float y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
    }

    public enum KeyCode
    {
        None,
        F7,
        LeftControl,
        RightControl,
        LeftShift,
        RightShift,
        LeftAlt,
        RightAlt,
    }

    public static class Input
    {
        public static bool GetKey(KeyCode key) => false;
        public static bool GetKeyDown(KeyCode key) => false;
    }

    public class Object
    {
        public string name { get; set; }
        public static T[] FindObjectsOfType<T>() where T : Object => Array.Empty<T>();
        public static void DontDestroyOnLoad(Object target) { }
        public static void Destroy(Object obj) { }
    }

    public class Component : Object
    {
        public GameObject gameObject => new GameObject("stub");
        public Transform transform => new Transform();
        public T GetComponent<T>() where T : Component => null;
        public T GetComponentInParent<T>() where T : Component => null;
    }

    public class MonoBehaviour : Component { }

    public class GameObject : Object
    {
        public GameObject(string name) { this.name = name; }
        public Transform transform => new Transform();
        public T AddComponent<T>() where T : Component, new() => new T();
        public T GetComponent<T>() where T : Component => null;
        public T GetComponentInParent<T>() where T : Component => null;
    }

    public class Transform : Component
    {
        public Vector3 position { get; set; }
    }

    public class Collider : Component { }

    public static class Physics
    {
        public static Collider[] OverlapSphere(Vector3 position, float radius) => Array.Empty<Collider>();
    }

    public static class Debug
    {
        public static void Log(object message) { }
    }
}

namespace UnityEngine.Networking
{
    public class UploadHandlerRaw : IDisposable
    {
        public UploadHandlerRaw(byte[] data) { }
        public void Dispose() { }
    }

    public class DownloadHandlerBuffer : IDisposable
    {
        public string text => string.Empty;
        public void Dispose() { }
    }

    public class UnityWebRequest : IDisposable
    {
        public const string kHttpVerbPOST = "POST";
        public enum Result { Success, ConnectionError, ProtocolError, DataProcessingError }

        public UnityWebRequest(string url, string method) { }
        public UploadHandlerRaw uploadHandler { get; set; }
        public DownloadHandlerBuffer downloadHandler { get; set; }
        public int timeout { get; set; }
        public Result result => Result.Success;
        public string error => string.Empty;
        public long responseCode => 200;

        public void SetRequestHeader(string name, string value) { }
        public AsyncOperation SendWebRequest() => new AsyncOperation();
        public void Dispose() { }
    }

    public class AsyncOperation : UnityEngine.Object
    {
        public event Action<AsyncOperation> completed;
    }
}
