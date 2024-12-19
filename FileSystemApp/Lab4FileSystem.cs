using System;
using System.Collections.Generic;
using System.Collections;

class FileSystem
{
    private int blockSize = 512; // Розмір блоку
    private int maxDescriptors;
    private byte[] storage; // Емуляція блочного носія
    private BitArray blockMap; // Бітова карта
    private List<FileDescriptor> descriptors = new();
    private Dictionary<string, int> directory = new(); // Директорія

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

    public void Stat(string name)
    {
        if (!directory.TryGetValue(name, out int index))
        {
            Console.WriteLine("Файл не знайдено.");
            return;
        }

        var descriptor = descriptors[index];
        Console.WriteLine($"Інформація про файл {name}:");
        Console.WriteLine($"Тип: {descriptor.Type}");
        Console.WriteLine($"Розмір: {descriptor.Size} байт");
        Console.WriteLine($"Посилання: {descriptor.Links}");
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
}

class FileDescriptor
{
    public FileType Type { get; set; }
    public int Size { get; set; }
    public int Links { get; set; }
    public List<int> BlockMap { get; set; } = new();
}

enum FileType
{
    Regular,
    Directory
}
