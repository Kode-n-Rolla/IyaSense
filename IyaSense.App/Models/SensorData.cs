namespace IyaSense.App.Models {
    public class SensorData {
        public float Temperature { get; set; }
        public float Humidity { get; set; }
        public int SoilMoisture { get; set; }
        public bool PumpRunning { get; set; }
        public bool AutoMode { get; set; }
    }
}
