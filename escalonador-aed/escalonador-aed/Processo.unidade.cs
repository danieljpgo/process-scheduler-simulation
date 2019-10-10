using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace escalonador_aed
{
    class ProcessoUnidade
    {
        // Atributos
        public Processo Processo { get; set; }
        public ProcessoUnidade Proximo { get; set; }

        // Construtor
        public ProcessoUnidade(Processo processo)
        {
            Processo = processo;
        }

        public ProcessoUnidade()
        {

        }
    }
}
