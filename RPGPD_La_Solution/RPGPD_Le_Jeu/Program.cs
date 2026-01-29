namespace RPGPD_Le_Jeu
{
    internal class Program
    {

        // Constantes

        static void Main(string[] args)
        {
            // Variables
            int choixjoueur;

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
            Console.WriteLine("1 = Play");
            Console.WriteLine("2 = Quit");
            Console.WriteLine("3 = Do a backflip");



            return choix;
        }
        // Fin fonction Menu
    }
}
