using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TCPManager : MonoBehaviour
{
    public static TCPManager Instance;

    [Header("Rede")]
    public int porta = 7777;

    [HideInInspector]
    public string ipHost = "";

    private TcpListener servidor;
    private TcpClient cliente;
    private NetworkStream stream;

    private Thread threadReceber;

    public bool conectado = false;
    public bool souHost = false;

    public Action<string> AoReceberMensagem;
    public Action AoConectar;

    private Queue<string> mensagensRecebidas =
        new Queue<string>();

    private Queue<Action> filaPrincipal =
        new Queue<Action>();

    private void Awake()
    {
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

    public void CriarSala()
    {
        try
        {
            souHost = true;

            servidor =
                new TcpListener(
                    IPAddress.Any,
                    porta
                );

            servidor.Start();

            Debug.Log(
                "Sala criada! Aguardando cliente..."
            );

            servidor.BeginAcceptTcpClient(
                AceitarCliente,
                null
            );
        }
        catch (Exception erro)
        {
            Debug.LogError(
                "Erro ao criar sala: " +
                erro.Message
            );
        }
    }

    private void AceitarCliente(
        IAsyncResult resultado
    )
    {
        try
        {
            cliente =
                servidor.EndAcceptTcpClient(
                    resultado
                );

            cliente.NoDelay = true;

            stream =
                cliente.GetStream();

            conectado = true;

            Debug.Log(
                "Cliente conectado!"
            );

            lock (filaPrincipal)
            {
                filaPrincipal.Enqueue(
                    () =>
                    {
                        AoConectar?.Invoke();
                    }
                );
            }

            IniciarRecebimento();
        }
        catch (Exception erro)
        {
            Debug.LogError(
                "Erro ao aceitar cliente: " +
                erro.Message
            );
        }
    }

    public void EntrarSala(
        string ip
    )
    {
        try
        {
            souHost = false;

            ipHost = ip;

            cliente =
                new TcpClient();

            cliente.Connect(
                ip,
                porta
            );

            cliente.NoDelay = true;

            stream =
                cliente.GetStream();

            conectado = true;

            Debug.Log(
                "Conectado ao Host!"
            );

            lock (filaPrincipal)
            {
                filaPrincipal.Enqueue(
                    () =>
                    {
                        AoConectar?.Invoke();
                    }
                );
            }

            IniciarRecebimento();
        }
        catch (Exception erro)
        {
            Debug.LogError(
                "Erro ao conectar: " +
                erro.Message
            );
        }
    }

    public void EnviarMensagem(
        string mensagem
    )
    {
        if (!conectado ||
            stream == null)
        {
            return;
        }

        try
        {
            byte[] dados =
                Encoding.UTF8.GetBytes(
                    mensagem + "\n"
                );

            stream.Write(
                dados,
                0,
                dados.Length
            );
        }
        catch (Exception erro)
        {
            Debug.LogError(
                "Erro ao enviar mensagem TCP: " +
                erro.Message
            );
        }
    }

    private void IniciarRecebimento()
    {
        threadReceber =
            new Thread(Receber);

        threadReceber.IsBackground =
            true;

        threadReceber.Start();
    }

    private void Receber()
    {
        byte[] buffer =
            new byte[1024];

        StringBuilder mensagensPendentes =
            new StringBuilder();

        while (conectado)
        {
            try
            {
                int tamanho =
                    stream.Read(
                        buffer,
                        0,
                        buffer.Length
                    );

                if (tamanho <= 0)
                {
                    conectado = false;
                    break;
                }

                string recebido =
                    Encoding.UTF8.GetString(
                        buffer,
                        0,
                        tamanho
                    );

                mensagensPendentes.Append(
                    recebido
                );

                string conteudo =
                    mensagensPendentes.ToString();

                string[] mensagens =
                    conteudo.Split('\n');

                for (
                    int i = 0;
                    i < mensagens.Length - 1;
                    i++
                )
                {
                    if (!string.IsNullOrWhiteSpace(
                        mensagens[i]
                    ))
                    {
                        lock (mensagensRecebidas)
                        {
                            mensagensRecebidas.Enqueue(
                                mensagens[i]
                            );
                        }
                    }
                }

                mensagensPendentes.Clear();

                mensagensPendentes.Append(
                    mensagens[
                        mensagens.Length - 1
                    ]
                );
            }
            catch (Exception erro)
            {
                if (conectado)
                {
                    Debug.LogError(
                        "Erro ao receber TCP: " +
                        erro.Message
                    );
                }

                conectado = false;
            }
        }
    }

    private void Update()
    {
        lock (filaPrincipal)
        {
            while (filaPrincipal.Count > 0)
            {
                filaPrincipal.Dequeue()?.Invoke();
            }
        }

        lock (mensagensRecebidas)
        {
            while (
                mensagensRecebidas.Count > 0
            )
            {
                string mensagem =
                    mensagensRecebidas.Dequeue();

                AoReceberMensagem?.Invoke(
                    mensagem
                );
            }
        }
    }

    private void OnApplicationQuit()
    {
        conectado = false;

        stream?.Close();
        cliente?.Close();
        servidor?.Stop();
    }
}