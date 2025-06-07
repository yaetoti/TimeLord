namespace TimeLord {
  public struct Timer {
    public float Duration { get; set; }
    public float Time { get; set; }
    public float Progress {
      get {
        if (Duration == 0.0f) {
          return 1.0f;
        }
        
        return Time / Duration;
      }
    }

    public bool IsFinished => Time >= Duration;
    
    public Timer(float duration) {
      Duration = duration;
      Time = 0.0f;
    }
  
    public void Reset() {
      Time = 0.0f;
    }

    public void Reset(float duration) {
      Duration = duration;
      Time = 0.0f;
    }
  
    public void Update(float deltaTime) {
      if (IsFinished) {
        return;
      }
      
      Time += deltaTime;
      if (Time >= Duration) {
        Time = Duration;
      }
    }
  }
}
