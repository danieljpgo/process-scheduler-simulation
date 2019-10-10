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
            if (!F[prioridadeDaFila].FilaVazia()) //Se a fila não está vazia
            {
                ProcessoSendoExecutado = F[prioridadeDaFila].DesenfileirarProcesso();  //captura o processo a ser executado

                if (ProcessoSendoExecutado == null)// se o processo é null, houve erro na execução e finaliza por aqui
                    return;

                // @TODO 
                double tempoReservadoProcessadorD = 0.01 * Quantum;
                int tempoReservadoProcessador = Convert.ToInt32(tempoReservadoProcessadorD); //calcula o tempo a thread irá dormir para simular a execução do processo
                int tempoJaUsado = 0;

                ProcessoSendoExecutado.IniciaCiclo();  //inicia a contagem de tempo do processo

                //Ciclos podem ser partidos
                if (ProcessoSendoExecutado.TempoSobra > 0) //se o tempo de sobra(processo não conseguiu completar o ciclo anteriormente, o que gera tempo de sobra de um ciclo anterior)
                {
                    if (ProcessoSendoExecutado.TempoSobra > TempoMaxExecucao)
                    {
                        Thread.Sleep(TempoMaxExecucao);
                        ProcessoSendoExecutado.TempoSobra -= TempoMaxExecucao;
                        tempoReservadoProcessador = 0;
                    }

                    else //executa o resto do ciclo do processo
                    {
                        Thread.Sleep(ProcessoSendoExecutado.TempoSobra);
                        tempoReservadoProcessador -= ProcessoSendoExecutado.TempoSobra;
                        tempoJaUsado = ProcessoSendoExecutado.TempoSobra;
                        ProcessoSendoExecutado.NumeroCiclos--; //Ciclo completo
                        ProcessoSendoExecutado.TempoSobra = 0;
                    }
                }

                if (tempoReservadoProcessador >= TempoMaxExecucao) //se o tempo de execução for superior ao máximo tempo de execução
                {
                    Thread.Sleep(TempoMaxExecucao); //Coloca a thread para dormir
                    prioridadeDaFila = ProcessoSendoExecutado.ReduzirPrioridade() - 1; //reduz a prioridade do processo e seta o índice da fila de prioridade desejada  
                    ProcessoSendoExecutado.TempoSobra = tempoReservadoProcessador - TempoMaxExecucao;
                }

                else //executa o tempo reservado de Quantum reservado pelo processo
                {
                    int tempoRestante = tempoReservadoProcessador - tempoJaUsado;

                    if (tempoRestante > 0)
                    {
                        Thread.Sleep(tempoRestante);
                        ProcessoSendoExecutado.TempoSobra = tempoReservadoProcessador;
                    }
                }

                ProcessoSendoExecutado.CicloCompleto(); //Finaliza a contagem de tempo do processo em execução

                if (ProcessoSendoExecutado.NumeroCiclos > 0) //Existem ciclos a serem executados?
                {
                    F[prioridadeDaFila].EnfileirarProcesso(ProcessoSendoExecutado);//Se existem, ele insere o processo de volta na fila de espera, na ultima posição
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
                Thread.Sleep(TempoMaximoDeEspera); //Thread dorme para não verificar a todo instante

                ProcessoFila[] FilasAux = new ProcessoFila[4]; //fila auxiliar

                for (int p = 0; p < FilasAux.Length; p++) //instânciando as filas
                    FilasAux[p] = new ProcessoFila();

                Monitor.Enter(filaProcessos);

                for (int x = 0; x < filaProcessos.Length - 1; x++) //percorre a fila prioridade 1 até a 4
                {
                    int nProcs = filaProcessos[x].ContadorProcesso;  //captura o número de processos na fila
                    int prioridadeFila = x; //define a prioridade

                    for (int u = 0; u < nProcs; u++)
                    {
                        Processo processoEmAnalise = filaProcessos[prioridadeFila].DesenfileirarProcesso(); //retira o processo da fila

                        if (processoEmAnalise.TempoEspera.ElapsedMilliseconds > TempoMaximoDeEspera) //verifica necessidade de subir prioridade
                        {
                            prioridadeFila = processoEmAnalise.ElevarPrioridade() - 1; //eleva a prioridade do processo e altera o valor da variavel prioridade
                            FilasAux[prioridadeFila - 1].EnfileirarProcesso(processoEmAnalise); //coloca o processo na fila auxiliar
                        }

                        else
                        {
                            filaProcessos[prioridadeFila].EnfileirarProcesso(processoEmAnalise); //coloca o processo de volta na mesma fila
                        }

                        prioridadeFila = x; //altera o valor da prioridade para evitar bugs
                    }
                }

                for (int i = 1; i < filaProcessos.Length; i++) //percorre as filas para adicionar os processos em suas devidas filas
                {
                    while (!FilasAux[i - 1].FilaVazia())
                        filaProcessos[i].EnfileirarProcesso(FilasAux[i - 1].DesenfileirarProcesso());
                }

                Monitor.Exit(filaProcessos);

            }
        }
    }
}
