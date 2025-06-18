using System;

public class CartPoleEnv
{
    // Constants
    const float gravity = 9.8f;
    const float massCart = 1.0f;
    const float massPole = 0.1f;
    const float totalMass = massCart + massPole;
    const float length = 0.5f; // actually half the pole's length
    const float poleMassLength = massPole * length;
    const float forceMag = 10.0f;
    const float tau = 0.02f; // seconds between state updates

    const float thetaThresholdRadians = 12 * (float)Math.PI / 180;
    const float xThreshold = 2.4f;

    // State: [x, x_dot, theta, theta_dot]
    public float[] state;
    private Random rand = new Random();

    public CartPoleEnv()
    {
        Reset();
    }

    public float[] Reset()
    {
        state = new float[4];
        for (int i = 0; i < 4; i++)
            state[i] = (float)(rand.NextDouble() * 0.1 - 0.05); // small random values
        return state;
    }

    public (float[] nextState, float reward, bool done) Step(int action)
    {
        float x = state[0];
        float xDot = state[1];
        float theta = state[2];
        float thetaDot = state[3];

        float force = (action == 1) ? forceMag : -forceMag;
        float costheta = (float)Math.Cos(theta);
        float sintheta = (float)Math.Sin(theta);

        float temp = (force + poleMassLength * thetaDot * thetaDot * sintheta) / totalMass;
        float thetaAcc = (gravity * sintheta - costheta * temp) /
                         (length * (4.0f / 3.0f - massPole * costheta * costheta / totalMass));
        float xAcc = temp - poleMassLength * thetaAcc * costheta / totalMass;

        // Update state
        x += tau * xDot;
        xDot += tau * xAcc;
        theta += tau * thetaDot;
        thetaDot += tau * thetaAcc;

        state[0] = x;
        state[1] = xDot;
        state[2] = theta;
        state[3] = thetaDot;

        bool done = x < -xThreshold || x > xThreshold ||
                    theta < -thetaThresholdRadians || theta > thetaThresholdRadians;

        float reward = done ? 0.0f : 1.0f;

        return (state, reward, done);
    }

    public void Render()
    {
        Console.WriteLine($"x: {state[0]:F2}, xDot: {state[1]:F2}, theta: {state[2]:F2}, thetaDot: {state[3]:F2}");
    }
}
