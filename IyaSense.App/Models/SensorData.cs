namespace IyaSense.App.Models {
    public class SensorData {
        public required float Temperature { get; set; }
        public required float Humidity { get; set; }
        public required int SoilMoisture { get; set; }
        public required bool PumpRunning { get; set; }
        public required bool AutoMode { get; set; }
        public required long CooldownRemaining { get; set; }
    }
}
