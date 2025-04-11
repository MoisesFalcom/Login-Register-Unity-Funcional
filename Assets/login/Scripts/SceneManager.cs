using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System;

public class SceneManager : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private NetworkManager networkManager;

    private VisualElement loginUI;
    private VisualElement registerUI;

    private TextField userNameInput;
    private TextField emailInput;
    private TextField passwordInput;
    private TextField reenterPasswordInput;
    private Label messageLabel;

    private int currentUserId = 0;
    private string sessionStartTime = "";

    private void OnEnable()
    {
        var root = uiDocument.rootVisualElement;

        loginUI = root.Q<VisualElement>("Login");
        registerUI = root.Q<VisualElement>("Register");

        if (loginUI == null || registerUI == null)
        {
            Debug.LogError("❌ No se encontró 'Login' o 'Register' en el UI.");
            return;
        }

        // === REGISTRO ===
        userNameInput = registerUI.Q<TextField>("UserNameInput");
        emailInput = registerUI.Q<TextField>("EmailInput");
        var passwords = registerUI.Query<TextField>("Password").ToList();
        passwordInput = passwords[0];
        reenterPasswordInput = passwords[1];
        messageLabel = registerUI.Q<Label>("Text");

        var registerButton = registerUI.Q<Button>("RegisterButton");
        if (registerButton != null)
            registerButton.clicked += SubmitRegister;

        // === LOGIN ===
        var loginUsernameField = loginUI.Q<TextField>("LoginUser");
        var loginPasswordField = loginUI.Q<TextField>("LoginPass");
        var loginMessageLabel = loginUI.Q<Label>("Text");
        var loginButton = loginUI.Q<Button>("LoginButton");

        if (loginButton != null)
        {
            loginButton.clicked += () =>
            {
                string username = loginUsernameField.value.Trim();
                string password = loginPasswordField.value.Trim();

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    loginMessageLabel.text = "Por favor completa tus datos.";
                    return;
                }

                loginMessageLabel.text = "Procesando login...";

                networkManager.LoginUser(username, password, (response) =>
                {
                    loginMessageLabel.text = response.message;
                    Debug.Log($"🧠 Username: {username}, Password: {password}");
                    Debug.Log($"🪪 Response userId: {response.userId}");

                    if (response.done)
                    {
                        currentUserId = response.userId;
                        sessionStartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        Debug.Log($"✅ Usuario autenticado. ID: {currentUserId}, Inicio: {sessionStartTime}");
                    }
                });
            };
        }

        ShowLogin();
    }

    public void SubmitRegister()
    {
        if (string.IsNullOrEmpty(userNameInput.value) ||
            string.IsNullOrEmpty(emailInput.value) ||
            string.IsNullOrEmpty(passwordInput.value) ||
            string.IsNullOrEmpty(reenterPasswordInput.value))
        {
            messageLabel.text = "Por favor llena todos los campos";
            return;
        }

        if (passwordInput.value != reenterPasswordInput.value)
        {
            messageLabel.text = "Las contraseñas no coinciden";
            return;
        }

        messageLabel.text = "Procesando...";

        networkManager.CreateUser(userNameInput.value, emailInput.value, passwordInput.value, (response) =>
        {
            messageLabel.text = response.message;
        });
    }

    public void ShowLogin()
    {
        loginUI.style.display = DisplayStyle.Flex;
        registerUI.style.display = DisplayStyle.None;
    }

    public void ShowRegister()
    {
        loginUI.style.display = DisplayStyle.None;
        registerUI.style.display = DisplayStyle.Flex;
    }

    private void OnApplicationQuit()
    {
        if (currentUserId != 0 && !string.IsNullOrEmpty(sessionStartTime))
        {
            string endTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Debug.Log($"🚪 Cerrando aplicación - Guardando sesión con ID {currentUserId}");
            StartCoroutine(SaveSessionBeforeExit(currentUserId, sessionStartTime, endTime));
        }
    }

    private IEnumerator SaveSessionBeforeExit(int userId, string start, string end)
    {
        bool done = false;

        networkManager.SaveSession(userId, start, end, (res) =>
        {
            Debug.Log("📤 Sesión guardada: " + res.message);
            done = true;
        });

        float timeout = 3f;
        while (!done && timeout > 0f)
        {
            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }

        if (!done)
        {
            Debug.LogWarning("⚠️ La sesión no se guardó antes de que cerrara la app.");
        }
    }
}
