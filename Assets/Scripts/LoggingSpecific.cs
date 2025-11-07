using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CompasXR.Systems
{
    public class LogService : MonoBehaviour
    {
        public static LogService Instance { get; private set; }
        public static SimpleLogger Logger { get; private set; }

        [Header("File Settings")]
        [Tooltip("Directory name under Application.persistentDataPath")]
        public string folderName = "RoboticTerritoriesLoggingSpecific";

        [Tooltip("Include device ID in filename")]
        public bool includeDeviceId = true;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            var dir = System.IO.Path.Combine(Application.persistentDataPath, folderName);
            var stamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var device = includeDeviceId ? "_" + SystemInfo.deviceUniqueIdentifier : "";
            var file = $"{stamp}{device}_log.txt";

            Logger = new SimpleLogger(dir, file);
            Logger.Log("LogService initialized.");
        }

        // Optional: static convenience methods so you can call from anywhere.
        public static void Log(string msg)
        {
            EnsureRunning();
            Logger.Log(msg);
        }

        public static void LogDict(System.Collections.Generic.Dictionary<string, object> data, string title = "Dictionary")
        {
            EnsureRunning();
            Logger.Log(data, title);
        }

        public static void LogGO(GameObject go, string title = "GameObject")
        {
            EnsureRunning();
            Logger.Log(go, title);
        }

        private static void EnsureRunning()
        {
            if (Logger != null) return;

            // Auto-bootstrap if missing, so you can call LogService.Log(...) without
            // manually placing it in the scene.
            var go = new GameObject("[LogService]");
            go.AddComponent<LogService>();
        }

        void OnApplicationQuit()
        {
            if (Logger != null)
            {
                Logger.Log("Application quitting — closing log file.");
                Debug.Log($"[LogService] Log file saved at: {Logger.FilePath}");
            }
        }
    }


    public class SimpleLogger
    {
        private readonly string _filePath;
        public string FilePath => _filePath;

        public SimpleLogger(string directoryPath, string fileName)
        {
            Directory.CreateDirectory(directoryPath);
            _filePath = Path.Combine(directoryPath, fileName);
        }

        public void Log(string message)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            var ts = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            File.AppendAllText(_filePath, $"[{ts}] [{scene}] {message}{Environment.NewLine}");
        }

        public void Log(Dictionary<string, object> data, string title = "Dictionary")
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            var ts = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            using (var w = new StreamWriter(_filePath, true, Encoding.UTF8))
            {
                w.WriteLine($"[{ts}] [{scene}] {title}:");
                foreach (var kv in data)
                {
                    w.WriteLine($"  {kv.Key}: {SafeToString(kv.Value)}");
                }
                w.WriteLine();
            }
        }

        public void Log(GameObject go, string title = "GameObject")
        {
            if (go == null)
            {
                Log($"{title}: <null>");
                return;
            }

            var scene = go.scene.IsValid() ? go.scene.name : "(no scene)";
            var t = go.transform;

            // Basic GO info
            var info = new Dictionary<string, object>
            {
                ["Name"] = go.name,
                ["Tag"] = go.tag,
                ["ActiveSelf"] = go.activeSelf,
                ["ActiveInHierarchy"] = go.activeInHierarchy,
                ["Layer"] = go.layer,
                ["Scene"] = scene,
                ["Position"] = t.position,
                ["RotationEuler"] = t.rotation.eulerAngles,
                ["Scale"] = t.localScale,
                ["Path"] = GetTransformPath(t),
                ["Components"] = go.GetComponents<Component>().Select(c => c ? c.GetType().Name : "(null)").ToArray()
            };

            Log(info, title);
        }

        public void LogGameObjectList(List<GameObject> gameObjects, string title = "GameObject List")
        {
            if (gameObjects == null)
            {
                Log($"{title}: <null>");
                return;
            }

            for (int i = 0; i < gameObjects.Count; i++)
            {
                Log(gameObjects[i], $"GameObject Name : {gameObjects[i]} [{i}]");
            }
        }

        private static string GetTransformPath(Transform tr)
        {
            var stack = new Stack<string>();
            while (tr != null)
            {
                stack.Push(tr.name);
                tr = tr.parent;
            }
            return string.Join("/", stack);
        }

        private static string SafeToString(object obj)
        {
            if (obj == null) return "null";
            switch (obj)
            {
                case Vector3 v: return $"({v.x:F4}, {v.y:F4}, {v.z:F4})";
                case Quaternion q: return $"Quat({q.x:F4}, {q.y:F4}, {q.z:F4}, {q.w:F4})";
                case IEnumerable<object> e: return string.Join(", ", e.Select(SafeToString));
                default: return obj.ToString();
            }
        }
    }
}
