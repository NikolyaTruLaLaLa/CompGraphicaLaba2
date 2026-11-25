namespace LABA_6.Core.Models
{
    public class Light
    {
        public Point3 Position { get; set; }
        public float Intensity { get; set; } = 1.0f;
        public float Ambient { get; set; } = 0.2f; // Фоновый свет (чтобы тени не были черными)

        public Light(Point3 position)
        {
            Position = position;
        }
    }
}