using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Música")]
    public AudioClip musicaAmbiente;
    public float volumenMusica = 0.1f;

    [Header("SFX")]
    public AudioClip sfxAprobar;
    public AudioClip sfxEscalar;
    public AudioClip sfxRechazar;
    public AudioClip sfxObjetoRecogido;
    public AudioClip sfxPenalizacion;
    public AudioClip sfxSoborno;
    public float volumenSFX = 0.1f;

    private AudioSource sourceMusica;
    private AudioSource sourceSFX;

    void Awake()
    {
        // Singleton — persiste entre escenas
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Crear dos AudioSources — uno para música, uno para SFX
        sourceMusica = gameObject.AddComponent<AudioSource>();
        sourceMusica.loop = true;
        sourceMusica.volume = volumenMusica;
        sourceMusica.spatialBlend = 0f; // 2D

        sourceSFX = gameObject.AddComponent<AudioSource>();
        sourceSFX.loop = false;
        sourceSFX.spatialBlend = 0f;
        sourceSFX.volume = volumenSFX;
    }

    void Start()
    {
        PlayMusica();
    }

    public void PlayMusica()
    {
        if (musicaAmbiente == null) return;
        sourceMusica.clip = musicaAmbiente;
        sourceMusica.Play();
    }

    public void PausarMusica()
    {
        sourceMusica.Pause();
    }

    public void ReanudarMusica()
    {
        sourceMusica.UnPause();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sourceSFX.PlayOneShot(clip);
    }

    // Métodos específicos para cada evento
    public void SFXAprobar() => PlaySFX(sfxAprobar);
    public void SFXEscalar() => PlaySFX(sfxEscalar);
    public void SFXRechazar() => PlaySFX(sfxRechazar);
    public void SFXObjetoRecogido() => PlaySFX(sfxObjetoRecogido);
    public void SFXPenalizacion() => PlaySFX(sfxPenalizacion);
    public void SFXSoborno() => PlaySFX(sfxSoborno);

    public void SetVolumenMusica(float vol)
    {
        volumenMusica = vol;
        sourceMusica.volume = vol;
    }
}