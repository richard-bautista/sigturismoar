// UIManager.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using Siccity.GLTFUtility;

public class UIManager : MonoBehaviour
{
    public GameObject buttonPrefab; // Referencia al prefab del botón

    public GameObject botonCerrar;
    public Transform contentPanel;  // Referencia al panel donde se colocarán los botones
    private string markerName = "";

    private bool botonesVisibles = true;

    private List<GameObject> instantiatedButtons = new List<GameObject>();



    public IEnumerator SetImageFromUrl(string url, Image image)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error al descargar la imagen: " + request.error);
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }

    public void DisplayLugares(List<Lugar> lugares)
    {
        // Desinstancia los botones creados anteriormente



        foreach (var lugar in lugares)
        {
            if (!string.IsNullOrEmpty(lugar.imagen) && !string.IsNullOrEmpty(lugar.contenido))
            {
                GameObject newButton = Instantiate(buttonPrefab, contentPanel);
                instantiatedButtons.Add(newButton);  // Agrega el nuevo botón a la lista
                Image imageComponent = newButton.GetComponentInChildren<Image>();
                TextMeshProUGUI textMesh = newButton.GetComponentInChildren<TextMeshProUGUI>();

                if (textMesh != null && imageComponent != null)
                {
                    textMesh.text = lugar.nombre;
                    StartCoroutine(SetImageFromUrl(lugar.url_imagen_portada, imageComponent));

                    // Agrega un listener que pasa las URLs al método que maneja el cambio de escena
                    newButton.GetComponent<Button>().onClick.AddListener(() => OnLugarSelected(lugar.imagen, lugar.contenido));
                }
            }
        }


    }

    public void CerrarMarker()
    {
        VoidAR.GetInstance().removeTarget(markerName);
        foreach (var button in instantiatedButtons)
        {
            button.SetActive(true);
            botonCerrar.SetActive(false);
        }
    }

    private void OnLugarSelected(string urlMarker, string urlModel)
    {
        Debug.Log("Marcador seleccionado: " + urlMarker);
        Debug.Log("Modelo seleccionado: " + urlModel);

        // Buscar y desinstanciar el objeto ImageTarget1 si ya existe
        ImageTargetBehaviour existingMarker = GameObject.FindObjectOfType<ImageTargetBehaviour>();
        if (existingMarker != null)
        {
            Debug.Log("Desinstanciando el marcador existente.");
            RemoveExistingFiles();
            Destroy(existingMarker.gameObject);

        }

        StartCoroutine(DownloadAndSaveMarker(urlMarker, "marker.jpg", (markerPath) =>
        {
            StartCoroutine(DownloadAndSaveModel(urlModel, "model.glb", (modelPath) =>
            {

                foreach (var button in instantiatedButtons)
                {
                    button.SetActive(false);
                }
                botonCerrar.SetActive(true);
                var imageTarget1 = new GameObject("ImageTarget1");
                imageTarget1.transform.localPosition = Vector3.zero;
                imageTarget1.transform.localEulerAngles = Vector3.zero;
                imageTarget1.transform.localScale = Vector3.one;
                ImageTargetBehaviour marker1 = imageTarget1.AddComponent<ImageTargetBehaviour>();
                if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    // Configuración específica para Android o iOS
                    marker1.storageType = VoidAR.StorageType.Absolute;  // Usar la ruta completa en dispositivos móviles
                    markerName = markerPath;  // Usar el path del marcador descargado
                }
                else
                {
                    // Configuración específica para PC
                    marker1.storageType = VoidAR.StorageType.Absolute;
                    markerName = markerPath;  // Usar el path del marcador descargado
                }

                marker1.SetPath(markerName);

                /*var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Material mat = (Material)Resources.Load("dynamicMaterial");
                cube.GetComponent<Renderer>().material = mat;
                cube.transform.localPosition = new Vector3(0.0f, 0.25f, 0.0f);
                cube.transform.localEulerAngles = Vector3.zero;
                cube.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                cube.transform.SetParent(imageTarget1.transform);*/
                GameObject model = Importer.LoadFromFile(modelPath);
                model.transform.SetParent(imageTarget1.transform); // Establecer el modelo como hijo de ImageTarget1

                model.transform.localRotation = Quaternion.identity;

                model.transform.localPosition = new Vector3(0.0f, 0.25f, 0.0f);
                //model.transform.localEulerAngles = Vector3.zero;

                //model.transform.localScale = Vector3.one;
                model.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

                VoidAR.GetInstance().addTargetNew(marker1, imageTarget1);
                VoidAR.GetInstance().FinishAddImageTarget();

            }));
        }));


    }

    private IEnumerator DownloadAndSaveMarker(string url, string fileName, Action<string> callback)
    {
        // Agregar un parámetro de consulta único para evitar la caché
        string urlWithQuery = url + "?" + Guid.NewGuid().ToString();

        UnityWebRequest www = UnityWebRequest.Get(urlWithQuery);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al descargar el marcador: " + www.error);
            yield break;
        }

        // Guardar el archivo en la carpeta StreamingAssets
        string savePath = Path.Combine(Application.persistentDataPath, "marker.jpg").Replace('\\', '/');
        File.WriteAllBytes(savePath, www.downloadHandler.data);

        callback?.Invoke(savePath);
    }

    private IEnumerator DownloadAndSaveModel(string url, string fileName, Action<string> callback)
    {
        // Agregar un parámetro de consulta único para evitar la caché
        string urlWithQuery = url + "?" + Guid.NewGuid().ToString();

        UnityWebRequest www = UnityWebRequest.Get(urlWithQuery);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al descargar el modelo: " + www.error);
            yield break;
        }

        // Guardar el archivo en la carpeta StreamingAssets
        string savePath = Path.Combine(Application.persistentDataPath, "model.glb").Replace('\\', '/');
        File.WriteAllBytes(savePath, www.downloadHandler.data);

        callback?.Invoke(savePath);
    }

    private void RemoveExistingFiles()
    {
        // Archivo marker.jpg
        string markerPath = Path.Combine(Application.persistentDataPath, "marker.jpg").Replace('\\', '/');
        if (File.Exists(markerPath))
        {
            File.Delete(markerPath);
            Debug.Log("Se eliminó el archivo existente: marker.jpg");
        }

        // Archivo model.glb
        string modelPath = Path.Combine(Application.persistentDataPath, "model.glb").Replace('\\', '/');
        if (File.Exists(modelPath))
        {
            File.Delete(modelPath);
            Debug.Log("Se eliminó el archivo existente: model.glb");
        }
    }




}
