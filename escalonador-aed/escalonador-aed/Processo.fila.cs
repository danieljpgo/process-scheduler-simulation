using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;

namespace escalonador_aed
{
    class ProcessoFila
    {
        public ProcessoUnidade Atras { get; set; }
        public ProcessoUnidade Frente { get; set; }
        public int ContadorProcesso { get; set; }

        // @TODO Ajustar aqui
        // Classe Mutex, será usada para controlar a execução por múltiplas threads
        // https://docs.microsoft.com/en-us/dotnet/api/system.threading.mutex?view=netframework-4.8
        // http://www.macoratti.net/16/02/c_mutex1.htm
        Mutex mutex = new Mutex();

        // Construtor
        public ProcessoFila()
        {
            ProcessoUnidade sentinela = new ProcessoUnidade();
            Atras = sentinela;
            Frente = sentinela;
            ContadorProcesso = 0;
        }

        // Checar se a fila está vazia
        public bool FilaVazia()
        {

            mutex.WaitOne();
            if (Atras == Frente)
            {
                mutex.ReleaseMutex();
                return true;
            }

            else
            {
                mutex.ReleaseMutex();
                return false;
            }
        }

        // Enfileira um novo processo, ou seja, aloca um processo na ultima posição
        public void EnfileirarProcesso(Processo processo)
        {
            mutex.WaitOne();
            ProcessoUnidade processoNovo = new ProcessoUnidade(processo);

            // Caso o tempo de espera não esteja acontecendo, será iniciado
            if (!processoNovo.Processo.TempoEspera.IsRunning)
            {
                processoNovo.Processo.TempoEspera.Start();
            }

            if (FilaVazia())
            {
                Frente.Proximo = processoNovo;
            }

            Atras.Proximo = processoNovo;
            Atras = processoNovo;

            ContadorProcesso++;
            mutex.ReleaseMutex();
        }

        // Desenfileira um processo, ou seja, remove o primeiro da fila
        public Processo DesenfileirarProcesso()
        {
            mutex.WaitOne();
            if (!FilaVazia())
            {
                ProcessoUnidade aux = Frente.Proximo;
                Frente.Proximo = aux.Proximo;

                aux.Proximo = null;

                if (Frente.Proximo == null)
                {
                    Atras = Frente;
                }

                ContadorProcesso--;

                mutex.ReleaseMutex();
                return aux.Processo;
            }
            else
            {
                mutex.ReleaseMutex();
                return null;
            }
        }

        // Procurar uma Unidade de Processo na fila de Processos (findIndex() na mão)
        public Processo ProcuraProcesso(int indice)
        {
            if (!FilaVazia())
            {
                mutex.WaitOne();

                // Verificação para caso o indice seja maior que o possível
                if (indice >= ContadorProcesso)
                {
                    mutex.ReleaseMutex();
                    return null;
                }

                ProcessoUnidade aux = Frente.Proximo;

                // Avança o ponteiro até chegar no indice que deseja obter o processo
                for (int cont = 0; cont < indice; cont++)
                {
                    aux = aux.Proximo;
                }

                mutex.ReleaseMutex();
                return aux.Processo;
            }
            return null;
        }

    }
}
