namespace IntervalTimer

open System.Diagnostics
open Fabulous
open Fabulous.XamarinForms
open Fabulous.XamarinForms.LiveUpdate
open Xamarin.Forms


module App = 
    type Message = 
        | MainPageMessage of MainPage.Message 

    type Model = 
      { MainPageModel: MainPage.Model }

    type Pages = 
        { MainPage: ViewElement}


    let init () = 
        let mainModel, mainMessage = MainPage.init()
        let initialModel = 
            { MainPageModel = mainModel }
        initialModel, mainMessage
    

    let update message model =
        match message with
        | MainPageMessage message ->
            let mainModel, command = MainPage.update message model.MainPageModel
            { model with MainPageModel = mainModel }, Cmd.ofMsg (MainPageMessage command)


    let getPages allPages =
        let mainPage = allPages.MainPage
        [mainPage]


    let view (model: Model) dispatch =
        let mainPage = MainPage.view model.MainPageModel (MainPageMessage >> dispatch)

        let allPages = 
            { MainPage = mainPage }
        View.NavigationPage(
            pages=getPages allPages    
        )

    






type App () as app = 
    inherit Application ()

    let init = App.init
    let update = App.update
    let view = App.view   

    let runner =
       XamarinFormsProgram.mkProgram init update view
       |> Program.withConsoleTrace
       |> XamarinFormsProgram.run app
