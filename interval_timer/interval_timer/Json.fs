namespace DomainOutput


module Json =
    open Newtonsoft.Json

    let serialize obj = 
        JsonConvert.SerializeObject obj

