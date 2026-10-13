using UnityEngine;
using System.Globalization;

public class BolaCliente2x2 : MonoBehaviour
{
    [Header("Rede")]
    public float suavizacaoRede = 30f;
    public float distanciaParaTeleportar = 3f;

    [Header("Predicao")]
    public float maximoTempoPredicao = 0.05f;

    private ClienteUDP clienteUDP;
    private Rigidbody2D rb;

    private Vector3 posicaoRede;
    private Vector2 velocidadeRede;

    private float tempoUltimaMensagem = 0f;

    void Start()
    {
        clienteUDP =
            ClienteUDP.Instance;

        rb =
            GetComponent<Rigidbody2D>();

        posicaoRede =
            transform.position;

        velocidadeRede =
            Vector2.zero;

        tempoUltimaMensagem =
            Time.time;

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.simulated =
                false;
        }

        if (clienteUDP != null)
        {
            clienteUDP.AoReceberMensagem +=
                ReceberMensagemRede;
        }
    }

    void Update()
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

        Vector3 posicaoPrevista =
            posicaoRede +
            new Vector3(
                velocidadeRede.x,
                velocidadeRede.y,
                0f
            ) *
            tempoDesdeUltimoPacote;

        float distancia =
            Vector3.Distance(
                transform.position,
                posicaoRede
            );

        if (distancia >
            distanciaParaTeleportar)
        {
            transform.position =
                posicaoRede;
        }
        else
        {
            transform.position =
                Vector3.Lerp(
                    transform.position,
                    posicaoPrevista,
                    Time.deltaTime *
                    suavizacaoRede
                );
        }
    }

    private void ReceberMensagemRede(
        string mensagem
    )
    {
        if (!mensagem.StartsWith(
            "BOLA:"
        ))
        {
            return;
        }

        string[] dados =
            mensagem.Split(':');

        if (dados.Length != 5)
            return;

        float x;
        float y;
        float velocidadeX;
        float velocidadeY;

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

        bool velocidadeXValida =
            float.TryParse(
                dados[3],
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out velocidadeX
            );

        bool velocidadeYValida =
            float.TryParse(
                dados[4],
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out velocidadeY
            );

        if (!xValido ||
            !yValido ||
            !velocidadeXValida ||
            !velocidadeYValida)
        {
            return;
        }

        posicaoRede =
            new Vector3(
                x,
                y,
                transform.position.z
            );

        velocidadeRede =
            new Vector2(
                velocidadeX,
                velocidadeY
            );

        tempoUltimaMensagem =
            Time.time;
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