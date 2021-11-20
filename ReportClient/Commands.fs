namespace ReportClient

open System
open CliWrap
open Microsoft.FSharp.Reflection

type Commander =
  abstract shutdown : unit -> bool
  abstract restart : unit -> bool

type InCommand =
  { command : string }

module Commands =

  let create os =
    match os with
    | Windows x ->
      { new Commander with
          member x.shutdown () =
            false

          member x.restart () =
            false }
    | Mac x ->
      { new Commander with
          member x.shutdown () =
            false

          member x.restart () =
            false }

  type Command =
  | Shutdown
  | Restart
  with
    static member fromstring command = Utilities.fromString<Command> command

  let run (commander : Commander) command =
    let command = Command.fromstring command
    match command with
    | Some x ->
        match x with
        | Shutdown -> Some (commander.shutdown ())
        | Restart -> Some (commander.restart ())
    | None -> 
        None