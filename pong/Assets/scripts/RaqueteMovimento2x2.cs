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
    public float suavizacaoRede = 15f;
    public float intervaloEnvio = 0.03f;

    private ClienteUDP clienteUDP;

    private float posicaoYRede;
    private float proximoEnvio = 0f;

    private bool estavaMovendo = false;

    void Start()
    {
        clienteUDP = ClienteUDP.Instance;

        posicaoYRede =
            transform.position.y;

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

        transform.Translate(
            Vector2.up *
            movimento *
            velocidade *
            Time.deltaTime
        );

        LimitarMovimento();

        bool estaMovendo =
            movimento != 0f;

        if (estaMovendo &&
            Time.time >= proximoEnvio)
        {
            EnviarPosicao();

            proximoEnvio =
                Time.time +
                intervaloEnvio;
        }

        // Envia uma última posição
        // quando soltar a tecla
        if (!estaMovendo &&
            estavaMovendo)
        {
            EnviarPosicao();
        }

        estavaMovendo =
            estaMovendo;
    }

    private void MovimentoRede()
    {
        Vector3 posicao =
            transform.position;

        posicao.y =
            Mathf.Lerp(
                posicao.y,
                posicaoYRede,
                Time.deltaTime *
                suavizacaoRede
            );

        transform.position =
            posicao;
    }

    private void EnviarPosicao()
    {
        string mensagem =
            "RAQUETE:" +
            transform.position.y.ToString(
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

        if (dados.Length != 3)
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

        // A própria raquete já está sendo
        // controlada localmente
        if (clienteUDP.meuId ==
            numeroJogador)
        {
            return;
        }

        float y;

        if (!float.TryParse(
            dados[2],
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out y
        ))
        {
            return;
        }

        posicaoYRede = y;
    }

    private void LimitarMovimento()
    {
        Vector3 posicao =
            transform.position;

        // Player 1 e 3:
        // metade superior
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

        // Player 2 e 4:
        // metade inferior
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