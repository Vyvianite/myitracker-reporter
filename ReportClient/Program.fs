namespace ReportClient

open System
open Newtonsoft.Json
open HttpApi

module Program =

  [<EntryPoint>]
  let main args =
    match OSType.check () with //try to get os type
    | Error x ->
      1

    | Ok os ->
      let log = Logger.log os //Initialize a logger with ostype for proper paths
      let report = //gathers os specific data and returns a report object
        Reporter.create(os).Collate()

      Config.read ()
      |>function // Match the result of config.read
        | Error _ -> 
            Error "Error Reading config file"
        | Ok config ->
          let json = JsonConvert.SerializeObject //function alias for brevity
          Map [ "token", config.Token
                "customerId", config.CustomerId
                "processes", (json report.Processes)
                "events", (json report.Events)
                "drives", (json report.Drives)
                "memory", (json report.Memory)
                "OS", (json report.OSData)
                "computerName", report.ComputerName
                "computerId",report.ComputerId ]
          |> Some
          |> post (Uri <| config.Server + "logger/") "report" //Posting report to config uri
          |> Async.RunSynchronously
        
      |>function //match on the result, either a passed through config error, or a posting/network error.
        | Ok _ -> 0 // Oll Korrect
        | Error i ->
          log i true
          1
    

