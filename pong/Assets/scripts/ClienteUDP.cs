using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class ClienteUDP : MonoBehaviour
{
    public static ClienteUDP Instance;

    [Header("Servidor")]
    public string ipServidor = "127.0.0.1";
    public int portaServidor = 5001;

    private UdpClient cliente;
    private Thread threadReceber;

    private bool rodando = false;

    public int meuId = -1;

    private volatile bool iniciarPartida = false;

    // Outros scripts podem ouvir mensagens da rede
    public Action<string> AoReceberMensagem;

    private Queue<string> mensagensRecebidas =
        new Queue<string>();

    private void Awake()
{
    Application.runInBackground = true;

    if (Instance != null &&
        Instance != this)
    {
        Destroy(gameObject);
        return;
    }

    Instance = this;

    DontDestroyOnLoad(
        gameObject
    );
}

    void Update()
    {
        if (iniciarPartida)
        {
            iniciarPartida = false;

            Debug.Log(
                "Entrando na partida como Player " +
                meuId
            );

            SceneManager.LoadScene(
                "Jogo"
            );
        }

        // Entrega mensagens da rede
        // para os scripts do jogo
        lock (mensagensRecebidas)
        {
            while (mensagensRecebidas.Count > 0)
            {
                string mensagem =
                    mensagensRecebidas.Dequeue();

                AoReceberMensagem?.Invoke(
                    mensagem
                );
            }
        }
    }

    public void Conectar()
    {
        if (rodando)
            return;

        try
        {
            cliente = new UdpClient();

            cliente.Connect(
                ipServidor,
                portaServidor
            );

            rodando = true;

            threadReceber =
                new Thread(ReceberDados);

            threadReceber.IsBackground = true;
            threadReceber.Start();

            EnviarMensagem(
                "HELLO"
            );

            Debug.Log(
                "HELLO enviado para o servidor."
            );
        }
        catch (Exception erro)
        {
            Debug.LogError(
                "Erro ao conectar ao servidor: " +
                erro.Message
            );
        }
    }

    private void ReceberDados()
    {
        IPEndPoint remoto =
            new IPEndPoint(
                IPAddress.Any,
                0
            );

        while (rodando)
        {
            try
            {
                byte[] dados =
                    cliente.Receive(
                        ref remoto
                    );

                string mensagem =
                    Encoding.UTF8.GetString(
                        dados
                    );

                if (mensagem.StartsWith(
                    "ASSIGN:"
                ))
                {
                    string idTexto =
                        mensagem.Substring(7);

                    int id;

                    if (int.TryParse(
                        idTexto,
                        out id
                    ))
                    {
                        meuId = id;

                        Debug.Log(
                            "Recebi meu ID: Player " +
                            meuId
                        );
                    }
                }
                else if (
                    mensagem ==
                    "INICIAR_PARTIDA"
                )
                {
                    Debug.Log(
                        "Servidor mandou iniciar a partida!"
                    );

                    iniciarPartida = true;
                }
                else if (
                    mensagem ==
                    "SALA_CHEIA"
                )
                {
                    Debug.Log(
                        "Sala cheia!"
                    );
                }
                else
                {
                    lock (mensagensRecebidas)
                    {
                        mensagensRecebidas.Enqueue(
                            mensagem
                        );
                    }
                }
            }
            catch (Exception erro)
            {
                if (rodando)
                {
                    Debug.LogError(
                        "Erro ao receber dados: " +
                        erro.Message
                    );
                }
            }
        }
    }

    public void EnviarMensagem(
        string mensagem
    )
    {
        if (cliente == null)
            return;

        try
        {
            byte[] dados =
                Encoding.UTF8.GetBytes(
                    mensagem
                );

            cliente.Send(
                dados,
                dados.Length
            );
        }
        catch (Exception erro)
        {
            Debug.LogError(
                "Erro ao enviar mensagem: " +
                erro.Message
            );
        }
    }

    private void OnApplicationQuit()
    {
        rodando = false;

        cliente?.Close();
    }
}