namespace Quaver.Shared.Screens.Gameplay.ModCharting.Timeline;

public interface ISegmentPayload
{
    void Update(float progress, Segment segment);

    /// <summary>
    ///     The segment is entering *by positive time progression*, i.e. Going back in time doesn't call this
    /// </summary>
    /// <param name="segment"></param>
    public void OnEnter(Segment segment)
    {
    }

    /// <summary>
    ///     The segment is leaving *by positive time progression*
    /// </summary>
    /// <param name="segment"></param>
    public void OnLeave(Segment segment)
    {
    }
}