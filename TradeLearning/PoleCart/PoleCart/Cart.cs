namespace PoleCart
{
    public class Cart
    {
        // Physical constants
        public double MassCart = 1.0;
        public double g = 9.81;
        public double MassPole = 0.1;
        public double l = 20; // Half-length in meters

        // State variables
        public double X { get; set; }
        public double dx { get; private set; } = 0.0;      // Cart velocity

        public double XMin { get; set; }
        public double XMax { get; set; }


        public double θ { get; set; } = 0.0;     // Pole angle (radians)
        public double dθ { get; private set; } = 0.0;  // Pole angular velocity

        public Cart()
        {
            Reset();
        }

        public void Reset()
        {
            X = 0.0;
            dx = 0.0;
            θ = 0;
            dθ = 0.0;
        }

        double groundFriction = 0.1f;
        double airFriction = 0.1f;
        public void Step(double F, double dt)
        {
            // Complete but complex and not working. Try with zero mass pole.
            // https://coneural.org/florian/papers/05_cart_pole.pdf
            //var sinθ = Math.Sin(θ);
            //var cosθ = Math.Cos(θ);

            var m = MassCart + MassPole;
            var cosθ = Math.Cos(θ);
            var sinθ = Math.Sin(θ);
            var temp = (F + l * dθ * dθ * sinθ) / m;
            var d2θ = (g * sinθ - cosθ * temp) / (l * (4.0 / 3.0 - MassPole * cosθ * cosθ / m));
            var d2x = temp - l * d2θ * cosθ / m;

            dx += dt * d2x;
            X += dt * dx;
            dθ += dt * d2θ;
            θ += dt * dθ;

            // Collision detection
            if (X < XMin)
            {
                X = XMin - (X - XMin);
                this.dx = -this.dx;
            }
            else if (X > XMax)
            {
                X = XMax - (X - XMax);
                this.dx = -this.dx;
            }
            dθ *= (1 - airFriction * dt);
            dx *= (1 - groundFriction * dt);
        }

        public void Step3(double F, double dt)
        {
            // Complete but complex and not working. Try with zero mass pole.
            // https://coneural.org/florian/papers/05_cart_pole.pdf
            var sinθ = Math.Sin(θ);
            var cosθ = Math.Cos(θ);

            var m = MassCart + MassPole;

            var t1 = (-F - MassPole * l * dθ * dθ * sinθ) / m;
            var d2θ = (g * sinθ + cosθ * t1) / (l * ((4 / 3) - MassPole * cosθ * cosθ / m));

            var d2x = (F + MassPole * l * dθ * dθ * sinθ - d2θ * cosθ) / m;

            // Newton's law F = ma
            this.dx += dt * d2x;
            this.dx *= groundFriction;

            var dx = dt * this.dx;
            X += dx;

            // Collision detection
            if (X < XMin)
            {
                X = XMin - (X - XMin);
                this.dx = -this.dx;
            }
            else if (X > XMax)
            {
                X = XMax - (X - XMax);
                this.dx = -this.dx;
            }
            dθ += dt * d2θ;
            dθ *= airFriction;

            θ += dt * dθ;
        }

        public void Step2(double force, double dt)
        {
            // Newton's law F = ma
            var XAcc = force / MassCart;
            this.dx += dt * XAcc;
            this.dx *= groundFriction;

            var dx = dt * this.dx;
            X += dx;

            // Collision detection
            if (X < XMin)
            {
                X = XMin - (X - XMin);
                this.dx = -this.dx;
            }
            else if (X > XMax)
            {
                X = XMax - (X - XMax);
                this.dx = -this.dx;
            }

            // Gravity
            var poleForce = Math.Sin(θ) * MassPole * g;
            dθ += dt * poleForce / MassPole - Math.Sin(dt * dt * XAcc);
            dθ *= airFriction;

            θ += dt * dθ;
        }

    }
}