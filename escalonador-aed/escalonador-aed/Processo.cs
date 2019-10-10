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
        public double OcupacaoCPU { get; set; }
        public int NumeroCiclos { get; set; }
        public int TempoSobra { get; set; }
        public string Estado { get; set; }
        public Stopwatch TempoExecucao { get; set; }
        public Stopwatch TempoEspera { get; set; }

        // Construtor
        public Processo(int PID, string Nome, int Prioridade, double OcupacaoCPU, int NumeroCiclos)
        {
            this.PID = PID;
            this.Nome = Nome;
            this.Prioridade = Prioridade;
            this.OcupacaoCPU = OcupacaoCPU;
            this.NumeroCiclos = NumeroCiclos;
            Estado = "PRONTO";
            TempoExecucao = new Stopwatch();
            TempoEspera = new Stopwatch();
        }

        // Questão: 
        // Fazer com que um processo tenha sua prioridade alterada em meio à sua execução automaticamente,
        // gerando perda ou ganho de prioridade dado ao tempo de execução ou espera.

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

        // Questão:
        // O escalonador deve implementar alguma política de promoção ou rebaixamento automáticos de prioridades
        // de processos:

        // Regra 1 - Eleva a prioridade caso seja menor do que 5
        // Regra 2 - Reduz a prioridade caso seja maior que 1
        public int ElevarPrioridade()
        {
            if (Prioridade < 5)
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
}
