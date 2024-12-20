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

    public void Read(int fd, int size)
    {
        if (!openFiles.TryGetValue(fd, out OpenFile openFile))
        {
            Console.WriteLine("FD не знайдено.");
            return;
        }

        var descriptor = descriptors[openFile.DescriptorIndex];
        if (openFile.Offset + size > descriptor.Size)
        {
            size = descriptor.Size - openFile.Offset; 
            Console.WriteLine("Попередження: зчитування обмежено до кінця файлу.");
        }

        if (size <= 0)
        {
            Console.WriteLine("Немає даних для зчитування.");
            return;
        }

        byte[] data = new byte[size];
        Array.Copy(storage, openFile.Offset, data, 0, size);

        openFile.Offset += size;
        Console.WriteLine($"Зчитано {size} байт: {BitConverter.ToString(data)}");
    }

    public void Write(int fd, int size)
    {
        if (!openFiles.TryGetValue(fd, out OpenFile openFile))
        {
            Console.WriteLine("FD не знайдено.");
            return;
        }

        var descriptor = descriptors[openFile.DescriptorIndex];
        if (openFile.Offset + size > storage.Length)
        {
            Console.WriteLine("Недостатньо місця для запису.");
            return;
        }

        if (openFile.Offset + size > descriptor.Size)
        {
            descriptor.Size = openFile.Offset + size;
        }

        byte[] data = new byte[size];
        for (int i = 0; i < size; i++) data[i] = (byte)(i % 256);

        Array.Copy(data, 0, storage, openFile.Offset, size);
        openFile.Offset += size;

        Console.WriteLine($"Записано {size} байт.");
    }

        public void Unlink(string name)
    {
        if (!directory.TryGetValue(name, out int descriptorIndex))
        {
            Console.WriteLine("Файл не знайдено.");
            return;
        }

        var descriptor = descriptors[descriptorIndex];
        descriptor.Links--;

        if (descriptor.Links == 0)
        {
            if (!openFiles.Values.Any(openFile => openFile.DescriptorIndex == descriptorIndex))
            {
                foreach (int block in descriptor.BlockMap)
                {
                    blockMap[block] = false;
                }
                descriptors[descriptorIndex] = null;
                Console.WriteLine($"Ресурси файлу {name} звільнено.");
            }
        }

        directory.Remove(name);
        Console.WriteLine($"Файл {name} видалено.");
    }

        public void Truncate(string name, int newSize)
    {
        if (!directory.TryGetValue(name, out int descriptorIndex))
        {
            Console.WriteLine("Файл не знайдено.");
            return;
        }

        var descriptor = descriptors[descriptorIndex];

        if (newSize < descriptor.Size)
        {
            // Зменшення розміру
            int blocksToFree = (descriptor.Size - newSize + blockSize - 1) / blockSize;

            if (descriptor.BlockMap.Count == 0)
            {
                Console.WriteLine("Файл вже пустий.");
                return;
            }

            for (int i = 0; i < blocksToFree && descriptor.BlockMap.Count > 0; i++)
            {
                int blockIndex = descriptor.BlockMap.Count - 1; // Останній блок
                blockMap[descriptor.BlockMap[blockIndex]] = false; // Звільняємо блок
                descriptor.BlockMap.RemoveAt(blockIndex); // Видаляємо блок
            }
        }
        else if (newSize > descriptor.Size)
        {
            // Збільшення розміру
            int blocksToAllocate = (newSize - descriptor.Size + blockSize - 1) / blockSize;
            for (int i = 0; i < blocksToAllocate; i++)
            {
                int freeBlock = blockMap.Cast<bool>().ToList().FindIndex(b => !b);
                if (freeBlock == -1)
                {
                    Console.WriteLine("Недостатньо місця для збільшення файлу.");
                    return;
                }
                blockMap[freeBlock] = true;
                descriptor.BlockMap.Add(freeBlock);
            }
        }

        descriptor.Size = newSize;
        Console.WriteLine($"Розмір файлу {name} змінено на {newSize} байт.");
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


