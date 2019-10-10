using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.ComponentModel;

namespace escalonador_aed
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        // Array de gerenciadores que executam os processos
        ProcessoGerenciador[] gerenciadorProcessos;

        // @TODO Corrigir aqui, o numero de Filas
        ProcessoFila[] processoFilas { get; set; } = new ProcessoFila[32];
        int totalProcesso;
        int numThreadExibe;
        int tempoRefreshInterface;

        // Array de Threads para serem executadas ao mesmo tempo
        Thread[] ThreadsExecucao;
        Thread ContPrioridadeEspera;
        Thread ThreadInterface;

        // Preencher as filas
        ProcessoGerador gerador = new ProcessoGerador();

        public MainWindow()
        {
            InstanciarFilas();
            InitializeComponent();
        }

        // Refresh interface (atualizar os valores)
        private void AtualizaInterface()
        {
            while (!ProcessoGerenciador.FilaProcesssoVazia(processoFilas)) //Enquanto houver processos a serem executados
            {
                try
                {
                    Dispatcher.Invoke(
                    new Action(() =>
                    {
                        numThreadExibe = ComboBoxThreadExibicao.SelectedIndex;
                    }));

                    AtualizarValores(numThreadExibe);//Tabela é atualizada
                    Thread.Sleep(tempoRefreshInterface);//Thread dorme por tempo determinado pelo usuario
                }

                catch (TaskCanceledException) //exceção disparada quando tentam finalizar o programa antes das tarefas serem concluidas
                {
                    return; //finaliza a thread
                }
            }

            Dispatcher.Invoke(
                new Action(() =>
                {
                    DataGridGerenciador.Items.Clear();//Limpa a tela para remover os processos que restaram no DataGrid
                    MessageBox.Show("Todos os processos foram concluídos ", TotalProcessosConcluidos().ToString(), MessageBoxButton.OK, MessageBoxImage.Information);
                    AtualizarControles(true);//libera e limpa todos os controles

                    LabelNomeProcesso.Content = "";
                    LabelPID.Content = "";
                    LabelPrioridade.Content = "";
                    LabelTempoExec.Content = "";

                    InterromperThreads();
                }));
        }

        //Método que instancia todas as filas de todas as prioridades
        private void InstanciarFilas()
        {
            for (int i = 0; i < processoFilas.Length; i++)
                processoFilas[i] = new ProcessoFila();
        }

        //Evento de click do botão de iniciar a simulação
        private void BtnIniciaSimulacao_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //captura os valores dados pelo usuario
                int q = int.Parse(TxtQuantum.Text);
                int threads = (Convert.ToInt32(ComboBoxNumeroThreads.SelectedIndex + 1)); //número de threads
                tempoRefreshInterface = int.Parse(TxtTempoInterface.Text); //tempo de atualização da interface
                int tempoVerificacao = int.Parse(TxtTempoVerif.Text); //tempo de verificação de execução
                int tempoEspera = int.Parse(TxtTempoEspera.Text); //tempo máximo de espera antes de subir a prioridade
                totalProcesso = 0;

                //Tratamento de erros
                if (q <= 0)
                {
                    MessageBox.Show("Quantum inválido. O tempo deve ser superior a 0.");
                }

                else if (tempoRefreshInterface <= 0)
                {
                    MessageBox.Show("Tempo de atualização de interface inválido. O tempo deve ser superior a 0.");
                }

                else if (tempoVerificacao <= 0)
                {
                    MessageBox.Show("Tempo de verificação de prioridades inválido. O tempo deve ser superior a 0.");
                }

                else if (threads <= 0)
                {
                    MessageBox.Show("Indique um número de threads.");
                }


                //bloco executado quando não há erros
                else //sem erros de números inferiores a 0
                {
                    //declarando tamanho dos vetores de threads e gerenciadores de processos
                    gerenciadorProcessos = new ProcessoGerenciador[threads];
                    ThreadsExecucao = new Thread[threads];

                    //preenche as filas com os processos necessários
                    gerador.PreencherFilaDeProcessos(processoFilas);

                    //loop para descobrir quantos processos necessitam de serem executados
                    foreach (ProcessoFila fila in processoFilas)
                        totalProcesso += fila.ContadorProcesso;

                    //instanciando todos os gerenciadores de processo
                    for (int ax = 0; ax < threads; ax++)
                    {
                        gerenciadorProcessos[ax] = new ProcessoGerenciador(q, tempoVerificacao);
                    }

                    int a = 0;

                    //inicia as threads de execução
                    foreach (ProcessoGerenciador g in gerenciadorProcessos)
                    {
                        ThreadsExecucao[a] = new Thread(() => g.ExecutarProcesso(processoFilas));
                        ThreadsExecucao[a].Start();
                        a++;
                    }

                    ContPrioridadeEspera = new Thread(() => ProcessoGerenciador.ControlePrioridadeEspera(processoFilas, tempoEspera));
                    ContPrioridadeEspera.Start();

                    ThreadInterface = new Thread(AtualizaInterface);

                    //Adiciona items no ComboBox de Threads, de acordo com o numero de Threads que o usuario solicitar
                    for (int ab = 0; ab < threads; ab++)
                        ComboBoxThreadExibicao.Items.Add(ab + 1);

                    ComboBoxThreadExibicao.SelectedIndex = 0;
                    ThreadInterface.Start();
                    AtualizarControles(false);
                }
            }

            catch (FormatException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //Atualiza os controles de acordo com a necessidade
        //Desabilita botões e caixas de textos, para impedir que o usuário use comandos em momentos inapropriados
        private void AtualizarControles(bool valor)
        {
            BtnIniciaSimulacao.IsEnabled = valor;
            TxtQuantum.IsEnabled = valor;
            TxtTempoInterface.IsEnabled = valor;
            TxtTempoVerif.IsEnabled = valor;
            ComboBoxNumeroThreads.IsEnabled = valor;
            TxtTempoEspera.IsEnabled = valor;
            ComboBoxThreadExibicao.IsEnabled = !valor;
            BtnPausar.IsEnabled = !valor;
            Btn_Interromper.IsEnabled = !valor;

            //Se o boleano for verdadeiro, significa que os botões estão sendo ligados novamente
            //Portanto, as caixas precisam ser limpas
            if (valor)
            {
                ComboBoxThreadExibicao.Items.Clear();
            }
        }

        private int TotalProcessosConcluidos()
        {
            int total = 0;
            foreach (ProcessoGerenciador g in gerenciadorProcessos)
                total += g.ProcessosFinalizados;

            return total;
        }

        //Atualiza os valores da tela
        private void AtualizarValores(int indice)
        {
            Dispatcher.Invoke(
                new Action(() =>
                {
                    DataGridGerenciador.Items.Clear(); //limpa o grid
                    for (int i = (processoFilas.Length - 1); i >= 0; i--)
                    {
                        Monitor.Enter(processoFilas);
                        for (int u = 0; u < processoFilas[i].ContadorProcesso; u++)
                        {
                            DataGridGerenciador.Items.Add(processoFilas[i].ProcuraProcesso(u));
                        }
                        Monitor.Exit(processoFilas);

                        Processo processoExecutando = gerenciadorProcessos[indice].ProcessoSendoExecutado;

                        LabelNomeProcesso.Content = processoExecutando.Nome;
                        LabelPID.Content = processoExecutando.PID;
                        LabelPrioridade.Content = processoExecutando.Prioridade;
                        LabelTempoExec.Content = processoExecutando.TempoExecucao.ElapsedMilliseconds + "ms";
                        LabelCiclco.Content = processoExecutando.NumeroCiclos;
                    }
                }));

        }


        private void InterromperThreads()
        {
            //Finaliza todas as threads para permitir a finalização do programa sem problemas
            if (ThreadsExecucao != null)
            {
                foreach (Thread t in ThreadsExecucao)
                {
                    try
                    {
                        if (t.ThreadState.ToString() == "Suspended") //se a thread estiver suspensa
                            t.Resume();//resume a thread

                        if (t != null)
                            t.Abort();//aborta a thread
                    }

                    catch (ThreadStateException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }

            if (ThreadInterface != null)
            {
                try
                {
                    ThreadInterface.Abort();
                }

                catch (ThreadStateException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            if (ContPrioridadeEspera != null)
            {
                try
                {
                    ContPrioridadeEspera.Abort();
                }

                catch (ThreadStateException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //Interrompe as threads assim de sair
            InterromperThreads();
        }

        private void BtnPausar_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < gerenciadorProcessos.Length; i++)
            {
                ThreadsExecucao[i].Suspend();//suspende a thread de execução
                gerenciadorProcessos[i].ProcessoSendoExecutado.TempoExecucao.Stop();
            }

            foreach (ProcessoFila f in processoFilas)
            {
                for (int i = 0; i < f.ContadorProcesso; i++)
                    f.ProcuraProcesso(i).TempoEspera.Stop();
            }

            ContPrioridadeEspera.Suspend();

            BtnPausar.IsEnabled = false;
            BtnResumir.IsEnabled = true;
        }

        private void BtnResumir_Click(object sender, RoutedEventArgs e)
        {
            Monitor.Enter(processoFilas);
            for (int i = 0; i < gerenciadorProcessos.Length; i++)
            {
                ThreadsExecucao[i].Resume();
                gerenciadorProcessos[i].ProcessoSendoExecutado.TempoExecucao.Start();
            }

            foreach (ProcessoFila f in processoFilas)
            {
                for (int i = 0; i < f.ContadorProcesso; i++)
                    f.ProcuraProcesso(i).TempoEspera.Start();
            }

            ContPrioridadeEspera.Resume();

            BtnPausar.IsEnabled = true;
            BtnResumir.IsEnabled = false;
            Btn_Interromper.IsEnabled = true;
            Monitor.Exit(processoFilas);
        }

        private void Btn_Interromper_Click(object sender, RoutedEventArgs e)
        {
            InterromperThreads();//interrompe todas as threads
            DataGridGerenciador.Items.Clear(); //limpa o grid de processos
            AtualizarControles(true);//atualiza os controles
            InstanciarFilas();//reseta todas as filas
            //limpa todos os labels
            LabelNomeProcesso.Content = "";
            LabelPID.Content = "";
            LabelPrioridade.Content = "";
            LabelTempoExec.Content = "";
        }

    }
}
