using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bia.Internal.BookLibrary.Data;
using ClosedXML;
using ClosedXML.Excel;
using Microsoft.Extensions.Configuration;

namespace BooksLoader
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            Console.WriteLine("Укажите полный путь до excel файла");
            var path = Console.ReadLine();
            if (File.Exists(path))
            {
                try
                {
                    DeleteUnDefinatedBooks.Do(path);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.Read();
                }
            }
            else
            {
                Console.WriteLine("Указаный файл не найден");
            }
        }
    }
}
