using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuRede : MonoBehaviour
{
    [Header("Interface")]
    public TMP_InputField campoIP;
    public TMP_Text textoStatus;

    private TCPManager tcp;

    void Start()
    {
        tcp = TCPManager.Instance;

        tcp.AoConectar += Conectado;
    }

    public void CriarSala()
    {
        textoStatus.text =
            "AGUARDANDO JOGADOR...";

        tcp.CriarSala();
    }

    public void EntrarSala()
    {
        string ip =
            campoIP.text.Trim();

        if (string.IsNullOrEmpty(ip))
        {
            textoStatus.text =
                "DIGITE O IP DO HOST";

            return;
        }

        textoStatus.text =
            "CONECTANDO...";

        tcp.EntrarSala(ip);
    }

    private void Conectado()
    {
        textoStatus.text =
            "CONECTADO!";

        if (UDPManager.Instance != null)
        {
            UDPManager.Instance.Inicializar();
        }

        SceneManager.LoadScene(
            "Jogo"
        );
    }

    private void OnDestroy()
    {
        if (tcp != null)
        {
            tcp.AoConectar -= Conectado;
        }
    }
}