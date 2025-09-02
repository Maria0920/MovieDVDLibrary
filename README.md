# Community DVD Library Management System 🎬

This project is a **C# .NET 8 console application**.  
It simulates a community library system that manages a collection of movie DVDs, allowing both **staff** and **members** to interact with the system through role-based menus.

---
 
## 📌 Features

### Staff Functions
- Add movie DVDs to the system.  
- Remove DVDs (or remove a movie entirely if no copies remain).  
- Register new members with a four-digit password.  
- Remove registered members (only if they have returned all DVDs).  
- Search for a member’s phone number by full name.  
- Find all members currently renting a particular movie.  

### Member Functions
- View all movies in dictionary order by title.  
- Display details of a particular movie by title.  
- Borrow up to **five (5)** movies at a time (no duplicate copies).  
- Return DVDs to the library.  
- View current DVDs borrowed by the member.  
- Display the **top three most frequently borrowed movies** in descending order of borrow count.  

---

## 🛠️ Technologies
- **Language:** C#  
- **Framework:** .NET 8 Console Application  
- **IDE:** Microsoft Visual Studio 2022 (Community Edition)  
- **Custom Data Structures:**  
  - `MovieCollection`: Hash table for movies (custom hash + collision resolution).  
  - `MemberCollection`: Array-based collection of members.  
  - `Movie`, `Member`: Object-oriented models.  

---

## 🚀 How to Run
1. Clone the repository:
   ```bash
   git clone https://github.com/your-username/DVDLibrary.git

2. Open the project in Visual Studio 2022.
3. Build the solution.
4. Run the application (console).

## 📖 Algorithms & Analysis
As required by the assignment, the project includes:
A custom algorithm to find the top 3 most frequently borrowed DVDs.
Theoretical analysis of the algorithm’s time complexity.
Empirical analysis (measuring runtime performance).
A description of the hash function and collision resolution strategy used in MovieCollection.

## 📷 Example Console Flow
```
Welcome to the Community DVD Library
====================================

1. Staff Login
2. Member Login
0. Exit
------------------------------------
Please make a selection (1-2, or 0 to exit): 
```

## ⚠️ Notes

- This application does not use built-in C# collections (Hashtable, List, Dictionary) or third-party libraries, as per assignment requirements.

- All data structures are implemented from scratch.
