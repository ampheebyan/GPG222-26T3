using UnityEngine;

public class SteeringWheelAlignment : MonoBehaviour
{
    public Vector2 minmaxAngle = new Vector2(30.0f, -30.0f);
    private float _angle = 0;
    public void SetAngle(float angle)
    {
        _angle = angle;
    }

    private void FixedUpdate()
    {
        float inverse = Mathf.InverseLerp(-1, 1, _angle);
        var angles = transform.eulerAngles;
        angles.z = Mathf.Lerp(minmaxAngle.x, minmaxAngle.y, inverse);
        transform.eulerAngles = angles;
    }
}
