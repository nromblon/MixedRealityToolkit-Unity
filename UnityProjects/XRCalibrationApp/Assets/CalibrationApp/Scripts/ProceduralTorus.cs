using UnityEngine;

namespace CalibrationApp
{
    /// <summary>
    /// Generates a torus mesh in the local XY plane (ring opening faces local +Z),
    /// so it reads as a circular ring when placed in front of the camera.
    /// Dependency-free fallback used because ProBuilder is not installed.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter))]
    public class ProceduralTorus : MonoBehaviour
    {
        [SerializeField] private float majorRadius = 0.06f;
        [SerializeField] private float minorRadius = 0.012f;
        [SerializeField] private int majorSegments = 32;
        [SerializeField] private int minorSegments = 16;

        void Awake() { Build(); }
        void OnValidate() { Build(); }

        public void Build()
        {
            var mf = GetComponent<MeshFilter>();
            if (mf == null) return;

            int major = Mathf.Max(3, majorSegments);
            int minor = Mathf.Max(3, minorSegments);
            int ring = minor + 1;
            int vCount = (major + 1) * ring;

            var verts = new Vector3[vCount];
            var norms = new Vector3[vCount];
            var uvs = new Vector2[vCount];

            int vi = 0;
            for (int i = 0; i <= major; i++)
            {
                float u = (float)i / major * Mathf.PI * 2f;
                float cosU = Mathf.Cos(u);
                float sinU = Mathf.Sin(u);
                Vector3 center = new Vector3(cosU * majorRadius, sinU * majorRadius, 0f);

                for (int j = 0; j <= minor; j++)
                {
                    float v = (float)j / minor * Mathf.PI * 2f;
                    float cosV = Mathf.Cos(v);
                    float sinV = Mathf.Sin(v);
                    Vector3 dir = new Vector3(cosU * cosV, sinU * cosV, sinV);
                    verts[vi] = center + dir * minorRadius;
                    norms[vi] = dir;
                    uvs[vi] = new Vector2((float)i / major, (float)j / minor);
                    vi++;
                }
            }

            var tris = new int[major * minor * 6];
            int ti = 0;
            for (int i = 0; i < major; i++)
            {
                for (int j = 0; j < minor; j++)
                {
                    int a = i * ring + j;
                    int b = (i + 1) * ring + j;
                    int c = (i + 1) * ring + (j + 1);
                    int d = i * ring + (j + 1);
                    tris[ti++] = a; tris[ti++] = b; tris[ti++] = c;
                    tris[ti++] = a; tris[ti++] = c; tris[ti++] = d;
                }
            }

            var mesh = new Mesh { name = "TorusMesh" };
            mesh.vertices = verts;
            mesh.normals = norms;
            mesh.uv = uvs;
            mesh.triangles = tris;
            mesh.RecalculateBounds();
            mf.sharedMesh = mesh;
        }
    }
}
