using System;
using System.Collections.Generic;
using System.Collections;
public class FileSystem
{
    private int blockSize = 512; // Розмір блоку
    private int maxDescriptors;
    private byte[] storage; // Емуляція блочного носія
    private BitArray blockMap; // Бітова карта
    private List<FileDescriptor> descriptors = new();
    private Dictionary<string, int> directory = new(); // Директорія
    private Dictionary<int, OpenFile> openFiles = new(); // FD -> Стан
    private int nextFd = 0;

    public FileSystem(int maxDescriptors, int storageSize)
    {
        this.maxDescriptors = maxDescriptors;
        storage = new byte[storageSize];
        blockMap = new BitArray(storageSize / blockSize);
    }

    public void Mkfs()
    {
        descriptors = new List<FileDescriptor>(maxDescriptors);
        directory.Clear();
        blockMap.SetAll(false);
        openFiles.Clear();
        nextFd = 0;
        Console.WriteLine("Файлова система ініціалізована.");
    }

    public void Create(string name)
    {
        if (directory.ContainsKey(name))
        {
            Console.WriteLine("Файл вже існує.");
            return;
        }

        if (descriptors.Count >= maxDescriptors)
        {
            Console.WriteLine("Досягнуто максимуму дескрипторів.");
            return;
        }

        var descriptor = new FileDescriptor
        {
            Type = FileType.Regular,
            Size = 0,
            Links = 1
        };

        descriptors.Add(descriptor);
        directory[name] = descriptors.Count - 1;
        Console.WriteLine($"Файл {name} створено.");
    }

    public void Ls()
    {
        if (directory.Count == 0)
        {
            Console.WriteLine("Директорія порожня.");
            return;
        }

        Console.WriteLine("Список файлів у директорії:");
        foreach (var entry in directory)
        {
            Console.WriteLine($"Ім'я файлу: {entry.Key}, Номер дескриптора: {entry.Value}");
        }
    }

    public int Open(string name)
    {
        if (!directory.TryGetValue(name, out int descriptorIndex))
        {
            Console.WriteLine("Файл не знайдено.");
            return -1; // Помилка
        }

        int fd = nextFd++;
        openFiles[fd] = new OpenFile
        {
            DescriptorIndex = descriptorIndex,
            Offset = 0
        };
        Console.WriteLine($"Файл {name} відкрито. FD: {fd}");
        return fd;
    }

    public void Close(int fd)
    {
        if (!openFiles.ContainsKey(fd))
        {
            Console.WriteLine("FD не знайдено.");
            return;
        }

        openFiles.Remove(fd);
        Console.WriteLine($"FD {fd} закрито.");
    }

    public void Seek(int fd, int offset)
    {
        if (!openFiles.TryGetValue(fd, out OpenFile openFile))
        {
            Console.WriteLine("FD не знайдено.");
            return;
        }

        var descriptor = descriptors[openFile.DescriptorIndex];
        if (offset < 0 || offset > descriptor.Size)
        {
            Console.WriteLine("Неприпустиме зміщення.");
            return;
        }

        openFile.Offset = offset;
        Console.WriteLine($"Зміщення для FD {fd} встановлено на {offset}.");
    }
}

public class FileDescriptor
{
    public FileType Type { get; set; }
    public int Size { get; set; }
    public int Links { get; set; }
    public List<int> BlockMap { get; set; } = new();
}

public enum FileType
{
    Regular,
    Directory
}

class OpenFile
{
    public int DescriptorIndex { get; set; }
    public int Offset { get; set; }
}
