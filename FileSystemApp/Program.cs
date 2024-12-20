class Program
{
    static void Main()
    {
        // Ініціалізація 
        FileSystem fs = new FileSystem(10, 1024 * 1024); // 10 дескрипторів, 1 МБ пам'яті
        Console.WriteLine(">>> Ініціалізація файлової системи:");
        fs.Mkfs();

        // Створення файлів
        Console.WriteLine("\n>>> Створення файлів:");
        fs.Create("file1");
        fs.Create("file2");
        fs.Ls();

        // Відкриття файлів
        Console.WriteLine("\n>>> Відкриття файлів:");
        int fd1 = fs.Open("file1");
        int fd2 = fs.Open("file2");

        // Запис до файлу
        Console.WriteLine("\n>>> Запис даних до файлуfile1:");
        fs.Write(fd1, 1024); // Записуємо 1024 байта
        fs.Ls();

        // Читання з файлу
        Console.WriteLine("\n>>> Читання з файлу из file1:");
        fs.Seek(fd1, 0); 
        fs.Read(fd1, 16); // Читаємо перші 16 байт

        // Зменшення розміру файла
        Console.WriteLine("\n>>> Зменшення розміру file1 до 512 байт:");
        fs.Truncate("file1", 512);
        fs.Ls();

        // Збільшення розміру файла
        Console.WriteLine("\n>>> Збільшення розміру file1 до 2048 байт:");
        fs.Truncate("file1", 2048);
        fs.Ls();

        // Видалення файла
        Console.WriteLine("\n>>> ВИдалення file1:");
        fs.Unlink("file1");
        fs.Ls();

        // Закриття файлів
        Console.WriteLine("\n>>> Закриття файлів:");
        fs.Close(fd1);
        fs.Close(fd2);

        Console.WriteLine("\n>>> Кінець роботи програми!");
    }
}
