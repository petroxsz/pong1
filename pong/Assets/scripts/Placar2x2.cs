using UnityEngine;
using TMPro;

public class Placar2x2 : MonoBehaviour
{
    [Header("Interface")]
    public TMP_Text placarTime1;
    public TMP_Text placarTime2;
    public TMP_Text textoVencedor;

    private ClienteUDP clienteUDP;

    private int pontosTime1 = 0;
    private int pontosTime2 = 0;

    private bool jogoFinalizado = false;

    void Start()
    {
        clienteUDP =
            ClienteUDP.Instance;

        AtualizarPlacar();

        if (textoVencedor != null)
        {
            textoVencedor
                .gameObject
                .SetActive(false);
        }

        if (clienteUDP != null)
        {
            clienteUDP.AoReceberMensagem +=
                ReceberMensagemRede;
        }
    }

    private void ReceberMensagemRede(
        string mensagem
    )
    {
        if (mensagem.StartsWith("PLACAR:"))
        {
            ReceberPlacar(
                mensagem
            );
        }
        else if (
            mensagem.StartsWith("VITORIA:")
        )
        {
            ReceberVitoria(
                mensagem
            );
        }
    }

    private void ReceberPlacar(
        string mensagem
    )
    {
        string[] dados =
            mensagem.Split(':');

        if (dados.Length != 3)
            return;

        int novoTime1;
        int novoTime2;

        if (!int.TryParse(
            dados[1],
            out novoTime1
        ))
        {
            return;
        }

        if (!int.TryParse(
            dados[2],
            out novoTime2
        ))
        {
            return;
        }

        pontosTime1 =
            novoTime1;

        pontosTime2 =
            novoTime2;

        AtualizarPlacar();
    }

    private void AtualizarPlacar()
    {
        if (placarTime1 != null)
        {
            placarTime1.text =
                pontosTime1.ToString();
        }

        if (placarTime2 != null)
        {
            placarTime2.text =
                pontosTime2.ToString();
        }
    }

    private void ReceberVitoria(
        string mensagem
    )
    {
        if (jogoFinalizado)
            return;

        string[] dados =
            mensagem.Split(':');

        if (dados.Length != 2)
            return;

        int timeVencedor;

        if (!int.TryParse(
            dados[1],
            out timeVencedor
        ))
        {
            return;
        }

        jogoFinalizado = true;

        if (textoVencedor != null)
        {
            textoVencedor
                .gameObject
                .SetActive(true);

            textoVencedor.text =
                "TIME " +
                timeVencedor +
                " VENCEU!";
        }
    }

    private void OnDestroy()
    {
        if (clienteUDP != null)
        {
            clienteUDP.AoReceberMensagem -=
                ReceberMensagemRede;
        }
    }
}