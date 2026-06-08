namespace CalibrationApp
{
    /// <summary>Definition of a single calibration marker: id and angular offset (degrees).</summary>
    public struct MarkerDef
    {
        public int id;
        public float az;
        public float el;

        public MarkerDef(int id, float az, float el)
        {
            this.id = id;
            this.az = az;
            this.el = el;
        }
    }

    /// <summary>Shared, single source of truth for the 13 calibration markers and distance.</summary>
    public static class CalibrationMarkers
    {
        public const float MarkerDistance = 2.0f;

        public static readonly MarkerDef[] All =
        {
            new MarkerDef(1,   0,   0),
            new MarkerDef(2,  15,   0),
            new MarkerDef(3, -15,   0),
            new MarkerDef(4,   0,  15),
            new MarkerDef(5,   0, -15),
            new MarkerDef(6,  25,   0),
            new MarkerDef(7, -25,   0),
            new MarkerDef(8,   0,  25),
            new MarkerDef(9,   0, -25),
            new MarkerDef(10,  18,  18),
            new MarkerDef(11, -18,  18),
            new MarkerDef(12,  18, -18),
            new MarkerDef(13, -18, -18),
        };
    }
}
