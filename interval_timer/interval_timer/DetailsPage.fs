namespace IntervalTimer

open System
open Fabulous
open Fabulous.XamarinForms
open Fabulous.XamarinForms.LiveUpdate
open Xamarin.Forms
open IntervalTimer.Components
open IntervalTimer.DataBase
open Timers.Timers
open DomainOutput.DomainOutput


module DetailsPage =     
    type Message = 
        | LaunchClicked
        | DeleteTimer of Timer
        | TimerDeleted of Timer

    type ExternalMessage = 
        | NoOp
        | GoBackAfterTimerStarted
        | GoBackAfterTimerRemoved of Timer
    
    type Model = 
        { Timer: Timer }
       
    let tryDeleteAsync dbpath (timer: Timer) = async {
        let! confirmDelete = popupConfirmation("Confirm Delete", "Are you sure you want to delete this timer?", "YES", "NO")
        if confirmDelete then
            do! deleteTimer dbpath timer
            return Some (TimerDeleted timer)
        else
            return None
    }
    
    let init timer () = 
        let initialTime = {Hour = 0; Minute = 0; Second = 0}
        let initialModel = 
            { Timer = timer }              
        initialModel, Cmd.none    
     
    let update dbPath message model = 
        match message with
        | LaunchClicked ->
            model.Timer |> timerToJson |> (sendTcp "10.0.0.201")
            model, Cmd.none, ExternalMessage.GoBackAfterTimerStarted

        | DeleteTimer timer ->
            let cmd = Cmd.ofAsyncMsgOption (tryDeleteAsync dbPath timer)
            model, cmd, ExternalMessage.NoOp

        | TimerDeleted timer ->
            model, Cmd.none, (ExternalMessage.GoBackAfterTimerRemoved timer)

    let displayField title details = 
        let mainElement = 
            View.Label(                
                text = details
            )
        rowEntryWithTitle title mainElement

    let durationToString duration = 
        sprintf "%d : %d : %d" duration.Hour duration.Minute duration.Second

    let view model dispatch = 
        View.ContentPage(
            title = "Timer Details",
            content = View.StackLayout(
                [displayField "Name" model.Timer.Name
                 displayField "High Interval" (durationToString model.Timer.High)
                 displayField "Low Interval" (durationToString model.Timer.Low)
                 displayField "Warmup Interval" (durationToString model.Timer.Warmup)
                 displayField "Cooldown Interval" (durationToString model.Timer.Cooldown)
                 displayField "Reps" (sprintf "%d" model.Timer.Reps)
                 displayField "Sets" (sprintf "%d" model.Timer.Sets)
                 View.StackLayout(
                    orientation = StackOrientation.Horizontal,
                    children = [
                        View.Button(
                            text="Start Timer",
                            horizontalOptions = LayoutOptions.Center,
                            width = 200.0,
                            command = fun () -> dispatch LaunchClicked
                        )
                        View.Button(
                            text="Delete Timer",
                            horizontalOptions = LayoutOptions.Center,
                            width = 200.0,
                            command = fun () -> dispatch (DeleteTimer model.Timer)
                        )
                    ]
                 )
                 
                ]
            )
        )
