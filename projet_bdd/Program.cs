using System;
using System.IO;
using MySql.Data.MySqlClient;
namespace SalleSportApp
{
    internal class Program
    {
        
        // Server : le nom du serveur (localhost ou 127.0.0.1)
        // User Id / Password : le compte MySQL
        private const string connectionString =
            "Server=localhost;User Id=root;Password=monMotDePasse;";

        private const string sqlFileName = "script_salle_sport.sql";

        static void Main(string[] args)
        {
            try
            {
                // on charge le script sql
                string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                string sqlPath = Path.Combine(exeDir, sqlFileName);

                if (!File.Exists(sqlPath))
                {
                    Console.WriteLine($"Fichier SQL introuvable : {sqlPath}");
                    return;
                }

                string sqlScript = File.ReadAllText(sqlPath);

                // on execute le script (création BDD + tables + INSERT)
                ExecuteSqlScript(sqlScript);

                Console.WriteLine("Script SQL exécuté avec succès.");

                // 3) Exemple : afficher tous les membres
                AfficherMembres();

                Console.WriteLine("Terminé. Appuyez sur une touche pour quitter.");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur : " + ex.Message);
            }
        }

        /// <summary>
        /// Exécute un script SQL complet (plusieurs commandes séparées par ';').
        /// </summary>
        private static void ExecuteSqlScript(string script)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                // Découper grossièrement sur ';'
                string[] commandes = script.Split(
                    new[] { ';' },
                    StringSplitOptions.RemoveEmptyEntries);

                foreach (var cmdTextRaw in commandes)
                {
                    string cmdText = cmdTextRaw.Trim();
                    if (string.IsNullOrWhiteSpace(cmdText))
                        continue;

                    using (var cmd = new MySqlCommand(cmdText, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// Exemple simple de requête SELECT sur la table Membre.
        /// </summary>
        private static void AfficherMembres()
        {
            // Si ton script fait USE salle_sport; la base est déjà sélectionnée.
            // Sinon, ajoute "Database=salle_sport;" dans la connectionString.
            using (var conn = new MySqlConnection(connectionString + "Database=salle_sport;"))
            {
                conn.Open();

                string sql = "SELECT ID_Membre, Nom, Prenom, Mail FROM Membre ORDER BY ID_Membre;";

                using (var cmd = new MySqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    Console.WriteLine("Liste des membres :");
                    while (reader.Read())
                    {
                        int id = reader.GetInt32("ID_Membre");
                        string nom = reader.GetString("Nom");
                        string prenom = reader.GetString("Prenom");
                        string mail = reader.IsDBNull(reader.GetOrdinal("Mail"))
                                      ? ""
                                      : reader.GetString("Mail");

                        Console.WriteLine($"{id} - {nom} {prenom} ({mail})");
                    }
                }
            }
        }
    }
}
