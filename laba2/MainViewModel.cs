using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using laba2.Core.Histogram;
using Microsoft.Win32;

namespace laba2
{
    /// <summary>
    /// ViewModel-представление одного столбика гистограммы, подготовленное для отрисовки.
    /// Содержит нормализованную высоту, рассчитанную относительно максимума.
    /// </summary>
    public class HistogramBinVM
    {
        /// <summary> Текстовая метка интервала (например, "0-16"). </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary> Сырое количество пикселей в интервале. </summary>
        public int Value { get; set; }

        /// <summary> Нормализованная высота столбика в пикселях для отрисовки в UI. </summary>
        public double Height { get; set; }

        /// <summary> Всплывающая подсказка, отображаемая при наведении на столбик. </summary>
        public string ToolTip => $"{Label}: {Value} пикс.";
    }

    /// <summary>
    /// Универсальная реализация <see cref="ICommand"/> для MVVM-привязок.
    /// Позволяет вызывать методы ViewModel напрямую из XAML без code-behind.
    /// </summary>
    /// <remarks>
    /// Использует <see cref="CommandManager.RequerySuggested"/> для автоматического
    /// обновления доступности команды при изменении состояния UI.
    /// </remarks>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        /// <summary>
        /// Создаёт новый экземпляр команды.
        /// </summary>
        /// <param name="execute">Действие, выполняемое при вызове команды.</param>
        /// <param name="canExecute">Опциональный предикат доступности команды. Если не задан — команда всегда доступна.</param>
        /// <exception cref="ArgumentNullException">Если <paramref name="execute"/> равен <c>null</c>.</exception>
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <inheritdoc/>
        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute();

        /// <inheritdoc/>
        public void Execute(object? parameter) => _execute();

        /// <inheritdoc/>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

    /// <summary>
    /// Основной ViewModel приложения, координирующий загрузку изображения,
    /// извлечение каналов R/G/B и построение гистограмм.
    /// </summary>
    /// <remarks>
    /// Реализует <see cref="INotifyPropertyChanged"/> для корректной работы привязок WPF.
    /// Вся тяжёлая работа с изображениями сосредоточена в <see cref="ProcessImage"/>.
    /// </remarks>
    internal class MainViewModel : INotifyPropertyChanged
    {
        /// <summary> Команда открытия диалога выбора изображения. </summary>
        public ICommand LoadImageCommand { get; }

        /// <summary> Коллекция столбиков гистограммы красного канала. </summary>
        public ObservableCollection<HistogramBinVM> RedBins { get; }

        /// <summary> Коллекция столбиков гистограммы зелёного канала. </summary>
        public ObservableCollection<HistogramBinVM> GreenBins { get; }

        /// <summary> Коллекция столбиков гистограммы синего канала. </summary>
        public ObservableCollection<HistogramBinVM> BlueBins { get; }

        /// <summary> Оригинальное изображение, загруженное пользователем. </summary>
        public BitmapImage? OriginalImage { get; private set; }

        /// <summary> Визуализация красного канала. </summary>
        public BitmapImage? RedChannelImage { get; private set; }

        /// <summary> Визуализация зелёного канала. </summary>
        public BitmapImage? GreenChannelImage { get; private set; }

        /// <summary> Визуализация синего канала. </summary>
        public BitmapImage? BlueChannelImage { get; private set; }

        /// <summary>
        /// Инициализирует ViewModel, создаёт коллекции и команду загрузки.
        /// </summary>
        public MainViewModel()
        {
            RedBins = new ObservableCollection<HistogramBinVM>();
            GreenBins = new ObservableCollection<HistogramBinVM>();
            BlueBins = new ObservableCollection<HistogramBinVM>();

            LoadImageCommand = new RelayCommand(LoadImage);
        }

        /// <summary>
        /// Открывает диалог выбора файла и запускает обработку выбранного изображения.
        /// </summary>
        private void LoadImage()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Изображения (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|Все файлы (*.*)|*.*",
                Title = "Выберите изображение"
            };

            if (dialog.ShowDialog() == true)
            {
                ProcessImage(dialog.FileName);
            }
        }

        /// <summary>
        /// Загружает изображение по указанному пути, извлекает каналы R/G/B,
        /// строит гистограммы и создаёт визуализации каналов.
        /// </summary>
        /// <param name="filePath">Полный путь к файлу изображения.</param>
        private void ProcessImage(string filePath)
        {
            // 1. Загрузка с OnLoad — чтобы файл не блокировался процессом.
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(filePath);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze(); // Freeze делает объект потокобезопасным и ускоряет рендеринг.

            // 2. Конвертация в Bgra32 — единый формат для удобного чтения пикселей.
            var formatted = new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0);
            int width = formatted.PixelWidth;
            int height = formatted.PixelHeight;
            int stride = width * 4;
            byte[] pixels = new byte[height * stride];
            formatted.CopyPixels(pixels, stride, 0);

