namespace ReportClient

open System
open System.Text
open System.IO
open System.Runtime.InteropServices

//This type is basically a parameterized module.
type Logger (os) = 
  let directory =
    match os with 
    | Windows x -> @"C:\ProgramData\MyItracker\MyItrackerService\"
    | Mac x -> "./"
    //| Linux x -> "./"

  (*Cycles between two log files so the combined size never gets more than two megs*)
  let recycler directory =
    let alpha = FileInfo (Path.Combine(directory, "alpha.log"))
    let beta = FileInfo (Path.Combine(directory, "beta.log"))

    //Check if the file is over a mb.
    let tooBig (file : FileInfo) =
      file.Exists && file.Length / 1024L / 1024L >= 1L //If file doesn't exist, it obviously isn't too big.

    //Decides which file to write too, and if they are both too big, delete the oldest one.
    match (alpha.Exists, tooBig alpha, beta.Exists, tooBig beta) with
    | (false, _, _, _) ->
        alpha.Create () |> ignore
        alpha.FullName
    | (_, false, _, _) ->
        alpha.FullName
    | (_, _, false, _) ->
        beta.Create () |> ignore
        beta.FullName
    | (_, _, _, false) ->
        beta.FullName
    | (_, _, _, _) ->
        alpha.Delete ()
        File.Move (beta.FullName, alpha.FullName)
        beta.FullName

  ///<summary> Writes to a platform specific log file with optional ending delimiter </summary>
  member this.log(text, ?finished) =
    try
      Directory.CreateDirectory directory |> ignore //Make sure directory exists
      let fullPath = recycler directory //recycle log files to manage size
      use writer = new StreamWriter(fullPath, true)
      writer.BaseStream.Seek(0L, SeekOrigin.End) |> ignore //Find the end of the file for appending
      writer.WriteLine (sprintf "%s; %s" text (DateTime.UtcNow.ToString "yyyy-MM-dd HH:mm:ss.fff")) //Write text and datetime
      match finished with
      | Some x ->
          writer.WriteLine "=========="
          writer.WriteLine ()
      | None -> ()
      writer.Flush ()
      writer.Close ()
    with
    | _ -> () //Simply swallow any errors. If this part doesn't work then there's nowhere to log it.