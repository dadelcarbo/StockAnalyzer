namespace PoleCart;
public class CartPole
{
    // Physical constants
    public double Gravity = 9.81;
    public double MassCart = 1.0;
    public double MassPole = 0.1;
    public double PoleLength = 0.5; // Half-length in meters
    public double TimeStep = 0.02;

    // State variables
    public double X { get; private set; } = 0.0;         // Cart position
    public double XDot { get; private set; } = 0.0;      // Cart velocity
    public double Theta { get; private set; } = 0.0;     // Pole angle (radians)
    public double ThetaDot { get; private set; } = 0.0;  // Pole angular velocity

    public CartPole()
    {
        Reset();
    }

    public void Reset()
    {
        X = 0.0;
        XDot = 0.0;
        Theta = 0.05; // Slightly off vertical
        ThetaDot = 0.0;
    }

    public void Step(double force)
    {
        double totalMass = MassCart + MassPole;
        double poleMassLength = MassPole * PoleLength;
        double cosTheta = Math.Cos(Theta);
        double sinTheta = Math.Sin(Theta);

        double temp = (force + poleMassLength * ThetaDot * ThetaDot * sinTheta) / totalMass;
        double thetaAcc = (Gravity * sinTheta - cosTheta * temp) /
                          (PoleLength * (4.0 / 3.0 - MassPole * cosTheta * cosTheta / totalMass));
        double xAcc = temp - poleMassLength * thetaAcc * cosTheta / totalMass;

        // Euler integration
        X += TimeStep * XDot;
        XDot += TimeStep * xAcc;
        Theta += TimeStep * ThetaDot;
        ThetaDot += TimeStep * thetaAcc;
    }

    public double GetPoleAngleDegrees()
    {
        return Theta * 180.0 / Math.PI;
    }
}