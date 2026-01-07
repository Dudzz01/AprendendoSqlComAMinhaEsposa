using UnityEngine;
using System.IO;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Networking;
#endif

public static class DatabaseUtils
{
    public static string GetDatabasePath(string filename)
    {
        
        string dst = Path.Combine(Application.persistentDataPath, filename);

        if (!File.Exists(dst))
        {
            
            string src = Path.Combine(Application.streamingAssetsPath, filename);
            byte[] data;

#if UNITY_ANDROID && !UNITY_EDITOR
            // Android usa UnityWebRequest
            var www = UnityWebRequest.Get(src);
            www.SendWebRequest();
            while (!www.isDone) {}
            data = www.downloadHandler.data;
#else
            
            using (var fs = new FileStream(
                src,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite   
            ))
            {
                data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
            }
#endif

            
            File.WriteAllBytes(dst, data);
        }

        return dst;
    }
}
