using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace HenryLab
{
# if UNITY_EDITOR
    public class AssetCounter
    {
        [MenuItem("Tools/VR Explorer/Count Project Files %#&c")] // Shift + Ctrl + Alt + C
        public static void CountAllAssets()
        {
            CountActiveGameObjectsInScene();
            string assetsPath = Application.dataPath; // ��ȡAssetsĿ¼����·��
            int totalFiles = CountFilesInDirectory(assetsPath, includeMeta: true);
            Debug.Log($"Total files in Assets (including .meta): {totalFiles}");

            int scriptFiles = CountFilesByExtension(assetsPath, ".cs");
            Debug.Log($"Total C# scripts: {scriptFiles}");

            int prefabFiles = CountFilesByExtension(assetsPath, ".prefab");
            Debug.Log($"Total prefabs: {prefabFiles}");
        }

        public static void CountActiveGameObjectsInScene()
        {
            GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            int activeCount = 0;
            int totalCount = 0;

            foreach(GameObject root in rootObjects)
            {
                Transform[] allTransforms = root.GetComponentsInChildren<Transform>(true);
                totalCount += allTransforms.Length;

                foreach(Transform t in allTransforms)
                {
                    if(t.gameObject.activeInHierarchy) // Check if GameObject is active
                    {
                        activeCount++;
                    }
                }
            }

            Debug.Log($"Scene GameObject Report:\n" +
                      $"Active GameObjects: {activeCount}\n" +
                      $"Inactive GameObjects: {totalCount - activeCount}\n" +
                      $"Total GameObjects: {totalCount}\n" +
                      $"Root Objects: {rootObjects.Length}");
        }

        /// <summary>
        /// �ݹ�ͳ��Ŀ¼�������ļ�����
        /// </summary>
        private static int CountFilesInDirectory(string path, bool includeMeta = false)
        {
            if(!Directory.Exists(path))
            {
                Debug.LogError($"Directory not found: {path}");
                return 0;
            }

            int count = 0;

            // ͳ�Ƶ�ǰĿ¼�ļ�
            foreach(var file in Directory.GetFiles(path))
            {
                if(includeMeta || !file.EndsWith(".meta"))
                {
                    count++;
                }
            }

            // �ݹ�ͳ����Ŀ¼
            foreach(var dir in Directory.GetDirectories(path))
            {
                count += CountFilesInDirectory(dir, includeMeta);
            }

            return count;
        }

        /// <summary>
        /// ����չ��ͳ���ļ������������ִ�Сд��
        /// </summary>
        private static int CountFilesByExtension(string rootPath, string extension)
        {
            if(!extension.StartsWith("."))
                extension = "." + extension;

            var allFiles = Directory.GetFiles(rootPath, "*" + extension, SearchOption.AllDirectories)
                                  .Where(file => !file.EndsWith(".meta")); // �ų�.meta�ļ�

            return allFiles.Count();
        }
    }
#endif
}