
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using static System.Collections.Specialized.BitVector32;


namespace Bibliotek
{
    internal class BiblioteksProgram
    {
        /* Ändra dessa utom programmet installeras på en annan dator*/
        private const string USERS_FILE = "C:\\Users\\isak.hermansson2\\source\\repos\\Biblio\\users.txt";
        private const string BOOK_DATA_FILE = "C:\\Users\\isak.hermansson2\\source\\repos\\Biblio\\bookdata.txt";
        private const string BORROWED_BOOKS_FILE = "C:\\Users\\isak.hermansson2\\source\\repos\\Biblio\\borrowedbooks.txt";

        static void Main(string[] args)
        {
            MainMenu();
        }

        /*
         * Registera en användare.
         * Låter användaren mata in sina uppgifter
         * Användaren sparas om alla uppgifter har matats in
         */
        static void RegistrationPage()
        {
            Console.WriteLine("För att registrera dig ange förnamn, efternamn, personnummer och lösenord.");
            Console.WriteLine("");

            Console.Write("Förnamn: ");
            var firstName = Console.ReadLine();

            Console.Write("Efternamn: ");
            var lastName = Console.ReadLine();

            Console.Write("Personnummer: ");
            var personalNumber = Console.ReadLine();

            Console.Write("Lösenord: ");


            var password = Console.ReadLine();

            Console.WriteLine("");

            if (UserInfoIncomplete(firstName, lastName, personalNumber, password))
            {
                Console.Clear();
                Console.WriteLine("Ange riktiga uppgifter för att kunna registrera dig.");
                Console.WriteLine("");
                RegistrationPage();
                return;
            }
            else if (UserRegistered(personalNumber))
            {
                Console.Clear();
                Console.WriteLine("Du är redan registrerad. Ange ett nytt personnummer för att registrera dig.");
                Console.WriteLine("");
                RegistrationPage();
                return;
            }

            var line = $"{firstName} {lastName} {personalNumber} {password}";
            string[] lines = { line };

            File.AppendAllLines(USERS_FILE, lines);

            Console.WriteLine("Du är nu registrerad och kan logga in.");
            return;
        }


        /*
         * Huvudmeny där man kan logga in, registrera användare och hantera sina böcker.
         */
        static void MainMenu()
        {
            String user = null;
            Console.Clear();
            // Visa menyn tills användaren väljer Avsluta (4)
            while (true)
            {
               
                Console.WriteLine("");
                Console.WriteLine("Huvudmeny");
                Console.WriteLine("1. logga in");
                Console.WriteLine("2. Registrera användare");
                Console.WriteLine("3. Låna/Lämna tillbaka böcker");
                Console.WriteLine("4. Avsluta");
                Console.WriteLine("");
                Console.Write("Ange ett menyval: ");
                String choice = Console.ReadLine();

                // kontrollera att användare har matat in ett nummer
                int action;
                bool isNumber = int.TryParse(choice, out action);
                if (isNumber == false)
                {
                    Console.WriteLine("Vänligen ange ett nummer mellan 1 och 4.");
                    continue;
                }
                switch (action)
                {
                    case 1:
                        user = Login();
                        if (user != null)
                        {
                            Console.Clear();
                            Console.WriteLine("");
                            Console.WriteLine("Välkommen " + user);
                            Console.WriteLine("");
                        }
                        break;
                    case 2:
                        RegistrationPage();
                        break;
                    case 3:                        if (user == null)
                        {
                            Console.WriteLine("");
                            Console.WriteLine("Vänligen logga in först");
                            Console.WriteLine("");
                        }
                        else
                        {
                            BooksMenu(user);
                            Console.Clear();
                        }
                        break;
                    case 4:
                        return;
                    default:
                        Console.WriteLine("Vänligen ange ett nummer mellan 1 och 4.");
                        continue;
                }
            }
        }

