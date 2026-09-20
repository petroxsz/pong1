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

public bool JogoFinalizado
{
    get { return jogoFinalizado; }
}

    void Start()
    {
        AtualizarPlacar();

        textoVencedor.gameObject.SetActive(false);
    }

    public void MarcarPontoJogador1()
    {
        if (jogoFinalizado)
            return;

        pontosJogador1++;

        AtualizarPlacar();

        if (pontosJogador1 >= pontosParaVencer)
        {
            FinalizarJogo(1);
        }
    }

    public void MarcarPontoJogador2()
    {
        if (jogoFinalizado)
            return;

        pontosJogador2++;

        AtualizarPlacar();

        if (pontosJogador2 >= pontosParaVencer)
        {
            FinalizarJogo(2);
        }
    }

    private void AtualizarPlacar()
    {
        placarJogador1.text = pontosJogador1.ToString();
        placarJogador2.text = pontosJogador2.ToString();
    }

    private void FinalizarJogo(int vencedor)
    {
        jogoFinalizado = true;

        bola.PararBola();

        textoVencedor.gameObject.SetActive(true);
        textoVencedor.text = "JOGADOR " + vencedor + " VENCEU!";
    }
}