using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class APIController : MonoBehaviour
{
    public string apiUrl = "https://sigturismo.up.railway.app/api/v1/lugares";
    public UIManager uiManager; // Referencia al UIManager

    void Start()
    {
        StartCoroutine(GetLugares(apiUrl));
    }

    IEnumerator GetLugares(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            // Reemplazar isNetworkError con la nueva forma de comprobar errores
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(webRequest.error);
            }
            else
            {
                ListaLugares lugares = JsonUtility.FromJson<ListaLugares>("{\"lugares\":" + webRequest.downloadHandler.text + "}");
                uiManager.DisplayLugares(lugares.lugares);
            }
        }
    }
}
