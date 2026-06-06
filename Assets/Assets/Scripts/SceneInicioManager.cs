using UnityEngine;
using UnityEngine.UIElements;

public class SceneInicioManager : MonoBehaviour
{
    public UIDocument uiDocument;

    private TextField inputNombre;
    private Button btnJugar;
    private Label labelError;

    void Awake()
    {
        var root = uiDocument.rootVisualElement;

        inputNombre = root.Q<TextField>("input-nombre");
        btnJugar = root.Q<Button>("btn-jugar");
        labelError = root.Q<Label>("label-error");

        btnJugar.clicked += OnJugar;

        // Crear GameManager si no existe
        if (GameManager.Instance == null)
        {
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
        }

        var btnSalir = root.Q<Button>("btn-salir");

        if (btnSalir != null)
        {
            btnSalir.clicked += () =>
            {
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                #else
                    Application.Quit();
                #endif
            };
        }

        // Mostrar récord personal
        Label labelRecord = root.Q<Label>("label-record");
        int record = PlayerPrefs.GetInt("RecordPersonal", 0);
        if (record > 0)
            labelRecord.text = $"Tu récord personal: {record} pts";
    }

    void OnJugar()
    {
        string nombre = inputNombre?.value.Trim();

        if (string.IsNullOrEmpty(nombre))
        {
            labelError.text = "Ingresa tu nombre para continuar";
            labelError.RemoveFromClassList("hidden");
            return;
        }

        GameManager.Instance.SetNombreJugador(nombre);
        GameManager.Instance.IrATutorial();
    }
}