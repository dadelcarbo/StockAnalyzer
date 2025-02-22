using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.IO;
using StockAnalyzer.StockLogging;

namespace StockAnalyzer.StockHelpers
{
    public interface IPersistable
    {
        string Name { get; }
    }

    public class Persister<T> where T : IPersistable, new()
    {
        private Persister() { }
        private static Persister<T> instance;
        public static Persister<T> Instance => instance ??= new Persister<T>();
        public ObservableCollection<T> Items { get; set; } = new ObservableCollection<T>();

        private string folder = null;
        private string extension = null;

        public async Task InitializeAsync(string folder, string extension)
        {
            this.folder = folder;
            this.extension = extension;
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var items = new List<T>();
            foreach (var filePath in Directory.EnumerateFiles(folder, "*." + extension).OrderBy(f => f))
            {
                try
                {
                    using var fs = File.OpenRead(filePath);
                    items.Add(await JsonSerializer.DeserializeAsync<T>(fs));
                }
                catch (Exception ex)
                {
                    StockLog.Write(ex);
                }
            }
            this.Items = new ObservableCollection<T>(items.OrderBy(i => i.Name));
        }
        public void Initialize(string folder, string extension)
        {
            this.folder = folder;
            this.extension = extension;
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var items = new List<T>();
            foreach (var filePath in Directory.EnumerateFiles(folder, "*." + extension).OrderBy(f => f))
            {
                try
                {
                    using var fs = File.OpenRead(filePath);
                    items.Add(JsonSerializer.Deserialize<T>(fs));
                }
                catch (Exception ex)
                {
                    StockLog.Write(ex);
                }
            }
            this.Items = new ObservableCollection<T>(items.OrderBy(i => i.Name));
        }

        public void Save(T item)
        {
            if (string.IsNullOrEmpty(folder))
                throw new InvalidOperationException($"Persister of type {typeof(T)} is not initialized");

            try
            {
                using var fs = File.Create(Path.Combine(folder, $"{item.Name}.{extension}"));
                JsonSerializer.Serialize(fs, item);

                if (!Items.Any(i => i.Name == item.Name))
                {
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
            }
        }

        public void Delete(T item)
        {
            if (!string.IsNullOrEmpty(item?.Name))
            {
                var fileName = Path.Combine(folder, $"{item.Name}.{extension}");
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
                this.Items.Remove(item);
            }
        }

        public T Reload(T item)
        {
            if (string.IsNullOrEmpty(folder))
                throw new InvalidOperationException($"Persister of type {typeof(T)} is not initialized");

            try
            {
                var filePath = Path.Combine(folder, $"{item.Name}.{extension}");
                if (File.Exists(filePath))
                {
                    using var fs = File.OpenRead(filePath);
                    return JsonSerializer.Deserialize<T>(fs);
                }
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
            }
            return default(T);
        }
    }
}