using UnityEngine;
using UnityEngine.InputSystem;

public class RaqueteMovimento : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 7f;
    public float limiteY = 4f;

    [Header("Jogador")]
    public bool jogador1 = true;

    void Update()
    {
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
    }
}