namespace Timers


module Timers =        
    type Duration = 
        { Hour: int
          Minute: int
          Second: int
        }

    type DurationType = 
        | Hours
        | Minutes
        | Seconds

    type DurationInterval = 
        | HighInterval
        | LowInterval
        | WarmupInterval
        | CooldownInterval

    type Timer = 
        { Reps: int
          Sets: int
          High: Duration
          Low: Duration
          Warmup: Duration
          Cooldown: Duration
        }

    let updateTimerField original field value = 
        match field with
        | Hours -> {original with Hour = value}
        | Minutes -> {original with Minute = value}
        | Seconds -> {original with Second = value}
    

