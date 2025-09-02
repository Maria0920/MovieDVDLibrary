using System;
using System.IO;
using System.Text.RegularExpressions;


namespace CommunityLibrarySystem
{
    public class Movie
    {
        private string titleOfMovie;
        private string genreOfMovie;
        private string classification;
        private int MovieDuration;
        private int availableNoOfCopies;
        private int countingBorrowOfMovie;  // Total borrow count
        private int currentBorrowCount;     // Current borrow count
        private int yearOfMovie;  // Added year field

        private static readonly string[] availableGenres = { "drama", "family", "animated", "adventure", "sci-fi", "action", "comedy", "thriller", "other" };
        private static readonly string[] classificationsAvailable = { "G", "PG", "M15+", "MA15+" };

        public Movie(string title, string genre, string classification, int duration, int availableNoofcopies, int year)
        {
            this.titleOfMovie = title;
            this.genreOfMovie = genre;
            this.classification = classification;
            this.MovieDuration = duration;
            this.availableNoOfCopies = availableNoofcopies;
            this.countingBorrowOfMovie = 0;
            this.currentBorrowCount = 0;
            this.yearOfMovie = year;
        }

        // For encapsulated fields, We are using Public getter and setter methods 

        public string gettingTitle() => titleOfMovie;
        public void settingTitle(string value) => titleOfMovie = value;

        //public property for genreOfMovie
        public string gettingGenre() => genreOfMovie;
        public void settingGenre(string value)
        {
            if (Array.Exists(availableGenres, g => g.Equals(value, StringComparison.OrdinalIgnoreCase)))
            {
                genreOfMovie = value;
            }
            else
            {
                throw new ArgumentException($"Please make sure the genre is one of the following: {string.Join(", ", availableGenres)}");
            }
        }

        //public property for classification
        public string gettingClassification() => classification;
        public void settingClassification(string value)
        {
            if (Array.Exists(classificationsAvailable, c => c.Equals(value, StringComparison.OrdinalIgnoreCase)))
            {
                classification = value;
            }
            else
            {
                throw new ArgumentException($"Please make sure the classification is one of the following: {string.Join(", ", classificationsAvailable)}");
            }
        }
        //getter and setter for MovieDuration, available number of copies and borrow count in the system
        public int gettingDuration() => MovieDuration;
        public void settingDuration(int value) => MovieDuration = value;

        public int gettingCopiesAvailable() => availableNoOfCopies;
        public void settingCopiesAvailable(int value) => availableNoOfCopies = value;

        public int gettingBorrowCount() => countingBorrowOfMovie;
        public int gettingCurrentBorrowCount() => currentBorrowCount;
        public void BorrowCountIncrement()
        {
            countingBorrowOfMovie++;
            currentBorrowCount++;
        }
        public void CurrentBorrowCountDecrement()
        {
            if (currentBorrowCount > 0)
            {
                currentBorrowCount--;
            }
        }
        public void setBorrowCount(int total, int current)
        {
            countingBorrowOfMovie = total;
            currentBorrowCount = current;
        }
        public void decrementAvailableCopies() => availableNoOfCopies--;
        public void incrementAvailableCopies() => availableNoOfCopies++;

        public int gettingYear() => yearOfMovie;
        public void settingYear(int value) => yearOfMovie = value;

        //String representation of the movie is provided by overriding the ToString method
        public override string ToString()
        {
            return $"{titleOfMovie} ({yearOfMovie}), {genreOfMovie}, {classification}, {MovieDuration} min, {availableNoOfCopies}: available no of copies, Total borrowed: {countingBorrowOfMovie} times, Currently borrowed: {currentBorrowCount} times";
        }
    }

    public class MovieCollection
    {
        private const int fixedTableSize = 8003;
        private Movie[] movies;
        private bool[] occupied;
        private const string MOVIES_FILE = "movies.txt";

        private string NormalizeKey(string title, int year)
        {
            // Remove "the", "a", "an" and all non-alphanumeric characters
            string cleanedTitle = Regex.Replace(title.ToLower(), @"\b(the|a|an)\b|\W", "");
            return cleanedTitle + year;
        }


        /// Returns true if we already have 1000 movies in the table.
        public bool IsFull()
        {
            int count = 0;
            foreach (bool slot in occupied)
                if (slot)
                    count++;
            return count >= 1000;
        }



        public MovieCollection()
        {
            movies = new Movie[fixedTableSize];
            occupied = new bool[fixedTableSize];
            everUsed = new bool[fixedTableSize];    
            isDeleted = new bool[fixedTableSize];    
            LoadMoviesFromFile();
        }

        public Movie[] gettingTopThreeBorrowedMoviesByGenre(string genre)
        {
            Movie best1 = null, best2 = null, best3 = null;

            for (int i = 0; i < fixedTableSize; i++)
            {
                Movie m = movies[i];
                if (m == null) continue;
                if (!m.gettingGenre().Equals(genre, StringComparison.OrdinalIgnoreCase))
                    continue;

                int count = m.gettingBorrowCount();
                if (best1 == null || count > best1.gettingBorrowCount())
                {
                    best3 = best2;
                    best2 = best1;
                    best1 = m;
                }
                else if (best2 == null || count > best2.gettingBorrowCount())
                {
                    best3 = best2;
                    best2 = m;
                }
                else if (best3 == null || count > best3.gettingBorrowCount())
                {
                    best3 = m;
                }
            }

            // pack into array
            int n = (best1 != null ? 1 : 0)
                  + (best2 != null ? 1 : 0)
                  + (best3 != null ? 1 : 0);
            Movie[] top3 = new Movie[n];
            int idx = 0;
            if (best1 != null) top3[idx++] = best1;
            if (best2 != null) top3[idx++] = best2;
            if (best3 != null) top3[idx++] = best3;
            return top3;
        }





