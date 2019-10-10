using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;

namespace escalonador_aed
{
    class ProcessoGerador
    {
        StreamReader reader;
        string nomeArquivo = "dados_AED_SO_TI.txt";

        // Construtor
        public ProcessoGerador()
        {
            // Checar se o arquivo existe
            if (File.Exists(nomeArquivo))
            {
                reader = new StreamReader(nomeArquivo);
            }
        }

        //Metódo que monta um novo processo e retorna esse processo
        private Processo MontaNovoProcesso()
        {
            string linha = reader.ReadLine();

            string[] celula = linha.Split(';', ',');

            int PID = 0, prioridade = 0, numeroCiclos = 0;
            string nome = "VAZIO";


            PID = int.Parse(celula[0]);
            nome = celula[1];
            prioridade = int.Parse(celula[2]);
            numeroCiclos = int.Parse(celula[3]);

            Processo p = new Processo(PID, nome, prioridade, numeroCiclos);

            return p;
        }

        public void PreencherFilaDeProcessos(ProcessoFila[] f)
        {
            if (File.Exists(nomeArquivo)) //verifica se o arquivo existe
            {
                reader = new StreamReader(nomeArquivo); //instância o objeto

                while (!reader.EndOfStream) //enquanto o arquivo não tiver sido lido completamente
                {
                    Processo processo = MontaNovoProcesso(); //no método MontaNovoProcesso ocorre a leitura do arquivo e montagem do processo
                    int prioridade = processo.Prioridade;

                    try
                    {
                        f[prioridade].EnfileirarProcesso(processo);
                    }

                    catch (IndexOutOfRangeException)
                    {
                        MessageBox.Show("Erro inesperado: Prioridade com valor incorreto.");
                    }
                }
                reader.Close();
            }
        }
    }
}
