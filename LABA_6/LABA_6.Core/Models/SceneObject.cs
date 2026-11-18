using LABA_6.Core.Models;

namespace LABA_6.Core.Models
{
    public class SceneObject
    {
        public Mesh Mesh { get; set; }
        public Matrix4x4 ModelMatrix { get; set; }
        public int Color { get; set; }

        public SceneObject(Mesh mesh, Matrix4x4 modelMatrix, int color)
        {
            Mesh = mesh;
            ModelMatrix = modelMatrix;
            Color = color;
        }
    }
}