using UnityEngine;
using System.Globalization;

public class BolaMovimento : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 7f;
    public float aumentoVelocidade = 0.35f;
    public float velocidadeMaxima = 13f;

    [Header("Rede")]
    public float suavizacaoRede = 15f;
    public float intervaloEnvio = 0.03f;

    private Rigidbody2D rb;

    private PongManager pongManager;
    private TCPManager tcp;
    private UDPManager udp;

    private Vector3 posicaoRede;

    private float proximoEnvio = 0f;

    void Start()
    {
        rb =
            GetComponent<Rigidbody2D>();

        pongManager =
            FindFirstObjectByType<PongManager>();

        tcp =
            TCPManager.Instance;

        udp =
            UDPManager.Instance;

        posicaoRede =
            transform.position;

        if (udp != null)
        {
            udp.AoReceberMensagem +=
                ReceberMensagemRede;
        }

        if (tcp != null &&
            tcp.conectado)
        {
            if (tcp.souHost)
            {
                rb.simulated = true;

                IniciarMovimento();
            }
            else
            {
                rb.linearVelocity =
                    Vector2.zero;

                rb.simulated = false;
            }
        }
        else
        {
            IniciarMovimento();
        }
    }

    void Update()
    {
        if (tcp == null ||
            !tcp.conectado)
        {
            return;
        }

        if (!tcp.souHost)
        {
            float distancia =
                Vector3.Distance(
                    transform.position,
                    posicaoRede
                );

            if (distancia > 3f)
            {
                transform.position =
                    posicaoRede;
            }
            else
            {
                transform.position =
                    Vector3.Lerp(
                        transform.position,
                        posicaoRede,
                        Time.deltaTime *
                        suavizacaoRede
                    );
            }
        }
    }

    void FixedUpdate()
    {
        if (tcp == null ||
            !tcp.conectado)
        {
            return;
        }

        if (!tcp.souHost)
            return;

        if (pongManager != null &&
            pongManager.JogoFinalizado)
        {
            return;
        }

        if (Time.time >= proximoEnvio)
        {
            EnviarPosicaoBola();

            proximoEnvio =
                Time.time +
                intervaloEnvio;
        }
    }

    void IniciarMovimento()
    {
        Vector2 direcao =
            new Vector2(
                1f,
                0.5f
            ).normalized;

        rb.linearVelocity =
            direcao *
            velocidade;
    }

    private void OnCollisionEnter2D(
        Collision2D colisao
    )
    {
        if (tcp != null &&
            tcp.conectado &&
            !tcp.souHost)
        {
            return;
        }

        if (
            colisao.gameObject.name ==
            "Raquete1" ||
            colisao.gameObject.name ==
            "Raquete2"
        )
        {
            float velocidadeAtual =
                rb.linearVelocity.magnitude;

            float novaVelocidade =
                Mathf.Min(
                    velocidadeAtual +
                    aumentoVelocidade,
                    velocidadeMaxima
                );

            rb.linearVelocity =
                rb.linearVelocity.normalized *
                novaVelocidade;
        }
    }

    private void OnTriggerEnter2D(
        Collider2D outro
    )
    {
        if (tcp != null &&
            tcp.conectado &&
            !tcp.souHost)
        {
            return;
        }

        if (outro.CompareTag(
            "GolDireita"
        ))
        {
            pongManager
                .MarcarPontoJogador1();

            if (!pongManager
                .JogoFinalizado)
            {
                ReiniciarBola(1);
            }
        }
        else if (
            outro.CompareTag(
                "GolEsquerda"
            )
        )
        {
            pongManager
                .MarcarPontoJogador2();

            if (!pongManager
                .JogoFinalizado)
            {
                ReiniciarBola(-1);
            }
        }
    }

    private void ReiniciarBola(
        int direcaoX
    )
    {
        transform.position =
            Vector3.zero;

        Vector2 direcao =
            new Vector2(
                direcaoX,
                Random.Range(
                    -0.5f,
                    0.5f
                )
            ).normalized;

        rb.linearVelocity =
            direcao *
            velocidade;
    }

    private void EnviarPosicaoBola()
    {
        if (udp == null)
            return;

        string mensagem =
            "BOLA:" +
            transform.position.x.ToString(
                "F3",
                CultureInfo.InvariantCulture
            ) +
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
        if (tcp == null ||
            tcp.souHost)
        {
            return;
        }

        if (!mensagem.StartsWith(
            "BOLA:"
        ))
        {
            return;
        }

        string[] dados =
            mensagem.Split(':');

        if (dados.Length != 3)
            return;

        float x;
        float y;

        bool xValido =
            float.TryParse(
                dados[1],
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out x
            );

        bool yValido =
            float.TryParse(
                dados[2],
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out y
            );

        if (!xValido ||
            !yValido)
        {
            return;
        }

        posicaoRede =
            new Vector3(
                x,
                y,
                transform.position.z
            );
    }

    public void PararBola()
    {
        if (rb != null &&
            rb.simulated)
        {
            rb.linearVelocity =
                Vector2.zero;
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