class Program
{
    static void Main()
    {
        FileSystem fs = new FileSystem(10, 1024 * 1024); // 10 дескрипторів, 1 МБ пам'яті
        fs.Mkfs();

        fs.Create("file1");
        fs.Create("file2");
        fs.Ls();

        int fd1 = fs.Open("file1");
        int fd2 = fs.Open("file2");

        fs.Seek(fd1, 0); // Встановити початок
        fs.Seek(fd2, 10); // Неправильне значення, якщо розмір файлу = 0

        fs.Close(fd1);
        fs.Close(fd2);
    }
}
