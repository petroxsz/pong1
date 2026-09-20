using UnityEngine;

public class BolaMovimento : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 7f;
    public float aumentoVelocidade = 0.35f;
    public float velocidadeMaxima = 13f;

    private Rigidbody2D rb;
    private PongManager pongManager;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        pongManager = FindFirstObjectByType<PongManager>();

        IniciarMovimento();
    }

    void IniciarMovimento()
    {
        Vector2 direcao = new Vector2(1f, 0.5f).normalized;

        rb.linearVelocity = direcao * velocidade;
    }

    private void OnCollisionEnter2D(Collision2D colisao)
    {
        if (colisao.gameObject.name == "Raquete1" ||
            colisao.gameObject.name == "Raquete2")
        {
            float velocidadeAtual = rb.linearVelocity.magnitude;

            float novaVelocidade = Mathf.Min(
                velocidadeAtual + aumentoVelocidade,
                velocidadeMaxima
            );

            rb.linearVelocity =
                rb.linearVelocity.normalized * novaVelocidade;
        }
    }

    private void OnTriggerEnter2D(Collider2D outro)
{
    if (outro.CompareTag("GolDireita"))
    {
        pongManager.MarcarPontoJogador1();

        if (!pongManager.JogoFinalizado)
        {
            ReiniciarBola(1);
        }
    }
    else if (outro.CompareTag("GolEsquerda"))
    {
        pongManager.MarcarPontoJogador2();

        if (!pongManager.JogoFinalizado)
        {
            ReiniciarBola(-1);
        }
    }
}

    private void ReiniciarBola(int direcaoX)
    {
        transform.position = Vector3.zero;

        Vector2 direcao = new Vector2(
            direcaoX,
            Random.Range(-0.5f, 0.5f)
        ).normalized;

        rb.linearVelocity = direcao * velocidade;
    }

    public void PararBola()
{
    rb.linearVelocity = Vector2.zero;
}
}