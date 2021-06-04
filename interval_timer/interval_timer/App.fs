namespace IntervalTimer

open System.Diagnostics
open Fabulous
open Fabulous.XamarinForms
open Fabulous.XamarinForms.LiveUpdate
open Xamarin.Forms
open Timers.Timers

module App = 
    type Message = 
        | MainPageMessage of MainPage.Message
        | CreateTimerPageMessage of CreateTimerPage.Message 
        | SettingsPageMessage of SettingsPage.Message
        | DetailsPageMessage of DetailsPage.Message
        | NavigationPopped
        | ListViewSelectedItemChanged of int
        | GoToEditPage of Timer option
        | GoToViewPage of Timer
        | UpdateWhenTimerAdded of Timer
        | UpdateWhenTimerUpdated of Timer
        | UpdateWhenTimerDeleted of Timer
        | UpdateWhenTimerStarted
        | FlyoutPresented

    type Model = 
      { 
        MainPageModel: MainPage.Model
        CreateTimerModel: CreateTimerPage.Model option
        SettingsPageModel: SettingsPage.Model option
        DetailsPageModel: DetailsPage.Model option
        FlyoutPresented: bool
      }

    type Pages = 
        { 
          MainPage : ViewElement
          CreateTimerPage: ViewElement option
          SettingsPage: ViewElement option
          DetailsPage: ViewElement option
        }

    let init dbPath () = 
        let mainPage, mainMessage = MainPage.init dbPath ()
        let initialModel = 
            { MainPageModel = mainPage
              CreateTimerModel = None
              SettingsPageModel = None
              DetailsPageModel = None
              FlyoutPresented = false}
        initialModel, (Cmd.map MainPageMessage mainMessage)
    
    let navigationMapper (model: Model) =
        let settingsModel = model.SettingsPageModel
        let createTimerModel = model.CreateTimerModel
        let detailsModel = model.DetailsPageModel
        match settingsModel, createTimerModel, detailsModel with
        | None, None, None   -> model
        | Some _, None, None -> { model with SettingsPageModel = None }
        | _, Some _, None    -> { model with CreateTimerModel = None }
        | _, _, Some _ -> { model with DetailsPageModel = None }

    let update dbPath message model =
        let handleMainExternalMessage externalMessage = 
            match externalMessage with
            | MainPage.ExternalMessage.NoOp                      -> Cmd.none
            | MainPage.ExternalMessage.NavigateToNewTimer timer  -> Cmd.ofMsg (GoToEditPage timer)
            | MainPage.ExternalMessage.NavigateToViewTimer timer -> Cmd.ofMsg (GoToViewPage timer)

        let handleCreateTimerExternalMessage externalMessage =
            match externalMessage with
            | CreateTimerPage.ExternalMessage.NoOp                          -> Cmd.none
            | CreateTimerPage.ExternalMessage.GoBackAfterTimerAdded timer   -> Cmd.ofMsg (UpdateWhenTimerAdded timer)
            | CreateTimerPage.ExternalMessage.GoBackAfterTimerUpdated timer -> Cmd.ofMsg (UpdateWhenTimerUpdated timer)

        let handleDetailsExternalMessage externalMessage =
            match externalMessage with
            | DetailsPage.ExternalMessage.NoOp -> Cmd.none
            | DetailsPage.ExternalMessage.GoBackAfterTimerStarted -> Cmd.ofMsg UpdateWhenTimerStarted
            | DetailsPage.ExternalMessage.GoBackAfterTimerRemoved timer -> Cmd.ofMsg (UpdateWhenTimerDeleted timer)

        match message with
        | NavigationPopped ->
            navigationMapper model, Cmd.none

        | MainPageMessage message ->
            let m, externalMessage = MainPage.update message model.MainPageModel
            let command2 = handleMainExternalMessage externalMessage
            let batchCommand = Cmd.batch [command2]
            { model with MainPageModel = m }, batchCommand

        | GoToEditPage timer ->
            let editPage, editMessage = CreateTimerPage.init timer ()
            {model with CreateTimerModel = Some editPage}, Cmd.none

        | GoToViewPage timer ->
            let viewPage, viewMessage = DetailsPage.init timer ()
            {model with DetailsPageModel = Some viewPage}, Cmd.none
            
        | CreateTimerPageMessage message ->
            let createTimerModel, cmdMessage, externalMessage = CreateTimerPage.update dbPath message model.CreateTimerModel.Value
            let cmdMessage2 = handleCreateTimerExternalMessage externalMessage
            let batchCommand = Cmd.batch[(Cmd.map CreateTimerPageMessage cmdMessage); cmdMessage2]
            { model with CreateTimerModel = Some createTimerModel }, batchCommand

        | DetailsPageMessage message ->
            let detailsModel, cmdMessage, externalMessage = DetailsPage.update dbPath message model.DetailsPageModel.Value
            let cmdMessage2 = handleDetailsExternalMessage externalMessage
            let batchCommand = Cmd.batch[(Cmd.map DetailsPageMessage cmdMessage); cmdMessage2]
            {model with DetailsPageModel = Some detailsModel}, batchCommand
                
        | SettingsPageMessage message ->
            let m = SettingsPage.update message model.SettingsPageModel.Value
            {model with SettingsPageModel = Some m}, Cmd.none

        | ListViewSelectedItemChanged idx ->
            let settingsPage, settingsMessage = SettingsPage.init()
            { model with SettingsPageModel = Some settingsPage; FlyoutPresented = not model.FlyoutPresented }, Cmd.none

        | UpdateWhenTimerAdded timer ->
            let mainMessage = Cmd.ofMsg (MainPageMessage (MainPage.Message.TimerAdded timer))
            {model with CreateTimerModel = None}, mainMessage

        | UpdateWhenTimerUpdated timer ->
            let mainMessage = Cmd.ofMsg (MainPageMessage (MainPage.Message.TimerUpdated timer))
            {model with CreateTimerModel = None}, mainMessage

        | UpdateWhenTimerDeleted timer ->
            let mainMessage = Cmd.ofMsg (MainPageMessage (MainPage.Message.TimerDeleted timer))
            {model with DetailsPageModel = None}, mainMessage

        | UpdateWhenTimerStarted ->
            {model with DetailsPageModel = None}, Cmd.none

        | FlyoutPresented ->
            {model with FlyoutPresented = not model.FlyoutPresented}, Cmd.none

    let getPages allPages =
        let mainPage = allPages.MainPage
        let createTimerPage = allPages.CreateTimerPage
        let settingsPage = allPages.SettingsPage
        let detailsPage = allPages.DetailsPage
        match settingsPage, createTimerPage, detailsPage with
        | None, None, None                      -> [mainPage]
        | Some settings, None, None             -> [mainPage; settings]
        | _, Some createTimer, _                -> [mainPage; createTimer]
        | _, _, Some details                    -> [mainPage; details]        

    let view (model: Model) dispatch =
        let mainPage = MainPage.view model.MainPageModel (MainPageMessage >> dispatch)

        let createTimerPage = 
            model.CreateTimerModel
            |> Option.map (fun cModel -> CreateTimerPage.view cModel (CreateTimerPageMessage >> dispatch))        
        
        let settingsPage = 
            model.SettingsPageModel 
            |> Option.map (fun sModel -> SettingsPage.view sModel (SettingsPageMessage >> dispatch))        

        let detailsPage = 
            model.DetailsPageModel
            |> Option.map (fun dModel -> DetailsPage.view dModel (DetailsPageMessage >> dispatch))
        
        let allPages = 
            { MainPage = mainPage
              CreateTimerPage = createTimerPage
              SettingsPage = settingsPage
              DetailsPage = detailsPage}

        View.FlyoutPage(
            isPresentedChanged = (fun _ -> dispatch FlyoutPresented),
            isPresented = model.FlyoutPresented,
            flyout = View.ContentPage(
                title ="flyoutPage",    
                content = View.StackLayout( children = [
                    View.ListView(
                        selectionMode = ListViewSelectionMode.None,
                        items = [ 
                            View.TextCell "Settings" 
                        ], 
                        itemTapped = (fun idx -> dispatch (ListViewSelectedItemChanged idx))
                    )
                ])
            ), 
            detail = View.NavigationPage( 
                title = "details",
                popped = (fun _ -> dispatch NavigationPopped),
                pages = getPages allPages 
            )
        )




type App (dbPath) as app = 
    inherit Application ()

    let init = App.init dbPath
    let update = App.update dbPath
    let view = App.view   

    let runner =
       XamarinFormsProgram.mkProgram init update view
       |> Program.withConsoleTrace
       |> XamarinFormsProgram.run app
