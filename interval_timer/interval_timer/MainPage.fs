namespace IntervalTimer

open System.Diagnostics
open Fabulous
open Fabulous.XamarinForms
open Fabulous.XamarinForms.LiveUpdate
open Xamarin.Forms
open Timers.Timers
open IntervalTimer.DataBase


module MainPage = 
    type Message = 
    | CreateNewTimer
    | TimersLoaded of Timer list
    | TimerAdded of Timer
    | TimerUpdated of Timer
    | TimerSelectedIdx of int
    | TimerDeleted of Timer

    type ExternalMessage =
    | NoOp
    | NavigateToNewTimer of Timer option
    | NavigateToViewTimer of Timer

    type Model =
        { Timers: Timer list option }

    let loadAsync dbPath = async {
        let! timers = loadAllTimers dbPath
        return TimersLoaded timers
    }

    let init dbPath () = 
        let initialModel = {Timers = None}
        let initialCommand = Cmd.ofAsyncMsg (loadAsync dbPath)
        initialModel, initialCommand

    let update message model = 
        match message with
        | CreateNewTimer ->
            model, ExternalMessage.NavigateToNewTimer None

        | TimersLoaded timers ->
            {model with Timers = Some timers}, ExternalMessage.NoOp
        
        | TimerAdded timer ->
            let timers = timer :: model.Timers.Value
            {model with Timers = Some timers}, ExternalMessage.NoOp

        | TimerUpdated timer ->
            //!< TODO: handle updates here
            model, ExternalMessage.NoOp

        | TimerSelectedIdx idx ->
            let timer = model.Timers.Value.[idx]
            model, (ExternalMessage.NavigateToViewTimer timer)        

        | TimerDeleted timer ->
            let newTimers =
                model.Timers.Value
                |> List.filter (fun t -> t <> timer)
            {model with Timers = Some newTimers }, ExternalMessage.NoOp

    let view model dispatch = 
        let savedTimers = 
            match model.Timers with
            | Some timers ->
                [for timer in timers -> View.TextCell(text = timer.Name)]                
            | None ->
                []
                
        View.ContentPage(
            title = "Saved Timers",
            toolbarItems = [
                View.ToolbarItem(
                    text = "New Timer",                                        
                    command = fun () -> dispatch CreateNewTimer
                )
            ],
            content = View.ListView(                
                items = savedTimers,
                selectionMode = ListViewSelectionMode.None,
                itemTapped = (fun idx -> dispatch (TimerSelectedIdx idx))                
            )
        )   