        /*
         * Visar bokmenyn där man kan låna och lämna tillbaka böcker
         */
        static void BooksMenu(String user)
        {
            Console.Clear();

            // Visa menyn tills användaren väljer att avsluta (val 5)
            while (true)
            {
                Console.WriteLine("");
                Console.WriteLine("Bibiloteksmeny");
                Console.WriteLine("1. Sök efter en bok");
                Console.WriteLine("2. Visa mina lånade böcker");
                Console.WriteLine("3. Låna en bok");
                Console.WriteLine("4. Lämna tillbaka en bok");
                Console.WriteLine("5. Tillbaka till huvudmeny");
                Console.WriteLine("");
                Console.Write("Ange ett menyval: ");
                String choice = Console.ReadLine();

                //Kontrollera att användaren har matat in ett nummer
                int action;
                bool isNumber = int.TryParse(choice, out action);
                if (isNumber == false)
                {
                    Console.WriteLine("Vänligen ange ett nummer mellan 1 och 5.");
                    continue;
                }
                switch (action)
                {
                    case 1:
                        Search();
                        break;
                    case 2:
                        ShowBorrowedBooks(user);
                        break;
                    case 3:
                        BorrowBooks(user);
                        break;
                    case 4:
                        ReturnBook(user);
                        break;
                    case 5:
                        return;
                    default:
                        Console.WriteLine("Vänligen ange ett nummer mellan 1 och 5.");
                        continue;
                }
            }
        }

        /*
         *  Funktion som låter användaren låna en bok.
         */
        static void BorrowBooks(String user)
        {
            Boolean bookFound = false;
            int book = -1;

            // Fortsätt tills vi har hittat en bok att låna eller att användaren väljer att avbryta
            while (!bookFound)
            {
                Console.Write("Skriv boknummer för den bok du vill låna ('q' för att avbryta) : ");
                String choice = Console.ReadLine();
                
                if (choice == "q")
                {
                    return;
                }

                // kontroller att användaren har matat in en siffra
                bool isNumber = int.TryParse(choice, out book);
                if (isNumber == false)
                {
                    Console.WriteLine("");
                    Console.WriteLine("Vänligen ange boknummer som en siffra.");
                    continue;
                }
                book = Convert.ToInt32(choice);

                // Kontrollera att boken finns
                if (BookExists(book) == false)
                {
                    Console.WriteLine("");
                    Console.WriteLine("Hittar ej bok med boknummer {0}", book);
                    continue;
                }

                // Kontrollera att användaren inte redan har lånat boken
                String[] borrowedBooks = getBorrowedBooks(user);
                for (int i = 0; i < borrowedBooks.Length; i++)
                {
                    if (Convert.ToInt32(borrowedBooks[i]) == book)
                    {
                        Console.WriteLine("");
                        Console.WriteLine("Du har redan bok nummer {0} utlånad", book);
                        return;
                    }
                }

                // Nu har vi kontrollerat att boken finns och att användaren inte redan har lånat boken. Så sätt bokkFound = true
                bookFound = true;

            }

            // Spara information om lånet i BORROWED_BOOKS_FILE
            StreamWriter writer = File.AppendText(BORROWED_BOOKS_FILE);
            writer.WriteLine(user + "\t" + book);
            writer.Close();

            Console.WriteLine("");
            Console.WriteLine("Du har nu lånat bok nummer {0}", book);
        }

        /*
         *  Kontrollerar om en bok finns
         *   Kontrollerar om angivet boknummer finns i BOOK_DATA_FILE
       */
        static Boolean BookExists(int book)
        {
            string[] books = System.IO.File.ReadAllLines(BOOK_DATA_FILE);
            Boolean bookExists = false;
            for (int i = 1; i < books.Length; i++)
            {
                String[] parts = books[i].Split('\t');
                if (Convert.ToInt32(parts[0]) == book)
                {
                    return true;
                }
            }
            return false;
        }


