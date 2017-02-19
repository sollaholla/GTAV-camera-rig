#pragma warning disable 1587
/// 
/// Soloman Northrop © 2017
/// 
/// Please leave credit to the original author.
///

using GTA.Native;

public static class Mathf
{
    /// <summary>
    /// Clamp the value "value" between min, and max.
    /// </summary>
    /// <param name="value">The value we wish to clamp.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <returns></returns>
    public static float Clamp(float value, float min, float max)
    {
        if (value < min)
        {
            value = min;
        }
        else if (value > max)
        {
            value = max;
        }
        return value;
    }
}

/// <summary>
/// The view mode of a <see cref="FollowCam"/>.
/// </summary>
public enum FollowCamViewMode
{
    ThirdPersonNear,
    ThirdPersonMed,
    ThirdPersonFar,
    FirstPerson = 4
}

/// <summary>
/// A class dedicated to natives related CAM::FOLLOW_CAM
/// </summary>
public static class FollowCam
{
    /// <summary>
    /// The view mode of the current <see cref="FollowCam"/>.
    /// </summary>
    public static FollowCamViewMode ViewMode {
        get {
            if (IsFollowingVehicle)
                return (FollowCamViewMode)Function.Call<int>(Hash.GET_FOLLOW_VEHICLE_CAM_VIEW_MODE);
            return (FollowCamViewMode)Function.Call<int>(Hash.GET_FOLLOW_PED_CAM_VIEW_MODE);
        }
        set {
            if (IsFollowingVehicle)
            {
                Function.Call(Hash.SET_FOLLOW_VEHICLE_CAM_VIEW_MODE, (int)value);
                return;
            }
            Function.Call(Hash.SET_FOLLOW_PED_CAM_VIEW_MODE, (int)value);
        }
    }

    /// <summary>
    /// Returns true if the current <see cref="FollowCam"/> is following a <see cref="GTA.Vehicle"/>
    /// </summary>
    public static bool IsFollowingVehicle => Function.Call<bool>(Hash.IS_FOLLOW_VEHICLE_CAM_ACTIVE);
    
    /// <summary>
    /// Returns true if the current <see cref="FollowCam"/> is following a <see cref="GTA.Ped"/>
    /// </summary>
    public static bool IsFollowingPed => Function.Call<bool>(Hash.IS_FOLLOW_PED_CAM_ACTIVE);

    /// <summary>
    /// Disable the first person view mode.
    /// </summary>
    public static void DisableFirstPerson()
    {
        Function.Call(ViewMode == FollowCamViewMode.FirstPerson
            ? Hash._DISABLE_FIRST_PERSON_CAM_THIS_FRAME
            : Hash._DISABLE_VEHICLE_FIRST_PERSON_CAM_THIS_FRAME);
    }
}