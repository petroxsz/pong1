using UnityEngine;
using UnityEngine.InputSystem;
using System.Globalization;

public class RaqueteMovimento2x2 : MonoBehaviour
{
    [Header("Jogador")]
    public int numeroJogador = 1;

    [Header("Movimento")]
    public float velocidade = 7f;

    [Header("Rede")]
    public float suavizacaoRede = 30f;
    public float intervaloEnvio = 0.03f;
    public float maximoTempoPredicao = 0.05f;

    private ClienteUDP clienteUDP;

    private float posicaoYRede;
    private float velocidadeYRede;

    private float tempoUltimaMensagem = 0f;
    private float proximoEnvio = 0f;

    private bool estavaMovendo = false;

    void Start()
    {
        clienteUDP =
            ClienteUDP.Instance;

        posicaoYRede =
            transform.position.y;

        velocidadeYRede = 0f;

        tempoUltimaMensagem =
            Time.time;

        if (clienteUDP != null)
        {
            clienteUDP.AoReceberMensagem +=
                ReceberMensagemRede;
        }
    }

    void Update()
    {
        if (clienteUDP == null)
            return;

        bool souDono =
            clienteUDP.meuId ==
            numeroJogador;

        if (souDono)
        {
            MovimentoLocal();
        }
        else
        {
            MovimentoRede();
        }
    }

    private void MovimentoLocal()
    {
        float movimento = 0f;

        if (Keyboard.current.wKey.isPressed)
        {
            movimento = 1f;
        }

        if (Keyboard.current.sKey.isPressed)
        {
            movimento = -1f;
        }

        float velocidadeY =
            movimento *
            velocidade;

        transform.Translate(
            Vector2.up *
            velocidadeY *
            Time.deltaTime
        );

        LimitarMovimento();

        bool estaMovendo =
            movimento != 0f;

        if (estaMovendo &&
            Time.time >= proximoEnvio)
        {
            EnviarPosicao(
                velocidadeY
            );

            proximoEnvio =
                Time.time +
                intervaloEnvio;
        }

        // Ao soltar a tecla,
        // envia velocidade 0.
        if (!estaMovendo &&
            estavaMovendo)
        {
            EnviarPosicao(
                0f
            );
        }

        estavaMovendo =
            estaMovendo;
    }

    private void MovimentoRede()
    {
        float tempoDesdeUltimoPacote =
            Time.time -
            tempoUltimaMensagem;

        tempoDesdeUltimoPacote =
            Mathf.Clamp(
                tempoDesdeUltimoPacote,
                0f,
                maximoTempoPredicao
            );

        float yPrevisto =
            posicaoYRede +
            velocidadeYRede *
            tempoDesdeUltimoPacote;

        // Mantém a previsão dentro
        // da área permitida.
        if (numeroJogador == 1 ||
            numeroJogador == 3)
        {
            yPrevisto =
                Mathf.Clamp(
                    yPrevisto,
                    0.3f,
                    4f
                );
        }
        else
        {
            yPrevisto =
                Mathf.Clamp(
                    yPrevisto,
                    -4f,
                    -0.3f
                );
        }

        Vector3 posicao =
            transform.position;

        posicao.y =
            Mathf.Lerp(
                posicao.y,
                yPrevisto,
                Time.deltaTime *
                suavizacaoRede
            );

        transform.position =
            posicao;
    }

    private void EnviarPosicao(
        float velocidadeY
    )
    {
        string mensagem =
            "RAQUETE:" +
            transform.position.y.ToString(
                "F3",
                CultureInfo.InvariantCulture
            ) +
            ":" +
            velocidadeY.ToString(
                "F3",
                CultureInfo.InvariantCulture
            );

        clienteUDP.EnviarMensagem(
            mensagem
        );
    }

    private void ReceberMensagemRede(
        string mensagem
    )
    {
        if (!mensagem.StartsWith(
            "RAQUETE:"
        ))
        {
            return;
        }

        string[] dados =
            mensagem.Split(':');

        if (dados.Length != 4)
            return;

        int id;

        if (!int.TryParse(
            dados[1],
            out id
        ))
        {
            return;
        }

        if (id != numeroJogador)
            return;

        // A raquete local continua
        // sendo controlada diretamente.
        if (clienteUDP.meuId ==
            numeroJogador)
        {
            return;
        }

        float y;
        float velocidadeY;

        bool yValido =
            float.TryParse(
                dados[2],
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out y
            );

        bool velocidadeValida =
            float.TryParse(
                dados[3],
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out velocidadeY
            );

        if (!yValido ||
            !velocidadeValida)
        {
            return;
        }

        posicaoYRede = y;

        velocidadeYRede =
            velocidadeY;

        tempoUltimaMensagem =
            Time.time;
    }

    private void LimitarMovimento()
    {
        Vector3 posicao =
            transform.position;

        if (numeroJogador == 1 ||
            numeroJogador == 3)
        {
            posicao.y =
                Mathf.Clamp(
                    posicao.y,
                    0.3f,
                    4f
                );
        }
        else if (
            numeroJogador == 2 ||
            numeroJogador == 4
        )
        {
            posicao.y =
                Mathf.Clamp(
                    posicao.y,
                    -4f,
                    -0.3f
                );
        }

        transform.position =
            posicao;
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