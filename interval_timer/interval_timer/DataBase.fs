namespace IntervalTimer

open Timers.Timers
open SQLite

module DataBase = 
    type TimerDbObject() = 
        [<PrimaryKey; AutoIncrement>]
        member val Id = 0 with get, set
        member val Name = "" with get, set
        member val Reps = 0 with get, set
        member val Sets = 0 with get, set
        member val HighDurationMs = 0 with get, set
        member val LowDurationMs = 0 with get, set
        member val CooldownDurationMs = 0 with get, set
        member val WarmupDurationMs = 0 with get, set

    type BoardSettingsDbObject() =
        [<PrimaryKey; AutoIncrement>]
        member val Ip = "" with get, set

    let convertToTimerObject (item: Timer) = 
        let obj = TimerDbObject()
        obj.Id <- item.Id
        obj.Name <- item.Name
        obj.Reps <- item.Reps
        obj.Sets <- item.Sets
        obj.HighDurationMs <- durationToMilliseconds item.High
        obj.LowDurationMs <- durationToMilliseconds item.Low
        obj.CooldownDurationMs <- durationToMilliseconds item.Cooldown
        obj.WarmupDurationMs <- durationToMilliseconds item.Warmup
        obj

    let objectToTimer (obj: TimerDbObject) : Timer = 
        { Id = obj.Id
          Name = obj.Name
          Reps = obj.Reps
          Sets = obj.Sets
          High = millisecondToDuration obj.HighDurationMs
          Low = millisecondToDuration obj.LowDurationMs
          Warmup = millisecondToDuration obj.WarmupDurationMs
          Cooldown = millisecondToDuration obj.CooldownDurationMs
        }

    let connect dbPath = async {
        let db = SQLiteAsyncConnection(SQLiteConnectionString dbPath)
        do! db.CreateTableAsync<TimerDbObject>() |> Async.AwaitTask |> Async.Ignore
        return db
    }

    let loadAllTimers dbPath = async {
        let! database = connect dbPath
        let! objects = database.Table<TimerDbObject>().ToListAsync() |> Async.AwaitTask
        return objects |> Seq.toList |> List.map objectToTimer
    }

    let insertTimer dbPath timer = async {
        let! database = connect dbPath
        let object = convertToTimerObject timer
        do! database.InsertAsync(object) |> Async.AwaitTask |> Async.Ignore
        let! rowIdObj = database.ExecuteScalarAsync("select last_insert_rowid()", [||]) |> Async.AwaitTask
        let rowId = rowIdObj |> int
        return { timer with Id = rowId }
    }

    let updateTimer dbPath timer = async {
        let! database = connect dbPath
        let object = convertToTimerObject timer
        do! database.UpdateAsync(object) |> Async.AwaitTask |> Async.Ignore
        return timer
    }

    let deleteTimer dbPath timer = async {
        let! database = connect dbPath
        let object = convertToTimerObject timer
        do! database.DeleteAsync(object) |> Async.AwaitTask |> Async.Ignore
    }