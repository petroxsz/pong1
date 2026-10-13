using UnityEngine;
using System.Globalization;

public class BolaCliente2x2 : MonoBehaviour
{
    [Header("Rede")]
    public float suavizacaoRede = 15f;
    public float distanciaParaTeleportar = 3f;

    private ClienteUDP clienteUDP;
    private Rigidbody2D rb;

    private Vector3 posicaoRede;

    void Start()
    {
        clienteUDP = ClienteUDP.Instance;
        rb = GetComponent<Rigidbody2D>();

        posicaoRede = transform.position;

        // O cliente não calcula física da bola.
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        if (clienteUDP != null)
        {
            clienteUDP.AoReceberMensagem +=
                ReceberMensagemRede;
        }
    }

    void Update()
    {
        float distancia =
            Vector3.Distance(
                transform.position,
                posicaoRede
            );

        // Quando a bola volta ao centro depois de um gol,
        // não queremos que ela atravesse o campo lentamente.
        if (distancia > distanciaParaTeleportar)
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

    private void ReceberMensagemRede(
        string mensagem
    )
    {
        if (!mensagem.StartsWith("BOLA:"))
            return;

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

        if (!xValido || !yValido)
            return;

        posicaoRede =
            new Vector3(
                x,
                y,
                transform.position.z
            );
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