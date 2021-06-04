namespace IntervalTimer

open System
open Fabulous
open Fabulous.XamarinForms
open Fabulous.XamarinForms.LiveUpdate
open Xamarin.Forms


module SettingsPage = 
    type Message = 
    | UpdateIpAddress
    | IpInput of string 

    type Model =
        { IpAddress: string option
          IpValid: bool}

    let init () = 
        let initialModel = {IpAddress = None; IpValid = false}
        initialModel, Cmd.none

    let validateIpAddress = not << String.IsNullOrWhiteSpace

    let update message model = 
        match message with
        | UpdateIpAddress ->
            model
        | IpInput ip ->
            {model with IpAddress = Some ip; IpValid = validateIpAddress ip}
            

    let view model dispatch = 
        let initialText = 
            match model.IpAddress with
            | Some ip -> ip
            | None -> "Enter IP Address"

        View.ContentPage(
            title = "Settings",
            content = View.StackLayout(
                [View.Entry(
                    placeholder = initialText,                    
                    width = 200.0,
                    horizontalOptions = LayoutOptions.Center,
                    textChanged = (fun text -> dispatch (IpInput text.NewTextValue))
                 )
                 View.Button(
                    text="Update IP Address",
                    horizontalOptions = LayoutOptions.Center,
                    width = 200.0,
                    command = fun () -> dispatch UpdateIpAddress
                 )
                ]
            )
        )
