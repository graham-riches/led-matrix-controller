namespace IntervalTimer

open System.Diagnostics
open Fabulous
open Fabulous.XamarinForms
open Fabulous.XamarinForms.LiveUpdate
open Xamarin.Forms


module MainPage = 
    //!< TODO: move this
    let rec zip xs ys = 
        match xs with
        | [] -> []
        | headX :: tailX ->
            match ys with
            | [] -> []
            | headY :: tailY -> (headX, headY) :: zip tailX tailY
    
    let hourSelector = zip ([0..23] |> List.map string) [0..23] 
    let minuteSelector = zip ([0..59] |> List.map string) [0..59] 
    let secondSelector = zip ([0..59] |> List.map string) [0..59] 

    type Duration = 
        { Hour: int
          Minute: int
          Second: int
        }
    
    type DurationType = 
        | Hours
        | Minutes
        | Seconds
    
    
    type Timer =
        { Reps: int
          Sets: int
          High: Duration
          Low: Duration
          Warmup: Duration
          Cooldown: Duration
        }

    type DurationInterval = 
        | HighInterval
        | LowInterval
        | WarmupInterval
        | CooldownInterval

    

    type Message = 
        | PickerItemChanged of (DurationInterval*DurationType*string option)

    type Model = 
        { Reps: int
          Sets: int
          High: Duration
          Low: Duration
          Warmup: Duration
          Cooldown: Duration
        }
    

    let init () = 
        let initialTime = {Hour = 0; Minute = 0; Second = 0}
        let initialModel = 
            { Reps = 0
              Sets = 0
              High = initialTime
              Low = initialTime
              Warmup = initialTime
              Cooldown = initialTime
            }
        initialModel, Cmd.none

    let optToInt value = 
        match value with
        | Some i -> int i
        | None -> 0

    let updateTimerField original (field: DurationType) value = 
        match field with
        | Hours -> {original with Hour = optToInt value}
        | Minutes -> {original with Minute = optToInt value}
        | Seconds -> {original with Second = optToInt value}

    let update message model = 
        match message with
        | PickerItemChanged (interval, duration, value) ->
            match interval with
                | HighInterval -> {model with Model.High = updateTimerField model.High duration value}, message
                | LowInterval-> {model with Model.Low = updateTimerField model.Low duration value}, message
                | WarmupInterval-> {model with Model.Warmup = updateTimerField model.Warmup duration value}, message
                | CooldownInterval-> {model with Model.Cooldown = updateTimerField model.Cooldown duration value}, message

    let durationSelector (title: string) (intervalSelect: DurationInterval) dispatch =
        View.Grid(
            rowdefs = [Dimension.Auto],
            coldefs = [Dimension.Absolute 150.0; Dimension.Absolute 200.0],
            children = [
                View.Label(
                    text = title,                                        
                    horizontalOptions = LayoutOptions.Start,
                    verticalOptions = LayoutOptions.Center,
                    padding=Thickness(10.0)
                ).Row(0).Column(0)
                View.StackLayout(
                    orientation = StackOrientation.Horizontal,
                    spacing = 10.0,
                    children = [
                        View.Picker(
                            title = "HH",
                            items = List.map fst hourSelector,
                            width=45.0,
                            verticalOptions = LayoutOptions.Center,
                            selectedIndexChanged = (fun (i, item) -> dispatch (PickerItemChanged (intervalSelect, Hours, item)))
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
                            selectedIndexChanged = (fun (i, item) -> dispatch (PickerItemChanged (intervalSelect, Minutes, item)))
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
                            selectedIndexChanged = (fun (i, item) -> dispatch (PickerItemChanged (intervalSelect, Seconds, item)))
                        )
                    ]
                 ).Row(0).Column(1)
            ]                       
        )


    let view model dispatch = 
        View.ContentPage(
            title = "Interval Timer Configuration",
            content = View.StackLayout(
                [durationSelector "High Interval" HighInterval dispatch
                 durationSelector "Low Interval" LowInterval dispatch
                 durationSelector "Warmup Duration" WarmupInterval dispatch
                 durationSelector "Cooldown Duration" CooldownInterval dispatch
                ]
            )
        )
