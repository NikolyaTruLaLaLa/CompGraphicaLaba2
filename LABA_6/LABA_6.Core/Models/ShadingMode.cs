namespace LABA_6.Core.Models
{
    public enum ShadingMode
    {
        None,     // Без света (только текстура или цвет)
        Gouraud,  // Интерполяция цвета
        Phong,    // Интерполяция нормалей
        Toon      // Интерполяция нормалей + ступенчатый свет
    }
}