        private void LoadMoviesFromFile()
        {
            try
            {
                if (File.Exists(MOVIES_FILE))
                {
                    string[] lines = File.ReadAllLines(MOVIES_FILE);
                    foreach (string line in lines)
                    {
                        string[] parts = line.Split('|');
                        if (parts.Length == 8)  // Updated to include borrow counts
                        {
                            string title = parts[0];
                            string genre = parts[1];
                            string classification = parts[2];
                            int duration = int.Parse(parts[3]);
                            int copies = int.Parse(parts[4]);
                            int year = int.Parse(parts[5]);
                            int totalBorrows = int.Parse(parts[6]);
                            int currentBorrows = int.Parse(parts[7]);

                            Movie movie = new Movie(title, genre, classification, duration, copies, year);
                            movie.setBorrowCount(totalBorrows, currentBorrows);
                            addMovieInSystem(movie);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading movies from file: {ex.Message}");
            }
        }

        public void SaveMoviesToFile()
        {
            // 1) count non-null slots
            int count = 0;
            for (int i = 0; i < fixedTableSize; i++)
                if (movies[i] != null)
                    count++;

            // 2) allocate
            string[] lines = new string[count];

            // 3) fill
            int idx = 0;
            for (int i = 0; i < fixedTableSize; i++)
            {
                Movie m = movies[i];
                if (m != null)
                {
                    lines[idx++] = $"{m.gettingTitle()}|{m.gettingGenre()}|{m.gettingClassification()}|{m.gettingDuration()}|{m.gettingCopiesAvailable()}|{m.gettingYear()}|{m.gettingBorrowCount()}|{m.gettingCurrentBorrowCount()}";
                }
            }

            // 4) write
            File.WriteAllLines(MOVIES_FILE, lines);
        }


        // Heapify for heap sort
        private static void Heapify<T>(T[] arr, int n, int i, Comparison<T> comp)
        {
            int largest = i;
            int left = 2 * i + 1;
            int right = 2 * i + 2;

            if (left < n && comp(arr[left], arr[largest]) > 0)
                largest = left;
            if (right < n && comp(arr[right], arr[largest]) > 0)
                largest = right;

            if (largest != i)
            {
                var tmp = arr[i];
                arr[i] = arr[largest];
                arr[largest] = tmp;
                Heapify(arr, n, largest, comp);
            }
        }

        // Generic heap sort
        private static void HeapSort<T>(T[] arr, Comparison<T> comp)
        {
            int n = arr.Length;
            for (int i = n / 2 - 1; i >= 0; i--)
                Heapify(arr, n, i, comp);

            for (int i = n - 1; i > 0; i--)
            {
                var tmp = arr[0];
                arr[0] = arr[i];
                arr[i] = tmp;
                Heapify(arr, i, 0, comp);
            }
        }

        //using double hashing technique to avoid collision
        private int HashNoOne(string key)
        {
            int hash = 0;
            key = key.ToLower();
            foreach (char c in key)
            {
                hash = (hash * 31 + c) % fixedTableSize;
            }
            return hash;
        }

        private int HashNoTwo(string key)
        {
            int hash = 0;
            key = key.ToLower();
            foreach (char c in key)
            {
                hash = (hash * 17 + c) % (fixedTableSize - 1);
            }
            return hash + 1;
        }


        public void addMovieInSystem(Movie movie)
        {
            string key = NormalizeKey(movie.gettingTitle(), movie.gettingYear());
            int index = HashNoOne(key);
            int sizeOfStep = HashNoTwo(key);

            while (occupied[index])
            {
                index = (index + sizeOfStep) % fixedTableSize;
            }

            movies[index] = movie;
            occupied[index] = true;
            everUsed[index] = true;
            isDeleted[index] = false;
            SaveMoviesToFile(); // Save after adding a movie
        }

        public Movie? gettingMovie(string title)
        {
            string key = title.ToLower();
            int index = HashNoOne(key);
            int theStepSize = HashNoTwo(key);

            while (occupied[index])
            {
                if (movies[index] != null && movies[index].gettingTitle().ToLower() == key)
                {
                    return movies[index];
                }
                index = (index + theStepSize) % fixedTableSize;
            }

            return null;
        }


        bool[] everUsed;
        bool[] isDeleted;

        public void removingMoviefromsystem(string title, int year)
        {
            var key = NormalizeKey(title, year);
            int idx = HashNoOne(key);
            int step = HashNoTwo(key);

            // Probe while slot was ever used (either occupied or deleted)
            while (everUsed[idx])
            {
                var m = movies[idx];
                if (m != null
                    && m.gettingTitle().Equals(title, StringComparison.OrdinalIgnoreCase)
                    && m.gettingYear() == year)
                {
                    movies[idx] = null;
                    occupied[idx] = false;
                    isDeleted[idx] = true;   // mark as tombstone
                    SaveMoviesToFile();
                    return;
                }
                idx = (idx + step) % fixedTableSize;
            }
        }



        //get all movies
        public Movie[] getAllMoviesInSystem()
        {
            // 1) Count non‑null slots
            int count = 0;
            foreach (var m in movies)
                if (m != null)
                    count++;

            // 2) Allocate and populate
            Movie[] arr = new Movie[count];
            int idx = 0;
            foreach (var m in movies)
                if (m != null)
                    arr[idx++] = m;

            // 3) Heap‑sort by title ascending
            HeapSort(arr,
                (x, y) => string.Compare(x.gettingTitle(),
                                          y.gettingTitle(),
                                          StringComparison.Ordinal));
            return arr;
        }



        //get the top 3 borrowed movies
        public Movie[] gettingTopThreeBorrowedMovies()
        {
            // Step 0: placeholders for the top‑3
            Movie best1 = null;
            Movie best2 = null;
            Movie best3 = null;

            // Step 1: scan the entire hash table
            for (int i = 0; i < fixedTableSize; i++)
            {
                Movie m = movies[i];
                if (m == null) continue;

                int c = m.gettingBorrowCount();
                if (best1 == null || c > best1.gettingBorrowCount())
                {
                    // new #1, shift old #1 → #2, #2 → #3
                    best3 = best2;
                    best2 = best1;
                    best1 = m;
                }
                else if (best2 == null || c > best2.gettingBorrowCount())
                {
                    // new #2, shift old #2 → #3
                    best3 = best2;
                    best2 = m;
                }
                else if (best3 == null || c > best3.gettingBorrowCount())
                {
                    // new #3
                    best3 = m;
                }
            }

            // Step 2: pack non‑null bests into a result array
            int count = 0;
            if (best1 != null) count++;
            if (best2 != null) count++;
            if (best3 != null) count++;

            Movie[] top3 = new Movie[count];
            int idx = 0;
            if (best1 != null) top3[idx++] = best1;
            if (best2 != null) top3[idx++] = best2;
            if (best3 != null) top3[idx++] = best3;

            return top3;
        }

        public Movie? gettingMovieByTitleAndYear(string title, int year)
        {
            string key = title.ToLower();
            int index = HashNoOne(key);
            int theStepSize = HashNoTwo(key);

            while (occupied[index])
            {
                if (movies[index] != null &&
                    movies[index].gettingTitle().ToLower() == key &&
                    movies[index].gettingYear() == year)
                {
                    return movies[index];
                }
                index = (index + theStepSize) % fixedTableSize;
            }

            return null;
        }

        public Movie[] gettingMoviesByTitle(string title)
        {
            // 1) count matches
            int total = 0;
            string search = title.ToLower();
            for (int i = 0; i < fixedTableSize; i++)
            {
                if (occupied[i] && movies[i] != null
                    && movies[i].gettingTitle().ToLower().Contains(search))
                {
                    total++;
                }
            }

            // 2) allocate
            Movie[] result = new Movie[total];

            // 3) fill
            int idx = 0;
            for (int i = 0; i < fixedTableSize; i++)
            {
                if (occupied[i] && movies[i] != null
                    && movies[i].gettingTitle().ToLower().Contains(search))
                {
                    result[idx++] = movies[i];
                }
            }

            return result;
        }


        public class Member
        {
            private string firstName;
            private string lastName;
            private string phoneNo;
            private string password;
            private Movie[] borrowedMoviesInSystem;
            private int borrowedMoviesCount;

            public Member(string firstName, string lastName, string phoneNumber, string password)
            {
                this.firstName = firstName;
                this.lastName = lastName;
                this.phoneNo = phoneNumber;
                this.password = password;
                this.borrowedMoviesInSystem = new Movie[5];
                this.borrowedMoviesCount = 0;
            }

            // Public getter and setter methods

            public string FirstName() => firstName;
            public void setFirstName(string value) => firstName = value;

            public string LastName() => lastName;
            public void setLastName(string value) => lastName = value;

            public string getPhoneNo() => phoneNo;
            public void setPhoneNo(string value) => phoneNo = value;

            public string getPass() => password;
            public void setPass(string value) => password = value;

            public int getBorrowedMoviesCount() => borrowedMoviesCount;
            public Movie[] GetMoviesBorrowed() => borrowedMoviesInSystem;

            public bool movieBorrow(Movie movie)
            {
                if (borrowedMoviesCount < 5 && !moviesBorrowed(movie.gettingTitle(), movie.gettingYear()))
                {
                    borrowedMoviesInSystem[borrowedMoviesCount++] = movie;
                    return true;
                }
                return false;
            }

            public bool returningMovie(Movie movie)
            {
                for (int n = 0; n < borrowedMoviesCount; n++)
                {
                    if (borrowedMoviesInSystem[n].gettingTitle() == movie.gettingTitle() &&
                        borrowedMoviesInSystem[n].gettingYear() == movie.gettingYear())
                    {
                        for (int m = n; m < borrowedMoviesCount - 1; m++)
                        {
                            borrowedMoviesInSystem[m] = borrowedMoviesInSystem[m + 1];
                        }
                        borrowedMoviesInSystem[--borrowedMoviesCount] = null!;
                        return true;
                    }
                }
                return false;
            }

            public bool moviesBorrowed(string title, int year)
            {
                for (int n = 0; n < borrowedMoviesCount; n++)
                {
                    if (borrowedMoviesInSystem[n].gettingTitle() == title &&
                        borrowedMoviesInSystem[n].gettingYear() == year)
                    {
                        return true;
                    }
                }
                return false;
            }

            // Keep the old method for backward compatibility
            public bool moviesBorrowed(string title)
            {
                for (int n = 0; n < borrowedMoviesCount; n++)
                {
                    if (borrowedMoviesInSystem[n].gettingTitle() == title)
                    {
                        return true;
                    }
                }
                return false;
            }

            public override string ToString()
            {
                return $"{firstName} {lastName} : {phoneNo}";
            }
        }

        public class MemberCollection
        {
            private const int TableSize = 1009;
            private Member[] members;
            private bool[] occupied;

            public MemberCollection()
            {
                members = new Member[TableSize];
                occupied = new bool[TableSize];
            }

            private int HashOne(string key)
            {
                int hash = 0;
                foreach (char c in key.ToLower())
                {
                    hash = (hash * 31 + c) % TableSize;
                }
                return hash;
            }

            private int HashTwo(string key)
            {
                int hash = 0;
                foreach (char c in key.ToLower())
                {
                    hash = (hash * 17 + c) % (TableSize - 1);
                }
                return hash + 1;
            }

            private string CreateKey(string firstName, string lastName)
            {
                return (firstName + ":" + lastName).ToLower();
            }

            public bool addingMember(Member member)
            {
                string key = CreateKey(member.FirstName(), member.LastName());
                int index = HashOne(key);
                int stepSize = HashTwo(key);

                while (occupied[index])
                {
                    if (members[index] != null &&
                        CreateKey(members[index].FirstName(), members[index].LastName()) == key)
                    {
                        Console.WriteLine("Member is already registered.");
                        return false;
                    }
                    index = (index + stepSize) % TableSize;
                }

                members[index] = member;
                occupied[index] = true;
                return true;
            }

            public bool RemoveMember(string firstName, string lastName)
            {
                string key = CreateKey(firstName, lastName);
                int index = HashOne(key);
                int stepSize = HashTwo(key);

                while (occupied[index])
                {
                    if (members[index] != null && CreateKey(members[index].FirstName(), members[index].LastName()) == key)
                    {
                        if (members[index].getBorrowedMoviesCount() == 0)
                        {
                            members[index] = null;
                            occupied[index] = false;
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("As a member please return all the borrowed movies first.");
                            return false;
                        }
                    }
                    index = (index + stepSize) % TableSize;
                }

                Console.WriteLine("This member can't be found in the system.");
                return false;
            }

            public Member? gettingMember(string firstName, string lastName)
            {
                string key = CreateKey(firstName, lastName);
                int index = HashOne(key);
                int stepSize = HashTwo(key);

                while (occupied[index])
                {
                    if (members[index] != null && CreateKey(members[index].FirstName(), members[index].LastName()) == key)
                    {
                        return members[index];
                    }
                    index = (index + stepSize) % TableSize;
                }
                return null;
            }

            public Member[] gettingAllMembers()
            {
                // 1) Count non‑null slots
                int count = 0;
                foreach (var m in members)
                    if (m != null)
                        count++;

                // 2) Allocate and populate
                Member[] result = new Member[count];
                int idx = 0;
                foreach (var m in members)
                    if (m != null)
                        result[idx++] = m;

                return result;
            }
        }


        class Program
        {
            private const string ESC_KEY = "\u001b";  // Escape key code

            private static bool HandleEscKey()
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Escape)
                    {
                        Console.WriteLine("\nReturning to previous menu...");
                        return true;
                    }
                }
                return false;
            }

            private static string ReadLineWithEsc()
            {
                string input = "";
                while (true)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Escape)
                    {
                        Console.WriteLine("\nCancelling input...");
                        return null;
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        Console.WriteLine();
                        return input;
                    }
                    else if (key.Key == ConsoleKey.Backspace && input.Length > 0)
                    {
                        input = input.Substring(0, input.Length - 1);
                        Console.Write("\b \b");
                    }
                    else if (!char.IsControl(key.KeyChar))
                    {
                        input += key.KeyChar;
                        Console.Write(key.KeyChar);
                    }
                }
            }

