namespace IntervalTimer

open System.Diagnostics
open Fabulous
open Fabulous.XamarinForms
open Fabulous.XamarinForms.LiveUpdate
open Xamarin.Forms
open IntervalTimer.Helpers

module Components = 
    //!< Helper that puts a view element into an aligned grid row with a title
    //!< TODO: add configurable width/height for row and columns
    let rowEntryWithTitle title (mainElement: ViewElement) = 
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
                mainElement.Row(0).Column(1)
            ])

    //!< Helper to create a single element selector that uses a rowEntryWithTitle
    let singleElementSelector title items dispatch source = 
        let mainElement =
            View.StackLayout(
                orientation = StackOrientation.Horizontal,
                spacing = 10.0,
                children = [
                    View.Picker(
                        items = List.map fst items,
                        width = 75.0,
                        verticalOptions = LayoutOptions.Center,
                        selectedIndexChanged = (fun (i, item) -> dispatch (source i))
                    )]
            )
        rowEntryWithTitle title mainElement

    //!< Displays a pop-up alert
    let popupAlert (title, message, cancel) = 
        Application.Current.MainPage.DisplayAlert(title, message, cancel)
        |> Async.AwaitTask

    //!< popup alert with confirmation option
    let popupConfirmation (title, message, accept, cancel) =
        Application.Current.MainPage.DisplayAlert(title = title, message = message, accept = accept, cancel = cancel)
        |> Async.AwaitTask
    