using TMPro;
using UnityEngine;

public class MenuRede : MonoBehaviour
{
    [Header("Interface")]
    public TMP_InputField campoIP;
    public TMP_Text textoStatus;

    [Header("Rede")]
    public ClienteUDP clienteUDP;

    public void CriarSala()
    {
        textoStatus.text =
            "O SERVIDOR É UM PROGRAMA SEPARADO";
    }

    public void EntrarSala()
    {
        string ip =
            campoIP.text.Trim();

        if (string.IsNullOrEmpty(ip))
        {
            textoStatus.text =
                "DIGITE O IP DO SERVIDOR";

            return;
        }

        if (clienteUDP == null)
        {
            textoStatus.text =
                "CLIENTE UDP NÃO CONFIGURADO";

            return;
        }

        textoStatus.text =
            "CONECTANDO...";

        clienteUDP.ipServidor = ip;

        clienteUDP.Conectar();
    }
}