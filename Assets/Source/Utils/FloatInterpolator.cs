namespace TimeLord {
  public class FloatInterpolator {
    public float From { get; set; }
    public float To { get; set; }
    public float Value => From + (To - From) * Progress;

    public float Time => m_timer.Time;
    public float Duration => m_timer.Duration;
    public float Progress => m_timer.Progress;
    public bool IsFinished => m_timer.IsFinished;

    private Timer m_timer;
    
    public FloatInterpolator(float from, float to, float duration) {
      From = from;
      To = to;
      m_timer = new Timer(duration);
    }
    
    public void Update(float deltaTime) {
      m_timer.Update(deltaTime);
    }
  }
}