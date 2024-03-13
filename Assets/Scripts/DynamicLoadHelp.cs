using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using Piglet;

public class DynamicLoadHelp : MonoBehaviour
{
    public GameObject target;
    public ImageTargetBehaviour marker1;
    private GltfImportTask _task;

    void Awake()
    {
        // Crear y configurar el marcador
        target = new GameObject("target");
        target.transform.localPosition = Vector3.zero;
        target.transform.localEulerAngles = Vector3.zero;
        target.transform.localScale = Vector3.one;
        marker1 = target.AddComponent<ImageTargetBehaviour>();
    }

    void Start()
    {
        // Descargar marcador
        string markerURL = TargetData.instance.urlMarker;
        StartCoroutine(DownloadAndRename(markerURL, "marker.jpg",
            onSuccess: () =>
            {
                string markerPath = GetMarkerPath();
                if (!string.IsNullOrEmpty(markerPath))
                {
                    SetupMarker(markerPath);

                    // Cargar la textura del marcador
                    byte[] bytes = File.ReadAllBytes(markerPath);
                    Texture2D markerTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, true);
                    markerTexture.LoadImage(bytes);

                    // Configura el marcador con la textura cargada
                    // Aquí debes implementar la lógica para asignar la textura al marcador
                }
                else
                {
                    Debug.LogError("No se pudo obtener la ruta del marcador.");
                }
            },
            onFailure: (error) =>
            {
                Debug.LogError("No se pudo descargar el marcador: " + error);
            }));

        // Descargar modelo 3D
        string modelURL = TargetData.instance.urlModel;
        StartCoroutine(DownloadAndRename(modelURL, "model.glb",
            onSuccess: () =>
            {
                string modelPath = Path.Combine(Application.persistentDataPath, "model.glb");
                StartCoroutine(LoadModel(modelPath));
            },
            onFailure: (error) =>
            {
                Debug.LogError("No se pudo descargar el modelo: " + error);
            }));
    }

    string GetMarkerPath()
    {
        string markerPath = "";

        // Determinar la plataforma actual
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            markerPath = Path.Combine(Application.persistentDataPath, "marker.jpg");
        }
        else
        {
            // En Windows, utiliza "StreamingAssets"
            markerPath = Path.Combine(Application.streamingAssetsPath, "marker.jpg");

            // Reemplaza la barra invertida por la barra normal
            markerPath = markerPath.Replace("\\", "/");
        }

        return markerPath;
    }

    void SetupMarker(string filePath)
    {
        marker1.storageType = VoidAR.StorageType.Absolute;
        marker1.SetPath(filePath);
    }

    IEnumerator LoadModel(string localPath)
    {
        var importOptions = new GltfImportOptions();
        importOptions.AutoScale = true;
        importOptions.AutoScaleSize = 1.0f;

        _task = RuntimeGltfImporter.GetImportTask(localPath,importOptions);
        _task.OnCompleted = OnComplete;

        yield return null;
    }
     private void OnComplete(GameObject importedModel)
    {
        var anim = importedModel.GetComponent<Animation>();
        var animList = importedModel.GetComponent<AnimationList>();

        var clipKey = animList.Clips[1].name;
   
        
        anim.Play(clipKey);

        Debug.Log("Success!");
    }

    IEnumerator DownloadAndRename(string url, string newName, Action onSuccess, Action<string> onFailure)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onFailure?.Invoke(request.error);
        }
        else
        {
            try
            {
                // Guardar el archivo con el nuevo nombre
                string filePath = Path.Combine(Application.persistentDataPath, newName);
                File.WriteAllBytes(filePath, request.downloadHandler.data);
                onSuccess?.Invoke();
            }
            catch (Exception e)
            {
                onFailure?.Invoke(e.Message);
            }
        }
    }
}
