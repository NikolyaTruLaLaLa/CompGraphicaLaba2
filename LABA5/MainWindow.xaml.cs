using System.Windows;
using System.Windows.Controls;
using BezierSpline.Views;

namespace BezierSpline
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            taskList.SelectedIndex = 2; // по умолчанию открываем 3-е задание
        }

        private void TaskList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Защита от вызова во время инициализации XAML
            if (taskHost == null) return;

            taskHost.Content = taskList.SelectedIndex switch
            {
                0 => new Task1View(),
                1 => new Task2View(),
                2 => new BezierSplineView(),
                _ => null
            };
        }
    }
}