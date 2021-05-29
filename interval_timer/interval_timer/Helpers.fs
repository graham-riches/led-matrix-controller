namespace IntervalTimer

module Helpers = 
    let rec zip xs ys = 
        match xs with
        | [] -> []
        | headX :: tailX ->
            match ys with
            | [] -> []
            | headY :: tailY -> (headX, headY) :: zip tailX tailY

