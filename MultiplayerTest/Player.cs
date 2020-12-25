using softaware.ViewPort.Core;

namespace MultiplayerTest
{
    public class Player : NotifyPropertyChanged
    {
        private double x;
        private double y;

        private string color;

        private double targetX;
        private double targetY;

        public double X
        {
            get => x;
            set => SetProperty(ref x, value);
        }

        public double Y
        {
            get => y;
            set => SetProperty(ref y, value);
        }

        public string Color
        {
            get => color;
            set => SetProperty(ref color, value);
        }

        public double TargetX
        {
            get => targetX;
            set => SetProperty(ref targetX, value);
        }

        public double TargetY
        {
            get => targetY;
            set => SetProperty(ref targetY, value);
        }
    }
}
