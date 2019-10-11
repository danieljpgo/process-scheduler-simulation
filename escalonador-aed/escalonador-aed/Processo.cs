using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace escalonador_aed
{
    public class Processo
    {
        // Atributos 
        public int PID { get; set; }
        public string Nome { get; set; }
        public int Prioridade { get; set; }
        public int NumeroCiclos { get; set; }
        public int TempoSobra { get; set; }
        public string Estado { get; set; }
        public Stopwatch TempoExecucao { get; set; }
        public Stopwatch TempoEspera { get; set; }

        // Construtor
        public Processo(int PID, string Nome, int Prioridade, int NumeroCiclos)
        {
            this.PID = PID;
            this.Nome = Nome;
            this.Prioridade = Prioridade;
            this.NumeroCiclos = NumeroCiclos;
            Estado = "PRONTO";
            TempoExecucao = new Stopwatch();
            TempoEspera = new Stopwatch();
        }

        // Quando um ciclo é completado (processamento), o numero de ciclos é reduzido em 1,
        // o tempo de execução é resetado e o tempo de sobra é zerado para controle
        public void CicloCompleto()
        {
            Estado = "ESPERA";
            TempoExecucao.Reset();
            TempoExecucao.Stop();
            if (TempoSobra <= 0)
            {
                TempoSobra = 0;
                if (NumeroCiclos > 0)
                {
                    NumeroCiclos--;
                    TempoEspera.Start();
                }
            }

            Estado = "PRONTO";
        }

        public void IniciaCiclo()
        {
            Estado = "EXECUTANDO";
            TempoEspera.Reset();
            TempoEspera.Stop();
            TempoExecucao.Start();
        }

        // Eleva a prioridade caso seja menor do que 32
        // Reduz a prioridade caso seja maior que 1
        public int ElevarPrioridade()
        {
            if (Prioridade < 32)
            {
                Prioridade++;
            }
            return Prioridade;
        }

        public int ReduzirPrioridade()
        {
            if (Prioridade > 1)
            {
                Prioridade--;
            }
            return Prioridade;
        }

    }
}
