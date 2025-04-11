using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : MonoBehaviour
{
    [Serializable]
    public class UserData
    {
        public string username;
        public string email;
        public string pass; // Mantén esto como "pass"
    }

    [Serializable]
    public class LoginData
    {
        public string username;
        public string pass; // Usa "pass" igual que el backend
    }

    [Serializable]
    public class Response
    {
        public bool done;
        public string message;
        public int userId;
    }

    [Serializable]
    public class SessionData
    {
        public int userId;
        public string startTime;
        public string endTime;
    }

    // Registro de usuario
    public void CreateUser(string userName, string email, string pass, Action<Response> callback)
    {
        UserData data = new UserData
        {
            username = userName,
            email = email,
            pass = pass
        };

        string json = JsonUtility.ToJson(data);
        StartCoroutine(SendRequest("http://3.226.245.135:3000/createUser", json, callback));
    }

    // Login de usuario
    public void LoginUser(string username, string pass, Action<Response> callback)
    {
        LoginData data = new LoginData
        {
            username = username,
            pass = pass // No encriptado
        };

        string json = JsonUtility.ToJson(data);
        StartCoroutine(SendRequest("http://3.226.245.135:3000/loginUser", json, callback));
    }

    // Envío HTTP genérico
    private IEnumerator SendRequest(string url, string json, Action<Response> callback)
    {
        using UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error en la petición: " + request.error);
            yield break;
        }

        string responseText = request.downloadHandler.text;
        Debug.Log("Respuesta del servidor: " + responseText);

        if (string.IsNullOrEmpty(responseText) || !responseText.Trim().StartsWith("{"))
        {
            Debug.LogError("La respuesta no es JSON válido");
            yield break;
        }

        Response parsed = JsonUtility.FromJson<Response>(responseText);
        callback?.Invoke(parsed);
    }

    // Guardar sesión
    public void SaveSession(int userId, string startTime, string endTime, Action<Response> callback)
    {
        string formattedStartTime = DateTime.Parse(startTime).ToString("yyyy-MM-dd HH:mm:ss");
        string formattedEndTime = DateTime.Parse(endTime).ToString("yyyy-MM-dd HH:mm:ss");

        Debug.Log("Start Time: " + formattedStartTime);
        Debug.Log("End Time: " + formattedEndTime);

        SessionData session = new SessionData
        {
            userId = userId,
            startTime = formattedStartTime,
            endTime = formattedEndTime
        };

        string json = JsonUtility.ToJson(session);
        StartCoroutine(SendRequest("http://3.226.245.135:3000/saveSession", json, callback));
    }
}
