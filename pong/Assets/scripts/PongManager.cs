using UnityEngine;
using TMPro;

public class PongManager : MonoBehaviour
{
    [Header("Placar")]
    public int pontosJogador1 = 0;
    public int pontosJogador2 = 0;
    public int pontosParaVencer = 5;

    [Header("Interface")]
    public TMP_Text placarJogador1;
    public TMP_Text placarJogador2;
    public TMP_Text textoVencedor;

    [Header("Bola")]
    public BolaMovimento bola;

    private bool jogoFinalizado = false;

    private TCPManager tcp;

    public bool JogoFinalizado
    {
        get
        {
            return jogoFinalizado;
        }
    }

    void Start()
    {
        tcp =
            TCPManager.Instance;

        AtualizarPlacar();

        textoVencedor
            .gameObject
            .SetActive(false);

        if (tcp != null)
        {
            tcp.AoReceberMensagem +=
                ReceberMensagemRede;
        }
    }

    public void MarcarPontoJogador1()
    {
        if (jogoFinalizado)
            return;

        if (tcp != null &&
            tcp.conectado &&
            !tcp.souHost)
        {
            return;
        }

        pontosJogador1++;

        AtualizarPlacar();
        EnviarPlacar();

        if (
            pontosJogador1 >=
            pontosParaVencer
        )
        {
            FinalizarJogo(1);
        }
    }

    public void MarcarPontoJogador2()
    {
        if (jogoFinalizado)
            return;

        if (tcp != null &&
            tcp.conectado &&
            !tcp.souHost)
        {
            return;
        }

        pontosJogador2++;

        AtualizarPlacar();
        EnviarPlacar();

        if (
            pontosJogador2 >=
            pontosParaVencer
        )
        {
            FinalizarJogo(2);
        }
    }

    private void AtualizarPlacar()
    {
        placarJogador1.text =
            pontosJogador1.ToString();

        placarJogador2.text =
            pontosJogador2.ToString();
    }

    private void EnviarPlacar()
    {
        if (tcp == null ||
            !tcp.conectado ||
            !tcp.souHost)
        {
            return;
        }

        tcp.EnviarMensagem(
            "PLACAR:" +
            pontosJogador1 +
            ":" +
            pontosJogador2
        );
    }

    private void FinalizarJogo(
        int vencedor
    )
    {
        jogoFinalizado = true;

        bola.PararBola();

        textoVencedor
            .gameObject
            .SetActive(true);

        textoVencedor.text =
            "JOGADOR " +
            vencedor +
            " VENCEU!";

        if (tcp != null &&
            tcp.conectado &&
            tcp.souHost)
        {
            tcp.EnviarMensagem(
                "VITORIA:" +
                vencedor
            );
        }
    }

    private void ReceberMensagemRede(
        string mensagem
    )
    {
        if (tcp == null ||
            tcp.souHost)
        {
            return;
        }

        if (mensagem.StartsWith(
            "PLACAR:"
        ))
        {
            string[] dados =
                mensagem.Split(':');

            if (dados.Length != 3)
                return;

            int.TryParse(
                dados[1],
                out pontosJogador1
            );

            int.TryParse(
                dados[2],
                out pontosJogador2
            );

            AtualizarPlacar();
        }
        else if (
            mensagem.StartsWith(
                "VITORIA:"
            )
        )
        {
            string[] dados =
                mensagem.Split(':');

            if (dados.Length != 2)
                return;

            int vencedor;

            if (!int.TryParse(
                dados[1],
                out vencedor
            ))
            {
                return;
            }

            jogoFinalizado = true;

            bola.PararBola();

            textoVencedor
                .gameObject
                .SetActive(true);

            textoVencedor.text =
                "JOGADOR " +
                vencedor +
                " VENCEU!";
        }
    }

    private void OnDestroy()
    {
        if (tcp != null)
        {
            tcp.AoReceberMensagem -=
                ReceberMensagemRede;
        }
    }
}