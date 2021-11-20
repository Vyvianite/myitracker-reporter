namespace ReportClient

open System
open System.Text
open System.IO
open System.Runtime.InteropServices
open System.Security.Cryptography
open System.Globalization

type OSData = 
  { Name : string
    Description : string
    Version : string }

type OSType =
  | Windows of OSData
  | Mac of OSData
  //| Linux of OSData
  member this.unwrap () =
    match this with
    | Windows x -> x
    | Mac x -> x

module OSType =
  let private osFactory name =
    { Name = name
      Description = RuntimeInformation.OSDescription
      Version = Environment.OSVersion.VersionString }

  ///<summary> Checks for a supported runtime and returns a result union. </summary>
  let check () =
    try 
      if RuntimeInformation.IsOSPlatform(OSPlatform.Windows) then
        Ok (Windows (osFactory "Windows" ))
      elif RuntimeInformation.IsOSPlatform(OSPlatform.OSX) then
        Ok (Mac (osFactory "Mac"))
      //elif RuntimeInformation.IsOSPlatform(OSPlatform.Linux) then
        //Ok (Linux (osFactory "Linux"))
      else
        Error "Unsupported platform"
    with
      | _ as e -> Error ("OS checking threw an exception: " + e.Message)
