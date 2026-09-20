using UnityEngine;
using UnityEngine.InputSystem;

public class RaqueteMovimento : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 7f;
    public float limiteY = 4f;

    [Header("Jogador")]
    public bool jogador1 = true;

    private TCPManager tcp;

    void OnEnable()
{
    tcp = TCPManager.Instance;

    if (tcp != null)
    {
        tcp.AoReceberMensagem += ReceberMensagemRede;
    }
}

    void Update()
    {
        if (tcp == null || !tcp.conectado)
            return;

        bool souDonoDaRaquete =
            (jogador1 && tcp.souHost) ||
            (!jogador1 && !tcp.souHost);

        if (!souDonoDaRaquete)
            return;

        float movimento = 0f;

        if (jogador1)
        {
            if (Keyboard.current.wKey.isPressed)
                movimento = 1f;

            if (Keyboard.current.sKey.isPressed)
                movimento = -1f;
        }
        else
        {
            if (Keyboard.current.upArrowKey.isPressed)
                movimento = 1f;

            if (Keyboard.current.downArrowKey.isPressed)
                movimento = -1f;
        }

        transform.Translate(
            Vector2.up * movimento * velocidade * Time.deltaTime
        );

        Vector3 posicao = transform.position;

        posicao.y = Mathf.Clamp(
            posicao.y,
            -limiteY,
            limiteY
        );

        transform.position = posicao;

        // Só envia enquanto estiver se movimentando
        if (movimento != 0f)
{
    int numeroJogador = jogador1 ? 1 : 2;

    string mensagem =
        "RAQUETE:" +
        numeroJogador + ":" +
        transform.position.y.ToString(
            System.Globalization.CultureInfo.InvariantCulture
        );

    Debug.Log("ENVIANDO: " + mensagem);

    tcp.EnviarMensagem(mensagem);
}
    }

    private void ReceberMensagemRede(string mensagem)
    {
        Debug.Log("RECEBI: " + mensagem);

        if (!mensagem.StartsWith("RAQUETE:"))
            return;

        string[] dados = mensagem.Split(':');

        if (dados.Length != 3)
            return;

        int numeroJogador = int.Parse(dados[1]);

        float posicaoY;

        if (!float.TryParse(
            dados[2],
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out posicaoY))
        {
            return;
        }

        if (jogador1 && numeroJogador == 1)
        {
            transform.position = new Vector3(
                transform.position.x,
                posicaoY,
                transform.position.z
            );
        }
        else if (!jogador1 && numeroJogador == 2)
        {
            transform.position = new Vector3(
                transform.position.x,
                posicaoY,
                transform.position.z
            );
        }
    }

    private void OnDisable()
{
    if (tcp != null)
    {
        tcp.AoReceberMensagem -= ReceberMensagemRede;
    }
}
}