            // 3. Разделение интерливного массива BGRA на три независимых канала.
            byte[] r = new byte[width * height];
            byte[] g = new byte[width * height];
            byte[] b = new byte[width * height];

            for (int i = 0; i < pixels.Length; i += 4)
            {
                b[i / 4] = pixels[i];     // Blue
                g[i / 4] = pixels[i + 1]; // Green
                r[i / 4] = pixels[i + 2]; // Red
            }

            // 4. Построение гистограмм для каждого канала.
            var redHist = HistogramBuilder.BuildFromBytes(r, 256, 16, "Red Channel");
            var greenHist = HistogramBuilder.BuildFromBytes(g, 256, 16, "Green Channel");
            var blueHist = HistogramBuilder.BuildFromBytes(b, 256, 16, "Blue Channel");

            // 5. Подготовка данных для UI (с нормализацией высоты столбиков).
            FillBins(RedBins, redHist);
            FillBins(GreenBins, greenHist);
            FillBins(BlueBins, blueHist);

            // 6. Создание визуализаций каналов и обновление свойств.
            OriginalImage = ConvertToBitmapImage(formatted);
            RedChannelImage = CreateChannelImage(r, width, height, 2);
            GreenChannelImage = CreateChannelImage(g, width, height, 1);
            BlueChannelImage = CreateChannelImage(b, width, height, 0);

            OnPropertyChanged(nameof(OriginalImage));
            OnPropertyChanged(nameof(RedChannelImage));
            OnPropertyChanged(nameof(GreenChannelImage));
            OnPropertyChanged(nameof(BlueChannelImage));
        }

        /// <summary>
        /// Заполняет коллекцию столбиков гистограммы, нормализуя их высоту
        /// относительно максимального значения <see cref="HistogramData.MaxData"/>.
        /// </summary>
        /// <param name="bins">Целевая коллекция для отрисовки в UI.</param>
        /// <param name="hist">Источник агрегированных данных.</param>
        private void FillBins(ObservableCollection<HistogramBinVM> bins, HistogramData hist)
        {
            bins.Clear();
            double maxVal = hist.MaxData == 0 ? 1 : hist.MaxData; // Защита от деления на ноль.
            const double maxHeight = 150.0;

            foreach (var kvp in hist.Data)
            {
                bins.Add(new HistogramBinVM
                {
                    Label = kvp.Key,
                    Value = kvp.Value,
                    Height = ((double)kvp.Value / maxVal) * maxHeight
                });
            }
        }

        /// <summary>
        /// Создаёт монохромное изображение по одному цветовому каналу.
        /// </summary>
        /// <param name="channelData">Массив значений указанного канала.</param>
        /// <param name="width">Ширина изображения.</param>
        /// <param name="height">Высота изображения.</param>
        /// <param name="channelIndex">
        /// Индекс канала в формате Bgra32: 0 — Blue, 1 — Green, 2 — Red.
        /// </param>
        /// <returns>Готовое изображение для привязки в XAML.</returns>
        private BitmapImage CreateChannelImage(byte[] channelData, int width, int height, int channelIndex)
        {
            byte[] pixels = new byte[width * height * 4];
            for (int i = 0; i < channelData.Length; i++)
            {
                pixels[i * 4 + channelIndex] = channelData[i];
                pixels[i * 4 + 3] = 255; // Alpha = 255 (непрозрачность).
            }
            var bitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, pixels, width * 4);
            return ConvertToBitmapImage(bitmap);
        }

        /// <summary>
        /// Конвертирует <see cref="BitmapSource"/> в <see cref="BitmapImage"/> через
        /// поток в памяти. Нужно для корректной привязки к <c>Image.Source</c> в XAML.
        /// </summary>
        /// <param name="source">Исходный объект изображения.</param>
        /// <returns>Замороженный <see cref="BitmapImage"/>, готовый к использованию в UI-потоке.</returns>
        private BitmapImage ConvertToBitmapImage(BitmapSource source)
        {
            var image = new BitmapImage();
            using (var stream = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(source));
                encoder.Save(stream);
                stream.Position = 0;

                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Уведомляет UI об изменении свойства. Имя свойства определяется автоматически
        /// благодаря атрибуту <see cref="CallerMemberNameAttribute"/>.
        /// </summary>
        /// <param name="n">Имя изменившегося свойства (подставляется компилятором).</param>
        protected void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}