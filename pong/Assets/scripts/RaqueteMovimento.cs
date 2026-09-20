using UnityEngine;
using UnityEngine.InputSystem;
using System.Globalization;

public class RaqueteMovimento : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 7f;
    public float limiteY = 4f;

    [Header("Jogador")]
    public bool jogador1 = true;

    [Header("Rede")]
    public float suavizacaoRede = 15f;
    public float intervaloEnvio = 0.03f;

    private TCPManager tcp;
    private UDPManager udp;

    private float posicaoYRede;
    private float proximoEnvio = 0f;

    void Start()
    {
        tcp = TCPManager.Instance;
        udp = UDPManager.Instance;

        posicaoYRede =
            transform.position.y;

        if (udp != null)
        {
            udp.AoReceberMensagem +=
                ReceberMensagemRede;
        }
    }

    void Update()
    {
        if (tcp == null ||
            !tcp.conectado)
        {
            return;
        }

        bool souDonoDaRaquete =
            (jogador1 && tcp.souHost) ||
            (!jogador1 && !tcp.souHost);

        if (souDonoDaRaquete)
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

        if (jogador1)
        {
            if (Keyboard.current.wKey.isPressed)
            {
                movimento = 1f;
            }

            if (Keyboard.current.sKey.isPressed)
            {
                movimento = -1f;
            }
        }
        else
        {
            if (Keyboard.current.upArrowKey.isPressed)
            {
                movimento = 1f;
            }

            if (Keyboard.current.downArrowKey.isPressed)
            {
                movimento = -1f;
            }
        }

        transform.Translate(
            Vector2.up *
            movimento *
            velocidade *
            Time.deltaTime
        );

        Vector3 posicao =
            transform.position;

        posicao.y =
            Mathf.Clamp(
                posicao.y,
                -limiteY,
                limiteY
            );

        transform.position =
            posicao;

        if (movimento != 0f &&
            Time.time >= proximoEnvio)
        {
            EnviarPosicao();

            proximoEnvio =
                Time.time +
                intervaloEnvio;
        }
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
        if (udp == null)
            return;

        int numeroJogador =
            jogador1 ? 1 : 2;

        string mensagem =
            "RAQUETE:" +
            numeroJogador +
            ":" +
            transform.position.y.ToString(
                "F3",
                CultureInfo.InvariantCulture
            );

        udp.EnviarMensagem(
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

        int numeroJogador;

        if (!int.TryParse(
            dados[1],
            out numeroJogador
        ))
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

        if (jogador1 &&
            numeroJogador == 1)
        {
            posicaoYRede = y;
        }

        if (!jogador1 &&
            numeroJogador == 2)
        {
            posicaoYRede = y;
        }
    }

    private void OnDestroy()
    {
        if (udp != null)
        {
            udp.AoReceberMensagem -=
                ReceberMensagemRede;
        }
    }
}