        /*
         * Skriver ut de böcker som användaren user har lånat
        */
        static void ShowBorrowedBooks(String user)
        {
            // Hämta alla användaren users lånade böcker. Arrayen innehåller boknummer
            String[] borrowedBooks = getBorrowedBooks(user);

            // Hämta alla böcker som finns i biblioteket
            String[] allBooks = System.IO.File.ReadAllLines(BOOK_DATA_FILE);

            if (borrowedBooks.Length == 0)
            {
                Console.WriteLine("\nDu har inga lånade böcker.\n");
                return;
            }

            Console.WriteLine("\nDu har lånat följande böcker:\n");
            String header = String.Format("{0,-7} {1,-35} {2,-25} {3, -20} {4,-8}", "Nummer", "Boktitel", "Författare", "Ämne", "ISBN");
            Console.WriteLine(header);

            // Gå igenom alla böcker och kontrollera om boken finns i listan på böcker som user har lånat
            for (int i = 1; i < allBooks.Length; i++)
            {
                String[] parts = allBooks[i].Split('\t');

                // Kontrollera om user har lånat boken
                if (borrowedBooks.Contains(parts[0]))
                {
                    String data = String.Format("{0,-7} {1,-35} {2,-25} {3, -20} {4,-8}", parts[0], parts[1], parts[2], parts[3], parts[4]);
                    Console.WriteLine(data);
                }
            }
        }

        /* 
         * Lämna tillbaka bok
         */ 
        static void ReturnBook(String user)
        {
            int book = -1;
            Boolean bookFound = false;

            // Fortsätt tills användaren user har lämnat tillbaka en bok eller valt att avsluta
            while (bookFound == false)
            {
                ShowBorrowedBooks(user);
                Console.WriteLine("");
                Console.Write("Ange boknummer för den bok du vill du lämna tillbaka ('q' för att avsluta): ");
                String choice = Console.ReadLine();

                if (choice == "q")
                {
                    return;
                }

                // kontrollera att användaren matat in en siffra
                bool isNumber = int.TryParse(choice, out book);
                if (isNumber == false)
                {
                    Console.WriteLine("");
                    Console.WriteLine("Vänligen ange boknummer som en siffra.");
                    continue;
                }

                // Kontrollera att användaren angett en bok som finns
                if (BookExists(book) == false)
                {
                    Console.WriteLine("");
                    Console.WriteLine("Angiven bok existerar inte.");
                    continue;
                }
                // Alla tester gjorda sätt bookFound till true
                bookFound = true;
            }

            // Läs in alla lånade böcker och ta sedan bort filen
            String[] allBorrowedBooks = System.IO.File.ReadAllLines(BORROWED_BOOKS_FILE);
            File.Delete(BORROWED_BOOKS_FILE);

            // Skapa en ny tom fil för lånade böcker
            FileStream fileSteam = File.Create(BORROWED_BOOKS_FILE);
            StreamWriter writer = new StreamWriter(fileSteam);

            // Skriv tillbaka alla lånade böcker utom den vi skall ta bort
            for (int i = 0; i < allBorrowedBooks.Length; i++)
            {
                String[] parts = allBorrowedBooks[i].Split('\t');
                if (parts[0] == user && Convert.ToInt32(parts[1]) == book)
                {
                }
                else
                {
                    writer.WriteLine(parts[0] + "\t" + parts[1]);
                }
            }
            writer.Close();
            Console.WriteLine("");
            Console.WriteLine("Bok nummer {0} tillbakalämnad.", book);
        }

        /*
         * Hämtar alla böcker som användaren user har lånat
         * Returnerar Array med boknummer
         */
        static String[] getBorrowedBooks(String user)
        {
            // Använder en lista som sedan görs om till array
            List<String> borrowedBooks = new List<string>();
            string[] allBorrowed = System.IO.File.ReadAllLines(BORROWED_BOOKS_FILE);
            for (int i = 0; i < allBorrowed.Length; i++)
            {
                String line = allBorrowed[i];
                String[] parts = line.Split('\t');
                
                if (parts[0] == user)
                {
                    borrowedBooks.Add(parts[1]);
                }
            }
            return borrowedBooks.ToArray();
        }

