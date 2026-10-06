using UnityEngine;

public class GearShiftConnector : MonoBehaviour
{
// Trying to keep player input objects and stuff separate
    public SteeringController CarController;
    public void OnGearShiftUp()
    {
        CarController.GearShiftUp();
    }

    public void OnGearShiftDown()
    {
        CarController.GearShiftDown();
    }
}
