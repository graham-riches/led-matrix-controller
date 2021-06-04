namespace IntervalTimer

open System
open Fabulous
open Fabulous.XamarinForms
open Fabulous.XamarinForms.LiveUpdate
open Xamarin.Forms
open IntervalTimer.Components
open IntervalTimer.Helpers
open IntervalTimer.DataBase
open Timers.Timers
open DomainOutput.DomainOutput


module CreateTimerPage =     
    let repSetSelector = zip ([0..100] |> List.map string) [0..100]
    
    type Message = 
        | DurationSelectorChanged of (DurationInterval*DurationType*int)
        | RepSelectorChanged of int
        | SetSelectorChanged of int
        | SaveClicked
        | NameStringInput of string
        | TimerAdded of Timer
        | TimerUpdated of Timer

    type ExternalMessage = 
        | NoOp
        | GoBackAfterTimerAdded of Timer
        | GoBackAfterTimerUpdated of Timer
    
    type Model = 
        { Timer: Timer
          NameValid: bool }        
    
    let init timer () = 
        let initialTime = {Hour = 0; Minute = 0; Second = 0}
        let initialTimer =
            match timer with
            | Some t -> t
            | None ->
                { Id = 0
                  Name = ""
                  Reps = 0
                  Sets = 0
                  High = initialTime
                  Low = initialTime
                  Warmup = initialTime
                  Cooldown = initialTime }
        let initialModel = 
            { Timer = initialTimer
              NameValid = false } 
        initialModel, Cmd.none    

    let validateName = not << String.IsNullOrWhiteSpace

    let createOrUpdateAsync dbPath timer = async {
        match timer.Id with
        | 0 ->
            let! newTimer = insertTimer dbPath timer
            return TimerAdded newTimer
        | _ ->
            let! updatedTimer = updateTimer dbPath timer
            return TimerUpdated updatedTimer
    }

    let trySaveAsync dbPath model = async {
        if model.NameValid then
            let! msg = createOrUpdateAsync dbPath model.Timer
            return Some msg
        else            
            popupAlert("Invalid Timer", "Name must be valid", "Ok") |> ignore
            return None    
    }
        
    let update dbPath message model = 
        match message with
        | DurationSelectorChanged (interval, duration, value) ->
            match interval with
                | HighInterval -> 
                    {model with Timer = {model.Timer with Timer.High = updateTimerField model.Timer.High duration value}}, Cmd.none, ExternalMessage.NoOp
                | LowInterval -> 
                    {model with Timer = {model.Timer with Timer.Low = updateTimerField model.Timer.Low duration value}}, Cmd.none, ExternalMessage.NoOp
                | WarmupInterval -> 
                    {model with Timer = {model.Timer with Timer.Warmup = updateTimerField model.Timer.Warmup duration value}}, Cmd.none, ExternalMessage.NoOp
                | CooldownInterval -> 
                    {model with Timer = {model.Timer with Timer.Cooldown = updateTimerField model.Timer.Cooldown duration value}}, Cmd.none, ExternalMessage.NoOp

        | RepSelectorChanged value ->
            {model with Timer = {model.Timer with Timer.Reps = value}}, Cmd.none, ExternalMessage.NoOp

        | SetSelectorChanged value ->
            {model with Timer = {model.Timer with Timer.Sets = value}}, Cmd.none, ExternalMessage.NoOp

        | SaveClicked ->
            let cmd = Cmd.ofAsyncMsgOption (trySaveAsync dbPath model)            
            model, cmd, ExternalMessage.NoOp

        | NameStringInput name ->
            {model with Timer = {model.Timer with Name = name}; NameValid = validateName name}, Cmd.none, ExternalMessage.NoOp

        | TimerAdded timer ->
            model, Cmd.none, ExternalMessage.GoBackAfterTimerAdded timer

        | TimerUpdated timer ->
            model, Cmd.none, ExternalMessage.GoBackAfterTimerUpdated timer

    let timeSelector dispatch message intervalSelect =         
        let hourSelector   = zip ([0..23] |> List.map (sprintf "%02i")) [0..23] 
        let minuteSelector = zip ([0..59] |> List.map (sprintf "%02i")) [0..59] 
        let secondSelector = zip ([0..59] |> List.map (sprintf "%02i")) [0..59]
        View.StackLayout(
            orientation = StackOrientation.Horizontal,
            spacing = 10.0,
            children = [
                View.Picker(
                    title = "HH",
                    items = List.map fst hourSelector,
                    width=45.0,
                    verticalOptions = LayoutOptions.Center,
                    selectedIndexChanged = (fun (i, item) -> dispatch (message (intervalSelect, Hours, i)))
                )
                View.Label(
                    text = ":", 
                    verticalOptions = LayoutOptions.Center
                )
                View.Picker(
                    title = "MM",
                    items = List.map fst minuteSelector,
                    width=45.0,
                    verticalOptions = LayoutOptions.Center,
                    selectedIndexChanged = (fun (i, item) -> dispatch (message (intervalSelect, Minutes, i)))
                )
                View.Label(
                    text = ":",
                    verticalOptions = LayoutOptions.Center
                )
                View.Picker(
                    title = "SS",
                    items = List.map fst secondSelector,
                    width=45.0,
                    verticalOptions = LayoutOptions.Center,
                    selectedIndexChanged = (fun (i, item) -> dispatch (message (intervalSelect, Seconds, i)))
                )
            ]
         )

    let durationSelector title dispatch message intervalSelect  =
        let mainElement = timeSelector dispatch message intervalSelect
        rowEntryWithTitle title mainElement  
       
    let nameEntry title dispatch = 
        let mainElement = 
            View.Entry(                
                textChanged = (fun text -> dispatch (NameStringInput text.NewTextValue))
            )
        rowEntryWithTitle title mainElement

    let view model dispatch = 
        View.ContentPage(
            title = "Interval Timer Configuration",
            content = View.StackLayout(
                [nameEntry "Name" dispatch
                 durationSelector "High Interval" dispatch DurationSelectorChanged HighInterval
                 durationSelector "Low Interval" dispatch DurationSelectorChanged LowInterval
                 durationSelector "Warmup Duration" dispatch DurationSelectorChanged WarmupInterval
                 durationSelector "Cooldown Duration" dispatch DurationSelectorChanged CooldownInterval
                 singleElementSelector "Reps" repSetSelector dispatch RepSelectorChanged
                 singleElementSelector "Sets" repSetSelector dispatch SetSelectorChanged
                 View.Button(
                    text="Save Timer",
                    horizontalOptions = LayoutOptions.Center,
                    width = 200.0,
                    command = fun () -> dispatch SaveClicked
                    )
                ]
            )
        )
