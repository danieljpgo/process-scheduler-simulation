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
            // Enquanto houver processos a serem executados
            while (!ProcessoGerenciador.FilaProcesssoVazia(processoFilas)) 
            {
                try
                {
                    Dispatcher.Invoke(
                    new Action(() =>
                    {
                        numThreadExibe = ComboBoxThreadExibicao.SelectedIndex;
                    }));

                    // Tabela é atualizada
                    AtualizarValores(numThreadExibe);

                    // Thread dorme por tempo determinado pelo usuario
                    Thread.Sleep(tempoRefreshInterface);
                }

                // Exceção disparada quando tentam finalizar o programa antes das tarefas serem concluidas
                catch (TaskCanceledException) 
                {
                    // finaliza a thread
                    return;
                }
            }

            Dispatcher.Invoke(
                new Action(() =>
                {
                    // Limpa a tela para remover os processos que restaram no DataGrid
                    DataGridGerenciador.Items.Clear();
                    MessageBox.Show("Todos os processos foram concluídos ", TotalProcessosConcluidos().ToString(), MessageBoxButton.OK, MessageBoxImage.Information);

                    // libera e limpa todos os controles
                    AtualizarControles(true);

                    LabelNomeProcesso.Content = "";
                    LabelPID.Content = "";
                    LabelPrioridade.Content = "";
                    LabelTempoExec.Content = "";

                    InterromperThreads();
                }));
        }

        // Método que instancia todas as filas de todas as prioridades
        private void InstanciarFilas()
        {
            for (int i = 0; i < processoFilas.Length; i++)
                processoFilas[i] = new ProcessoFila();
        }

        // Evento de click do botão de iniciar a simulação
        private void BtnIniciaSimulacao_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // captura os valores dados pelo usuario
                int q = int.Parse(TxtQuantum.Text);
                // número de threads
                int threads = (Convert.ToInt32(ComboBoxNumeroThreads.SelectedIndex + 1)); 
                // Tempo de atualização da interface
                tempoRefreshInterface = int.Parse(TxtTempoInterface.Text);
                //tempo de verificação de execução
                int tempoVerificacao = int.Parse(TxtTempoVerif.Text);
                //tempo máximo de espera antes de subir a prioridade
                int tempoEspera = int.Parse(TxtTempoEspera.Text);
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


                // Bloco executado quando não há erros
                // Sem erros de números inferiores a 0
                else
                {
                    // Declarando tamanho dos vetores de threads e gerenciadores de processos
                    gerenciadorProcessos = new ProcessoGerenciador[threads];
                    ThreadsExecucao = new Thread[threads];

                    // Preenche as filas com os processos necessários
                    gerador.PreencherFilaDeProcessos(processoFilas);

                    // Loop para descobrir quantos processos necessitam de serem executados
                    foreach (ProcessoFila fila in processoFilas)
                        totalProcesso += fila.ContadorProcesso;

                    // Instanciando todos os gerenciadores de processo
                    for (int ax = 0; ax < threads; ax++)
                    {
                        gerenciadorProcessos[ax] = new ProcessoGerenciador(q, tempoVerificacao);
                    }

                    int a = 0;

                    // Inicia as threads de execução
                    foreach (ProcessoGerenciador g in gerenciadorProcessos)
                    {
                        ThreadsExecucao[a] = new Thread(() => g.ExecutarProcesso(processoFilas));
                        ThreadsExecucao[a].Start();
                        a++;
                    }

                    ContPrioridadeEspera = new Thread(() => ProcessoGerenciador.ControlePrioridadeEspera(processoFilas, tempoEspera));
                    ContPrioridadeEspera.Start();

                    ThreadInterface = new Thread(AtualizaInterface);

                    // Adiciona items no ComboBox de Threads, de acordo com o numero de Threads que o usuario solicitar
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

        // Atualiza os controles de acordo com a necessidade
        // Desabilita botões e caixas de textos, para impedir que o usuário use comandos em momentos inapropriados
        private void AtualizarControles(bool valor)
        {
            BtnIniciaSimulacao.IsEnabled = valor;
            TxtQuantum.IsEnabled = valor;
            TxtTempoInterface.IsEnabled = valor;
            TxtTempoVerif.IsEnabled = valor;
            ComboBoxNumeroThreads.IsEnabled = valor;
            TxtTempoEspera.IsEnabled = valor;
            ComboBoxThreadExibicao.IsEnabled = !valor;
            Btn_Interromper.IsEnabled = !valor;

            // Se o boleano for verdadeiro, significa que os botões estão sendo ligados novamente
            // Portanto, as caixas precisam ser limpas
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

        // Atualiza os valores da tela
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
            // Finaliza todas as threads para permitir a finalização do programa sem problemas
            if (ThreadsExecucao != null)
            {
                foreach (Thread t in ThreadsExecucao)
                {
                    try
                    {
                        //se a thread estiver suspensa
                        if (t.ThreadState.ToString() == "Suspended")
                            t.Resume();

                        if (t != null)
                            t.Abort();
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

        private void Btn_Interromper_Click(object sender, RoutedEventArgs e)
        {
            //interrompe todas as threads
            InterromperThreads();            
            DataGridGerenciador.Items.Clear();
            //limpa o grid de processos
            AtualizarControles(true);
            InstanciarFilas();
            //limpa todos os labels
            LabelNomeProcesso.Content = "";
            LabelPID.Content = "";
            LabelPrioridade.Content = "";
            LabelTempoExec.Content = "";
        }

    }
}
