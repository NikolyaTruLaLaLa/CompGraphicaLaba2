using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using laba2.Imaging;
using Microsoft.Win32;

namespace laba2.ViewModels
{
    /// <summary>
    /// Основной ViewModel. Общая часть: команды, состояние сессии, INotifyPropertyChanged.
    /// Логика по заданиям — в partial-файлах MainViewModel.Rgb.cs и MainViewModel.Hsv.cs.
    /// </summary>
    internal partial class MainViewModel : INotifyPropertyChanged
    {
        public ICommand LoadImageCommand { get; }
        public ICommand SaveResultCommand { get; }
        public ICommand ResetHsvCommand { get; }

        protected ImageSession? Session { get; private set; }

        public MainViewModel()
        {
            LoadImageCommand = new RelayCommand(LoadImage);
            SaveResultCommand = new RelayCommand(SaveResult, () => Session != null);
            ResetHsvCommand = new RelayCommand(ResetHsv, () => Session != null);
        }

        private void LoadImage()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Изображения (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|Все файлы (*.*)|*.*",
                Title = "Выберите изображение"
            };

            if (dialog.ShowDialog() != true) return;

            Session = ImageSession.Load(dialog.FileName);
            OnImageLoaded(Session);
        }

        /// <summary>
        /// Реакция на новую загруженную картинку.
        /// Вызывает обработчики, определённые в partial-файлах по фичам.
        /// </summary>
        private void OnImageLoaded(ImageSession session)
        {
            Grayscale_OnImageLoaded(session);
            Rgb_OnImageLoaded(session);
            Hsv_OnImageLoaded(session);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

        protected bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(name);
            return true;
        }
    }
}