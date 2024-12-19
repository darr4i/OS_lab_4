class Program
{
    static void Main()
    {
        FileSystem fs = new FileSystem(10, 1024 * 1024); // 10 дескрипторів, 1 МБ пам'яті
        fs.Mkfs();

        fs.Create("file1");
        fs.Create("file2");

        fs.Ls(); 
    }
}
