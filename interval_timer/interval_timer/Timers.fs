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
        { Id: int
          Name: string
          Reps: int
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
    
    let hourToMinute hour =
        hour * 60

    let minuteToSecond minute = 
        minute * 60
    
    let secondToMillisecond second = 
        second * 1000
    
    let hourToMillisecond hour = 
        hourToMinute hour |> minuteToSecond |> secondToMillisecond        
 
    let minuteToMillisecond minute =
        minuteToSecond minute |> secondToMillisecond        
    
    let durationToMilliseconds duration = 
        let hourTime = hourToMillisecond duration.Hour
        let minuteTime = minuteToMillisecond duration.Minute
        let secondTime = secondToMillisecond duration.Second
        hourTime + minuteTime + secondTime
    
    let millisecondToDuration time =
        let hours = time / 3600000
        let minutes = (time - hourToMillisecond hours) / 60000
        let seconds = (time - (hourToMillisecond hours) - (minuteToMillisecond minutes)) / 1000
        { Hour = hours
          Minute = minutes
          Second = seconds }
