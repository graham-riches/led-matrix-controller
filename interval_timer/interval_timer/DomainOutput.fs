namespace DomainOutput

open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Text
open Timers.Timers
open DomainOutput.Json


module DomainOutput = 
    //!< domain output type of an interval timer that is in the format that the external
    //!< application expects
    type TimerOutput = 
        { total_intervals: int
          high_interval_ms: int
          low_interval_ms: int
          warmup_interval_ms: int
          cooldown_interval_ms: int
          repeat_times: int
        }

    //!< hourToMinute :: Hours -> Minutes
    let hourToMinute hour =
        hour * 60

    //!< minuteToSecond :: Minutes -> Seconds
    let minuteToSecond minute = 
        minute * 60

    //!< secondToMillisecond :: Seconds -> Milliseconds
    let secondToMillisecond second = 
        second * 1000

    //!< hourToMillisecond :: Hours -> Milliseconds
    let hourToMillisecond hour = 
        hourToMinute hour |> minuteToSecond |> secondToMillisecond        

    //!< minuteToMilliseconds :: Minutes -> Milliseconds
    let minuteToMillisecond minute =
        minuteToSecond minute |> secondToMillisecond        

    //!< durationToMilliseconds :: Duration -> Milliseconds
    let durationToMilliseconds duration = 
        let hourTime = hourToMillisecond duration.Hour
        let minuteTime = minuteToMillisecond duration.Minute
        let secondTime = secondToMillisecond duration.Second
        hourTime + minuteTime + secondTime
        
    //!< timerToTimerOutput :: Timer -> TimerOutput
    let timerToTimerOutput timer = 
        { total_intervals = timer.Reps
          repeat_times = timer.Sets
          high_interval_ms = durationToMilliseconds timer.High
          low_interval_ms = durationToMilliseconds timer.Low
          warmup_interval_ms = durationToMilliseconds timer.Warmup
          cooldown_interval_ms = durationToMilliseconds timer.Cooldown
        }

    //!< timerToJson :: Timer -> string
    let timerToJson timer = 
        timerToTimerOutput timer
        |> Json.serialize

    //!< TODO: get rid of hardcoded values here**
    let sendTcp (message: string) = 
        let client = new TcpClient()
        client.Connect ("10.0.0.201", 1234)
        let stream = client.GetStream()
        let bytes = Encoding.UTF8.GetBytes message
        stream.Write(bytes, 0, bytes.Length)
