namespace IntervalTimer

open System.Diagnostics
open Fabulous
open Fabulous.XamarinForms
open Fabulous.XamarinForms.LiveUpdate
open Xamarin.Forms
open IntervalTimer.Components
open IntervalTimer.Helpers
open Timers.Timers
open DomainOutput.DomainOutput


module MainPage = 
    //!< local helper to use a picker items list for rep and set selections
    let repSetSelector = zip ([0..100] |> List.map string) [0..100]

    //!< type to handle all main page events
    type Message = 
        | DurationSelectorChanged of (DurationInterval*DurationType*int)
        | RepSelectorChanged of int
        | SetSelectorChanged of int
        | ButtonClicked

    //!< main model object for the main page
    type Model = 
        { Timer: Timer }        

    //!< create the initial state of the model
    let init () = 
        let initialTime = {Hour = 0; Minute = 0; Second = 0}
        let initialModel = {            
            Timer = {
                Reps = 0
                Sets = 0
                High = initialTime
                Low = initialTime
                Warmup = initialTime
                Cooldown = initialTime }} 
        initialModel, Cmd.none    

    //!< function to handle update events from UI
    let update message model = 
        match message with
        | DurationSelectorChanged (interval, duration, value) ->
            match interval with
                | HighInterval -> {model with Timer = {model.Timer with Timer.High = updateTimerField model.Timer.High duration value}}
                | LowInterval-> {model with Timer = {model.Timer with Timer.Low = updateTimerField model.Timer.Low duration value}}
                | WarmupInterval-> {model with Timer = {model.Timer with Timer.Warmup = updateTimerField model.Timer.Warmup duration value}}
                | CooldownInterval-> {model with Timer = {model.Timer with Timer.Cooldown = updateTimerField model.Timer.Cooldown duration value}}
        | RepSelectorChanged value ->
            {model with Timer = {model.Timer with Timer.Reps = value}}
        | SetSelectorChanged value ->
            {model with Timer = {model.Timer with Timer.Sets = value}}
        | ButtonClicked ->
            timerToJson model.Timer |> sendTcp            
            model

    //!< creates a time selector of multiple dropdown menus to pick an interval time
    let timeSelector dispatch message intervalSelect =         
        let hourSelector = zip ([0..23] |> List.map (sprintf "%02i")) [0..23] 
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

    //!< creates a time selector inside of a rowEntryWithTitle
    let durationSelector title dispatch message intervalSelect  =
        let mainElement = timeSelector dispatch message intervalSelect
        rowEntryWithTitle title mainElement    

    //!< main UI view function
    let view model dispatch = 
        View.ContentPage(
            title = "Interval Timer Configuration",
            content = View.StackLayout(
                [durationSelector "High Interval" dispatch DurationSelectorChanged HighInterval
                 durationSelector "Low Interval" dispatch DurationSelectorChanged LowInterval
                 durationSelector "Warmup Duration" dispatch DurationSelectorChanged WarmupInterval
                 durationSelector "Cooldown Duration" dispatch DurationSelectorChanged CooldownInterval
                 singleElementSelector "Reps" repSetSelector dispatch RepSelectorChanged
                 singleElementSelector "Sets" repSetSelector dispatch SetSelectorChanged
                 View.Button(
                    text="Start",
                    horizontalOptions = LayoutOptions.Center,
                    width = 200.0,
                    command = fun () -> dispatch ButtonClicked
                    )
                ]
            )
        )
