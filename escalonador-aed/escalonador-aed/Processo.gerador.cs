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
        string nomeArquivo = "dados_AED_SO_TI";

        // Construtor
        public ProcessoGerador()
        {
            if (File.Exists(nomeArquivo))
            {
                reader = new StreamReader(nomeArquivo);
            }
        }
    }
}
