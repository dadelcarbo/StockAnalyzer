namespace PoleCart
{
    public class Cart
    {
        // Physical constants
        public double MassCart = 1.0;

        // State variables
        private double x = 0.0;
        public double X { get => x; private set => x = value; }
        public double XDot { get; private set; } = 0.0;      // Cart velocity

        public double XMin { get; set; }
        public double XMax { get; set; }


        public double Gravity = 2.81;
        public double MassPole = 0.1;
        public double PoleLength = 0.5; // Half-length in meters

        public double Theta { get; set; } = 0.0;     // Pole angle (radians)
        public double ThetaDot { get; private set; } = 0.0;  // Pole angular velocity

        public Cart()
        {
            Reset();
        }

        public void Reset()
        {
            X = 0.0;
            XDot = 0.0;
        }

        double groundFriction = 0.99f;
        double airFriction = 0.995f;

        public void Step(double force, double dt)
        {
            // Newton's law F = ma
            var XAcc = force / MassCart;
            XDot += dt * XAcc;
            XDot *= groundFriction;

            var dx = dt * XDot;
            X += dx;

            // Collision detection
            if (x < XMin)
            {
                X = XMin - (x - XMin);
                XDot = -XDot;
            }
            else if (x > XMax)
            {
                X = XMax - (x - XMax);
                XDot = -XDot;
            }

            // Gravity
            var poleForce = Math.Sin(Theta) * MassPole * Gravity;
            ThetaDot += dt * poleForce / MassPole - Math.Sin(dt * dt * XAcc);
            ThetaDot *= airFriction;

            Theta += dt * ThetaDot;

        }

    }
}