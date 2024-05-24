using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace CampeonatoFutebol
{
    public class Program
    {
        static string connectionString = "Server=127.0.0.1; Database=DBCampeonatoFutebol; User Id=sa; Password=SqlServer2019!; ";

        static void Main(string[] args)
        {
            bool exit = false;
            bool jogosCadastrados = VerificarJogosCadastrados();
            bool jogosJaGerados = jogosCadastrados;
            bool timesCadastrados = VerificarTimesCadastrados();

            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("--=== MENU CAMPEONATO DE FUTEBOL ===--\n");
                Console.WriteLine("1  - Cadastrar Time");
                Console.WriteLine("2  - Gerar Jogos");
                if (jogosCadastrados)
                {
                    Console.WriteLine("3  - Exibir Campeão");
                    Console.WriteLine("4  - Exibir Top 5 Times");
                    Console.WriteLine("5  - Exibir Time com Mais Gols");
                    Console.WriteLine("6  - Exibir Time que Tomou Mais Gols");
                    Console.WriteLine("7  - Exibir Jogo com Mais Gols");
                    Console.WriteLine("8  - Exibir Maior Número de Gols em Um Jogo por Time");
                    Console.WriteLine("9  - Imprimir Times do Campeonato");
                    Console.WriteLine("10 - Cancelar Campeonato");
                }
                Console.WriteLine("0  - Sair");
                Console.Write("\nSelecione uma opção: ");
                string opc = Console.ReadLine();
                Console.Clear();

                switch (opc)
                {
                    case "1":
                        CadastrarTime();
                        timesCadastrados = true;
                        break;
                    case "2":
                        if (!jogosJaGerados && timesCadastrados)
                        {
                            GerarJogos();
                            Console.WriteLine("Os jogos foram gerados com sucesso!");
                            Console.WriteLine("\nOs times finalizaram o campeonato!");
                            jogosCadastrados = true;
                            jogosJaGerados = true;
                        }
                        else if (jogosJaGerados)
                        {
                            Console.WriteLine("Não é possível gerar mais jogos. Os jogos já foram gerados.");
                        }
                        else if (!timesCadastrados)
                        {
                            Console.WriteLine("Não é possível gerar jogos porque não há times cadastrados.");
                        }
                        break;
                    case "3":
                        if (jogosCadastrados)
                            ExibirCampeao();
                        else
                            Console.WriteLine("Não é possível executar esta opção pois não há jogos cadastrados.");
                        break;
                    case "4":
                        if (jogosCadastrados)
                            ExibirTop5Times();
                        else
                            Console.WriteLine("Não é possível executar esta opção pois não há jogos cadastrados.");
                        break;
                    case "5":
                        if (jogosCadastrados)
                            ExibirTimeComMaisGols();
                        else
                            Console.WriteLine("Não é possível executar esta opção pois não há jogos cadastrados.");
                        break;
                    case "6":
                        if (jogosCadastrados)
                            ExibirTimeQueTomouMaisGols();
                        else
                            Console.WriteLine("Não é possível executar esta opção pois não há jogos cadastrados.");
                        break;
                    case "7":
                        if (jogosCadastrados)
                            ExibirJogoComMaisGols();
                        else
                            Console.WriteLine("Não é possível executar esta opção pois não há jogos cadastrados.");
                        break;
                    case "8":
                        if (jogosCadastrados)
                            ExibirMaiorNumeroDeGolsPorTime();
                        else
                            Console.WriteLine("Não é possível executar esta opção pois não há jogos cadastrados.");
                        break;
                    case "9":
                        if (timesCadastrados)
                            ImprimirTimesCadastrados();
                        else
                            Console.WriteLine("Não há times cadastrados para imprimir.");
                        break;
                    case "10":
                        ResetCampeonato();
                        break;

                    case "0":
                        exit = true;
                        Console.WriteLine("Fechando o programa...");
                        Console.WriteLine("\nPrograma Encerrado.");
                        break;
                    default:
                        Console.WriteLine("Opção inválida. Tente novamente.");
                        break;
                }

                if (!exit)
                {
                    Console.WriteLine("\nPressione qualquer tecla para voltar ao menu...");
                    Console.ReadKey();
                }
            }
        }
        #region MétodosMain
        static bool VerificarTimesCadastrados()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string countQuery = "SELECT COUNT(*) FROM Time";
                using (SqlCommand countCmd = new SqlCommand(countQuery, connection))
                {
                    int timeCount = (int)countCmd.ExecuteScalar();
                    return timeCount > 0;
                }
            }
        }

        static bool VerificarJogosCadastrados()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string countQuery = "SELECT COUNT(*) FROM Jogo";
                using (SqlCommand countCmd = new SqlCommand(countQuery, connection))
                {
                    int jogoCount = (int)countCmd.ExecuteScalar();
                    return jogoCount > 0;
                }
            }
        }

        static void CadastrarTime()
        {
            if (ContarTimesCadastrados() >= 5)
            {
                Console.WriteLine("O número máximo de 5 times já foi cadastrado.");
                return;
            }

            for (int i = 1; i <= 5; i++)
            {
                Console.Clear();
                Console.WriteLine($"Cadastro do Time {i}");
                Console.Write("Nome: ");
                string nome = Console.ReadLine();
                Console.Write("Apelido: ");
                string apelido = Console.ReadLine();
                Console.Write("Data de Criação (yyyy-mm-dd): ");
                DateTime dataCriacao;
                while (!DateTime.TryParse(Console.ReadLine(), out dataCriacao))
                {
                    Console.WriteLine("Formato de data inválido. Por favor, insira a data no formato yyyy-mm-dd.");
                    Console.Write("Data de Criação (yyyy-mm-dd): ");
                }

                var novoTime = new Time(0, nome, apelido, dataCriacao);
                InserirTimeNoBanco(novoTime);
            }
        }

        static int ContarTimesCadastrados()
        {
            int count = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string countQuery = "SELECT COUNT(*) FROM Time";
                using (SqlCommand countCmd = new SqlCommand(countQuery, connection))
                {
                    count = (int)countCmd.ExecuteScalar();
                }
            }
            return count;
        }

        static void InserirTimeNoBanco(Time time)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Time (Nome, Apelido, DataCriacao) VALUES (@Nome, @Apelido, @DataCriacao)";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Nome", time.Nome);
                    cmd.Parameters.AddWithValue("@Apelido", time.Apelido);
                    cmd.Parameters.AddWithValue("@DataCriacao", time.DataCriacao);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        static void GerarJogos()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("GerarJogos", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        static void ExibirCampeao()
        {
            string query = "SELECT TOP 1 Nome, Pontos FROM Time ORDER BY Pontos DESC, GolsMarcados - GolsSofridos DESC";
            ExibirResultado(query, "Campeão");
        }

        static void ExibirTop5Times()
        {
            string query = "SELECT TOP 5 Nome, Pontos FROM Time ORDER BY Pontos DESC, GolsMarcados - GolsSofridos DESC";
            ExibirResultado(query, "Top 5 Times");
        }

        static void ExibirTimeComMaisGols()
        {
            string query = "SELECT TOP 1 Nome, GolsMarcados FROM Time ORDER BY GolsMarcados DESC";
            ExibirResultado(query, "Time com Mais Gols");
        }

        static void ExibirTimeQueTomouMaisGols()
        {
            string query = "SELECT TOP 1 Nome, GolsSofridos FROM Time ORDER BY GolsSofridos DESC";
            ExibirResultado(query, "Time que Tomou Mais Gols");
        }

        static void ExibirJogoComMaisGols()
        {
            string query = @"
        SELECT TOP 1 
            JogoID,
            (SELECT Nome FROM Time WHERE TimeID = J.TimeCasaID) AS TimeCasa,
            (SELECT Nome FROM Time WHERE TimeID = J.TimeVisitanteID) AS TimeVisitante,
            GolsCasa,
            GolsVisitante,
            (GolsCasa + GolsVisitante) AS TotalGols
        FROM 
            Jogo J
        ORDER BY 
            TotalGols DESC";
            ExibirResultado(query, "Jogo com Mais Gols");
        }

        static void ExibirMaiorNumeroDeGolsPorTime()
        {
            string query = @"
        SELECT 
            TimeID,
            (SELECT Nome FROM Time WHERE TimeID = Combined.TimeID) AS NomeTime,
            MAX(Gols) AS MaxGols
        FROM 
            (
            SELECT 
                TimeCasaID AS TimeID,
                GolsCasa AS Gols
            FROM 
                Jogo
            UNION ALL
            SELECT 
                TimeVisitanteID AS TimeID,
                GolsVisitante AS Gols
            FROM 
                Jogo
            ) AS Combined
        GROUP BY 
            TimeID";
            ExibirResultado(query, "Maior Número de Gols em Um Jogo por Time");
        }

        static void ExibirResultado(string query, string descricao)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        Console.WriteLine($"\n{descricao}:\n");
                        while (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                Console.Write($"{reader.GetName(i)}: {reader[i]} ");
                            }
                            Console.WriteLine();
                        }
                    }
                }
            }
        }

        static void ImprimirTimesCadastrados()
        {
            string query = "SELECT TimeID, Nome FROM Time";
            ExibirResultado(query, "Times Cadastrados");
        }

        static void ResetCampeonato()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

               
                string deleteJogoQuery = "DELETE FROM Jogo";
                using (SqlCommand deleteJogoCmd = new SqlCommand(deleteJogoQuery, connection))
                {
                    deleteJogoCmd.ExecuteNonQuery();
                }

                
                string deleteTimeQuery = "DELETE FROM Time";
                using (SqlCommand deleteTimeCmd = new SqlCommand(deleteTimeQuery, connection))
                {
                    deleteTimeCmd.ExecuteNonQuery();
                }

                
                string resetIdentityQuery = "DBCC CHECKIDENT ('Jogo', RESEED, 0); DBCC CHECKIDENT ('Time', RESEED, 100)";
                using (SqlCommand resetIdentityCmd = new SqlCommand(resetIdentityQuery, connection))
                {
                    resetIdentityCmd.ExecuteNonQuery();
                }
                Console.WriteLine("O campeonato foi cancelado!");
                Console.WriteLine("Todos os registros cadastrados foram deletados.");
                Console.ReadLine();
                Console.Clear();
                Main(new string[] { });
            }
        }
        #endregion

    }
}
