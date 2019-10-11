using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;

namespace escalonador_aed
{
    class ProcessoGerenciador
    {
        // Atributos
        public int Quantum { get; set; }
        public Processo ProcessoSendoExecutado { get; set; }
        public int ProcessosFinalizados { get; set; }
        public int TempoMaxExecucao { get; set; }

        // Construtor
        public ProcessoGerenciador(int quantum, int tempoMaxExecucao)
        {
            this.Quantum = quantum;
            this.TempoMaxExecucao = tempoMaxExecucao;
        }

        // Checar se as filas de processos estão vazias
        public static bool FilaProcesssoVazia(ProcessoFila[] processoFilas)
        {
            for(int i = 0; i < processoFilas.Length; i++)
            {
                if (!processoFilas[i].FilaVazia())
                {
                    return false;
                }
            }

            return true;
        }

        public void ExecutarProcesso(ProcessoFila[] processoFilas)
        {
            int prioridade = 0;
            int contVazias = 0;
            do
            {
                for (int i = (processoFilas.Length - 1); i >= 0; i--)
                {
                    // prioridade é o indice do vetor de Filas
                    if (!processoFilas[i].FilaVazia())
                    {
                        prioridade = i;

                        // Condição para sair do Loop, caso encontre algum processo
                        i = -1;
                    }
                    else
                    {
                        contVazias++;
                    }
                }
     
                // Quando todas as filas estiverem vazias
                if (contVazias == processoFilas.Length)
                {
                    // Finalizador do While
                    prioridade = -1;
                }

                // i é diferente de -1
                if (prioridade != -1)
                {
                    SimularExecucao(processoFilas, prioridade);
                }
            } while (prioridade != -1);
        }

        private void SimularExecucao(ProcessoFila[] F, int prioridadeDaFila)
        {
            // Se a fila não está vazia
            if (!F[prioridadeDaFila].FilaVazia()) 
            {
                // Captura o processo a ser executado
                ProcessoSendoExecutado = F[prioridadeDaFila].DesenfileirarProcesso();

                // Se o processo é null, houve erro na execução e finaliza por aqui
                if (ProcessoSendoExecutado == null)
                    return;

                double tempoReservadoProcessadorD = 0.01 * Quantum;

                // Calcula o tempo a thread irá dormir para simular a execução do processo
                int tempoReservadoProcessador = Convert.ToInt32(tempoReservadoProcessadorD);
                int tempoJaUsado = 0;

                ProcessoSendoExecutado.IniciaCiclo();  //inicia a contagem de tempo do processo

                // Ciclos podem ser partidos
                // se o tempo de sobra(processo não conseguiu completar o ciclo anteriormente, o que gera tempo de sobra de um ciclo anterior)
                if (ProcessoSendoExecutado.TempoSobra > 0)
                {
                    if (ProcessoSendoExecutado.TempoSobra > TempoMaxExecucao)
                    {
                        Thread.Sleep(TempoMaxExecucao);
                        ProcessoSendoExecutado.TempoSobra -= TempoMaxExecucao;
                        tempoReservadoProcessador = 0;
                    }

                    // Executa o resto do ciclo do processo
                    else
                    {
                        Thread.Sleep(ProcessoSendoExecutado.TempoSobra);
                        tempoReservadoProcessador -= ProcessoSendoExecutado.TempoSobra;
                        tempoJaUsado = ProcessoSendoExecutado.TempoSobra;

                        // Ciclo completo
                        ProcessoSendoExecutado.NumeroCiclos--;
                        ProcessoSendoExecutado.TempoSobra = 0;
                    }
                }

                // Se o tempo de execução for superior ao máximo tempo de execução
                if (tempoReservadoProcessador >= TempoMaxExecucao)
                {
                    // Coloca a thread para dormir
                    Thread.Sleep(TempoMaxExecucao);

                    // Reduz a prioridade do processo e seta o índice da fila de prioridade desejada  
                    prioridadeDaFila = ProcessoSendoExecutado.ReduzirPrioridade() - 1;
                    ProcessoSendoExecutado.TempoSobra = tempoReservadoProcessador - TempoMaxExecucao;
                }

                // Executa o tempo reservado de Quantum reservado pelo processo
                else
                {
                    int tempoRestante = tempoReservadoProcessador - tempoJaUsado;

                    if (tempoRestante > 0)
                    {
                        Thread.Sleep(tempoRestante);
                        ProcessoSendoExecutado.TempoSobra = tempoReservadoProcessador;
                    }
                }

                // Finaliza a contagem de tempo do processo em execução
                ProcessoSendoExecutado.CicloCompleto();

                // Existem ciclos a serem executados?
                if (ProcessoSendoExecutado.NumeroCiclos > 0)
                {
                    // Se existem, ele insere o processo de volta na fila de espera, na ultima posição
                    F[prioridadeDaFila].EnfileirarProcesso(ProcessoSendoExecutado);
                }

                else
                {
                    ProcessosFinalizados++;
                }
            }
        }

        // @TODO
        public static void ControlePrioridadeEspera(ProcessoFila[] filaProcessos, int TempoMaximoDeEspera)
        {
            while (true)
            {
                // Thread dorme para não verificar a todo instante
                Thread.Sleep(TempoMaximoDeEspera);

                // Fila auxiliar
                ProcessoFila[] FilasAux = new ProcessoFila[32];

                // Instânciando as filas
                for (int p = 0; p < FilasAux.Length; p++)
                    FilasAux[p] = new ProcessoFila();

                Monitor.Enter(filaProcessos);

                // Percorre a fila prioridade 
                for (int x = 0; x < filaProcessos.Length - 1; x++)
                {
                    // Captura o número de processos na fila
                    int nProcs = filaProcessos[x].ContadorProcesso;

                    // Define a prioridade
                    int prioridadeFila = x;

                    for (int u = 0; u < nProcs; u++)
                    {
                        // Retira o processo da fila
                        Processo processoEmAnalise = filaProcessos[prioridadeFila].DesenfileirarProcesso();

                        // Verifica necessidade de subir prioridade
                        if (processoEmAnalise.TempoEspera.ElapsedMilliseconds > TempoMaximoDeEspera)
                        {
                            // Eleva a prioridade do processo e altera o valor da variavel prioridade
                            prioridadeFila = processoEmAnalise.ElevarPrioridade() - 1;
                            // Coloca o processo na fila auxiliar
                            FilasAux[prioridadeFila - 1].EnfileirarProcesso(processoEmAnalise); 
                        }

                        else
                        {
                            // Coloca o processo de volta na mesma fila
                            filaProcessos[prioridadeFila].EnfileirarProcesso(processoEmAnalise);
                        }

                        // Altera o valor da prioridade para evitar bugs
                        prioridadeFila = x;
                    }
                }

                // Percorre as filas para adicionar os processos em suas devidas filas

                for (int i = 1; i < filaProcessos.Length; i++)
                {
                    while (!FilasAux[i - 1].FilaVazia())
                        filaProcessos[i].EnfileirarProcesso(FilasAux[i - 1].DesenfileirarProcesso());
                }

                Monitor.Exit(filaProcessos);

            }
        }
    }
}
