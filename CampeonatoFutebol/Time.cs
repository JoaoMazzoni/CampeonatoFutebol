using System;

namespace CampeonatoFutebol
{
    class Time
    {
        public int TimeID { get; set; }
        public string Nome { get; set; }
        public string Apelido { get; set; }
        public DateTime DataCriacao { get; set; }

        public Time(int timeID, string nome, string apelido, DateTime dataCriacao)
        {
            TimeID = timeID;
            Nome = nome;
            Apelido = apelido;
            DataCriacao = dataCriacao;
        }
    }
}
