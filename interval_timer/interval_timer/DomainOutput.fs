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
        |> sprintf "%s\r\n"
    
    let sendTcp (ipAddress: string) (message: string)  = 
        let client = new TcpClient()
        client.Connect (ipAddress, 1234)
        let stream = client.GetStream()
        let bytes = Encoding.UTF8.GetBytes message
        stream.Write(bytes, 0, bytes.Length)
        stream.Close()
        client.Close()
        client.Dispose()