        /*
         * Låter användaren logga in.
         * Passord kontrolleras och om det stämmer returneras användar id (personnummer)
         */ 
        static String Login()
        {
            Console.Clear();
            Console.WriteLine("Ange personnummer och passord för att logga in. q för att avbryta");
            Console.WriteLine("");

            while (true)
            {
                Console.Write("Personnummer: ");
                var personalNumber = Console.ReadLine();

                if (personalNumber == "q")
                {
                    return null;
                }
                Console.Write("Lösenord: ");

                var password = Console.ReadLine();

                if (isPasswordCorrect(personalNumber, password))
                {
                    Console.WriteLine("");
                    Console.WriteLine("Välkommen " + personalNumber);
                    Console.WriteLine("");
                    return personalNumber;
                }
                else
                {
                    Console.WriteLine("Fel passord. Vänligen försök igen");
                    Console.WriteLine("");
                }

            }
        }

        /*
         * Kontrollerar om en användare finns registerad
         * Returnerar true om användaren finns. Annars false
         */ 
        static bool UserRegistered(string personalNumber)
        {
            string[] users = System.IO.File.ReadAllLines(USERS_FILE);

            for (int i = 0; i < users.Length; i++)
            {
                var line = users[i].Trim();
                string[] parts = line.Split(' ');

                var currentPersonalNumber = parts[2];

                if (currentPersonalNumber == personalNumber)
                {
                    return true;
                }
            }

            return false;
        }

        /*
         * Kontrollerar om korrekt passord har angetts
         * returnerar true om det är korrekt annars false
         */ 
        static bool isPasswordCorrect(string personalNumber, String password)
        {
            string[] users = System.IO.File.ReadAllLines(USERS_FILE);

            for (int i = 0; i < users.Length; i++)
            {
                var line = users[i].Trim();
                string[] parts = line.Split(' ');

                var currentPersonalNumber = parts[2];


                if (currentPersonalNumber == personalNumber)
                {
                    var currentPassword = parts[3];
                    if (currentPassword == password)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        /*
         * Kontrollerar att alla 
         */ 
        static bool UserInfoIncomplete(string firstName, string lastName, string personalNumber, string password)
        {
            if (firstName == null || firstName == "") return true;
            if (lastName == null || lastName == "") return true;
            if (personalNumber == null || personalNumber == "") return true;
            if (password == null || password == "") return true;

            return false;
        }

        /*
         * Sök i "bokdatabasen"
         * Sökning görs med wildcard (contains= i all data som finns om böcker
         * dvs man ka söka på titel, författare, ämn och isbn
         * Träffar i sökningen skrivs ut
         */ 
        static void Search()
        {
            Console.Write("Ange bok eller författare att söka efter: ");
            var searchTerm = Console.ReadLine();
            string[] books = System.IO.File.ReadAllLines(BOOK_DATA_FILE);

            Boolean bookFound = false;

            // Gå igenom alla rader i BOOK_DATA_FILE
            for (int i = 1; i < books.Length; i++)
            {
                var line = books[i].Trim();
                string[] parts = line.Split('\t');

                // Kontrollera om någon data på raden innehåller söktexten
                if (parts[1].Contains(searchTerm) || parts[2].Contains(searchTerm) || parts[3].Contains(searchTerm) || parts[4].Contains(searchTerm))
                {
                    if (bookFound == false)
                    {
                        Console.WriteLine("");
                        String header = String.Format("{0,-7} {1,-35} {2,-25} {3, -20} {4,-8}", "Nummer", "Boktitel", "Författare", "Ämne", "ISBN");
                        Console.WriteLine(header);
                    }
                    String data = String.Format("{0,-7} {1,-35} {2,-25} {3, -20} {4,-8}", parts[0], parts[1], parts[2], parts[3], parts[4]);
                    Console.WriteLine(data);
                    bookFound = true;
                }
            }
        }
    }
}