            static void Main(string[] args)
            {
                try
                {
                    MovieCollection movieCollection = new MovieCollection();
                    MemberCollection memberCollection = new MemberCollection();
                    bool exit = false;

                    while (!exit)
                    {
                        try
                        {
                            Console.WriteLine("===============================================");
                            Console.WriteLine("COMMUNITY LIBRARY MOVIE DVD MANAGEMENT SYSTEM");
                            Console.WriteLine("===============================================");
                            Console.WriteLine("Main Menu");
                            Console.WriteLine("-----------------------------------------------");
                            Console.WriteLine("Select from the following:");
                            Console.WriteLine("1. Staff");
                            Console.WriteLine("2. Member");
                            Console.WriteLine("3. End the program");
                            Console.Write("Enter your choice ==> ");

                            if (HandleEscKey())
                            {
                                exit = true;
                                continue;
                            }

                            string option = Console.ReadLine();

                            switch (option)
                            {
                                case "1":
                                    StaffMenu(movieCollection, memberCollection);
                                    break;
                                case "2":
                                    MemberMenu(movieCollection, memberCollection);
                                    break;
                                case "3":
                                    movieCollection.SaveMoviesToFile();
                                    exit = true;
                                    break;
                                default:
                                    Console.WriteLine("Please try again as the choice is Invalid");
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"There is an error, please try again: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"There is an error, please try again: {ex.Message}");
                }
            }


            static void StaffMenu(MovieCollection movieCollection, MemberCollection memberCollection)
            {
                Console.Write("Please enter your username: ");
                string username = ReadLineWithEsc();
                if (username == null) return;

                Console.Write("Please enter your password: ");
                string password = ReadLineWithEsc();
                if (password == null) return;

                if (username == "staff" && password == "today123")
                {
                    bool exit = false;
                    while (!exit)
                    {
                        try
                        {
                            Console.WriteLine("\nStaff Menu");
                            Console.WriteLine("-----------------------------------------------------");
                            Console.WriteLine("1. Add DVDs to system");
                            Console.WriteLine("2. Remove DVDs from system");
                            Console.WriteLine("3. Register a new member to system");
                            Console.WriteLine("4. Remove a registered member from the system");
                            Console.WriteLine("5. Find a member contact phone number, given the member's name");
                            Console.WriteLine("6. Find Members who are currently renting a particular movie");
                            Console.WriteLine("7. Return to Main Menu");
                            Console.Write("Enter your choice ==> ");

                            if (HandleEscKey())
                            {
                                exit = true;
                                continue;
                            }

                            string option = Console.ReadLine();

                            switch (option)
                            {
                                case "1":
                                    addingMovie(movieCollection);
                                    break;
                                case "2":
                                    removingMovieFromSystem(movieCollection);
                                    break;
                                case "3":
                                    registeringMember(memberCollection);
                                    break;
                                case "4":
                                    removingMember(memberCollection);
                                    break;
                                case "5":
                                    findMemberContactNumber(memberCollection);
                                    break;
                                case "6":
                                    findingMembersBorrowingMovie(memberCollection);
                                    break;
                                case "7":
                                    exit = true;
                                    break;
                                default:
                                    Console.WriteLine("Please try again as it is an Invalid choice");
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"There is an error, please try again: {ex.Message}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("The credentials are invalid, the access is denied.");
                }
            }

            static void MemberMenu(MovieCollection movieCollection, MemberCollection memberCollection)
            {
                Console.Write("Please enter your first name: ");
                string firstName = ReadLineWithEsc();
                if (firstName == null) return;

                Console.Write("Please enter your last name: ");
                string lastName = ReadLineWithEsc();
                if (lastName == null) return;

                Console.Write("Please enter your password: ");
                string password = ReadLineWithEsc();
                if (password == null) return;

                // Always lookup in the collection—no default accounts
                Member? member = memberCollection.gettingMember(firstName, lastName);
                if (member == null || member.getPass() != password)
                {
                    Console.WriteLine("Invalid credentials, access denied.");
                    return;
                }

                // Logged in: show the same member submenu you already have
                bool exit = false;
                while (!exit)
                {
                    try
                    {
                        Console.WriteLine("\nMember Menu");
                        Console.WriteLine("--------------------------------------------");
                        Console.WriteLine("1. Browse all the Movies");
                        Console.WriteLine("2. Display information about a movie");
                        Console.WriteLine("3. Borrow a movie DVD");
                        Console.WriteLine("4. Return a movie DVD");
                        Console.WriteLine("5. List current borrowing movies");
                        Console.WriteLine("6. Display the top 3 movies rented by members");
                        Console.WriteLine("7. Return to Main Menu");
                        Console.Write("Enter your choice ==> ");

                        if (HandleEscKey())
                        {
                            exit = true;
                            continue;
                        }

                        string choice = Console.ReadLine();
                        switch (choice)
                        {
                            case "1":
                                displayMovies(movieCollection);
                                break;
                            case "2":
                                displayInfoOfMovie(movieCollection);
                                break;
                            case "3":
                                borrowTheMovie(movieCollection, member);
                                break;
                            case "4":
                                returnTheMovie(movieCollection, member);
                                break;
                            case "5":
                                listMoviesBorrowed(member);
                                break;
                            case "6":
                                displayTopThreeBorrowedMovies(movieCollection);
                                break;
                            case "7":
                                exit = true;
                                break;
                            default:
                                Console.WriteLine("The choice is not valid, please try again.");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"There is an error, please try again: {ex.Message}");
                    }
                }
            }


            static void addingMovie(MovieCollection movieCollection)
            {
                try
                {
                    // refuse to let the staff add a 1001st distinct title
                    if (movieCollection.IsFull())
                    {
                        Console.WriteLine("Cannot add more movies: system is at its 1000-movie capacity.");
                        return;
                    }

                    Console.WriteLine("\n=== Add a New Movie ===");
                    Console.WriteLine("Press ESC at any time to return to the Staff Menu.\n");

                    Console.Write("Please enter the title of the movie: ");
                    string title = ReadLineWithEsc();
                    if (title == null) return;

                    Console.Write("Please enter the year of the movie: ");
                    string yearInput = ReadLineWithEsc();
                    if (yearInput == null) return;
                    int year = int.Parse(yearInput);

                    // Check if it already exists
                    Movie? existingMovie = movieCollection.gettingMovieByTitleAndYear(title, year);
                    if (existingMovie != null)
                    {
                        Console.WriteLine($"\nMovie '{title}' ({year}) already exists in the system.");
                        Console.WriteLine($"Current copies: {existingMovie.gettingCopiesAvailable()}");
                        Console.Write("Enter number of additional copies to add: ");
                        string addInput = ReadLineWithEsc();
                        if (addInput == null) return;
                        int additional = int.Parse(addInput);
                        existingMovie.settingCopiesAvailable(existingMovie.gettingCopiesAvailable() + additional);
                        Console.WriteLine($"Successfully added {additional} copies. Total copies: {existingMovie.gettingCopiesAvailable()}");
                        return;
                    }

                    // --- GENRE VALIDATION LOOP ---
                    string[] validGenres = { "drama", "family", "animated", "adventure", "sci-fi", "action", "comedy", "thriller", "other" };
                    string genre;
                    while (true)
                    {
                        Console.WriteLine("\nAvailable genres: " + string.Join(", ", validGenres));
                        Console.Write("Please enter the genre of the movie: ");
                        genre = ReadLineWithEsc();
                        if (genre == null) return;
                        if (Array.Exists(validGenres, g =>
                                g.Equals(genre, StringComparison.OrdinalIgnoreCase)))
                        {
                            // normalize casing
                            genre = validGenres.First(g =>
                                g.Equals(genre, StringComparison.OrdinalIgnoreCase));
                            break;
                        }
                        Console.WriteLine("Invalid genre. Please try again.");
                    }

                    // --- CLASSIFICATION VALIDATION LOOP ---
                    string[] validClasses = { "G", "PG", "M15+", "MA15+" };
                    string classification;
                    while (true)
                    {
                        Console.WriteLine("\nAvailable classifications: " + string.Join(", ", validClasses));
                        Console.Write("Please enter the classification of the movie: ");
                        classification = ReadLineWithEsc();
                        if (classification == null) return;
                        if (Array.Exists(validClasses, c =>
                                c.Equals(classification, StringComparison.OrdinalIgnoreCase)))
                        {
                            classification = validClasses.First(c =>
                                c.Equals(classification, StringComparison.OrdinalIgnoreCase));
                            break;
                        }
                        Console.WriteLine("Invalid classification. Please try again.");
                    }

                    Console.Write("Please enter the duration (in minutes) of the movie: ");
                    string durationInput = ReadLineWithEsc();
                    if (durationInput == null) return;
                    int duration = int.Parse(durationInput);

                    Console.Write("Please enter the number of copies: ");
                    string copiesInput = ReadLineWithEsc();
                    if (copiesInput == null) return;
                    int copies = int.Parse(copiesInput);

                    // Create and add
                    Movie movie = new Movie(title, genre, classification, duration, copies, year);
                    movieCollection.addMovieInSystem(movie);
                    Console.WriteLine("\nThe movie has been successfully added to the system.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"There is an error, please try again: {ex.Message}");
                }
            }


            static void removingMovieFromSystem(MovieCollection movieCollection)
            {
                try
                {
                    Console.Write("Please enter the title of the movie to be removed: ");
                    string title = ReadLineWithEsc();
                    if (title == null) return;

                    // --- Array-based lookup ---
                    Movie[] found = movieCollection.gettingMoviesByTitle(title);
                    if (found.Length == 0)
                    {
                        Console.WriteLine("No movies found with that title.");
                        return;
                    }

                    Movie selectedMovie;
                    if (found.Length == 1)
                    {
                        selectedMovie = found[0];
                        Console.WriteLine($"\nOnly one match found: {selectedMovie.gettingTitle()} ({selectedMovie.gettingYear()}) - Available copies: {selectedMovie.gettingCopiesAvailable()}");
                    }
                    else
                    {
                        Console.WriteLine("\nFound movies:");
                        for (int i = 0; i < found.Length; i++)
                        {
                            var m = found[i];
                            Console.WriteLine($"{i + 1}. {m.gettingTitle()} ({m.gettingYear()}) - Available copies: {m.gettingCopiesAvailable()}");
                        }

                        Console.WriteLine("\nEnter the number of the movie you want to remove (or '0' to cancel):");
                        string selection = ReadLineWithEsc();
                        if (selection == null || selection == "0") return;

                        if (!int.TryParse(selection, out int index) || index < 1 || index > found.Length)
                        {
                            Console.WriteLine("Invalid selection.");
                            return;
                        }

                        selectedMovie = found[index - 1];
                    }

                    // --- NEW BORROW-CHECK ---
                    if (selectedMovie.gettingCurrentBorrowCount() > 0)
                    {
                        Console.WriteLine($"\nCannot remove '{selectedMovie.gettingTitle()}' ({selectedMovie.gettingYear()}):");
                        Console.WriteLine($"    {selectedMovie.gettingCurrentBorrowCount()} copy(ies) still borrowed.");
                        Console.WriteLine("    Please wait until all copies are returned before deleting this movie.");
                        return;
                    }

                    // Now ask how many copies to remove
                    Console.Write("Please enter the number of copies to be removed from the system: ");
                    string copiesInput = ReadLineWithEsc();
                    if (copiesInput == null) return;
                    int copiesToRemove = int.Parse(copiesInput);

                    int availableCopies = selectedMovie.gettingCopiesAvailable();
                    if (copiesToRemove > availableCopies)
                    {
                        Console.WriteLine($"Error: Cannot remove {copiesToRemove} copies. Only {availableCopies} copies are available.");
                    }
                    else if (copiesToRemove == availableCopies)
                    {
                        movieCollection.removingMoviefromsystem(
                            selectedMovie.gettingTitle(),
                            selectedMovie.gettingYear());
                        Console.WriteLine("The movie has been completely removed from the system.");
                    }
                    else
                    {
                        selectedMovie.settingCopiesAvailable(availableCopies - copiesToRemove);
                        Console.WriteLine($"Successfully removed {copiesToRemove} copies. {selectedMovie.gettingCopiesAvailable()} copies remaining.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"There is an error, please try again: {ex.Message}");
                }
            }



            static void registeringMember(MemberCollection memberCollection)
            {
                try
                {
                    Console.Write("Please enter your first name: ");
                    string firstName = ReadLineWithEsc();
                    if (firstName == null) return;

                    Console.Write("Please enter your last name: ");
                    string lastName = ReadLineWithEsc();
                    if (lastName == null) return;

                    // --- validate phone number is digits only ---
                    string phoneNumber;
                    while (true)
                    {
                        Console.Write("Please enter your contact number (digits only): ");
                        phoneNumber = ReadLineWithEsc();
                        if (phoneNumber == null) return;
                        if (!string.IsNullOrEmpty(phoneNumber) && IsDigitsOnly(phoneNumber))
                            break;
                        Console.WriteLine("Invalid input. Contact number must be numeric digits only.");
                    }

                    // --- validate 4-digit numeric password ---
                    string password;
                    while (true)
                    {
                        Console.Write("Please enter a four digit password: ");
                        password = ReadLineWithEsc();
                        if (password == null) return;
                        if (password.Length == 4 && IsDigitsOnly(password))
                            break;
                        Console.WriteLine("The password you have written is invalid. Please enter exactly four digits.");
                    }

                    Member member = new Member(firstName, lastName, phoneNumber, password);
                    if (memberCollection.addingMember(member))
                        Console.WriteLine("The member has been registered successfully in the system.");
                    else
                        Console.WriteLine("Registration failed: member may already exist or the system is full.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"There is an error, please try again: {ex.Message}");
                }
            }

            // Helper to check that a string contains only digits
            private static bool IsDigitsOnly(string str)
            {
                foreach (char c in str)
                    if (!char.IsDigit(c))
                        return false;
                return true;
            }


            static void removingMember(MemberCollection memberCollection)
            {
                try
                {
                    Console.Write("Please enter your first name: ");
                    string firstName = ReadLineWithEsc();
                    if (firstName == null) return;

                    Console.Write("Please enter your last name: ");
                    string lastName = ReadLineWithEsc();
                    if (lastName == null) return;

                    if (memberCollection.RemoveMember(firstName, lastName))
                    {
                        Console.WriteLine("The member has been removed successfully.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"There is an error, please try again: {ex.Message}");
                }
            }

            static void findMemberContactNumber(MemberCollection memberCollection)
            {
                try
                {
                    Console.Write("Please enter your first name: ");
                    string firstName = ReadLineWithEsc();
                    if (firstName == null) return;

                    Console.Write("Please enter your last name: ");
                    string lastName = ReadLineWithEsc();
                    if (lastName == null) return;

                    Member? member = memberCollection.gettingMember(firstName.ToLower(), lastName.ToLower());
                    if (member != null)
                    {
                        Console.WriteLine($"Contact Number: {member.getPhoneNo()}");
                    }
                    else
                    {
                        Console.WriteLine("This member cannot be found in the system.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"There is an error, please try again: {ex.Message}");
                }
            }


            static void findingMembersBorrowingMovie(MemberCollection memberCollection)
            {
                try
                {
                    Console.Write("Please enter the title of the movie: ");
                    string title = ReadLineWithEsc();
                    if (title == null) return;

                    Console.Write("Please enter the year of the movie: ");
                    string yearInput = ReadLineWithEsc();
                    if (yearInput == null) return;
                    int year = int.Parse(yearInput);

                    Member[] members = memberCollection.gettingAllMembers();
                    bool foundAny = false;
                    foreach (var member in members)
                    {
                        if (member != null)
                        {
                            Movie[] borrowedMovies = member.GetMoviesBorrowed();
                            for (int i = 0; i < member.getBorrowedMoviesCount(); i++)
                            {
                                if (borrowedMovies[i] != null &&
                                    borrowedMovies[i].gettingTitle().Equals(title, StringComparison.OrdinalIgnoreCase) &&
                                    borrowedMovies[i].gettingYear() == year)
                                {
                                    Console.WriteLine(member);
                                    foundAny = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (!foundAny)
                    {
                        Console.WriteLine("No members are currently borrowing this movie.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"There is an error, please try again: {ex.Message}");
                }
            }

            static void displayMovies(MovieCollection movieCollection)
            {
                try
                {
                    Movie[] movies = movieCollection.getAllMoviesInSystem();
                    // Sort movies alphabetically by title
                    Array.Sort(movies, (x, y) => string.Compare(x.gettingTitle(), y.gettingTitle(), StringComparison.OrdinalIgnoreCase));

                    Console.WriteLine("\nMovies in alphabetical order:");
                    Console.WriteLine("--------------------------------");
                    for (int n = 0; n < movies.Length; n++)
                    {
                        Console.WriteLine(movies[n]);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"There is an error, please try again: {ex.Message}");
                }
            }

            static void displayInfoOfMovie(MovieCollection movieCollection)
            {
                try
                {
                    Console.Write("Please enter the title of the movie: ");
                    string movieTitle = Console.ReadLine();
                    if (movieTitle == null) return;

                    // --- Array-based lookup ---
                    Movie[] foundMovies = movieCollection.gettingMoviesByTitle(movieTitle);
                    if (foundMovies.Length == 0)
                    {
                        Console.WriteLine("This movie cannot be found in the system.");
                        return;
                    }

                    Console.WriteLine("\nFound movies:");
                    for (int i = 0; i < foundMovies.Length; i++)
                    {
                        Console.WriteLine($"\n{foundMovies[i]}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"There is an error, please try again: {ex.Message}");
                }
            }



            static void borrowTheMovie(MovieCollection movieCollection, Member member)
            {
                try
                {
                    Console.Write("Enter the movie title to search: ");
                    string searchTitle = ReadLineWithEsc();
                    if (searchTitle == null) return;

                    // --- Array-based lookup ---
                    Movie[] foundMovies = movieCollection.gettingMoviesByTitle(searchTitle);
                    if (foundMovies.Length == 0)
                    {
                        Console.WriteLine("No movies found with that title.");
                        return;
                    }

                    Console.WriteLine("\nFound movies:");
                    for (int i = 0; i < foundMovies.Length; i++)
                    {
                        Movie movie = foundMovies[i];
                        Console.WriteLine($"{i + 1}. {movie.gettingTitle()} ({movie.gettingYear()}) - Available copies: {movie.gettingCopiesAvailable()}");
                    }

                    Console.WriteLine("\nEnter the numbers of movies you want to borrow (comma-separated, e.g., 1,3) or '0' to cancel:");
                    string input = ReadLineWithEsc();
                    if (input == null || input == "0") return;

                    string[] selections = input.Split(',');
                    bool allSuccessful = true;

                    foreach (string sel in selections)
                    {
                        if (int.TryParse(sel.Trim(), out int idx) && idx > 0 && idx <= foundMovies.Length)
                        {
                            Movie selectedMovie = foundMovies[idx - 1];
                            if (selectedMovie.gettingCopiesAvailable() > 0 && member.movieBorrow(selectedMovie))
                            {
                                selectedMovie.decrementAvailableCopies();
                                selectedMovie.BorrowCountIncrement();
                                Console.WriteLine($"Successfully borrowed: {selectedMovie.gettingTitle()} ({selectedMovie.gettingYear()})");
                            }
                            else
                            {
                                Console.WriteLine($"Cannot borrow {selectedMovie.gettingTitle()} ({selectedMovie.gettingYear()}).");
                                allSuccessful = false;
                            }
                        }
                    }

                    if (allSuccessful) movieCollection.SaveMoviesToFile();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"There is an error, please try again: {ex.Message}");
                }
            }


            static void returnTheMovie(MovieCollection movieCollection, Member member)
            {
                try
                {
                    Console.Write("Enter the movie title to return: ");
                    string searchTitle = ReadLineWithEsc();
                    if (searchTitle == null) return;

                    // --- Array-based lookup ---
                    Movie[] foundMovies = movieCollection.gettingMoviesByTitle(searchTitle);
                    if (foundMovies.Length == 0)
                    {
                        Console.WriteLine("No movies found with that title.");
                        return;
                    }

                    // Build array of only borrowed ones
                    int tempCount = 0;
                    for (int i = 0; i < foundMovies.Length; i++)
                        if (member.moviesBorrowed(foundMovies[i].gettingTitle(), foundMovies[i].gettingYear()))
                            tempCount++;

                    if (tempCount == 0)
                    {
                        Console.WriteLine("You haven't borrowed any versions of this movie.");
                        return;
                    }

                    Movie[] borrowed = new Movie[tempCount];
                    int bIdx = 0;
                    for (int i = 0; i < foundMovies.Length; i++)
                        if (member.moviesBorrowed(foundMovies[i].gettingTitle(), foundMovies[i].gettingYear()))
                            borrowed[bIdx++] = foundMovies[i];

                    Console.WriteLine("\nBorrowed movies to return:");
                    for (int i = 0; i < borrowed.Length; i++)
                        Console.WriteLine($"{i + 1}. {borrowed[i].gettingTitle()} ({borrowed[i].gettingYear()})");

                    Console.WriteLine("\nEnter the numbers of movies you want to return (comma-separated) or '0' to cancel:");
                    string input = ReadLineWithEsc();
                    if (input == null || input == "0") return;

                    string[] selections = input.Split(',');
                    bool allSuccessful = true;

                    foreach (string sel in selections)
                    {
                        if (int.TryParse(sel.Trim(), out int idx) && idx > 0 && idx <= borrowed.Length)
                        {
                            Movie selMovie = borrowed[idx - 1];
                            if (member.returningMovie(selMovie))
                            {
                                selMovie.incrementAvailableCopies();
                                selMovie.CurrentBorrowCountDecrement();
                                Console.WriteLine($"Successfully returned: {selMovie.gettingTitle()} ({selMovie.gettingYear()})");
                            }
                            else
                            {
                                Console.WriteLine($"Error returning {selMovie.gettingTitle()} ({selMovie.gettingYear()})");
                                allSuccessful = false;
                            }
                        }
                    }

                    if (allSuccessful) movieCollection.SaveMoviesToFile();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"There is an error, please try again: {ex.Message}");
                }
            }


            static void listMoviesBorrowed(Member member)
            {
                try
                {
                    if (member == null)
                    {
                        Console.WriteLine("Error: Member information is not available.");
                        return;
                    }

                    if (member.getBorrowedMoviesCount() == 0)
                    {
                        Console.WriteLine("No movies currently borrowed.");
                        return;
                    }

                    Movie[] borrowedMovies = member.GetMoviesBorrowed();
                    if (borrowedMovies == null)
                    {
                        Console.WriteLine("Error: Unable to retrieve borrowed movies.");
                        return;
                    }

                    // Create a new array with only the non-null movies
                    Movie[] validMovies = new Movie[member.getBorrowedMoviesCount()];
                    int validCount = 0;
                    for (int n = 0; n < member.getBorrowedMoviesCount(); n++)
                    {
                        if (borrowedMovies[n] != null)
                        {
                            validMovies[validCount++] = borrowedMovies[n];
                        }
                    }

                    // Sort the valid movies alphabetically by title
                    Array.Sort(validMovies, (x, y) => string.Compare(x.gettingTitle(), y.gettingTitle(), StringComparison.OrdinalIgnoreCase));

                    Console.WriteLine("\nCurrently borrowed movies (in alphabetical order):");
                    Console.WriteLine("--------------------------------");
                    bool hasMovies = false;
                    for (int n = 0; n < validCount; n++)
                    {
                        Console.WriteLine($"\n{validMovies[n]}");
                        hasMovies = true;
                    }

                    if (!hasMovies)
                    {
                        Console.WriteLine("No movies currently borrowed.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"There is an error, please try again: {ex.Message}");
                }
            }

            //displaying the top three borrowed movies 
            static void displayTopThreeBorrowedMovies(MovieCollection movieCollection)
            {
                try
                {
                    Console.WriteLine("Enter a genre to see the top three movies in that genre, or type \"all\" for the overall top three:");
                    Console.WriteLine("Available genres: drama, family, animated, adventure, sci-fi, action, comedy, thriller, other");
                    string choice = Console.ReadLine()?.Trim();

                    Movie[] theTopThreeMovies;
                    if (string.Equals(choice, "all", StringComparison.OrdinalIgnoreCase))
                    {
                        theTopThreeMovies = movieCollection.gettingTopThreeBorrowedMovies();
                    }
                    else
                    {
                        theTopThreeMovies = movieCollection.gettingTopThreeBorrowedMoviesByGenre(choice);
                    }

                    if (theTopThreeMovies == null || theTopThreeMovies.Length == 0)
                    {
                        Console.WriteLine("No movies found for your selection.");
                        return;
                    }

                    Console.WriteLine();
                    Console.WriteLine("The top three borrowed movies are:");
                    foreach (var movie in theTopThreeMovies)
                        Console.WriteLine($"{movie.gettingTitle()} ({movie.gettingYear()}) — borrowed {movie.gettingBorrowCount()} times");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"There was an error, please try again: {ex.Message}");
                }
            }
        }
    }
}