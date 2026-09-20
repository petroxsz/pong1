using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDPManager : MonoBehaviour
{
    public static UDPManager Instance;

    [Header("Rede")]
    public int porta = 5001;

    private UdpClient udp;
    private Thread threadReceber;

    private TCPManager tcp;

    private IPEndPoint clienteEndpoint;

    private bool inicializado = false;
    private bool rodando = false;
    private bool pronto = false;

    private float proximoHello = 0f;

    public Action<string> AoReceberMensagem;

    private Queue<string> mensagensRecebidas =
        new Queue<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public void Inicializar()
    {
        if (inicializado)
            return;

        tcp = TCPManager.Instance;

        if (tcp == null || !tcp.conectado)
            return;

        try
        {
            if (tcp.souHost)
            {
                udp = new UdpClient(porta);

                Debug.Log(
                    "UDP HOST iniciado na porta " + porta
                );
            }
            else
            {
                udp = new UdpClient();

                udp.Connect(
                    tcp.ipHost,
                    porta
                );

                Debug.Log(
                    "UDP CLIENTE conectado em " +
                    tcp.ipHost +
                    ":" +
                    porta
                );
            }

            rodando = true;
            inicializado = true;

            threadReceber =
                new Thread(Receber);

            threadReceber.IsBackground = true;
            threadReceber.Start();
        }
        catch (Exception erro)
        {
            Debug.LogError(
                "Erro ao iniciar UDP: " +
                erro.Message
            );
        }
    }

    private void Receber()
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
                    udp.Receive(ref remoto);

                string mensagem =
                    Encoding.UTF8.GetString(dados);

                if (tcp != null &&
                    tcp.souHost)
                {
                    clienteEndpoint =
                        new IPEndPoint(
                            remoto.Address,
                            remoto.Port
                        );
                }

                lock (mensagensRecebidas)
                {
                    mensagensRecebidas.Enqueue(
                        mensagem
                    );
                }
            }
            catch (Exception erro)
            {
                if (rodando)
                {
                    Debug.LogError(
                        "Erro UDP: " +
                        erro.Message
                    );
                }
            }
        }
    }

    private void Update()
    {
        if (!inicializado)
            return;

        if (tcp != null &&
            !tcp.souHost &&
            !pronto)
        {
            if (Time.time >= proximoHello)
            {
                EnviarMensagem("HELLO");

                proximoHello =
                    Time.time + 0.5f;
            }
        }

        lock (mensagensRecebidas)
        {
            while (mensagensRecebidas.Count > 0)
            {
                string mensagem =
                    mensagensRecebidas.Dequeue();

                if (tcp.souHost &&
                    mensagem == "HELLO")
                {
                    pronto = true;

                    EnviarMensagem(
                        "HELLO_OK"
                    );

                    continue;
                }

                if (!tcp.souHost &&
                    mensagem == "HELLO_OK")
                {
                    pronto = true;

                    Debug.Log(
                        "UDP pronto!"
                    );

                    continue;
                }

                AoReceberMensagem?.Invoke(
                    mensagem
                );
            }
        }
    }

    public void EnviarMensagem(
        string mensagem
    )
    {
        if (!inicializado ||
            udp == null)
        {
            return;
        }

        try
        {
            byte[] dados =
                Encoding.UTF8.GetBytes(
                    mensagem
                );

            if (tcp.souHost)
            {
                if (clienteEndpoint == null)
                    return;

                udp.Send(
                    dados,
                    dados.Length,
                    clienteEndpoint
                );
            }
            else
            {
                udp.Send(
                    dados,
                    dados.Length
                );
            }
        }
        catch (Exception erro)
        {
            Debug.LogError(
                "Erro ao enviar UDP: " +
                erro.Message
            );
        }
    }


    public void Parar()
{
    rodando = false;
    pronto = false;
    inicializado = false;

    try
    {
        udp?.Close();
    }
    catch
    {
    }

    udp = null;
    clienteEndpoint = null;
}

    private void OnApplicationQuit()
    {
        rodando = false;

        udp?.Close();
    }
}