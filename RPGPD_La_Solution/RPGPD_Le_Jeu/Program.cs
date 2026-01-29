namespace RPGPD_Le_Jeu
{
    internal class Program
    {

        // Constantes

        static void Main(string[] args)
        {
            // Variables
            int choixjoueur;
            int HP = 0;
            int MP = 0;

            // Début du programme
            do
            {
                Intro();
                choixjoueur = Menu();



            } while (true);
        }

        // Fonctions

        // Début fonction Introduction
        private static void Intro()
        {
            Console.WriteLine("Hello you... (Introduction)");
        }
        // Fin fonction Introduction

        // Début fonction Menu
        private static int Menu()
        {
            int choix = 0;
            string choixlu = "";
            do
            {
                Console.WriteLine("1 = Play");
                Console.WriteLine("2 = Quit");
                Console.WriteLine("3 = Do a backflip");
                choixlu = Console.ReadLine();
                if (choixlu != "1" && choixlu != "2" && choixlu != "3")
                { Console.WriteLine("Tes con"); }
            } while (choixlu != "1" && choixlu != "2" && choixlu != "3");
            return choix;
        }
        // Fin fonction Menu
    }
}
